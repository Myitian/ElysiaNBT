namespace ElysiaNBT.Serialization.Converters;

public sealed class NullableNbtConverter<T> : NbtConverter<T?>, IInstance<NullableNbtConverter<T>> where T : struct
{
    public static NullableNbtConverter<T> Instance { get; } = new();
    public override IReadOnlySet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return context?.GetDefaultWriteConverter<T>().GetTargetTagTypes(context) ?? SharedObjects.AllNormalTypes;
    }
    public override IReadOnlySet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return context?.GetDefaultReadConverter<T>().GetAcceptedTagTypes(context) ?? SharedObjects.AllNormalTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override T? ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return context.GetDefaultReadConverter<T>().ReadNbtBody(reader, context);
    }
    public override T? ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return context.GetDefaultReadConverter<T>().ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return context?.GetDefaultWriteConverter<T>().GetTargetTagType(context) ?? NbtTagType.Unknown;
    }
    public override NbtTagType GetTargetTagType(T? value, NbtSerializerContext context)
    {
        return context.GetDefaultWriteConverter<T>().GetTargetTagType(value ?? default, context);
    }
    public override void WriteNbt(INbtWriter writer, T? value, NbtSerializerContext context)
    {
        context.GetDefaultWriteConverter<T>().WriteNbt(writer, value ?? default, context);
    }
}
