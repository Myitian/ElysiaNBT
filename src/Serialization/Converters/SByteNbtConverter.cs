using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class SByteNbtConverter : NbtConverter<sbyte>
{
    public static readonly SByteNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Byte;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override sbyte ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetSByte();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, sbyte value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}