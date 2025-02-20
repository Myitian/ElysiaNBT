using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class DoubleNbtConverter : NbtConverter<double>, IInstance<DoubleNbtConverter>
{
    public static DoubleNbtConverter Instance { get; } = new();
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