using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace AcademiaDoZe.Application.Security;

public static class PasswordHasher
{
    private const int Iterations = 4;
    private const int MemorySizeKb = 65536; // 64 MB
    private const int DegreeOfParallelism = 1;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySizeKb
        };

        byte[] hash = argon2.GetBytes(HashSizeBytes);

        return $"ARGON2ID:{Iterations}:{MemorySizeKb}:{DegreeOfParallelism}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        string[] parts = hashedPassword.Split(':');
        if (parts.Length != 6 || parts[0] != "ARGON2ID")
            return false;

        if (!int.TryParse(parts[1], out int iterations) ||
            !int.TryParse(parts[2], out int memorySizeKb) ||
            !int.TryParse(parts[3], out int degreeOfParallelism))
            return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[4]);
            expectedHash = Convert.FromBase64String(parts[5]);
        }
        catch (FormatException)
        {
            return false;
        }

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = degreeOfParallelism,
            Iterations = iterations,
            MemorySize = memorySizeKb
        };

        byte[] actualHash = argon2.GetBytes(expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

