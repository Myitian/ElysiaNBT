using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class LongNbtConverter : NbtConverter<long>
{
    public static readonly LongNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Long;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

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