using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class DictionaryLikeNbtConverter<T> :
    NbtConverter<IEnumerable<KeyValuePair<string, T>>>,
    IReadOnlyNbtConverter<Dictionary<string, T>>
{
    public static DictionaryLikeNbtConverter<T> Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override Dictionary<string, T> ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        Dictionary<string, T> dict = [];
        while (reader.Read() is not TokenType.EndCompound)
        {
            if (reader.TokenType is not TokenType.Name)
                throw new Exception();
            string key = reader.GetString();
            IReadOnlyNbtConverter<T> converter = context.GetDefaultReadConverter<T>();
            T value = converter.ReadNbt(reader, context);
            dict[key] = value;
        }
        return dict;
    }
    public override Dictionary<string, T> ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        TokenType token = reader.Read();
        if (token != TokenType.StartCompound)
            throw new Exception();
        return ReadNbtBody(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Compound;
    }
    public override void WriteNbt(INbtWriter writer, IEnumerable<KeyValuePair<string, T>> value, NbtSerializerContext context)
    {
        writer.WriteStartCompound();
        foreach ((string k, T v) in value)
        {
            if (k is null)
                continue;
            IWriteOnlyNbtConverter<T> converter = context.GetDefaultWriteConverter<T>();
            writer.WriteName(k, converter.BaseGetTargetTagType(v, context));
            converter.WriteNbt(writer, v, context);
        }
        writer.WriteEndCompound();
    }
}
public class DictionaryLikeNbtConverter :
    NbtConverter<System.Collections.IDictionary>,
    IReadOnlyNbtConverter<Dictionary<string, object>>
{
    public static DictionaryLikeNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override Dictionary<string, object> ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        Dictionary<string, object> dict = [];
        NbtConverter<object> converter = context.ObjectNbtConverterInstance;
        while (reader.Read() is not TokenType.EndCompound)
        {
            if (reader.TokenType is not TokenType.Name)
                throw new Exception();
            string key = reader.GetString();
            object value = converter.ReadNbt(reader, context);
            dict[key] = value;
        }
        return dict;
    }
    public override Dictionary<string, object> ReadNbt(INbtReader reader, NbtSerializerContext context)
    {
        TokenType token = reader.Read();
        if (token != TokenType.StartCompound)
            throw new Exception();
        return ReadNbtBody(reader, context);
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Compound;
    }
    public override void WriteNbt(INbtWriter writer, System.Collections.IDictionary value, NbtSerializerContext context)
    {
        NbtConverter<object> converter = context.ObjectNbtConverterInstance;
        writer.WriteStartCompound();
        foreach (object k in value.Keys)
        {
            if (k is not string s)
                continue;
            object v = value[k]!;
            writer.WriteName(s, converter.GetTargetTagType(v, context));
            converter.WriteNbt(writer, v, context);
        }
        writer.WriteEndCompound();
    }
}
