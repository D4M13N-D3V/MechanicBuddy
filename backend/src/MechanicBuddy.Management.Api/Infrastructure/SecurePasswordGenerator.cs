using System.Security.Cryptography;

namespace MechanicBuddy.Management.Api.Infrastructure;

/// <summary>
/// Generates cryptographically-random passwords for per-tenant admin accounts.
/// </summary>
public static class SecurePasswordGenerator
{
    // Excludes ambiguous characters (0/O, 1/l/I) for readability when delivered.
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%^&*-_=+";
    private const string All = Lower + Upper + Digits + Symbols;

    public static string Generate(int length = 20)
    {
        if (length < 12) length = 12;

        var chars = new char[length];
        // Guarantee at least one of each class so it passes complexity rules.
        chars[0] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
        chars[1] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
        chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
        chars[3] = Symbols[RandomNumberGenerator.GetInt32(Symbols.Length)];
        for (var i = 4; i < length; i++)
        {
            chars[i] = All[RandomNumberGenerator.GetInt32(All.Length)];
        }

        // Fisher–Yates shuffle so the guaranteed chars aren't always first.
        for (var i = length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
