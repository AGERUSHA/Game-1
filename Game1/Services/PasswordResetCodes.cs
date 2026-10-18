using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Game1.Services;

/// <summary>
/// 6-digit "forgot password" codes sent by email (an alternative to clicking the link).
/// Kept in memory for 15 minutes, only as a SHA-256 hash; 5 wrong tries and the code is burned.
/// </summary>
public class PasswordResetCodes(IMemoryCache cache)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);
    private const int MaxAttempts = 5;

    private sealed class Entry
    {
        public required int UserId { get; init; }
        public required byte[] Hash { get; init; }
        public int Attempts;
    }

    /// <summary>New code for this email (replaces the previous one).</summary>
    public string Create(string email, int userId)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        cache.Set(Key(email), new Entry { UserId = userId, Hash = Hash(code) }, Lifetime);
        return code;
    }

    /// <summary>User id when the code is right (the code is then used up), otherwise null.</summary>
    public int? Verify(string email, string? code)
    {
        var key = Key(email);
        if (!cache.TryGetValue<Entry>(key, out var entry) || entry == null) return null;

        var digits = new string((code ?? "").Where(char.IsAsciiDigit).ToArray());
        if (digits.Length == 6 && CryptographicOperations.FixedTimeEquals(entry.Hash, Hash(digits)))
        {
            cache.Remove(key);
            return entry.UserId;
        }
        if (Interlocked.Increment(ref entry.Attempts) >= MaxAttempts) cache.Remove(key);
        return null;
    }

    private static string Key(string email) => "pwcode:" + email.Trim().ToLowerInvariant();
    private static byte[] Hash(string code) => SHA256.HashData(Encoding.UTF8.GetBytes(code));
}
