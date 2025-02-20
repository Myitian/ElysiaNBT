namespace ElysiaNBT;

public class NbtException : Exception
{
    public int Position { get; }

    public NbtException() : this(WarppedTextReader.EOF)
    {
    }
    public NbtException(int position)
    {
        Position = position;
    }
    public NbtException(string message, int position = WarppedTextReader.EOF) : base(message)
    {
        Position = position;
    }

    public static char ThrowIfNoMoreChars(int c)
    {
        if (c < 0)
            throw new NbtException($"Cannot read more chars");
        return (char)c;
    }
    public static byte ThrowIfNoMoreBytes(int b)
    {
        if (b < 0)
            throw new NbtException($"Cannot read more bytes");
        return (byte)b;
    }
    public static void ThrowEmptyPayload(int position)
    {
        throw new NbtException($"Payload is empty", position);
    }
    public static void ThrowInvalidChar(char c, int position)
    {
        throw new NbtException($"Invalid char '{c}' at position {position}", position);
    }
    public static void ThrowIfCharIsInvalid(int c, char expected, int position)
    {
        if (c < 0)
            throw new NbtException($"Cannot read more chars");
        if (c != expected)
            throw new NbtException($"Invalid char '{(char)c}' at position {position}, should be '{expected}'", position);
    }
    public static void ThrowIfCharIsInvalid(char c, char expected, int position)
    {
        if (c != expected)
            throw new NbtException($"Invalid char '{c}' at position {position}, should be '{expected}'", position);
    }
    public static NbtTagType ThrowIfTypeIsInvalid(int b, int position)
    {
        if (b < 0)
            throw new NbtException($"Cannot read more bytes");
        if (b > 12)
            throw new NbtException($"Invalid NBT type at position {position}", position);
        return (NbtTagType)b;
    }
    public static void ThrowUnknownEscapingSequence(char c, int position)
    {
        position--;
        throw new NbtException($"Unknown escaping sequence '\\{c}' at position {position}", position);
    }
}
