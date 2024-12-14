using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace ElysiaNBT;

public class StringNbtWriter : INbtWriter, IDisposable
{
    public const char SPACE = ' ';
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

    protected BitArray _inArray;
    protected bool _isFirstItem = true;
    protected char _quote = NO_QUOTE;
    protected bool _leaveOpen = true;
    protected TextWriter _baseWriter;

    private bool _disposed = false;

    public StringNbtOptions StringNbtOptions { get; protected set; }
    public NbtOptions Options => StringNbtOptions.GenericOptions;
    public INbtWriter.State LastState { get; protected set; }
    public INbtWriter.State CurrentState
    {
        get => field;
        protected set
        {
            LastState = field;
            field = value;
        }
    }
    public bool Inline { get; set; } = true;
    public bool CommaDone { get; protected set; } = true;
    public bool InEmptyLine { get; protected set; } = true;
    public int CurrentDepth { get; protected set; }

    public bool RequiresLengthInfo => false;

    public int Position { get; protected set; } = 0;

    public StringNbtWriter(Stream stream, NbtOptions options, bool leaveOpen = true)
        : this(stream, new StringNbtOptions(options), leaveOpen) { }
    public StringNbtWriter(StringBuilder builder, NbtOptions options)
        : this(builder, new StringNbtOptions(options)) { }
    public StringNbtWriter(TextWriter writer, NbtOptions options, bool leaveOpen = true)
        : this(writer, new StringNbtOptions(options), leaveOpen) { }
    public StringNbtWriter(Stream stream, StringNbtOptions options, bool leaveOpen = true)
        : this(new StreamWriter(stream, options.Encoding, -1, leaveOpen), options, false) { }
    public StringNbtWriter(StringBuilder builder, StringNbtOptions options)
        : this(new StringBuilderWriter(builder), options, false) { }
    public StringNbtWriter(TextWriter writer, StringNbtOptions options, bool leaveOpen = true)
    {
        StringNbtOptions = options;
        _inArray = new(options.GenericOptions.MaxDepth);
        _baseWriter = writer;
        _leaveOpen = leaveOpen;
        CurrentState = options.GenericOptions.HasRootName ?
            INbtWriter.State.WritingName :
            INbtWriter.State.WritingPayload;
        LastState = INbtWriter.State.Stopped;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (!_leaveOpen)
            _baseWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    public void AutoWriteSymbols(bool endToken = false)
    {
        switch (CurrentState)
        {
            case INbtWriter.State.WritingPayload when LastState is INbtWriter.State.WritingName:
                WriteColon();
                break;
            case INbtWriter.State.WritingName when _isFirstItem:
            case INbtWriter.State.WritingPayload when _isFirstItem:
                if (!Inline && CurrentDepth > 0)
                {
                    WriteLine();
                    WriteIndent();
                }
                break;
            case INbtWriter.State.WritingName:
            case INbtWriter.State.WritingPayload:
                if (!endToken)
                    WriteComma();
                if (!Inline)
                {
                    WriteLine();
                    WriteIndent();
                }
                break;
            default:
                throw new InvalidOperationException();
        }
        Inline = false;
    }
    public void WriteComma()
    {
        _baseWriter.Write(COMMA);
        Position++;
        if (Inline && StringNbtOptions.CommaSpace)
        {
            _baseWriter.Write(SPACE);
            Position++;
        }
    }
    public void WriteColon()
    {
        _baseWriter.Write(COLON);
        Position++;
        if (StringNbtOptions.ColonSpace)
        {
            _baseWriter.Write(SPACE);
            Position++;
        }
    }
    public void WriteLine()
    {
        _baseWriter.Write(StringNbtOptions.NewLine);
        Position += StringNbtOptions.NewLine.Length;
    }
    public void WriteIndent()
    {
        InEmptyLine = false;
        int indent = StringNbtOptions.Indent * CurrentDepth;
        for (int i = 0; i < indent; i++)
            _baseWriter.Write(StringNbtOptions.IndentString);
        Position += StringNbtOptions.IndentString.Length * indent;
    }
    public char SelectQuote(bool isName, ReadOnlySpan<char> str, StringNbtOptions.QuotePreferences? overrides = null)
    {
        StringNbtOptions.QuotePreferences pref = overrides
            ?? (isName ? StringNbtOptions.NameQuotePreferences : StringNbtOptions.PayloadQuotePreferences);
        switch (pref)
        {
            case StringNbtOptions.QuotePreferences.Default:
                foreach (char c in str)
                {
                    if (c is SINGLE_QUOTE)
                        return DOUBLE_QUOTE;
                    else if (c is DOUBLE_QUOTE)
                        return SINGLE_QUOTE;
                }
                return DOUBLE_QUOTE;
            case StringNbtOptions.QuotePreferences.DefaultWithNoQuote:
                {
                    if (str.IsEmpty)
                        return DOUBLE_QUOTE;
                    bool validUnquoted = true;
                    foreach (char c in str)
                    {
                        if (validUnquoted && StringNbtReader.IsValidUnquoted(c))
                            continue;
                        if (c is SINGLE_QUOTE)
                            return DOUBLE_QUOTE;
                        else if (c is DOUBLE_QUOTE)
                            return SINGLE_QUOTE;
                        validUnquoted = false;
                    }
                    return validUnquoted ? NO_QUOTE : DOUBLE_QUOTE;
                }
            case StringNbtOptions.QuotePreferences.Shorter:
                {
                    if (str.IsEmpty)
                        return DOUBLE_QUOTE;
                    int singleCount = 0, doubleCount = 0;
                    bool validUnquoted = true;
                    foreach (char c in str)
                    {
                        if (validUnquoted && StringNbtReader.IsValidUnquoted(c))
                            continue;
                        if (c is SINGLE_QUOTE)
                            singleCount++;
                        else if (c is DOUBLE_QUOTE)
                            doubleCount++;
                        validUnquoted = false;
                    }
                    return doubleCount > singleCount ? SINGLE_QUOTE : DOUBLE_QUOTE;
                }
            case StringNbtOptions.QuotePreferences.ForceSingle:
                return SINGLE_QUOTE;
            case StringNbtOptions.QuotePreferences.ForceDouble:
                return DOUBLE_QUOTE;
            default:
                throw new ArgumentOutOfRangeException(null, "QuotePreferences is invalid");
        }
    }
    public void WriteEscapedString(bool isName, ReadOnlySpan<char> str, StringNbtOptions.QuotePreferences? overrides = null)
    {
        char q = SelectQuote(isName, str, overrides);
        bool noQuote = q is NO_QUOTE;
        if (!noQuote)
        {
            _baseWriter.Write(q);
            Position++;
        }
        foreach (char c in str)
        {
            if (c is ESCAPE || !noQuote && c == q)
            {
                _baseWriter.Write(ESCAPE);
                Position++;
            }
            _baseWriter.Write(c);
            Position++;
        }
        if (!noQuote)
        {
            _baseWriter.Write(q);
            Position++;
        }
    }

    public bool IsInArray
    {
        get => _inArray[CurrentDepth];
        protected set => _inArray[CurrentDepth] = value;
    }

    public void WriteEndArray()
    {
        if (!IsInArray)
            throw new InvalidOperationException();
        CurrentDepth--;
        AutoWriteSymbols(true);
        CurrentState = INbtWriter.State.WritingPayload;
        _isFirstItem = false;
        _baseWriter.Write(RIGHT_SQUARE_BRACKET);
        Position++;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WriteEndCompound()
    {
        if (IsInArray)
            throw new InvalidOperationException();
        CurrentDepth--;
        AutoWriteSymbols(true);
        CurrentState = INbtWriter.State.WritingPayload;
        _isFirstItem = false;
        _baseWriter.Write(RIGHT_CURLY_BRACKET);
        Position++;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WriteName(ReadOnlySpan<char> name, NbtTagType tagType = NbtTagType.Unknown)
    {
        if (!Options.HasRootName && CurrentDepth == 0 && _isFirstItem)
            return;
        AutoWriteSymbols();
        _isFirstItem = false;
        WriteEscapedString(true, name);
        CurrentState = INbtWriter.State.WritingPayload;
    }

    public void WriteLength(int length)
    {
    }

    public void WriteStartList(NbtTagType itemType = NbtTagType.Unknown, int length = -1)
    {
        AutoWriteSymbols();
        _isFirstItem = true;
        _baseWriter.Write(LEFT_SQUARE_BRACKET);
        Position++;
        CurrentDepth++;
        IsInArray = true;
        CurrentState = INbtWriter.State.WritingPayload;
    }

    public void WriteStartByteArray(int length = -1)
    {
        AutoWriteSymbols();
        _isFirstItem = true;
        _baseWriter.Write("[B;");
        Position += 3;
        CurrentDepth++;
        IsInArray = true;
        CurrentState = INbtWriter.State.WritingPayload;
    }

    public void WriteStartIntArray(int length = -1)
    {
        AutoWriteSymbols();
        _isFirstItem = true;
        _baseWriter.Write("[I;");
        Position += 3;
        CurrentDepth++;
        IsInArray = true;
        CurrentState = INbtWriter.State.WritingPayload;
    }

    public void WriteStartLongArray(int length = -1)
    {
        AutoWriteSymbols();
        _isFirstItem = true;
        _baseWriter.Write("[L;");
        Position += 3;
        CurrentDepth++;
        IsInArray = true;
        CurrentState = INbtWriter.State.WritingPayload;
    }

    public void WriteStartCompound()
    {
        AutoWriteSymbols();
        _isFirstItem = true;
        _baseWriter.Write(LEFT_CURLY_BRACKET);
        Position++;
        CurrentDepth++;
        IsInArray = false;
        CurrentState = INbtWriter.State.WritingName;
    }

    public void WriteEnd()
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        _baseWriter.Write("END");
        Position += 3;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WriteTrue()
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        _baseWriter.Write("true");
        Position += 4;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WriteFalse()
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        _baseWriter.Write("false");
        Position += 5;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(byte payload)
    {
        WritePayload((sbyte)payload);
    }

    public void WritePayload(sbyte payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[5];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        buffer[w++] = 'b';
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(short payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[7];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        buffer[w++] = 's';
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(ushort payload)
    {
        WritePayload((short)payload);
    }

    public void WritePayload(int payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[11];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(uint payload)
    {
        WritePayload((int)payload);
    }

    public void WritePayload(long payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[21];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        buffer[w++] = 'L';
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(ulong payload)
    {
        WritePayload((long)payload);
    }

    public void WritePayload(float payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[32];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        buffer[w++] = 'f';
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(double payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        Span<char> buffer = stackalloc char[32];
        payload.TryFormat(buffer, out int w, provider: CultureInfo.InvariantCulture);
        buffer[w++] = 'd';
        _baseWriter.Write(buffer[..w]);
        Position += w;
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void WritePayload(ReadOnlySpan<char> payload)
    {
        AutoWriteSymbols();
        _isFirstItem = false;
        WriteEscapedString(false, payload);
        CurrentState = IsInArray ?
            INbtWriter.State.WritingPayload :
            INbtWriter.State.WritingName;
    }

    public void Flush()
    {
        _baseWriter.Flush();
    }

    class StringBuilderWriter(StringBuilder stringBuilder) : TextWriter
    {
        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(bool value)
            => stringBuilder.Append(value);
        public override void Write(char value)
            => stringBuilder.Append(value);
        public override void Write(char[]? buffer)
            => stringBuilder.Append(buffer);
        public override void Write(char[] buffer, int index, int count)
            => stringBuilder.Append(buffer, index, count);
        public override void Write(decimal value)
            => stringBuilder.Append(value);
        public override void Write(double value)
            => stringBuilder.Append(value);
        public override void Write(int value)
            => stringBuilder.Append(value);
        public override void Write(long value)
            => stringBuilder.Append(value);
        public override void Write(object? value)
            => stringBuilder.Append(value);
        public override void Write(ReadOnlySpan<char> buffer)
            => stringBuilder.Append(buffer);
        public override void Write(float value)
            => stringBuilder.Append(value);
        public override void Write(string? value)
            => stringBuilder.Append(value);
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0)
            => stringBuilder.AppendFormat(format, arg0);
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
            => stringBuilder.AppendFormat(format, arg0, arg1);
        public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
            => stringBuilder.AppendFormat(format, arg0, arg1, arg2);
        public override void Write([StringSyntax("CompositeFormat")] string format, params object?[] arg)
            => stringBuilder.AppendFormat(format, arg);
        public override void Write([StringSyntax("CompositeFormat")] string format, params ReadOnlySpan<object?> arg)
            => stringBuilder.AppendFormat(format, arg);
        public override void Write(StringBuilder? value)
            => stringBuilder.Append(value);
        public override void Write(uint value)
            => stringBuilder.Append(value);
        public override void Write(ulong value)
            => stringBuilder.Append(value);
    }
}