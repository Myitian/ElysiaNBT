using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ElysiaNBT.Serialization.Converters;

[RequiresUnreferencedCode("dynamic")]
public sealed class ListDynamicNbtConverter : NbtConverter<List<dynamic>>, IInstance<ListDynamicNbtConverter>
{
    public static ListDynamicNbtConverter Instance { get; } = new();
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

    public override List<dynamic> ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        int estimatedLength = reader.CurrentContentLength;
        List<dynamic> result = estimatedLength >= 0 ? new(estimatedLength) : [];
        NbtConverter<object> converter = (context.DynamicNbtConverterInstance?.CanRead is true ? context.DynamicNbtConverterInstance : null)
            ?? context.ObjectNbtConverterInstance;
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
    public override void WriteNbt(INbtWriter writer, List<dynamic> value, NbtSerializerContext context)
    {
        NbtConverter<object> converter = (context.DynamicNbtConverterInstance?.CanWrite is true ? context.DynamicNbtConverterInstance : null)
            ?? context.ObjectNbtConverterInstance;
        NbtTagType type = converter.GetTargetTagType(context);
        if (type is NbtTagType.Unknown)
        {
            if (value.Count == 0)
                type = NbtTagType.End;
            else
                type = converter.BaseGetTargetTagType(value.First(), context);
        }
        writer.WriteStartList(type, value.Count);
        foreach (dynamic item in value)
            converter.WriteNbt(writer, item, context);
        writer.WriteEndArray();
    }
}
