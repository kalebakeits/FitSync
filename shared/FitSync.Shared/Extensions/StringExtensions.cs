namespace FitSync.Shared.Extensions;

using System.Security.Cryptography;
using System.Text;

public static class StringExtensions
{
    public static string SHA256Hashed(this string src, string salt = "")
    {
        src += salt;
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(src.ToLowerInvariant()));
        string emailHash = Convert.ToHexString(bytes);
        return emailHash;
    }
}
