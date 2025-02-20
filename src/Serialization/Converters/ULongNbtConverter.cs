using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ULongNbtConverter : NbtConverter<ulong>, IInstance<ULongNbtConverter>
{
    public static ULongNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Long;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override ulong ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetULong();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Long;
    }
    public override void WriteNbt(INbtWriter writer, ulong value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}