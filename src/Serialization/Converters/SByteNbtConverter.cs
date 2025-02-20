using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class SByteNbtConverter : NbtConverter<sbyte>, IInstance<SByteNbtConverter>
{
    public static SByteNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Byte;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override sbyte ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetSByte();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Byte;
    }
    public override void WriteNbt(INbtWriter writer, sbyte value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}