using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public sealed class CharArrayNbtConverter : NbtConverter<ICollection<char>>, IInstance<CharArrayNbtConverter>
{
    private static readonly FrozenSet<NbtTagType> _tagTypes = FrozenSet.ToFrozenSet([NbtTagType.String]);
    public static CharArrayNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return _tagTypes;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override char[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetString().ToCharArray();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, ICollection<char> value, NbtSerializerContext context)
    {
        if (value is char[] array)
        {
            writer.WritePayload(array);
            return;
        }
        else if (value is ArraySegment<char> arraySeg)
        {
            writer.WritePayload(arraySeg);
            return;
        }
        List<char> list = value is List<char> lst ? lst : [.. value];
        writer.WritePayload(CollectionsMarshal.AsSpan(list));
    }
}