using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class UShortNbtConverter : NbtConverter<ushort>
{
    public static readonly UShortNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Short;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override ushort ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetUShort();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Short;
    }
    public override void WriteNbt(INbtWriter writer, ushort value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}