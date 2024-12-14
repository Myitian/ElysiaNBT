using System.Runtime.InteropServices;

namespace ElysiaNBT;

public interface INbtWriter
{
    bool RequiresLengthInfo { get; }
    NbtOptions Options { get; }
    State CurrentState { get; }
    int CurrentDepth { get; }
    int Position { get; }

    bool IsInArray { get; }

    void WriteEndArray();
    void WriteEndCompound();
    void WriteName(ReadOnlySpan<char> name, NbtTagType tagType = NbtTagType.Unknown);
    void WriteLength(int length);
    void WriteStartList(NbtTagType itemType = NbtTagType.Unknown, int length = -1);
    void WriteStartByteArray(int length = -1);
    void WriteStartIntArray(int length = -1);
    void WriteStartLongArray(int length = -1);
    void WriteStartCompound();
    void WriteEnd();
    void WriteTrue();
    void WriteFalse();
    void WritePayload(bool payload)
    {
        if (payload)
            WriteTrue();
        else
            WriteFalse();
    }
    void WritePayload(byte payload);
    void WritePayload(sbyte payload);
    void WritePayload(short payload);
    void WritePayload(ushort payload);
    void WritePayload(int payload);
    void WritePayload(uint payload);
    void WritePayload(long payload);
    void WritePayload(ulong payload);
    void WritePayload(float payload);
    void WritePayload(double payload);
    void WritePayload(ReadOnlySpan<char> payload);
    void WritePayload(ReadOnlySpan<bool> payload)
    {
        WriteStartByteArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<byte> payload)
    {
        WriteStartByteArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<sbyte> payload)
    {
        WriteStartByteArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<short> payload)
    {
        WriteStartList(NbtTagType.Short, payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<ushort> payload)
    {
        WriteStartList(NbtTagType.Short, payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<int> payload)
    {
        WriteStartIntArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<uint> payload)
    {
        WriteStartIntArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<long> payload)
    {
        WriteStartLongArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<ulong> payload)
    {
        WriteStartLongArray(payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<float> payload)
    {
        WriteStartList(NbtTagType.Float, payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload(ReadOnlySpan<double> payload)
    {
        WriteStartList(NbtTagType.Double, payload.Length);
        for (int i = 0; i < payload.Length; i++)
            WritePayload(payload[i]);
        WriteEndArray();
    }
    void WritePayload<T>(ReadOnlySpan<T> payload) where T : struct
    {
        Type t = typeof(T);
        if (t == SharedObjects.TypeChar)
            WritePayload(MemoryMarshal.Cast<T, char>(payload));
        else if (t == SharedObjects.TypeBool)
            WritePayload(MemoryMarshal.Cast<T, bool>(payload));
        else if (t == SharedObjects.TypeByte)
            WritePayload(MemoryMarshal.Cast<T, byte>(payload));
        else if (t == SharedObjects.TypeSByte)
            WritePayload(MemoryMarshal.Cast<T, sbyte>(payload));
        else if (t == SharedObjects.TypeShort)
            WritePayload(MemoryMarshal.Cast<T, short>(payload));
        else if (t == SharedObjects.TypeUShort)
            WritePayload(MemoryMarshal.Cast<T, ushort>(payload));
        else if (t == SharedObjects.TypeInt)
            WritePayload(MemoryMarshal.Cast<T, int>(payload));
        else if (t == SharedObjects.TypeUInt)
            WritePayload(MemoryMarshal.Cast<T, uint>(payload));
        else if (t == SharedObjects.TypeLong)
            WritePayload(MemoryMarshal.Cast<T, long>(payload));
        else if (t == SharedObjects.TypeULong)
            WritePayload(MemoryMarshal.Cast<T, ulong>(payload));
        else if (t == SharedObjects.TypeFloat)
            WritePayload(MemoryMarshal.Cast<T, float>(payload));
        else if (t == SharedObjects.TypeDouble)
            WritePayload(MemoryMarshal.Cast<T, double>(payload));
        else
            throw new NotSupportedException();
    }
    void WriteStartList(ReadOnlySpan<char> name, NbtTagType tagType = NbtTagType.Unknown, int length = -1)
    {
        WriteName(name, NbtTagType.List);
        WriteStartList(tagType, length);
    }
    void WriteStartByteArray(ReadOnlySpan<char> name, int length = -1)
    {
        WriteName(name, NbtTagType.Compound);
        WriteStartByteArray(length);
    }
    void WriteStartIntArray(ReadOnlySpan<char> name, int length = -1)
    {
        WriteName(name, NbtTagType.Compound);
        WriteStartIntArray(length);
    }
    void WriteStartLongArray(ReadOnlySpan<char> name, int length = -1)
    {
        WriteName(name, NbtTagType.Compound);
        WriteStartLongArray(length);
    }
    void WriteStartCompound(ReadOnlySpan<char> name)
    {
        WriteName(name, NbtTagType.Compound);
        WriteStartCompound();
    }
    void WriteTrue(ReadOnlySpan<char> name)
    {
        WriteName(name, NbtTagType.Byte);
        WriteTrue();
    }
    void WriteFalse(ReadOnlySpan<char> name)
    {
        WriteName(name, NbtTagType.Byte);
        WriteFalse();
    }
    void Write(ReadOnlySpan<char> name, bool payload)
    {
        WriteName(name, NbtTagType.Byte);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, byte payload)
    {
        WriteName(name, NbtTagType.Byte);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, sbyte payload)
    {
        WriteName(name, NbtTagType.Byte);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, short payload)
    {
        WriteName(name, NbtTagType.Short);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ushort payload)
    {
        WriteName(name, NbtTagType.Short);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, int payload)
    {
        WriteName(name, NbtTagType.Int);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, uint payload)
    {
        WriteName(name, NbtTagType.Int);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, long payload)
    {
        WriteName(name, NbtTagType.Long);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ulong payload)
    {
        WriteName(name, NbtTagType.Long);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, float payload)
    {
        WriteName(name, NbtTagType.Float);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, double payload)
    {
        WriteName(name, NbtTagType.Double);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<char> payload)
    {
        WriteName(name, NbtTagType.String);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<bool> payload)
    {
        WriteName(name, NbtTagType.ByteArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<byte> payload)
    {
        WriteName(name, NbtTagType.ByteArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<sbyte> payload)
    {
        WriteName(name, NbtTagType.ByteArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<short> payload)
    {
        WriteName(name, NbtTagType.List);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<ushort> payload)
    {
        WriteName(name, NbtTagType.List);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<int> payload)
    {
        WriteName(name, NbtTagType.IntArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<uint> payload)
    {
        WriteName(name, NbtTagType.IntArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<long> payload)
    {
        WriteName(name, NbtTagType.LongArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<ulong> payload)
    {
        WriteName(name, NbtTagType.LongArray);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<float> payload)
    {
        WriteName(name, NbtTagType.List);
        WritePayload(payload);
    }
    void Write(ReadOnlySpan<char> name, ReadOnlySpan<double> payload)
    {
        WriteName(name, NbtTagType.List);
        WritePayload(payload);
    }
    void Write<T>(ReadOnlySpan<char> name, ReadOnlySpan<T> payload) where T : struct
    {
        Type t = typeof(T);
        if (t == SharedObjects.TypeChar)
            Write(name, MemoryMarshal.Cast<T, char>(payload));
        else if (t == SharedObjects.TypeBool)
            Write(name, MemoryMarshal.Cast<T, bool>(payload));
        else if (t == SharedObjects.TypeByte)
            Write(name, MemoryMarshal.Cast<T, byte>(payload));
        else if (t == SharedObjects.TypeSByte)
            Write(name, MemoryMarshal.Cast<T, sbyte>(payload));
        else if (t == SharedObjects.TypeShort)
            Write(name, MemoryMarshal.Cast<T, short>(payload));
        else if (t == SharedObjects.TypeUShort)
            Write(name, MemoryMarshal.Cast<T, ushort>(payload));
        else if (t == SharedObjects.TypeInt)
            Write(name, MemoryMarshal.Cast<T, int>(payload));
        else if (t == SharedObjects.TypeUInt)
            Write(name, MemoryMarshal.Cast<T, uint>(payload));
        else if (t == SharedObjects.TypeLong)
            Write(name, MemoryMarshal.Cast<T, long>(payload));
        else if (t == SharedObjects.TypeULong)
            Write(name, MemoryMarshal.Cast<T, ulong>(payload));
        else if (t == SharedObjects.TypeFloat)
            Write(name, MemoryMarshal.Cast<T, float>(payload));
        else if (t == SharedObjects.TypeDouble)
            Write(name, MemoryMarshal.Cast<T, double>(payload));
        else
            throw new NotSupportedException();
    }
    void Flush();

    public enum State
    {
        WritingName,
        WritingPayload,
        Stopped
    }
}
