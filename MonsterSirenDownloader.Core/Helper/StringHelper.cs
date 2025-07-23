namespace MonsterSirenDownloader.Core.Helper;

public class StringHelper
{
    /// <summary>
    ///     Replace the tail dot ('.') to underscore ('_'), if the given string is
    ///     not ends with dot, no character will be replaced.
    /// </summary>
    /// <param name="s">a string</param>
    /// <returns>a string ends with underscore</returns>
    public static string ReplaceTailDot(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        var lastChar = s[^1];

        return lastChar == '.'
            ? string.Concat(s.AsSpan(0, s.Length - 1), "_")
            : s;
    }
}