using System.Collections.Frozen;

namespace ElysiaNBT.Serialization.Converters;

public class EnumNbtConverter<T> : NbtConverter<T> where T : struct, Enum
{
    protected static readonly Type _underlyingType = Enum.GetUnderlyingType(_type);
    protected static readonly FrozenSet<NbtTagType> _targetTagTypes = SharedObjects.BasicTypeMappings[_underlyingType];
    protected static readonly FrozenSet<NbtTagType> _acceptedTagTypes = SharedObjects.AcceeptedTypeMappings[_underlyingType];
    protected static readonly NbtTagType _targetType = SharedObjects.BasicTagTypeMappings[_underlyingType];
    public static readonly EnumNbtConverter<T> Instance = new();
    public override FrozenSet<NbtTagType> TargetTagTypes => _targetTagTypes;
    public override FrozenSet<NbtTagType> AcceptedTagTypes => _acceptedTagTypes;

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