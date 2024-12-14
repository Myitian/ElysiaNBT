using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class DecimalNbtConverter : NbtConverter<decimal>
{
    public static readonly DecimalNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Double;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.FloatAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override decimal ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return (decimal)reader.GetDouble();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Double;
    }
    public override void WriteNbt(INbtWriter writer, decimal value, NbtSerializerContext context)
    {
        writer.WritePayload((double)value);
    }
}