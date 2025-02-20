namespace ElysiaNBT;

public static class NbtTagTypeExtension
{
    public static TokenType GetToken(this NbtTagType tagType)
    {
        return tagType switch
        {
            NbtTagType.End => TokenType.EndCompound,
            NbtTagType.Byte => TokenType.Byte,
            NbtTagType.Short => TokenType.Short,
            NbtTagType.Int => TokenType.Int,
            NbtTagType.Long => TokenType.Long,
            NbtTagType.Float => TokenType.Float,
            NbtTagType.Double => TokenType.Double,
            NbtTagType.ByteArray => TokenType.StartByteArray,
            NbtTagType.String => TokenType.String,
            NbtTagType.List => TokenType.StartList,
            NbtTagType.Compound => TokenType.StartCompound,
            NbtTagType.IntArray => TokenType.StartIntArray,
            NbtTagType.LongArray => TokenType.StartLongArray,
            _ => throw new NbtException("Unknown NBT tag."),
        };
    }
}
