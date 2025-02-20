namespace ElysiaNBT.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NbtSourceGenerationAttribute : Attribute
{
    public bool DefaultIncludeProperties { get; set; } = true;
    public bool DefaultIncludeFields { get; set; } = false;
    public NbtIgnoreCondition DefaultIgnore { get; set; } = NbtIgnoreCondition.WhenWritingNull;
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NbtAdditionalConvertersAttribute(params Type[] converterTypes) : Attribute
{
    public Type[] ConverterTypes { get; } = converterTypes;
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NbtConverterOverridesAttribute(Type type, Type converterType) : Attribute
{
    public Type Type { get; } = type;
    public Type ConverterType { get; } = converterType;
}