using System.Security.Cryptography;
using SistemaFacturacionWinForms.Models;

namespace SistemaFacturacionWinForms.Services;

public static class Security
{
    private const int Iterations = 210_000;
    public static void SetPassword(User user, string plain)
    {
        if (plain.Length < 8) throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(plain, salt, Iterations, HashAlgorithmName.SHA256, 32);
        user.PasswordSalt = Convert.ToBase64String(salt);
        user.PasswordHash = Convert.ToBase64String(hash);
    }
    public static bool Verify(User user, string plain)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(user.PasswordSalt);
            byte[] expected = Convert.FromBase64String(user.PasswordHash);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(plain, salt, Iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException) { return false; }
    }
}
