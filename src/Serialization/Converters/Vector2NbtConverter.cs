using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public class Vector2NbtConverter : NbtConverter<Vector2>
{
    public static readonly Vector2NbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.List;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.ListLike;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override Vector2 ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        reader.Read();
        float x = reader.GetFloat();
        reader.Read();
        float y = reader.GetFloat();
        if (reader.Read() != TokenType.EndArray)
            throw new Exception();
        return new(x, y);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, Vector2 value, NbtSerializerContext context)
    {
        writer.WriteStartList(NbtTagType.Float, 2);
        writer.WritePayload(value.X);
        writer.WritePayload(value.Y);
        writer.WriteEndArray();
    }
}