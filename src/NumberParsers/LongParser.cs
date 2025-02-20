﻿using System.Globalization;

namespace ElysiaNBT.NumberParsers;

public static class LongParser
{
    public const byte MAX_CHAR_COUNT = 20;

    public const char SUFFIX_LOWER = 'l';
    public const char SUFFIX_UPPER = 'L';

    public static bool TryParse(ReadOnlySpan<char> s, out long result)
    {
        if (s.Length is > MAX_CHAR_COUNT or < 1)
            goto Failed;
        if (s[^1] is not (SUFFIX_LOWER or SUFFIX_UPPER))
            goto Failed;
        if (s.Length > 3 && s[0] is '+' or '-' && s[1] == '0' && s[2] == '0')
            goto Failed;
        if (s.Length > 2 && s[0] == '0' && s[1] == '0')
            goto Failed;
        return long.TryParse(s[..^1],
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result);
    Failed:
        result = 0;
        return false;
    }
}
