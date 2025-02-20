using System.Buffers;
using System.Text;

namespace ElysiaNBT;

public class BinaryNbtReader : BinaryReaderEx, INbtReader
{
    protected const int PAYLOAD_BUFFER_SIZE = 1024;

    protected int[] _arrayLength;
    protected NbtTagType[] _arrayPayloadType;
    protected Union _storedPayloadValue = new();
    protected int _remainingSize = 0;
    protected Encoding _encoding;
    protected Decoder _decoder;
    protected byte[] _tempByteBuffer = ArrayPool<byte>.Shared.Rent(PAYLOAD_BUFFER_SIZE);
    protected char[] _tempCharBuffer = ArrayPool<char>.Shared.Rent(PAYLOAD_BUFFER_SIZE);
    protected int _tempBufferCurrentLength = 0;

    public NbtOptions Options { get; protected set; }
    public TokenType TokenType { get; protected set; }
    public INbtReader.State CurrentState { get; protected set; }
    public int CurrentDepth { get; protected set; }
    public int CurrentContentLength { get; protected set; }
    public ref Union StoredPayloadValue => ref _storedPayloadValue;

    public NbtTagType PayloadType { get; protected set; }

    public BinaryNbtReader(byte[] bytes, NbtOptions options)
        : this(bytes, new BinaryNbtOptions(options)) { }
    public BinaryNbtReader(Stream stream, NbtOptions options, bool leaveOpen = true)
        : this(stream, new BinaryNbtOptions(options), leaveOpen) { }
    public BinaryNbtReader(byte[] bytes, BinaryNbtOptions options)
        : base(bytes)
    {
        Options = options.GenericOptions;
        _arrayLength = ArrayPool<int>.Shared.Rent(options.GenericOptions.MaxDepth);
        _arrayLength[0] = -1;
        _arrayPayloadType = ArrayPool<NbtTagType>.Shared.Rent(options.GenericOptions.MaxDepth);
        _encoding = options.StringEncoding;
        _decoder = _encoding.GetDecoder();
        UseVarInt = options.UseVarInt;
        IsLittleEndian = options.IsLittleEndian;
        CurrentState = INbtReader.State.ReadingName;
    }
    public BinaryNbtReader(Stream stream, BinaryNbtOptions options, bool leaveOpen = true)
        : base(stream, leaveOpen)
    {
        Options = options.GenericOptions;
        _arrayLength = ArrayPool<int>.Shared.Rent(options.GenericOptions.MaxDepth);
        _arrayLength[0] = -1;
        _arrayPayloadType = ArrayPool<NbtTagType>.Shared.Rent(options.GenericOptions.MaxDepth);
        _encoding = options.StringEncoding;
        _decoder = _encoding.GetDecoder();
        UseVarInt = options.UseVarInt;
        IsLittleEndian = options.IsLittleEndian;
        CurrentState = INbtReader.State.ReadingName;
    }

    public TokenType Read()
    {
        Skip();

        _storedPayloadValue = new();

        switch (CurrentState)
        {
            case INbtReader.State.ReadingName:
                int payloadType = ReadByte();
                if ((!Options.HasRootName || payloadType == -1) && CurrentDepth == 0)
                {
                    CurrentState = INbtReader.State.Stopped;
                    TokenType = TokenType.None;
                    break;
                }
                PayloadType = NbtException.ThrowIfTypeIsInvalid(payloadType, Position);
                if (PayloadType == NbtTagType.End)
                {
                    TokenType = TokenType.EndCompound;
                    CurrentDepth--;
                    if (_arrayLength[CurrentDepth] >= 0)
                    {
                        CurrentState = INbtReader.State.ReadingPayload;
                    }
                }
                else
                {
                    CurrentState = INbtReader.State.ReadingPayload;
                    TokenType = TokenType.Name;
                    CurrentContentLength = _remainingSize = ReadStringLength();
                }
                break;
            case INbtReader.State.ReadingPayload:
                if (!Options.HasRootName && CurrentDepth == 0)
                    PayloadType = NbtException.ThrowIfTypeIsInvalid(ReadByte(), Position);
                ref int arrLen = ref _arrayLength[CurrentDepth];
                if (arrLen == 0 || arrLen > 0 && _arrayPayloadType[CurrentDepth] == NbtTagType.End)
                {
                    TokenType = TokenType.EndArray;
                    CurrentDepth--;
                    if (_arrayLength[CurrentDepth] < 0)
                    {
                        CurrentState = INbtReader.State.ReadingName;
                    }
                }
                else
                {
                    if (arrLen > 0)
                    {
                        PayloadType = _arrayPayloadType[CurrentDepth];
                        arrLen--;
                    }
                    switch (PayloadType)
                    {
                        case NbtTagType.End:
                            TokenType = TokenType.EndCompound;
                            CurrentDepth--;
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Byte:
                            TokenType = TokenType.Byte;
                            StoredPayloadValue.I8 = ReadI8();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Short:
                            TokenType = TokenType.Short;
                            StoredPayloadValue.I16 = ReadI16();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Int:
                            TokenType = TokenType.Int;
                            StoredPayloadValue.I32 = ReadI32();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Long:
                            TokenType = TokenType.Long;
                            StoredPayloadValue.I64 = ReadI64();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Float:
                            TokenType = TokenType.Float;
                            StoredPayloadValue.FP32 = ReadFP32();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.Double:
                            TokenType = TokenType.Double;
                            StoredPayloadValue.FP64 = ReadFP64();
                            CurrentContentLength = -1;
                            break;
                        case NbtTagType.ByteArray:
                            TokenType = TokenType.StartByteArray;
                            CurrentDepth++;
                            _arrayPayloadType[CurrentDepth] = NbtTagType.Byte;
                            int len = ReadI32();
                            if (len < 0)
                                len = 0;
                            CurrentContentLength = _arrayLength[CurrentDepth] = len;
                            break;
                        case NbtTagType.String:
                            TokenType = TokenType.String;
                            CurrentContentLength = _remainingSize = ReadStringLength();
                            break;
                        case NbtTagType.List:
                            TokenType = TokenType.StartList;
                            CurrentDepth++;
                            _arrayPayloadType[CurrentDepth] = NbtException.ThrowIfTypeIsInvalid(ReadByte(), Position);
                            len = ReadI32();
                            if (len < 0)
                                len = 0;
                            CurrentContentLength = _arrayLength[CurrentDepth] = len;
                            break;
                        case NbtTagType.Compound:
                            TokenType = TokenType.StartCompound;
                            CurrentDepth++;
                            CurrentContentLength = _arrayLength[CurrentDepth] = -1;
                            break;
                        case NbtTagType.IntArray:
                            TokenType = TokenType.StartIntArray;
                            CurrentDepth++;
                            _arrayPayloadType[CurrentDepth] = NbtTagType.Int;
                            len = ReadI32();
                            if (len < 0)
                                len = 0;
                            CurrentContentLength = _arrayLength[CurrentDepth] = len;
                            break;
                        case NbtTagType.LongArray:
                            TokenType = TokenType.StartLongArray;
                            CurrentDepth++;
                            _arrayPayloadType[CurrentDepth] = NbtTagType.Long;
                            len = ReadI32();
                            if (len < 0)
                                len = 0;
                            CurrentContentLength = _arrayLength[CurrentDepth] = len;
                            break;
                        default:
                            throw new NbtException("Unknown NBT tag.");
                    }
                    if (_arrayLength[CurrentDepth] < 0)
                    {
                        CurrentState = INbtReader.State.ReadingName;
                    }
                }
                break;
            case INbtReader.State.Stopped:
                TokenType = TokenType.None;
                break;
        }
        return TokenType;
    }
    public bool GetBool(bool strict = false)
        => ((INbtReader)this).BaseGetBool(strict);
    public byte GetByte(bool strict = false)
        => ((INbtReader)this).BaseGetByte(strict);
    public sbyte GetSByte(bool strict = false)
        => ((INbtReader)this).BaseGetSByte(strict);
    public short GetShort(bool strict = false)
        => ((INbtReader)this).BaseGetShort(strict);
    public ushort GetUShort(bool strict = false)
        => ((INbtReader)this).BaseGetUShort(strict);
    public int GetInt(bool strict = false)
        => ((INbtReader)this).BaseGetInt(strict);
    public uint GetUInt(bool strict = false)
        => ((INbtReader)this).BaseGetUInt(strict);
    public long GetLong(bool strict = false)
        => ((INbtReader)this).BaseGetLong(strict);
    public ulong GetULong(bool strict = false)
        => ((INbtReader)this).BaseGetULong(strict);
    public float GetFloat(bool strict = false)
        => ((INbtReader)this).BaseGetFloat(strict);
    public double GetDouble(bool strict = false)
        => ((INbtReader)this).BaseGetDouble(strict);
    public string GetString()
    {
        switch (TokenType)
        {
            case TokenType.Name:
            case TokenType.String:
                char[] charBuffer = ArrayPool<char>.Shared.Rent(_encoding.GetMaxCharCount(_remainingSize));
                Span<char> _tempCharBuffer = charBuffer;
                int charUsed = 0;
                while (_remainingSize > 0)
                {
                    Span<byte> byteBuffer = _tempBufferCurrentLength + _remainingSize < _tempByteBuffer.Length ?
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength, _remainingSize) :
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength);
                    int readB = ReadBlock(byteBuffer);
                    _remainingSize -= readB;
                    byteBuffer = _tempByteBuffer.AsSpan(0, _tempBufferCurrentLength + readB);
                    _decoder.Convert(byteBuffer, _tempCharBuffer, _remainingSize == 0, out int byteUsed, out charUsed, out _);
                    _tempBufferCurrentLength = readB - byteUsed;
                    byteBuffer[byteUsed..].CopyTo(byteBuffer);
                    _tempCharBuffer = _tempCharBuffer[charUsed..];
                }
                string part = charBuffer.AsSpan(0, charUsed).ToString();
                ArrayPool<char>.Shared.Return(charBuffer);
                return part;
            case TokenType.Byte:
                return GetSByte().ToString();
            case TokenType.Short:
                return GetShort().ToString();
            case TokenType.Int:
                return GetInt().ToString();
            case TokenType.Long:
                return GetLong().ToString();
            case TokenType.Float:
                return GetFloat().ToString();
            case TokenType.Double:
                return GetDouble().ToString();
            default:
                throw new NotSupportedException();
        }
    }
    public char[] GetCharArray()
    {
        switch (TokenType)
        {
            case TokenType.Name:
            case TokenType.String:
                char[] charBuffer = ArrayPool<char>.Shared.Rent(_encoding.GetMaxCharCount(_remainingSize));
                Span<char> _tempCharBuffer = charBuffer;
                int charUsed = 0;
                while (_remainingSize > 0)
                {
                    Span<byte> byteBuffer = _tempBufferCurrentLength + _remainingSize < _tempByteBuffer.Length ?
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength, _remainingSize) :
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength);
                    int readB = ReadBlock(byteBuffer);
                    _remainingSize -= readB;
                    byteBuffer = _tempByteBuffer.AsSpan(0, _tempBufferCurrentLength + readB);
                    _decoder.Convert(byteBuffer, _tempCharBuffer, _remainingSize == 0, out int byteUsed, out charUsed, out _);
                    _tempBufferCurrentLength = readB - byteUsed;
                    byteBuffer[byteUsed..].CopyTo(byteBuffer);
                    _tempCharBuffer = _tempCharBuffer[charUsed..];
                }
                char[] part = charBuffer[..charUsed];
                ArrayPool<char>.Shared.Return(charBuffer);
                return part;
            case TokenType.Byte:
                return GetSByte().ToString().ToCharArray();
            case TokenType.Short:
                return GetShort().ToString().ToCharArray();
            case TokenType.Int:
                return GetInt().ToString().ToCharArray();
            case TokenType.Long:
                return GetLong().ToString().ToCharArray();
            case TokenType.Float:
                return GetFloat().ToString().ToCharArray();
            case TokenType.Double:
                return GetDouble().ToString().ToCharArray();
            default:
                throw new NotSupportedException();
        }
    }
    public ReadOnlySpan<char> GetCharSpan()
    {
        switch (TokenType)
        {
            case TokenType.Name:
            case TokenType.String:
                char[] charBuffer = GC.AllocateUninitializedArray<char>(_encoding.GetMaxCharCount(_remainingSize));
                bool completed = false;
                Span<char> _tempCharBuffer = charBuffer;
                int charUsed = 0;
                while (!completed)
                {
                    Span<byte> byteBuffer = _tempBufferCurrentLength + _remainingSize < _tempByteBuffer.Length ?
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength, _remainingSize) :
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength);
                    int readB = ReadBlock(byteBuffer);
                    _remainingSize -= readB;
                    byteBuffer = _tempByteBuffer.AsSpan(0, _tempBufferCurrentLength + readB);
                    _decoder.Convert(byteBuffer, _tempCharBuffer, false, out int byteUsed, out charUsed, out completed);
                    _tempBufferCurrentLength = readB - byteUsed;
                    byteBuffer[byteUsed..].CopyTo(byteBuffer);
                    _tempCharBuffer = _tempCharBuffer[charUsed..];
                }
                return charBuffer.AsSpan(0, charUsed);
            case TokenType.Byte:
                return GetSByte().ToString();
            case TokenType.Short:
                return GetShort().ToString();
            case TokenType.Int:
                return GetInt().ToString();
            case TokenType.Long:
                return GetLong().ToString();
            case TokenType.Float:
                return GetFloat().ToString();
            case TokenType.Double:
                return GetDouble().ToString();
            default:
                throw new NotSupportedException();
        }
    }
    public Span<char> GetSafeCharSpan()
    {
        switch (TokenType)
        {
            case TokenType.Name:
            case TokenType.String:
                char[] charBuffer = GC.AllocateUninitializedArray<char>(_encoding.GetMaxCharCount(_remainingSize));
                bool completed = false;
                Span<char> _tempCharBuffer = charBuffer;
                int charUsed = 0;
                while (!completed)
                {
                    Span<byte> byteBuffer = _tempBufferCurrentLength + _remainingSize < _tempByteBuffer.Length ?
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength, _remainingSize) :
                    _tempByteBuffer.AsSpan(_tempBufferCurrentLength);
                    int readB = ReadBlock(byteBuffer);
                    _remainingSize -= readB;
                    byteBuffer = _tempByteBuffer.AsSpan(0, _tempBufferCurrentLength + readB);
                    _decoder.Convert(byteBuffer, _tempCharBuffer, false, out int byteUsed, out charUsed, out completed);
                    _tempBufferCurrentLength = readB - byteUsed;
                    byteBuffer[byteUsed..].CopyTo(byteBuffer);
                    _tempCharBuffer = _tempCharBuffer[charUsed..];
                }
                return charBuffer.AsSpan(0, charUsed);
            case TokenType.Byte:
                return GetSByte().ToString().ToCharArray();
            case TokenType.Short:
                return GetShort().ToString().ToCharArray();
            case TokenType.Int:
                return GetInt().ToString().ToCharArray();
            case TokenType.Long:
                return GetLong().ToString().ToCharArray();
            case TokenType.Float:
                return GetFloat().ToString().ToCharArray();
            case TokenType.Double:
                return GetDouble().ToString().ToCharArray();
            default:
                throw new NotSupportedException();
        }
    }

    public void Skip()
    {
        if (_remainingSize > 0)
            SkipBytes(_remainingSize);
        _remainingSize = 0;
    }

    public TokenType ParsePayload()
    {
        return TokenType;
    }

    public override void Close()
    {
        ArrayPool<int>.Shared.Return(_arrayLength);
        ArrayPool<NbtTagType>.Shared.Return(_arrayPayloadType);
        ArrayPool<char>.Shared.Return(_tempCharBuffer);
        ArrayPool<byte>.Shared.Return(_tempByteBuffer);
        base.Close();
    }
}
