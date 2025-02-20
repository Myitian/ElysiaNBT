using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class IEnumerableNbtConverter<T> :
    NbtConverter<IEnumerable<T>>,
    IReadOnlyNbtConverter<List<T>>
{
    public static IEnumerableNbtConverter<T> Instance { get; } = new();
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

    public override List<T> ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return ListNbtConverter<T>.Instance.ReadNbtBody(reader, context);
    }
    public override List<T> ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (List<T>)base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, IEnumerable<T> value, NbtSerializerContext context)
    {
        IWriteOnlyNbtConverter<T> converter = context.GetDefaultWriteConverter<T>();
        NbtTagType type = converter.GetTargetTagType(context);
        int count = -1;
        if (value is ICollection<T> collection)
        {
            count = collection.Count;
            if (type is NbtTagType.Unknown)
                type = count == 0 ? NbtTagType.End : converter.BaseGetTargetTagType(collection.First(), context);
        }
        if (type is NbtTagType.Unknown)
        {
            bool first = true;
            foreach (T item in value)
            {
                if (first)
                {
                    type = converter.BaseGetTargetTagType(item, context);
                    writer.WriteStartList(type, count);
                    first = false;
                }
                converter.WriteNbt(writer, item, context);
            }
            if (first)
                writer.WriteStartList(NbtTagType.End, 0);
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStartList(type, count);
            foreach (T item in value)
                converter.WriteNbt(writer, item, context);
            writer.WriteEndArray();
        }
    }
}
public class IEnumerableNbtConverter :
    NbtConverter<System.Collections.IEnumerable>,
    IReadOnlyNbtConverter<List<object>>
{
    public static IEnumerableNbtConverter Instance { get; } = new();
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

    public override List<object> ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return ListNbtConverter<object>.Instance.ReadNbtBody(reader, context);
    }
    public override List<object> ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        return (List<object>)base.ReadNbt(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, System.Collections.IEnumerable value, NbtSerializerContext context)
    {
        NbtConverter<object> converter = context.ObjectNbtConverterInstance;
        NbtTagType type = NbtTagType.Unknown;
        int count = -1;
        if (value is System.Collections.ICollection collection)
        {
            count = collection.Count;
            if (count == 0)
                type = NbtTagType.End;
            else if (value is System.Collections.IList list && count > 0)
                type = converter.BaseGetTargetTagType(list[0], context);
        }
        if (type is NbtTagType.Unknown)
        {
            bool first = true;
            foreach (object item in value)
            {
                if (first)
                {
                    type = converter.BaseGetTargetTagType(item, context);
                    writer.WriteStartList(type, count);
                    first = false;
                }
                converter.WriteNbt(writer, item, context);
            }
            if (first)
                writer.WriteStartList(NbtTagType.End, 0);
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStartList(type, count);
            foreach (object item in value)
                converter.WriteNbt(writer, item, context);
            writer.WriteEndArray();
        }
    }
}