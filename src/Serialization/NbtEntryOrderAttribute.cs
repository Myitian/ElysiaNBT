namespace ElysiaNBT.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NbtEntryOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}