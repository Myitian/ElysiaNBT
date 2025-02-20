using System.Text;

namespace ElysiaNBT;

public struct StringNbtOptions(NbtOptions options)
{
    public static readonly StringNbtOptions Default = new();
    public static readonly StringNbtOptions Minimal = new()
    {
        Indent = 0,
        NewLine = "",
        ColonSpace = false,
        CommaSpace = false
    };
    public static readonly StringNbtOptions PrettyPrint = new()
    {
        Indent = 4,
        NewLine = Environment.NewLine,
        ColonSpace = true,
        CommaSpace = true
    };

    private int _indent = 0;

    public NbtOptions GenericOptions = options;
    public string IndentString { get; set; } = " ";
    public string NewLine { get; set; } = "";
    public bool ColonSpace { get; set; } = false;
    public bool CommaSpace { get; set; } = false;
    public int Indent
    {
        readonly get => _indent;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _indent = value;
        }
    }
    public Encoding? Encoding { get; set; }
    public QuotePreferences NameQuotePreferences { get; set; } = QuotePreferences.DefaultWithNoQuote;
    public QuotePreferences PayloadQuotePreferences { get; set; } = QuotePreferences.Default;

    public enum QuotePreferences
    {
        Default,
        DefaultWithNoQuote,
        Shorter,
        ForceSingle,
        ForceDouble
    }

    public StringNbtOptions() : this(new() { HasRootName = false })
    {
    }
}