using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class BoolNbtConverter : NbtConverter<bool>, IInstance<BoolNbtConverter>
{
    private static readonly FrozenSet<NbtTagType> _tagTypes = FrozenSet.ToFrozenSet([NbtTagType.Byte]);
    public static BoolNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return _tagTypes;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override bool ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetBool();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, bool value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}