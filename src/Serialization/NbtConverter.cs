namespace ElysiaNBT.Serialization;

public abstract class NbtConverter<T> : INbtConverter, IReadOnlyNbtConverter<T>, IWriteOnlyNbtConverter<T>
{
    internal static Type _type = typeof(T);
    public abstract bool CanRead { get; }
    public abstract bool CanWrite { get; }
    public abstract IReadOnlySet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null);
    public abstract IReadOnlySet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null);

    public virtual object? BaseReadNbtBody(INbtReader reader, Type type, NbtSerializerContext context)
    {
        return ReadNbtBody(reader, context);
    }
    public virtual object? BaseReadNbt(INbtReader reader, Type type, NbtSerializerContext context)
    {
        return ReadNbt(reader, context);
    }
    public virtual NbtTagType BaseGetTargetTagType(object? value, NbtSerializerContext context)
    {
        return GetTargetTagType((T)value!, context);
    }
    public virtual void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context)
    {
        WriteNbt(writer, (T)value!, context);
    }

    public abstract NbtTagType GetTargetTagType(NbtSerializerContext? context = null);
    public virtual NbtTagType GetTargetTagType(T value, NbtSerializerContext context)
    {
        return GetTargetTagType(context);
    }
    public virtual T ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        NbtTagType tag = reader.Read().GetTag();
        if (!GetAcceptedTagTypes().Contains(tag))
            throw new Exception();
        return ReadNbtBody(reader, context);
    }
    public abstract T ReadNbtBody(INbtReader reader, NbtSerializerContext context);
    public abstract void WriteNbt(INbtWriter writer, T value, NbtSerializerContext context);
}
