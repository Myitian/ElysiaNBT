using System.Collections.Frozen;
using System.Dynamic;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ExpandoObjectNbtConverter : NbtConverter<ExpandoObject>, IInstance<ExpandoObjectNbtConverter>
{
    public static ExpandoObjectNbtConverter Instance { get; } = new();

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.Compound;
    }

    public override ExpandoObject ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        ExpandoObject obj = new();
        IDictionary<string, object?> dict = obj;
        NbtConverter<object> converter = (context.DynamicNbtConverterInstance?.CanRead is true ? context.DynamicNbtConverterInstance : null)
            ?? context.ObjectNbtConverterInstance;
        while (reader.Read() is not TokenType.EndCompound)
        {
            if (reader.TokenType is not TokenType.Name)
                throw new Exception();
            string key = reader.GetString();
            dict[key] = converter.ReadNbt(reader, context);
        }
        return obj;
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return NbtTagType.Compound;
    }

    public override void WriteNbt(INbtWriter writer, ExpandoObject value, NbtSerializerContext context)
    {
        context.GetDefaultWriteConverter<IDictionary<string, object?>>().WriteNbt(writer, value, context);
    }
}
