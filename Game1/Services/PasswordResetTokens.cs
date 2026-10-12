using Game1.Models;
using Microsoft.AspNetCore.DataProtection;

namespace Game1.Services;

/// <summary>
/// Signed, encrypted, time-limited password-reset tokens (no database table needed).
/// The token embeds a fingerprint of the current password hash, so it stops working
/// as soon as the password is changed — every link can be used only once.
/// </summary>
public class PasswordResetTokens(IDataProtectionProvider provider)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    private readonly ITimeLimitedDataProtector protector =
        provider.CreateProtector("Game1.PasswordReset.v1").ToTimeLimitedDataProtector();

    public string Create(User user) => protector.Protect($"{user.Id}|{Stamp(user)}", Lifetime);

    /// <summary>User id if the token is valid and not expired, otherwise null (stamp is checked by <see cref="Matches"/>).</summary>
    public (int UserId, string Stamp)? Read(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var parts = protector.Unprotect(token).Split('|', 2);
            return int.TryParse(parts[0], out var id) && parts.Length == 2 ? (id, parts[1]) : null;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;   // tampered or expired
        }
    }

    public static bool Matches(User user, string stamp) => Stamp(user) == stamp;

    private static string Stamp(User user) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(user.PasswordHash ?? "")))[..16];
}
