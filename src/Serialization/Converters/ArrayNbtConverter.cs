using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ArrayNbtConverter<T> : NbtConverter<T[]>, IInstance<ArrayNbtConverter<T>>
{
    public static ArrayNbtConverter<T> Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.List;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListType<T>.Accepted;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override T[] ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        IReadOnlyNbtConverter<T> converter = context.GetDefaultReadConverter<T>();
        int estimatedLength = reader.CurrentContentLength;
        if (estimatedLength >= 0)
        {
            T[] result = GC.AllocateUninitializedArray<T>(estimatedLength);
            int i = 0;
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result[i++] = converter.ReadNbtBody(reader, context);
            }
            return result;
        }
        else
        {
            List<T> result = [];
            while (reader.Read() is not TokenType.EndArray)
            {
                if (reader.TokenType is TokenType.None)
                    throw new Exception();
                result.Add(converter.ReadNbtBody(reader, context));
            }
            return [.. result];
        }
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, T[] value, NbtSerializerContext context)
    {
        IWriteOnlyNbtConverter<T> converter = context.GetDefaultWriteConverter<T>();
        NbtTagType type = converter.GetTargetTagType(context);
        if (type is NbtTagType.Unknown)
        {
            if (value.Length == 0)
                type = NbtTagType.End;
            else
                type = converter.BaseGetTargetTagType(value.First(), context);
        }
        writer.WriteStartList(type, value.Length);
        foreach (T item in value)
            converter.WriteNbt(writer, item, context);
        writer.WriteEndArray();
    }
}
