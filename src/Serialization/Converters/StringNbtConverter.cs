using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class StringNbtConverter : NbtConverter<string>
{
    public static readonly StringNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.String;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.String;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override string ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetString();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, string value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}