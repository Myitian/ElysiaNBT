using System.Buffers.Binary;
using System.Numerics;

namespace ElysiaNBT;

public class BinaryWriterEx(Stream stream, bool leaveOpen = false) : IDisposable
{
    protected Stream _baseStream = stream;
    private readonly bool _leaveOpen = leaveOpen;
    private bool _disposed = false;

    public int Position { get; protected set; }
    public bool IsLittleEndian { get; set; }
    public bool UseVarInt { get; set; }

    protected void WriteBlock(ReadOnlySpan<byte> buffer)
    {
        _baseStream.Write(buffer);
        Position += buffer.Length;
    }
    protected void WriteByte(byte value)
    {
        _baseStream.WriteByte(value);
        Position++;
    }

    protected void WriteI8(sbyte value)
    {
        WriteByte((byte)value);
    }
    protected void WriteU8(byte value)
    {
        WriteByte(value);
    }
    protected void WriteI16(short value)
    {
        WriteU16((ushort)value);
    }
    protected void WriteU16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        WriteBlock(buffer);
    }
    protected void WriteStringLength(int length)
    {
        if (UseVarInt)
            WriteVarInt((uint)length);
        else
            WriteU16((ushort)length);
    }
    protected void WriteSimpleI32(int value)
    {
        WriteSimpleU32((uint)value);
    }
    protected void WriteI32(int value)
    {
        WriteU32((uint)value);
    }
    protected void WriteSimpleU32(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        WriteBlock(buffer);
    }
    protected void WriteU32(uint value)
    {
        if (UseVarInt)
            WriteVarInt(value, true);
        else
            WriteSimpleU32(value);
    }
    protected void WriteSimpleI64(long value)
    {
        WriteSimpleU64((ulong)value);
    }
    protected void WriteI64(long value)
    {
        WriteU64((ulong)value);
    }
    protected void WriteSimpleU64(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (IsLittleEndian)
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        else
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        WriteBlock(buffer);
    }
    protected void WriteU64(ulong value)
    {
        if (UseVarInt)
            WriteVarLong(value, true);
        else
            WriteSimpleU64(value);
    }
    protected void WriteFP32(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        if (IsLittleEndian)
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        else
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        WriteBlock(buffer);
    }
    protected void WriteFP64(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        if (IsLittleEndian)
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        else
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        WriteBlock(buffer);
    }
    protected void WriteVarInt(uint value, bool zigzag = false)
    {
        if (zigzag)
            value = BitOperations.RotateLeft(value, 1);
        while (true)
        {
            if ((value & 0xFFFFFF80) == 0)
            {
                WriteByte((byte)value);
                return;
            }
            else
            {
                WriteByte((byte)(value & 0x7F | 0x80));
                value >>= 7;
            }
        }
    }
    protected void WriteVarLong(ulong value, bool zigzag = false)
    {
        if (zigzag)
            value = BitOperations.RotateLeft(value, 1);
        while (true)
        {
            if ((value & 0xFFFFFFFFFFFFFF80) == 0L)
            {
                WriteByte((byte)value);
                return;
            }
            else
            {
                WriteByte((byte)(value & 0x7F | 0x80));
                value >>= 7;
            }
        }
    }
    public virtual void Flush()
    {
        _baseStream.Flush();
    }
    public virtual void Close()
    {
        Dispose(true);
    }
    public void Dispose()
    {
        Dispose(disposing: true);
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
