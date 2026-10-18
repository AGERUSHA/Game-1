using Game1.Models;

namespace Game1.Services;

/// <summary>
/// Stops password guessing on one account (sign-in form and the password checks in Settings):
/// after 10 wrong passwords in a row the account is locked for 10 minutes.
/// The counter lives on the user row (Users.FailedLogins / LockoutEnd), so it survives a restart.
/// Callers save the changes.
/// </summary>
public static class PasswordGuard
{
    private const int MaxFailures = 10;
    private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(10);

    public static bool IsLocked(User user) => user.LockoutEnd > DateTime.UtcNow;

    /// <summary>Whole minutes left until the lock ends (at least 1).</summary>
    public static int MinutesLeft(User user) =>
        Math.Max(1, (int)Math.Ceiling(((user.LockoutEnd ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes));

    public static void Failed(User user)
    {
        if (++user.FailedLogins < MaxFailures) return;
        user.LockoutEnd = DateTime.UtcNow + LockoutTime;
        user.FailedLogins = 0;
    }

    /// <summary>Right password, or the address was proven by email — start counting from zero.</summary>
    public static void Reset(User user)
    {
        user.FailedLogins = 0;
        user.LockoutEnd = null;
    }
}
