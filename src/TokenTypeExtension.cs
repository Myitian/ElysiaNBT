namespace ElysiaNBT;

public static class TokenTypeExtension
{
    public static NbtTagType GetTag(this TokenType tagType)
    {
        return tagType switch
        {
            TokenType.EndCompound => NbtTagType.End,
            TokenType.Byte or TokenType.True or TokenType.False => NbtTagType.Byte,
            TokenType.Short => NbtTagType.Short,
            TokenType.Int => NbtTagType.Int,
            TokenType.Long => NbtTagType.Long,
            TokenType.Float => NbtTagType.Float,
            TokenType.Double => NbtTagType.Double,
            TokenType.StartByteArray => NbtTagType.ByteArray,
            TokenType.String => NbtTagType.String,
            TokenType.StartList => NbtTagType.List,
            TokenType.StartCompound => NbtTagType.Compound,
            TokenType.StartIntArray => NbtTagType.IntArray,
            TokenType.StartLongArray => NbtTagType.LongArray,
            _ => NbtTagType.Unknown
        };
    }
}
