﻿using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class ObjectNbtConverter : NbtConverter<object>, IInstance<ObjectNbtConverter>
{
    public static ObjectNbtConverter Instance { get; } = new();

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

    public override object ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        return reader.TokenType switch
        {
            TokenType.StartCompound
                => context.GetDefaultReadConverter<Dictionary<string, object>>().ReadNbtBody(reader, context),
            TokenType.StartList
                => context.GetDefaultReadConverter<List<object>>().ReadNbtBody(reader, context),
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
        return NbtTagType.Unknown;
    }
    public override NbtTagType GetTargetTagType(object value, NbtSerializerContext context)
    {
        if (value is null)
            return context.NullTagType;
        INbtConverter converter = context.GetDefaultWriteConverter(value.GetType());
        return converter.BaseGetTargetTagType(value, context);
    }
    public override NbtTagType BaseGetTargetTagType(object? value, NbtSerializerContext context)
    {
        return GetTargetTagType(value!, context);
    }
    public override void WriteNbt(INbtWriter writer, object? value, NbtSerializerContext context)
    {
        if (value is null)
        {
            context.WriteNull(writer);
            return;
        }
        INbtConverter converter = context.GetDefaultWriteConverter(value.GetType());
        converter.BaseWriteNbt(writer, value, context);
    }
    public override void BaseWriteNbt(INbtWriter writer, object? value, NbtSerializerContext context)
    {
        WriteNbt(writer, value, context);
    }
}
