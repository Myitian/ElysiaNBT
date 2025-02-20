using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace ElysiaNBT;

public class BinaryNbtWriter : BinaryWriterEx, INbtWriter
{
    protected const int PAYLOAD_BUFFER_SIZE = 1024;

    protected Encoder _encoder;
    protected int[] _inArray;
    protected NbtTagType[] _arrayItemType;
    protected NbtTagType payloadType;
    protected int[]? _posMemory0;
    protected int[]? _posMemory1;
    protected int _tempBufferCurrentLength = 0;


    public bool RequiresLengthInfo => true;
    public BinaryNbtOptions BinaryNbtOptions { get; protected set; }
    public NbtOptions Options { get; protected set; }
    public INbtWriter.State CurrentState { get; protected set; }
    public int CurrentDepth { get; protected set; }

    public BinaryNbtWriter(Stream stream, NbtOptions options, bool leaveOpen = true)
        : this(stream, new BinaryNbtOptions(options), leaveOpen) { }
    public BinaryNbtWriter(Stream stream, BinaryNbtOptions options, bool leaveOpen = true)
        : base(stream, leaveOpen)
    {
        Options = options.GenericOptions;
        _inArray = ArrayPool<int>.Shared.Rent(Options.MaxDepth);
        _inArray[0] = -1;
        _encoder = options.StringEncoding.GetEncoder();
        _arrayItemType = ArrayPool<NbtTagType>.Shared.Rent(Options.MaxDepth);
        UseVarInt = options.UseVarInt;
        IsLittleEndian = options.IsLittleEndian;
        CurrentState = options.GenericOptions.HasRootName ?
            INbtWriter.State.WritingName :
            INbtWriter.State.WritingPayload;
    }

    protected int WriteString(ReadOnlySpan<char> str, bool flush)
    {
        WriteStringLength(_encoder.GetByteCount(str, true));
        bool completed = false;
        int consumed = 0;
        Span<byte> byteBuffer = stackalloc byte[PAYLOAD_BUFFER_SIZE];
        while (!completed)
        {
            _encoder.Convert(str, byteBuffer, flush, out int charsUsed, out int bytesUsed, out completed);
            if (charsUsed == 0 && bytesUsed == 0)
                break;
            WriteBlock(byteBuffer[..bytesUsed]);
            str = str[charsUsed..];
            consumed += charsUsed;
        }
        return consumed;
    }
    protected void SetPayloadType(NbtTagType type)
    {
        if (HavePos() && payloadType == NbtTagType.Unknown)
        {
            SwapPos();
            WriteByte((byte)type);
            payloadType = type;
            LoadPos();
        }
    }
    protected bool HavePos(bool slot1 = false)
    {
        if (slot1)
        {
            if (_posMemory1 is null)
                return false;
            return _posMemory1[CurrentDepth] != -1;
        }
        else
        {
            if (_posMemory0 is null)
                return false;
            return _posMemory0[CurrentDepth] != -1;
        }
    }
    protected void LoadPos(bool slot1 = false)
    {
        if (slot1)
        {
            if (_posMemory1 is null)
                throw new Exception();
            Position = _posMemory1[CurrentDepth];
            _posMemory1[CurrentDepth] = -1;
        }
        else
        {
            if (_posMemory0 is null)
                throw new Exception();
            Position = _posMemory0[CurrentDepth];
            _posMemory0[CurrentDepth] = -1;
        }
    }
    protected void SwapPos(bool slot1 = false)
    {
        if (slot1)
        {
            if (_posMemory1 is null)
                throw new Exception();
            (_posMemory1[CurrentDepth], Position) = (Position, _posMemory1[CurrentDepth]);
        }
        else
        {
            if (_posMemory0 is null)
                throw new Exception();
            (_posMemory0[CurrentDepth], Position) = (Position, _posMemory0[CurrentDepth]);
        }
    }
    protected void SavePos(bool slot1 = false)
    {
        if (slot1)
        {
            if (_posMemory1 is null)
            {
                _posMemory1 = ArrayPool<int>.Shared.Rent(Options.MaxDepth);
                Array.Fill(_posMemory1, -1);
            }
            _posMemory1[CurrentDepth] = Position;
        }
        else
        {
            if (_posMemory0 is null)
            {
                _posMemory0 = ArrayPool<int>.Shared.Rent(Options.MaxDepth);
                Array.Fill(_posMemory0, -1);
            }
            _posMemory0[CurrentDepth] = Position;
        }
    }
    protected bool TrySaveLengthPos(int length = -1)
    {
        if (length < 0)
        {
            if (UseVarInt)
                throw new NotSupportedException();
            if (!_baseStream.CanSeek)
                throw new NotSupportedException();
        }
        else
        {
            if (UseVarInt)
                return false;
            if (!_baseStream.CanSeek)
                return false;
        }
        SavePos(true);
        return true;
    }

    public bool IsInArray => _inArray[CurrentDepth] >= 0;
    public void WriteEndArray()
    {
        WriteLength(_inArray[CurrentDepth]);
        _inArray[CurrentDepth] = -1;
        CurrentDepth--;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WriteEndCompound()
    {
        WriteByte((byte)NbtTagType.End);
        _inArray[CurrentDepth--] = -1;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WriteName(ReadOnlySpan<char> name, NbtTagType tagType = NbtTagType.Unknown)
    {
        payloadType = tagType;
        if (tagType == NbtTagType.Unknown)
        {
            if (_baseStream.CanSeek)
                SavePos();
            else
                throw new NotSupportedException();
        }
        WriteByte((byte)tagType);
        WriteString(name, true);
        CurrentState = INbtWriter.State.WritingPayload;
    }
    public void WriteLength(int length)
    {
        if (HavePos(true))
        {
            SwapPos(true);
            WriteI32(length);
            LoadPos(true);
        }
    }
    public void WriteStartList(NbtTagType itemType = NbtTagType.Unknown, int length = -1)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.List);
        _inArray[++CurrentDepth] = 0;
        payloadType = itemType;
        if (itemType == NbtTagType.Unknown)
        {
            if (_baseStream.CanSeek)
                SavePos();
            else
                throw new NotSupportedException();
        }
        WriteByte((byte)itemType);
        TrySaveLengthPos(length);
        WriteI32(length);
        CurrentState = INbtWriter.State.WritingPayload;
    }
    public void WriteStartByteArray(int length = -1)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.ByteArray);
        _inArray[++CurrentDepth] = 0;
        TrySaveLengthPos(length);
        WriteI32(length);
        CurrentState = INbtWriter.State.WritingPayload;
    }
    public void WriteStartIntArray(int length = -1)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.IntArray);
        _inArray[++CurrentDepth] = 0;
        TrySaveLengthPos(length);
        WriteI32(length);
        CurrentState = INbtWriter.State.WritingPayload;
    }
    public void WriteStartLongArray(int length = -1)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.LongArray);
        _inArray[++CurrentDepth] = 0;
        TrySaveLengthPos(length);
        WriteI32(length);
        CurrentState = INbtWriter.State.WritingPayload;
    }
    public void WriteStartCompound()
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Compound);
        _inArray[++CurrentDepth] = -1;
        CurrentState = INbtWriter.State.WritingName;
    }
    public void WriteEnd()
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.End);
        WriteByte((byte)NbtTagType.End);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WriteTrue()
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Byte);
        WriteU8(1);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WriteFalse()
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Byte);
        WriteU8(0);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(byte payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Byte);
        WriteU8(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(bool payload)
    {
        if (payload)
            WriteTrue();
        else
            WriteFalse();
    }
    public void WritePayload(sbyte payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Byte);
        WriteI8(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(short payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Short);
        WriteI16(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ushort payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Short);
        WriteU16(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(int payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Int);
        WriteI32(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(uint payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Int);
        WriteU32(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(long payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Long);
        WriteI64(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ulong payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Long);
        WriteU64(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(float payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Float);
        WriteFP32(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(double payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.Double);
        WriteFP64(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ReadOnlySpan<char> payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.String);
        WriteString(payload, true);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ReadOnlySpan<byte> payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.ByteArray);
        WriteI32(payload.Length);
        WriteBlock(payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ReadOnlySpan<sbyte> payload)
    {
        WritePayload(MemoryMarshal.Cast<sbyte, byte>(payload));
    }
    public void WritePayload(ReadOnlySpan<int> payload)
    {
        WritePayload(MemoryMarshal.Cast<int, uint>(payload));
    }
    public void WritePayload(ReadOnlySpan<uint> payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.IntArray);
        WriteI32(payload.Length);
        if (BitConverter.IsLittleEndian == IsLittleEndian)
        {
            WriteBlock(MemoryMarshal.Cast<uint, byte>(payload));
        }
        else
        {
            foreach (uint v in payload)
                WriteSimpleU32(v);
        }
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }
    public void WritePayload(ReadOnlySpan<long> payload)
    {
        WritePayload(MemoryMarshal.Cast<long, ulong>(payload));
    }
    public void WritePayload(ReadOnlySpan<ulong> payload)
    {
        if (_inArray[CurrentDepth] >= 0)
            _inArray[CurrentDepth]++;
        SetPayloadType(NbtTagType.LongArray);
        WriteI32(payload.Length);
        if (BitConverter.IsLittleEndian == IsLittleEndian)
        {
            WriteBlock(MemoryMarshal.Cast<ulong, byte>(payload));
        }
        else
        {
            foreach (ulong v in payload)
                WriteSimpleU64(v);
        }
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public override void Close()
    {
        ArrayPool<int>.Shared.Return(_inArray);
        ArrayPool<NbtTagType>.Shared.Return(_arrayItemType);
        if (_posMemory0 is not null)
            ArrayPool<int>.Shared.Return(_posMemory0);
        if (_posMemory1 is not null)
            ArrayPool<int>.Shared.Return(_posMemory1);
        base.Close();
    }
}
