using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class LongNbtConverter : NbtConverter<long>, IInstance<LongNbtConverter>
{
    public static LongNbtConverter Instance { get; } = new();
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

    public override long ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetLong();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Long;
    }
    public override void WriteNbt(INbtWriter writer, long value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}