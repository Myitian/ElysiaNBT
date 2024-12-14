using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public class Vector3NbtConverter : NbtConverter<Vector3>
{
    public static readonly Vector3NbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.List;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.ListLike;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override Vector3 ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        reader.Read();
        float x = reader.GetFloat();
        reader.Read();
        float y = reader.GetFloat();
        reader.Read();
        float z = reader.GetFloat();
        if (reader.Read() != TokenType.EndArray)
            throw new Exception();
        return new(x, y, z);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, Vector3 value, NbtSerializerContext context)
    {
        writer.WriteStartList(NbtTagType.Float, 3);
        writer.WritePayload(value.X);
        writer.WritePayload(value.Y);
        writer.WritePayload(value.Z);
        writer.WriteEndArray();
    }
}