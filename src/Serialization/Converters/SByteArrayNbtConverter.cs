using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class SByteArrayNbtConverter :
    NbtConverter<ICollection<sbyte>>,
    IReadOnlyNbtConverter<sbyte[]>,
    IInstance<SByteArrayNbtConverter>
{
    public static SByteArrayNbtConverter Instance { get; } = new();
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

    public override sbyte[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            sbyte[] result = GC.AllocateUninitializedArray<sbyte>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetSByte();
            }
            return result;
        }
        else
        {
            List<sbyte> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetSByte());
            }
            return [.. result];
        }
    }
    public override sbyte[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (sbyte[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.ByteArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<sbyte> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case sbyte[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<sbyte> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<sbyte> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartByteArray(value.Count);
                foreach (sbyte b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}