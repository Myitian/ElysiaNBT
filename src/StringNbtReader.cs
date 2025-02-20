using ElysiaNBT.NumberParsers;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ElysiaNBT;

public class StringNbtReader : WarppedTextReader, INbtReader
{
    public const char COMMA = ',';
    public const char COLON = ':';
    public const char SEMICOLON = ';';
    public const char LEFT_SQUARE_BRACKET = '[';
    public const char RIGHT_SQUARE_BRACKET = ']';
    public const char LEFT_CURLY_BRACKET = '{';
    public const char RIGHT_CURLY_BRACKET = '}';

    public const char ESCAPE = '\\';
    public const char DOUBLE_QUOTE = '"';
    public const char SINGLE_QUOTE = '\'';
    public const char NO_QUOTE = '\0';

    protected const int READER_BUFFER_SIZE = 32;
    protected const int PAYLOAD_BUFFER_SIZE = 1024;

    protected BitArray _inArray;
    protected bool _isFirstItem = true;
    protected bool _parsed = true;
    protected bool _escaping = false;
    protected char _quote = NO_QUOTE;
    protected Union _storedPayloadValue = new();
    protected List<char> _fullPayload = new(PAYLOAD_BUFFER_SIZE);

    public bool CanRead => FillBuffer() > 0;
    public NbtOptions Options { get; protected set; }
    public TokenType TokenType { get; protected set; }
    public INbtReader.State CurrentState { get; protected set; }
    public int CurrentDepth { get; protected set; }
    public int CurrentContentLength => -1;
    public ref Union StoredPayloadValue => ref _storedPayloadValue;

    public StringNbtReader(string snbt, NbtOptions options)
        : this(snbt, new StringNbtOptions(options)) { }
    public StringNbtReader(Stream stream, NbtOptions options, bool leaveOpen = true)
        : this(stream, new StringNbtOptions(options), leaveOpen) { }
    public StringNbtReader(TextReader reader, NbtOptions options, bool leaveOpen = true)
        : this(reader, new StringNbtOptions(options), leaveOpen) { }
    public StringNbtReader(string snbt, StringNbtOptions options)
        : base(snbt, READER_BUFFER_SIZE)
    {
        Options = options.GenericOptions;
        _inArray = new(options.GenericOptions.MaxDepth);
        CurrentState = options.GenericOptions.HasRootName ?
            INbtReader.State.ReadingName :
            INbtReader.State.ReadingPayload;
    }
    public StringNbtReader(Stream stream, StringNbtOptions options, bool leaveOpen = true)
        : base(stream, READER_BUFFER_SIZE, null, leaveOpen)
    {
        Options = options.GenericOptions;
        _inArray = new(options.GenericOptions.MaxDepth);
        CurrentState = options.GenericOptions.HasRootName ?
            INbtReader.State.ReadingName :
            INbtReader.State.ReadingPayload;
    }
    public StringNbtReader(TextReader reader, StringNbtOptions options, bool leaveOpen = true)
        : base(reader, READER_BUFFER_SIZE, leaveOpen)
    {
        Options = options.GenericOptions;
        _inArray = new(options.GenericOptions.MaxDepth);
        CurrentState = options.GenericOptions.HasRootName ?
            INbtReader.State.ReadingName :
            INbtReader.State.ReadingPayload;
    }

    protected int PeekUntilSeperatorOrBufferEnd(Span<char> buffer)
    {
        int maxRead = FillBuffer(Math.Min(buffer.Length, _buffer.Length));
        int i = 0;
        while (i < maxRead)
        {
            if (IsSeperator(_buffer[i++]))
            {
                i--;
                break;
            }
        }
        _buffer.AsSpan()[..i].CopyTo(buffer);
        return i;
    }
    protected void Except(char c)
    {
        SkipWhitespace();
        NbtException.ThrowIfCharIsInvalid(ReadChar(), c, Position);
    }
    protected int SkipUnquotedString()
    {
        int read = 0;
        int r;
        while ((r = ReadChar()) != EOF)
        {
            char c = (char)r;
            read++;
            if (IsSeperator(c))
            {
                read--;
                UngetChar(c);
                _parsed = true;
                break;
            }
            if (!IsValidUnquoted(c))
                NbtException.ThrowInvalidChar(c, Position);
        }
        return read;
    }
    protected int ParseUnquotedString()
    {
        int read = 0;
        char c;
        while (!IsSeperator(c = NbtException.ThrowIfNoMoreChars(ReadChar())))
        {
            read++;
            if (!IsValidUnquoted(c))
                NbtException.ThrowInvalidChar(c, Position);
            _fullPayload.Add(c);
        }
        UngetChar(c);
        return read - 1;
    }
    protected int SkipQuotedString()
    {
        int read = 0;
        for (; ; )
        {
            char c = NbtException.ThrowIfNoMoreChars(ReadChar());
            read++;
            if (_escaping)
            {
                if (c != ESCAPE && c != _quote)
                    NbtException.ThrowUnknownEscapingSequence(c, Position);
                _escaping = false;
            }
            else
            {
                if (c == ESCAPE)
                    _escaping = true;
                else if (c == _quote)
                {
                    _parsed = true;
                    break;
                }
            }
        }
        return read;
    }
    protected int ParseQuotedString()
    {
        int read = 0;
        for (; ; )
        {
            char c = NbtException.ThrowIfNoMoreChars(ReadChar());
            read++;
            if (_escaping)
            {
                if (c == ESCAPE || c == _quote)
                    _fullPayload.Add(c);
                else
                    NbtException.ThrowUnknownEscapingSequence(c, Position);
                _escaping = false;
            }
            else
            {
                if (c == ESCAPE)
                    _escaping = true;
                else if (c == _quote)
                    break;
                else
                    _fullPayload.Add(c);
            }
        }
        return read;
    }

    public TokenType Read()
    {
        if (!_parsed)
            Skip();

        _parsed = false;
        _escaping = false;
        _quote = NO_QUOTE;
        _storedPayloadValue = new();
        if (_fullPayload.Count > 0)
        {
            _fullPayload.Clear();
            _fullPayload.Capacity = PAYLOAD_BUFFER_SIZE;
        }

        SkipWhitespace();

        switch (CurrentState)
        {
            case INbtReader.State.ReadingName:
                switch (PeekChar())
                {
                    case EOF when CurrentDepth == 0:
                        CurrentState = INbtReader.State.Stopped;
                        TokenType = TokenType.None;
                        _parsed = true;
                        goto SuperBreak;
                    case RIGHT_CURLY_BRACKET:
                        CurrentDepth--;
                        if (_inArray[CurrentDepth])
                        {
                            CurrentState = INbtReader.State.ReadingPayload;
                        }
                        TokenType = TokenType.EndCompound;
                        SkipChars(1);
                        _isFirstItem = false;
                        _parsed = true;
                        goto SuperBreak;
                }
                if (!Options.HasRootName && CurrentDepth == 0)
                {
                    CurrentState = INbtReader.State.Stopped;
                    TokenType = TokenType.None;
                    break;
                }
                if (_isFirstItem)
                    _isFirstItem = false;
                else
                    Except(COMMA);
                SkipWhitespace();
                switch (PeekChar())
                {
                    case SINGLE_QUOTE:
                        _quote = SINGLE_QUOTE;
                        _escaping = false;
                        _parsed = false;
                        SkipChars(1);
                        break;
                    case DOUBLE_QUOTE:
                        _quote = DOUBLE_QUOTE;
                        _escaping = false;
                        _parsed = false;
                        SkipChars(1);
                        break;
                    default:
                        _quote = NO_QUOTE;
                        _parsed = false;
                        break;
                }
                CurrentState = INbtReader.State.ReadingPayload;
                TokenType = TokenType.Name;
                break;
            case INbtReader.State.ReadingPayload:
                char c = NbtException.ThrowIfNoMoreChars(PeekChar());
                if (_inArray[CurrentDepth])
                {
                    if (PeekChar() is RIGHT_SQUARE_BRACKET)
                    {
                        TokenType = TokenType.EndArray;
                        _inArray[CurrentDepth] = false;
                        CurrentDepth--;
                        if (!_inArray[CurrentDepth])
                            CurrentState = INbtReader.State.ReadingName;
                        SkipChars(1);
                        _isFirstItem = false;
                        _parsed = true;
                        break;
                    }
                    if (_isFirstItem)
                        _isFirstItem = false;
                    else
                        Except(COMMA);
                }
                else if (Options.HasRootName || CurrentDepth != 0)
                    Except(COLON);

                SkipWhitespace();
                c = NbtException.ThrowIfNoMoreChars(PeekChar());
                switch (c)
                {
                    case LEFT_CURLY_BRACKET:
                        CurrentState = INbtReader.State.ReadingName;
                        CurrentDepth++;
                        _isFirstItem = true;
                        SkipChars(1);
                        TokenType = TokenType.StartCompound;
                        _parsed = true;
                        break;
                    case LEFT_SQUARE_BRACKET:
                        CurrentDepth++;
                        _isFirstItem = true;
                        SkipChars(1);
                        _inArray[CurrentDepth] = true;
                        _parsed = true;
                        if (PeekChar(1) == SEMICOLON &&
                            PeekChar(0) is not (SINGLE_QUOTE or DOUBLE_QUOTE))
                        {
                            switch (PeekChar(0))
                            {
                                case 'B':
                                    TokenType = TokenType.StartByteArray;
                                    break;
                                case 'I':
                                    TokenType = TokenType.StartIntArray;
                                    break;
                                case 'L':
                                    TokenType = TokenType.StartLongArray;
                                    break;
                                default:
                                    NbtException.ThrowInvalidChar((char)PeekChar(1), Position + 1);
                                    throw null!;
                            }
                            SkipChars(2);
                        }
                        else
                            TokenType = TokenType.StartList;
                        break;
                    case SINGLE_QUOTE:
                    case DOUBLE_QUOTE:
                        TokenType = TokenType.String;
                        _quote = c;
                        _escaping = false;
                        _parsed = false;
                        SkipChars(1);
                        if (!_inArray[CurrentDepth])
                            CurrentState = INbtReader.State.ReadingName;
                        break;
                    default:
                        TokenType = TokenType.UnknownPayload;
                        Span<char> tempBuffer = stackalloc char[32];
                        int read = PeekUntilSeperatorOrBufferEnd(tempBuffer);
                        if (read == 0)
                        {
                            NbtException.ThrowEmptyPayload(Position);
                        }
                        else if (read < 32)
                        {
                            _parsed = true;
                            ReadOnlySpan<char> parseBuffer = tempBuffer[..read];
                            _fullPayload.AddRange(parseBuffer);
                            SkipChars(read);
                            if (FloatParser.TryParse(parseBuffer, out StoredPayloadValue.FP32))
                                TokenType = TokenType.Float;
                            else if (SByteParser.TryParse(parseBuffer, out StoredPayloadValue.I8))
                                TokenType = TokenType.Byte;
                            else if (LongParser.TryParse(parseBuffer, out StoredPayloadValue.I64))
                                TokenType = TokenType.Long;
                            else if (ShortParser.TryParse(parseBuffer, out StoredPayloadValue.I16))
                                TokenType = TokenType.Short;
                            else if (IntParser.TryParse(parseBuffer, out StoredPayloadValue.I32))
                                TokenType = TokenType.Int;
                            else if (DoubleParser.TryParse(parseBuffer, out StoredPayloadValue.FP64))
                                TokenType = TokenType.Double;
                            else if (parseBuffer.Equals("true", StringComparison.OrdinalIgnoreCase))
                            {
                                TokenType = TokenType.True;
                                StoredPayloadValue.I8 = 1;
                            }
                            else if (parseBuffer.Equals("false", StringComparison.OrdinalIgnoreCase))
                            {
                                TokenType = TokenType.False;
                                StoredPayloadValue.I8 = 0;
                            }
                            else
                            {
                                TokenType = TokenType.String;
                                _quote = NO_QUOTE;
                                _escaping = false;
                                _parsed = true;
                                ParseUnquotedString();
                                parseBuffer = CollectionsMarshal.AsSpan(_fullPayload);
                                if (FloatParser.TryParse(parseBuffer, out StoredPayloadValue.FP32))
                                    TokenType = TokenType.Float;
                                else if (DoubleParser.TryParse(parseBuffer, out StoredPayloadValue.FP64))
                                    TokenType = TokenType.Double;
                            }
                        }
                        if (!_inArray[CurrentDepth])
                            CurrentState = INbtReader.State.ReadingName;
                        break;
                }
                break;
            case INbtReader.State.Stopped:
                TokenType = TokenType.None;
                break;
        }
    SuperBreak:
        return TokenType;
    }
    public bool GetBool(bool strict = false)
        => ((INbtReader)this).BaseGetBool(strict);
    public byte GetByte(bool strict = false)
        => ((INbtReader)this).BaseGetByte(strict);
    public sbyte GetSByte(bool strict = false)
        => ((INbtReader)this).BaseGetSByte(strict);
    public short GetShort(bool strict = false)
        => ((INbtReader)this).BaseGetShort(strict);
    public ushort GetUShort(bool strict = false)
        => ((INbtReader)this).BaseGetUShort(strict);
    public int GetInt(bool strict = false)
        => ((INbtReader)this).BaseGetInt(strict);
    public uint GetUInt(bool strict = false)
        => ((INbtReader)this).BaseGetUInt(strict);
    public long GetLong(bool strict = false)
        => ((INbtReader)this).BaseGetLong(strict);
    public ulong GetULong(bool strict = false)
        => ((INbtReader)this).BaseGetULong(strict);
    public float GetFloat(bool strict = false)
        => ((INbtReader)this).BaseGetFloat(strict);
    public double GetDouble(bool strict = false)
        => ((INbtReader)this).BaseGetDouble(strict);
    public string GetString()
    {
        if (!_parsed)
        {
            if (_quote != NO_QUOTE)
                ParseQuotedString();
            else
                ParseUnquotedString();
            _parsed = true;
        }
        return new(GetCharSpan());
    }
    public char[] GetCharArray()
    {
        return [.. _fullPayload];
    }
    public ReadOnlySpan<char> GetCharSpan()
    {
        return CollectionsMarshal.AsSpan(_fullPayload);
    }
    public Span<char> GetSafeCharSpan()
    {
        return GetCharArray();
    }

    public void Skip()
    {
        if (!_parsed)
        {
            if (_quote != NO_QUOTE)
                SkipQuotedString();
            else
                SkipUnquotedString();
            _parsed = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUnquoted(char c)
    {
        return c is >= '0' and <= '9'
            or >= 'A' and <= 'Z'
            or >= 'a' and <= 'z'
            or '.' or '_' or '-' or '+';
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSeperator(char c)
    {
        return char.IsWhiteSpace(c)
            || c is COLON or COMMA or RIGHT_CURLY_BRACKET or RIGHT_SQUARE_BRACKET;
    }
}
