using System.ComponentModel.DataAnnotations;

namespace Game1.Models;

/// <summary>
/// Account stored in SQL Server (table dbo.Users).
/// The plain password is never stored — only its salted PBKDF2 hash.
/// </summary>
public class User
{
    public int Id { get; set; }

    [MaxLength(100)] public string FirstName { get; set; } = "";
    [MaxLength(100)] public string LastName { get; set; } = "";

    [MaxLength(256)] public string Email { get; set; } = "";

    /// <summary>PBKDF2 (HMAC-SHA512, 100k iterations, random salt) — see PasswordHasher&lt;T&gt;.</summary>
    [MaxLength(512)] public string PasswordHash { get; set; } = "";

    /// <summary>student | teacher | admin</summary>
    [MaxLength(20)] public string Role { get; set; } = "student";

    /// <summary>active | blocked — blocked users cannot sign in.</summary>
    [MaxLength(20)] public string Status { get; set; } = "active";

    /// <summary>Uploaded photo, e.g. /uploads/avatars/12-ab34cd.webp (null → initials).</summary>
    [MaxLength(300)] public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public bool RememberMe { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

/// <summary>Admin → "Add user" form.</summary>
public class CreateUserViewModel
{
    public string Role { get; set; } = "student";
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
}

public class RegisterViewModel
{
    public string Role { get; set; } = "student";
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}

/// <summary>
/// Link between an account and an external provider (Google / Microsoft).
/// ProviderKey is the provider's stable user id ("sub"), not the email.
/// </summary>
public class ExternalLogin
{
    public int Id { get; set; }
    [MaxLength(50)] public string Provider { get; set; } = "";
    [MaxLength(200)] public string ProviderKey { get; set; } = "";
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Confirm the local password to attach Google / Microsoft to an existing account.</summary>
public class LinkAccountViewModel
{
    public string Provider { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
}
