using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class BoolNbtConverter : NbtConverter<bool>
{
    private static readonly FrozenSet<NbtTagType> _tagTypes = FrozenSet.ToFrozenSet([NbtTagType.Byte]);
    public static readonly BoolNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => _tagTypes;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override bool ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetBool();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, bool value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}