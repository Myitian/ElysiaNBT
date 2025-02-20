namespace ElysiaNBT.Serialization;

[Flags]
public enum DefaultIncluded : byte
{
    None,
    Public,
    NonPublic,
    All = Public | NonPublic
}