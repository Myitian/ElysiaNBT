using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class IntNbtConverter : NbtConverter<int>
{
    public static readonly IntNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Int;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override int ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetInt();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Int;
    }
    public override void WriteNbt(INbtWriter writer, int value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}