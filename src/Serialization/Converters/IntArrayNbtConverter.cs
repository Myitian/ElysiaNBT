using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class IntArrayNbtConverter :
    NbtConverter<ICollection<int>>,
    IReadOnlyNbtConverter<int[]>,
    IInstance<IntArrayNbtConverter>
{
    public static IntArrayNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntArray;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListIntArray;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override int[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            int[] result = GC.AllocateUninitializedArray<int>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetInt();
            }
            return result;
        }
        else
        {
            List<int> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetInt());
            }
            return [.. result];
        }
    }
    public override int[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (int[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.IntArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<int> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case int[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<int> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<int> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartIntArray(value.Count);
                foreach (int b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}