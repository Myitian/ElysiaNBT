using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public sealed class Vector4NbtConverter : NbtConverter<Vector4>, IInstance<Vector4NbtConverter>
{
    public static Vector4NbtConverter Instance { get; } = new();
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

    public override Vector4 ReadNbtBody(INbtReader reader, NbtSerializerContext context)
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
    public override void WriteNbt(INbtWriter writer, Vector4 value, NbtSerializerContext context)
    {
        writer.WriteStartList(NbtTagType.Float, 4);
        writer.WritePayload(value.X);
        writer.WritePayload(value.Y);
        writer.WritePayload(value.Z);
        writer.WritePayload(value.W);
        writer.WriteEndArray();
    }
}
