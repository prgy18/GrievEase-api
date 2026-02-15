using BCrypt.Net;

namespace GrievEase.API.Helpers;

public class PasswordHelper
{
    // Hash plain text password using BCrypt
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    // Verify plain text password against stored hash
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}