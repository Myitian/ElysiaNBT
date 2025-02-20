using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class IntNbtConverter : NbtConverter<int>, IInstance<IntNbtConverter>
{
    public static IntNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Int;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.IntAcceptedTypes;
    }

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