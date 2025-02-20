using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public sealed class Vector2NbtConverter : NbtConverter<Vector2>, IInstance<Vector2NbtConverter>
{
    public static Vector2NbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.List;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListLike;
    }

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