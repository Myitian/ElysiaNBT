using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class ULongNbtConverter : NbtConverter<ulong>
{
    public static readonly ULongNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Long;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

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