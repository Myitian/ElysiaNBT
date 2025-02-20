using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class FloatNbtConverter : NbtConverter<float>, IInstance<FloatNbtConverter>
{
    public static FloatNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Float;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.FloatAcceptedTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override float ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetFloat();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Float;
    }
    public override void WriteNbt(INbtWriter writer, float value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}