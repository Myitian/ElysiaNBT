using System.Collections.Frozen;
using System.Dynamic;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ReadOnlyDynamicNbtConverter : NbtConverter<dynamic>, IInstance<ReadOnlyDynamicNbtConverter>
{
    public static ReadOnlyDynamicNbtConverter Instance { get; } = new();

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.AllNormalTypes;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return SharedObjects.AllNormalTypes;
    }

    public override dynamic ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.TokenType switch
        {
            TokenType.StartCompound
                => context.GetDefaultReadConverter<ExpandoObject>().ReadNbtBody(reader, context),
            TokenType.StartList
                => (context.ListDynamicNbtConverterInstance ?? context.GetDefaultReadConverter<List<object>>()).ReadNbtBody(reader, context),
            TokenType.StartByteArray
                => context.GetDefaultReadConverter<byte[]>().ReadNbtBody(reader, context),
            TokenType.StartIntArray
                => context.GetDefaultReadConverter<int[]>().ReadNbtBody(reader, context),
            TokenType.StartLongArray
                => context.GetDefaultReadConverter<long[]>().ReadNbtBody(reader, context),
            TokenType.String
                => context.GetDefaultReadConverter<string>().ReadNbtBody(reader, context),
            TokenType.Byte
                => context.GetDefaultReadConverter<byte>().ReadNbtBody(reader, context),
            TokenType.Short
                => context.GetDefaultReadConverter<short>().ReadNbtBody(reader, context),
            TokenType.Int
                => context.GetDefaultReadConverter<int>().ReadNbtBody(reader, context),
            TokenType.Long
                => context.GetDefaultReadConverter<long>().ReadNbtBody(reader, context),
            TokenType.Float
                => context.GetDefaultReadConverter<float>().ReadNbtBody(reader, context),
            TokenType.Double
                => context.GetDefaultReadConverter<double>().ReadNbtBody(reader, context),
            TokenType.True or TokenType.False
                => context.GetDefaultReadConverter<bool>().ReadNbtBody(reader, context),
            _
                => throw new Exception(),
        };
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        throw new NotSupportedException();
    }
    public override void WriteNbt(INbtWriter writer, dynamic? value, NbtSerializerContext context)
    {
        throw new NotSupportedException();
    }
}
