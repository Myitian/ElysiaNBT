using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public sealed class BigIntegerNbtConverter : NbtConverter<BigInteger>, IInstance<BigIntegerNbtConverter>
{
    public static BigIntegerNbtConverter Instance { get; } = new();
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

    public override BigInteger ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetLong();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Long;
    }
    public override void WriteNbt(INbtWriter writer, BigInteger value, NbtSerializerContext context)
    {
        writer.WritePayload((long)value);
    }
}