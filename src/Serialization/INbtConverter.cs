namespace ElysiaNBT.Serialization;

public interface INbtConverter
{
    bool CanRead { get; }
    bool CanWrite { get; }
    IReadOnlySet<NbtTagType> TargetTagTypes { get; }
    IReadOnlySet<NbtTagType> AcceptedTagTypes { get; }
    object? BaseReadNbtBody(INbtReader reader, Type type, NbtSerializerContext context);
    object? BaseReadNbt(INbtReader reader, Type type, NbtSerializerContext context);
    NbtTagType BaseGetTargetTagType(object? value, NbtSerializerContext context);
    void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context);
}
