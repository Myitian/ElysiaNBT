namespace ElysiaNBT.Serialization;

public enum NbtIgnoreCondition : byte
{
    Never = 0,
    Always = 1,
    WhenWritingDefault = 2,
    WhenWritingNull = 3
}
