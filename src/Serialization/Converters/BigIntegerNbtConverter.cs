using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public class BigIntegerNbtConverter : NbtConverter<BigInteger>
{
    public static readonly BigIntegerNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Long;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

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