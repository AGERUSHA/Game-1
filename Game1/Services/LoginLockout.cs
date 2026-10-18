using Microsoft.Extensions.Caching.Memory;

namespace Game1.Services;

/// <summary>
/// Stops password guessing on one account: after 10 wrong passwords the email is locked
/// for 10 minutes (from any IP). A successful sign-in or password reset clears the counter.
/// </summary>
public class LoginLockout(IMemoryCache cache)
{
    private const int MaxFailures = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    private sealed class Counter { public int Failures; }

    public bool IsLocked(string email) =>
        cache.TryGetValue<Counter>(Key(email), out var c) && c!.Failures >= MaxFailures;

    public void Failed(string email)
    {
        var c = cache.GetOrCreate(Key(email), e => { e.AbsoluteExpirationRelativeToNow = Window; return new Counter(); })!;
        Interlocked.Increment(ref c.Failures);
    }

    public void Reset(string email) => cache.Remove(Key(email));

    private static string Key(string email) => "login-fail:" + email.Trim().ToLowerInvariant();
}
