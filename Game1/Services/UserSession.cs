using System.Security.Claims;
using Game1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Game1.Services;

/// <summary>Builds the auth cookie for a user. Called on sign-in and after profile changes (new name / photo).</summary>
public static class UserSession
{
    public const string AvatarClaim = "avatar";

    public static Task SignInAsync(HttpContext http, User user, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };
        if (!string.IsNullOrEmpty(user.AvatarUrl)) claims.Add(new(AvatarClaim, user.AvatarUrl));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = persistent });
    }

    /// <summary>Re-issue the cookie keeping the current "remember me" choice.</summary>
    public static async Task RefreshAsync(HttpContext http, User user)
    {
        var current = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await SignInAsync(http, user, current.Properties?.IsPersistent ?? false);
    }
}
