using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ShortNbtConverter : NbtConverter<short>, IInstance<ShortNbtConverter>
{
    public static ShortNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Short;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

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