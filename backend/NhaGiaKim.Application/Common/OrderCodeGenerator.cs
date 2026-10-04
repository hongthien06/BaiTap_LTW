using System.Security.Cryptography;

namespace NhaGiaKim.Application.Common;

/// <summary>Sinh ma don dang NGK-yyyyMMdd-XXXX (XXXX la 4 ky tu ngau nhien an toan).</summary>
public static class OrderCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // bo I,O,0,1 cho de doc

    public static string Generate(DateTime utcNow)
    {
        Span<char> suffix = stackalloc char[4];
        for (var i = 0; i < suffix.Length; i++)
        {
            suffix[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return $"NGK-{utcNow:yyyyMMdd}-{new string(suffix)}";
    }
}
