using System.Collections.Frozen;
using System.Numerics;

namespace ElysiaNBT.Serialization.Converters;

public sealed class Vector3NbtConverter : NbtConverter<Vector3>, IInstance<Vector3NbtConverter>
{
    public static Vector3NbtConverter Instance { get; } = new();
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