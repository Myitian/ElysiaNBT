using System.Globalization;

namespace ElysiaNBT.NumberParsers;

public static class DoubleParser
{
    public const char SUFFIX_LOWER = 'd';
    public const char SUFFIX_UPPER = 'D';

    public static bool TryParse(ReadOnlySpan<char> s, out double result)
    {
        if (s.Length < 1)
            goto Failed;
        if (s[^1] is SUFFIX_LOWER or SUFFIX_UPPER)
            s = s[..^1];
        return double.TryParse(s,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture,
            out result);
    Failed:
        result = 0;
        return false;
    }
}
