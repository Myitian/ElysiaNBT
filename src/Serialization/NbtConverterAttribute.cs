namespace ElysiaNBT.Serialization;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Enum |
    AttributeTargets.Interface, AllowMultiple = false)]
public sealed class NbtConverterAttribute(Type converter) : Attribute
{
    public Type Converter { get; } = converter;
}