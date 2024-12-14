using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class DoubleNbtConverter : NbtConverter<double>
{
    public static readonly DoubleNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Double;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.FloatAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override double ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetDouble();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Double;
    }
    public override void WriteNbt(INbtWriter writer, double value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}