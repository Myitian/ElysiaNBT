namespace ElysiaNBT.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NbtIgnoreAttribute : Attribute
{
    public NbtIgnoreAttribute() { }
    public NbtIgnoreCondition Condition { get; set; } = NbtIgnoreCondition.Always;
}
public enum NbtIgnoreCondition : byte
{
    Never = 0,
    Always = 1,
    WhenWritingDefault = 2,
    WhenWritingNull = 3
}
