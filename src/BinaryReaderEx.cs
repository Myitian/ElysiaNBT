using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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
        if (read == 0)
            throw new EndOfStreamException();
        Position += read;
        return read;
    }
    protected int ReadBlockExactly(Span<byte> buffer, bool throwOnEndOfStream = true)
    {
        int read = _baseStream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream);
        Position += read;
        return read;
    }
    protected int ReadByte()
    {
        int read = _baseStream.ReadByte();
        if (read == EOF)
            throw new EndOfStreamException();
        Position++;
        return read;
    }
    protected void SkipBytes(int skip)
    {
        if (skip <= 2048)
        {
            Span<byte> buffer = stackalloc byte[2048];
            while (skip > buffer.Length)
                skip -= ReadBlock(buffer);
            ReadBlockExactly(buffer[..skip]);
        }
        else
        {
            Span<byte> buffer = stackalloc byte[skip];
            ReadBlockExactly(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected sbyte ReadI8()
    {
        return (sbyte)ReadU8();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected byte ReadU8()
    {
        return (byte)ReadByte();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected short ReadI16()
    {
        return (short)ReadU16();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ushort ReadU16()
    {
        Span<byte> buffer = stackalloc byte[2];
        ReadBlockExactly(buffer);
        return IsLittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(buffer) : BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int ReadStringLength()
    {
        return UseVarInt ? (int)ReadVarInt() : ReadU16();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int ReadI32()
    {
        return (int)ReadU32();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected uint ReadU32()
    {
        if (UseVarInt)
            return ReadVarInt(true);
        Span<byte> buffer = stackalloc byte[4];
        ReadBlockExactly(buffer);
        return IsLittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(buffer) : BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected long ReadI64()
    {
        return (long)ReadU64();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ulong ReadU64()
    {
        if (UseVarInt)
            return ReadVarLong(true);
        Span<byte> buffer = stackalloc byte[8];
        ReadBlockExactly(buffer);
        return IsLittleEndian ? BinaryPrimitives.ReadUInt64LittleEndian(buffer) : BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected float ReadFP32()
    {
        return BitConverter.UInt32BitsToSingle(ReadU32());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            result |= (uint)(b & 0x7f) << shift;
            if ((b & 0x80) != 0x80)
                break;
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
            result |= (ulong)(b & 0x7f) << shift;
            if ((b & 0x80) != 0x80)
                break;
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
