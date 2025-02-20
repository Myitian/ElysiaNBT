namespace ElysiaNBT.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NbtEntryNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}