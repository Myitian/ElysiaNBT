using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class FloatNbtConverter : NbtConverter<float>
{
    public static readonly FloatNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Float;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.FloatAcceptedTypes;

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