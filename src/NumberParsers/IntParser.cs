using System.Globalization;

namespace ElysiaNBT.NumberParsers;

public static class IntParser
{
    public const byte MAX_CHAR_COUNT = 11;

    public static bool TryParse(ReadOnlySpan<char> s, out int result)
    {
        if (s.Length is > MAX_CHAR_COUNT or < 1)
            goto Failed;
        if (s.Length > 2 && s[0] is '+' or '-' && s[1] == '0' && s[2] == '0')
            goto Failed;
        if (s.Length > 1 && s[0] == '0' && s[1] == '0')
            goto Failed;
        return int.TryParse(s,
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result);
    Failed:
        result = 0;
        return false;
    }
}
