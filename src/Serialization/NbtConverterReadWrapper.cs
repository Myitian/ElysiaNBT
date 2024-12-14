namespace ElysiaNBT.Serialization;

public class NbtConverterReadWrapper<T, TBase>(IReadOnlyNbtConverter<TBase> converter) : IReadOnlyNbtConverter<T> where T : TBase
{
    public IReadOnlyNbtConverter<TBase> BaseConverter
        => converter;
    public bool CanRead
        => converter.CanRead;
    public IReadOnlySet<NbtTagType> AcceptedTagTypes
        => converter.AcceptedTagTypes;
    public object? BaseReadNbtBody(INbtReader reader, Type type, NbtSerializerContext context)
        => converter.BaseReadNbtBody(reader, type, context);
    public object? BaseReadNbt(INbtReader reader, Type type, NbtSerializerContext context)
        => converter.BaseReadNbt(reader, type, context);
    public T ReadNbt(INbtReader reader, NbtSerializerContext context)
        => (T)converter.ReadNbt(reader, context)!;
    public T ReadNbtBody(INbtReader reader, NbtSerializerContext context)
        => (T)converter.ReadNbtBody(reader, context)!;
}