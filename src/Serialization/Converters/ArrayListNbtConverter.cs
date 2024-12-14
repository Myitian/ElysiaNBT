using System.Collections;
using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class ArrayListNbtConverter : NbtConverter<ArrayList>
{
    public static readonly ArrayListNbtConverter Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => SharedObjects.List;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => SharedObjects.ListLike;

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override ArrayList ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        NbtConverter<object> converter = context.ObjectNbtConverterInstance;
        int estimatedLength = reader.CurrentContentLength;
        ArrayList result = estimatedLength >= 0 ? new(estimatedLength) : [];
        while (reader.Read() is not TokenType.EndArray)
        {
            if (reader.TokenType is TokenType.None)
                throw new Exception();
            result.Add(converter.ReadNbtBody(reader, context));
        }
        return result;
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.List;
    }
    public override void WriteNbt(INbtWriter writer, ArrayList value, NbtSerializerContext context)
    {
        NbtConverter<object> converter = context.ObjectNbtConverterInstance;
        NbtTagType type = value.Count == 0 ? NbtTagType.End : converter.BaseGetTargetTagType(value[1], context);
        writer.WriteStartList(type, value.Count);
        foreach (object item in value)
            converter.WriteNbt(writer, item, context);
        writer.WriteEndArray();
    }
}