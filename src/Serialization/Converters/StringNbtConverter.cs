using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class StringNbtConverter : NbtConverter<string>, IInstance<StringNbtConverter>
{
    public static StringNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.String;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.String;
    }

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