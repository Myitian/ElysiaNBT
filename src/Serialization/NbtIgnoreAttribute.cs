namespace ElysiaNBT.Serialization;

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Enum |
    AttributeTargets.Interface, AllowMultiple = false)]
public class NbtIgnoreAttribute : Attribute
{
    public NbtIgnoreAttribute() { }
    public NbtIgnoreCondition Condition { get; set; } = NbtIgnoreCondition.Always;
}
