using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public sealed class EnumNbtConverter<T> : NbtConverter<T>, IInstance<EnumNbtConverter<T>> where T : struct, Enum
{
    private static readonly Type _underlyingType = Enum.GetUnderlyingType(_type);
    private static readonly FrozenSet<NbtTagType> _targetTagTypes = SharedObjects.BasicTypeMappings[_underlyingType];
    private static readonly FrozenSet<NbtTagType> _acceptedTagTypes = SharedObjects.AcceeptedTypeMappings[_underlyingType];
    private static readonly NbtTagType _targetType = SharedObjects.BasicTagTypeMappings[_underlyingType];
    public static EnumNbtConverter<T> Instance { get; } = new();
    public override FrozenSet<NbtTagType> GetTargetTagTypes(NbtSerializerContext? context = null)
    {
        return _targetTagTypes;
    }
    public override FrozenSet<NbtTagType> GetAcceptedTagTypes(NbtSerializerContext? context = null)
    {
        return _acceptedTagTypes;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override T ReadNbtBody(INbtReader reader, NbtSerializerContext context)
    {
        INbtConverter converter = context.GetDefaultReadConverter(_underlyingType);
        return (T)converter.BaseReadNbtBody(reader, _underlyingType, context)!;
    }
    public override NbtTagType GetTargetTagType(NbtSerializerContext? context = null)
    {
        return _targetType;
    }
    public override void WriteNbt(INbtWriter writer, T value, NbtSerializerContext context)
    {
        INbtConverter converter = context.GetDefaultReadConverter(_underlyingType);
        converter.BaseWriteNbt(writer, value, context);
    }
}