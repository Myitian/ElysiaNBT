namespace ElysiaNBT.Serialization;

public interface INbtConverter
{
    bool CanRead { get; }
    bool CanWrite { get; }

    IReadOnlySet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null);
    IReadOnlySet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null);
    object? BaseReadNbtBody(INbtReader reader, Type type, NbtSerializerContext context);
    object? BaseReadNbt(INbtReader reader, Type type, NbtSerializerContext context);
    NbtTagType BaseGetTargetTagType(object? value, NbtSerializerContext context);
    void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context);
}
