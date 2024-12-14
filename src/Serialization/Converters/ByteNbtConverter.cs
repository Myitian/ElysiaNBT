using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class ByteNbtConverter : NbtConverter<byte>
{
    public static readonly ByteNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Byte;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override byte ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetByte();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, byte value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}