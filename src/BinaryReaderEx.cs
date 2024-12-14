namespace ElysiaNBT;

public class BinaryReaderEx(Stream stream, bool leaveOpen = false) : IDisposable
{
    public const int EOF = -1;

    protected bool _noMore = false;
    protected Stream _baseStream = stream;

    private readonly bool _leaveOpen = leaveOpen;
    private bool _disposed = false;
    public int Position { get; protected set; }
    public bool IsLittleEndian { get; set; }
    public bool UseVarInt { get; set; }

    public BinaryReaderEx(byte[] bytes) : this(new MemoryStream(bytes))
    {
    }

    protected int ReadBlock(Span<byte> buffer)
    {
        int read = _baseStream.Read(buffer);
        Position += read;
        return read;
    }
    protected int ReadByte()
    {
        int read = _baseStream.ReadByte();
        if (read == EOF)
        {
            if (_noMore)
                return EOF;
            _noMore = true;
            Position++;
        }
        Position++;
        return read;
    }
    protected bool SkipBytes(int skip)
    {
        int read = 0;
        while (skip > 0 && (read = _baseStream.ReadByte()) != EOF)
        {
            Position++;
            skip--;
        }
        if (read == EOF)
        {
            Position++;
            _noMore = true;
        }
        return _noMore;
    }

    protected sbyte ReadI8()
    {
        return (sbyte)ReadU8();
    }
    protected byte ReadU8()
    {
        int read = ReadByte();
        return read >= 0 ? (byte)read : throw new EndOfStreamException();
    }
    protected short ReadI16()
    {
        return (short)ReadU16();
    }
    protected ushort ReadU16()
    {
        byte a = ReadU8();
        byte b = ReadU8();
        return (ushort)(IsLittleEndian ? b << 8 | a : a << 8 | b);
    }
    protected int ReadStringLength()
    {
        return UseVarInt ? (int)ReadVarInt() : ReadU16();
    }
    protected int ReadI32()
    {
        return (int)ReadU32();
    }
    protected uint ReadU32()
    {
        if (UseVarInt)
        {
            return ReadVarInt(true);
        }
        else
        {
            uint a = ReadU16();
            uint b = ReadU16();
            return IsLittleEndian ? b << 16 | a : a << 16 | b;
        }
    }
    protected long ReadI64()
    {
        return (long)ReadU64();
    }
    protected ulong ReadU64()
    {
        if (UseVarInt)
        {
            return ReadVarLong(true);
        }
        else
        {
            ulong a = ReadU32();
            ulong b = ReadU32();
            return IsLittleEndian ? b << 32 | a : a << 32 | b;
        }
    }
    protected float ReadFP32()
    {
        return BitConverter.UInt32BitsToSingle(ReadU32());
    }
    protected double ReadFP64()
    {
        return BitConverter.UInt64BitsToDouble(ReadU64());
    }
    protected uint ReadVarInt(bool zigzag = false)
    {
        uint result = 0;
        int shift = 0;
        while (true)
        {
            int b = ReadByte();
            if (b < 0)
            {
                throw new EndOfStreamException();
            }
            result |= (uint)(b & 0x7f) << shift;
            if ((b & 0x80) != 0x80)
            {
                break;
            }
            shift += 7;
        }
        return zigzag ? result >> 1 | result << 31 : result;
    }
    protected ulong ReadVarLong(bool zigzag = false)
    {
        ulong result = 0;
        int shift = 0;
        while (true)
        {
            int b = ReadByte();
            if (b < 0)
            {
                throw new EndOfStreamException();
            }
            result |= (ulong)(b & 0x7f) << shift;
            if ((b & 0x80) != 0x80)
            {
                break;
            }
            shift += 7;
        }
        return zigzag ? result >> 1 | result << 63 : result;
    }

    public virtual void Close()
    {
        Dispose(true);
    }
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        try
        {
            if (disposing && !_leaveOpen)
                _baseStream.Close();
        }
        finally { }
    }
}
