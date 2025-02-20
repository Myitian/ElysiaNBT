using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ULongArrayNbtConverter :
    NbtConverter<ICollection<ulong>>,
    IReadOnlyNbtConverter<ulong[]>,
    IInstance<ULongArrayNbtConverter>
{
    public static ULongArrayNbtConverter Instance { get; } = new();
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

    public override ulong[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            ulong[] result = GC.AllocateUninitializedArray<ulong>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetULong();
            }
            return result;
        }
        else
        {
            List<ulong> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetULong());
            }
            return [.. result];
        }
    }
    public override ulong[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (ulong[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.LongArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<ulong> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case ulong[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<ulong> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<ulong> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartLongArray(value.Count);
                foreach (ulong b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}