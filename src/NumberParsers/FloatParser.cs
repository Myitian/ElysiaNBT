using System.Globalization;

namespace ElysiaNBT.NumberParsers;

public static class FloatParser
{
    public const char SUFFIX_LOWER = 'f';
    public const char SUFFIX_UPPER = 'F';

    public static bool TryParse(ReadOnlySpan<char> s, out float result)
    {
        if (s.Length < 1)
            goto Failed;
        if (s[^1] is not (SUFFIX_LOWER or SUFFIX_UPPER))
            goto Failed;
        return float.TryParse(s[..^1],
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture,
            out result);
    Failed:
        result = 0;
        return false;
    }
}
