using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth;

public static class SimplePasswordHasher
{
    // Format: PBKDF2$<iterations>$<saltBase64>$<hashBase64>
    public static string HashPassword(string password, int iterations = 100_000, int saltSize = 16, int keySize = 32)
    {
        var salt = RandomNumberGenerator.GetBytes(saltSize);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(keySize);
        return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hash)
    {
        try
        {
            var parts = hash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2") return false;
            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var key = Convert.FromBase64String(parts[3]);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var keyToCheck = pbkdf2.GetBytes(key.Length);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
        catch
        {
            return false;
        }
    }
}

