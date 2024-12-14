using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class UIntNbtConverter : NbtConverter<uint>
{
    public static readonly UIntNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.Int;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.IntAcceptedTypes;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override uint ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.GetUInt();
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Int;
    }
    public override void WriteNbt(INbtWriter writer, uint value, NbtSerializerContext context)
    {
        writer.WritePayload(value);
    }
}