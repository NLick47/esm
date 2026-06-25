using System.Security.Cryptography;
using System.Text;

namespace EventStreamManager.WebApi;


public static class TokenHelper
{
    private const int ExpiryHours = 24;

    /// <summary>
    /// 生成 Token
    /// </summary>
    public static string GenerateToken(string password)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"{password}|{timestamp}";
        var signature = ComputeHmac(payload, password);
        var tokenData = $"{timestamp}|{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenData));
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    public static bool ValidateToken(string token, string password)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split('|', 2);
            if (parts.Length != 2) return false;

            if (!long.TryParse(parts[0], out var timestamp)) return false;
            var signature = parts[1];

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - timestamp > ExpiryHours * 3600) return false;

            var payload = $"{password}|{timestamp}";
            var expectedSignature = ComputeHmac(payload, password);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expectedSignature));
        }
        catch
        {
            return false;
        }
    }

    private static string ComputeHmac(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
