using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ByteArrayNbtConverter :
    NbtConverter<ICollection<byte>>,
    IReadOnlyNbtConverter<byte[]>,
    IInstance<ByteArrayNbtConverter>
{
    public static ByteArrayNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ByteArray;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListByteArray;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override byte[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            byte[] result = GC.AllocateUninitializedArray<byte>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetByte();
            }
            return result;
        }
        else
        {
            List<byte> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetByte());
            }
            return [.. result];
        }
    }
    public override byte[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (byte[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.ByteArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<byte> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case byte[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<byte> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<byte> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartByteArray(value.Count);
                foreach (byte b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}