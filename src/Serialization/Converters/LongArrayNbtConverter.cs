using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class LongArrayNbtConverter :
    NbtConverter<ICollection<long>>,
    IReadOnlyNbtConverter<long[]>,
    IInstance<LongArrayNbtConverter>
{
    public static LongArrayNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.LongArray;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListLongArray;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override long[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            long[] result = GC.AllocateUninitializedArray<long>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetLong();
            }
            return result;
        }
        else
        {
            List<long> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetLong());
            }
            return [.. result];
        }
    }
    public override long[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (long[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.LongArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<long> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case long[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<long> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<long> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartLongArray(value.Count);
                foreach (long b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}