using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class DecimalNbtConverter : NbtConverter<decimal>, IInstance<DecimalNbtConverter>
{
    public static DecimalNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Double;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.FloatAcceptedTypes;
    }

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