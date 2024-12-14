using ElysiaNBT.Serialization.Converters;

namespace ElysiaNBT.Serialization;

public abstract class NbtSerializerContext
{
    public virtual NbtConverter<object> ObjectNbtConverterInstance { get; init; } = ObjectNbtConverter.Instance;
    public abstract NbtConverter<dynamic>? DynamicNbtConverterInstance { get; init; }
    public abstract NbtConverter<List<dynamic>>? ListDynamicNbtConverterInstance { get; init; }
    public virtual NbtConverter<object?> SkipNbtConverterInstance { get; init; } = SkipNbtConverter.Instance;

    public virtual NbtTagType NullTagType { get; init; } = NbtTagType.Compound;

    public abstract IReadOnlyNbtConverter<T> MakeReadWrapper<T>(INbtConverter converter);
    public abstract INbtConverter GetDefaultReadConverter(Type type);
    public virtual IReadOnlyNbtConverter<T> GetDefaultReadConverter<T>()
    {
        INbtConverter converter = GetDefaultReadConverter(typeof(T));
        if (converter is IReadOnlyNbtConverter<T> typedConverter)
            return typedConverter;
        return MakeReadWrapper<T>(converter);
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
