using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace ElysiaNBT.Serialization.Converters;

public class CharArrayNbtConverter : NbtConverter<ICollection<char>>
{
    private static readonly FrozenSet<NbtTagType> _tagTypes = FrozenSet.ToFrozenSet([NbtTagType.String]);
    public static readonly CharArrayNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => _tagTypes;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

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