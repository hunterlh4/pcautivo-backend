namespace PCautivoCore.Shared.Utils;

public static class PasswordUtil
{
    private static readonly int _workFactor = 13;

    public static string HashPassword(string password)
    {
        return BC.EnhancedHashPassword(password, _workFactor);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BC.EnhancedVerify(password, hashedPassword);
    }
}