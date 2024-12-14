using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public class QuaternionNbtConverter : NbtConverter<Quaternion>
{
    public static readonly QuaternionNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.List;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.ListLike;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override Quaternion ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        reader.Read();
        float x = reader.GetFloat();
        reader.Read();
        float y = reader.GetFloat();
        reader.Read();
        float z = reader.GetFloat();
        reader.Read();
        float w = reader.GetFloat();
        if (reader.Read() != TokenType.EndArray)
            throw new Exception();
        return new(x, y, z, w);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, Quaternion value, NbtSerializerContext context)
    {
        writer.WriteStartList(NbtTagType.Float, 4);
        writer.WritePayload(value.X);
        writer.WritePayload(value.Y);
        writer.WritePayload(value.Z);
        writer.WritePayload(value.W);
        writer.WriteEndArray();
    }
}