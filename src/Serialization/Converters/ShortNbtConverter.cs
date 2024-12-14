using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class ShortNbtConverter : NbtConverter<short>
{
    public static readonly ShortNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Short;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override short ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetShort();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Short;
    }
    public override void WriteNbt(INbtWriter writer, short value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}