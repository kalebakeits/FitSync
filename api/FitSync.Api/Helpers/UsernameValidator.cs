namespace FitSync.Api.Helpers;

using System.Text.RegularExpressions;

public static partial class UsernameValidator
{
    private static readonly Regex UsernameRegex = UserNameGeneratedRegex();

    public static bool IsValid(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return UsernameRegex.IsMatch(username);
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$", RegexOptions.Compiled)]
    private static partial Regex UserNameGeneratedRegex();
}
