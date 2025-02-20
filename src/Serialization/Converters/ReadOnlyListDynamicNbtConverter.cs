using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ReadOnlyListDynamicNbtConverter : NbtConverter<List<dynamic>>, IInstance<ReadOnlyListDynamicNbtConverter>
{
    public static ReadOnlyListDynamicNbtConverter Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.List;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.ListLike;
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;

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
        throw new NotSupportedException();
    }
}
