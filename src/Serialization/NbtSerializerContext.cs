using ElysiaNBT.Serialization.Converters;

namespace ElysiaNBT.Serialization;

public abstract class NbtSerializerContext
{
    public virtual NbtConverter<object> ObjectNbtConverterInstance { get; init; } = ObjectNbtConverter.Instance;
    public virtual NbtConverter<dynamic>? DynamicNbtConverterInstance { get; init; } = null;
    public virtual NbtConverter<List<dynamic>>? ListDynamicNbtConverterInstance { get; init; } = null;
    public virtual NbtConverter<object?> SkipNbtConverterInstance { get; init; } = SkipNbtConverter.Instance;

    public virtual NbtTagType NullTagType { get; init; } = NbtTagType.Compound;

    public abstract INbtConverter GetDefaultReadConverter(Type type);
    public virtual IReadOnlyNbtConverter<T> GetDefaultReadConverter<T>()
    {
        return (IReadOnlyNbtConverter<T>)GetDefaultReadConverter(typeof(T));
    }
    public abstract INbtConverter GetDefaultWriteConverter(Type type);
    public virtual IWriteOnlyNbtConverter<T> GetDefaultWriteConverter<T>()
    {
        return (IWriteOnlyNbtConverter<T>)GetDefaultWriteConverter(typeof(T));
    }
    public virtual void WriteNull(INbtWriter writer)
    {
        writer.WriteStartCompound();
        writer.WriteEndCompound();
    }
}