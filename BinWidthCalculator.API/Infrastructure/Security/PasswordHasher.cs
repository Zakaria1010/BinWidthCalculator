using System;
using System.Text;
using System.Security.Cryptography;
using BinWidthCalculator.Domain.Interfaces;

namespace BinWidthCalculator.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
            return false;
        var hashedInput = HashPassword(password);
        return hashedInput == passwordHash;
    }
}