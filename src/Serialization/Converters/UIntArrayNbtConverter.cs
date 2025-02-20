using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class UIntArrayNbtConverter :
    NbtConverter<ICollection<uint>>,
    IReadOnlyNbtConverter<uint[]>,
    IInstance<UIntArrayNbtConverter>
{
    public static UIntArrayNbtConverter Instance { get; } = new();
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

    public override uint[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            uint[] result = GC.AllocateUninitializedArray<uint>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None || i >= result.Length)
                    throw new Exception();
                result[i++] = reader.GetUInt();
            }
            return result;
        }
        else
        {
            List<uint> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(reader.GetUInt());
            }
            return [.. result];
        }
    }
    public override uint[] ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (uint[])base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.IntArray;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<uint> value, NbtSerializerContext context)
    {
        switch (value)
        {
            case uint[] array:
                writer.WritePayload(array);
                break;
            case ArraySegment<uint> arraySeg:
                writer.WritePayload(arraySeg);
                break;
            case List<uint> list:
                writer.WritePayload(CollectionsMarshal.AsSpan(list));
                break;
            default:
                writer.WriteStartIntArray(value.Count);
                foreach (uint b in value)
                    writer.WritePayload(b);
                writer.WriteEndArray();
                break;
        }
    }
}