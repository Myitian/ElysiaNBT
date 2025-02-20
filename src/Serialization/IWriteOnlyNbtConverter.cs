namespace ElysiaNBT.Serialization;

public interface IWriteOnlyNbtConverter<in T> where T : allows ref struct
{
    bool CanWrite { get; }

    IReadOnlySet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null);
    NbtTagType GetTargetTagType(NbtSerializerContext? context = null);
    NbtTagType GetTargetTagType(T value, NbtSerializerContext context);
    NbtTagType BaseGetTargetTagType(object? value, NbtSerializerContext context);
    void WriteNbt(INbtWriter writer, T value, NbtSerializerContext context);
    void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context);
}
