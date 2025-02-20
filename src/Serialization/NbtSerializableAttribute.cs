namespace ElysiaNBT.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NbtSerializableAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
    public string? ConverterPropertyName { get; set; }
}