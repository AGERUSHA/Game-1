using System.Security.Cryptography;
using System.Text;
using Game1.Data;
using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Services;

/// <summary>
/// 6-digit codes sent by email: confirm the address after sign-up, or reset a forgotten password.
/// Stored in the database (survive a restart) only as a SHA-256 hash; single use, 5 wrong tries burn the code.
/// </summary>
public class EmailCodes(AppDbContext db)
{
    private const int MaxAttempts = 5;

    /// <summary>A new code can be requested at most once a minute.</summary>
    public static readonly TimeSpan ResendDelay = TimeSpan.FromMinutes(1);

    public static TimeSpan Lifetime(string purpose) =>
        purpose == EmailCode.Confirm ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(15);

    /// <summary>New code for this user and purpose (replaces the previous one).</summary>
    public async Task<string> CreateAsync(User user, string purpose)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var row = await db.EmailCodes.SingleOrDefaultAsync(c => c.UserId == user.Id && c.Purpose == purpose);
        if (row == null) db.EmailCodes.Add(row = new EmailCode { UserId = user.Id, Purpose = purpose });
        row.CodeHash = Hash(code);
        row.Attempts = 0;
        row.CreatedAt = DateTime.UtcNow;
        row.ExpiresAt = row.CreatedAt + Lifetime(purpose);
        await db.SaveChangesAsync();
        return code;
    }

    /// <summary>True when a code was sent less than <see cref="ResendDelay"/> ago.</summary>
    public Task<bool> SentRecentlyAsync(User user, string purpose)
    {
        var since = DateTime.UtcNow - ResendDelay;
        return db.EmailCodes.AnyAsync(c => c.UserId == user.Id && c.Purpose == purpose && c.CreatedAt > since);
    }

    /// <summary>True when the code is right and not expired; the code is then used up.</summary>
    public async Task<bool> VerifyAsync(User user, string purpose, string? code)
    {
        var row = await db.EmailCodes.SingleOrDefaultAsync(c => c.UserId == user.Id && c.Purpose == purpose);
        if (row == null) return false;
        if (row.ExpiresAt < DateTime.UtcNow)
        {
            db.EmailCodes.Remove(row);
            await db.SaveChangesAsync();
            return false;
        }

        var digits = new string((code ?? "").Where(char.IsAsciiDigit).ToArray());
        var ok = digits.Length == 6 && CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(row.CodeHash), Convert.FromHexString(Hash(digits)));
        if (ok || ++row.Attempts >= MaxAttempts) db.EmailCodes.Remove(row);
        await db.SaveChangesAsync();
        return ok;
    }

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
