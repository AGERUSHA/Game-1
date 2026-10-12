using System.Security.Claims;
using Game1.Data;
using Game1.Models;
using Game1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game1.Controllers;

/// <summary>Account settings: profile, photo, password, delete account — saved to SQL Server.</summary>
[Authorize]
public class SettingsController(IEduDataService data, AppDbContext db, IPasswordHasher<User> hasher,
    LangService L, IWebHostEnvironment env) : Controller
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    public async Task<IActionResult> Index()
    {
        var user = await CurrentUserAsync();
        ViewData["HasPassword"] = !string.IsNullOrEmpty(user?.PasswordHash);
        ViewData["Providers"] = user == null ? new List<string>()
            : await db.ExternalLogins.Where(l => l.UserId == user.Id).Select(l => l.Provider).ToListAsync();
        return View(data.GetStudent());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(string? firstName, string? lastName)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        firstName = firstName?.Trim() ?? "";
        lastName = lastName?.Trim() ?? "";
        if (firstName.Length is 0 or > 100 || lastName.Length > 100)
            return Done("profile", L.T("Укажите имя (до 100 символов).", "Атыңызды көрсетіңіз (100 таңбаға дейін).", "Enter your first name (up to 100 characters)."), error: true);

        user.FirstName = firstName;
        user.LastName = lastName;
        await db.SaveChangesAsync();
        await UserSession.RefreshAsync(HttpContext, user);   // new name in the top bar right away
        return Done("profile", L.T("Профиль сохранён.", "Профиль сақталды.", "Profile saved."));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxAvatarBytes + 64 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? photo)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        var badFile = L.T("Загрузите изображение JPG, PNG или WebP до 2 МБ.", "2 МБ-қа дейінгі JPG, PNG немесе WebP суретін жүктеңіз.", "Upload a JPG, PNG or WebP image up to 2 MB.");
        if (photo == null || photo.Length == 0 || photo.Length > MaxAvatarBytes) return Done("profile", badFile, error: true);

        // Trust the file's actual bytes, not its name or the browser's content type.
        await using var ms = new MemoryStream();
        await photo.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var ext = DetectImage(bytes);
        if (ext == null) return Done("profile", badFile, error: true);

        var dir = Path.Combine(env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(dir);
        var fileName = $"{user.Id}-{Guid.NewGuid():N}{ext}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(dir, fileName), bytes);

        DeleteAvatarFile(user.AvatarUrl);
        user.AvatarUrl = $"/uploads/avatars/{fileName}";
        await db.SaveChangesAsync();
        await UserSession.RefreshAsync(HttpContext, user);
        return Done("profile", L.T("Фото обновлено.", "Фото жаңартылды.", "Photo updated."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAvatar()
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();
        DeleteAvatarFile(user.AvatarUrl);
        user.AvatarUrl = null;
        await db.SaveChangesAsync();
        await UserSession.RefreshAsync(HttpContext, user);
        return Done("profile", L.T("Фото удалено.", "Фото жойылды.", "Photo removed."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string? currentPassword, string? newPassword, string? confirmPassword)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        // Accounts created via Google / Microsoft have no password yet — they may set one without the current.
        if (!string.IsNullOrEmpty(user.PasswordHash) &&
            hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword ?? "") == PasswordVerificationResult.Failed)
            return Done("security", L.T("Текущий пароль неверный.", "Ағымдағы құпиясөз қате.", "Current password is incorrect."), error: true);
        if ((newPassword?.Length ?? 0) < 8)
            return Done("security", L.T("Новый пароль — минимум 8 символов.", "Жаңа құпиясөз — кемінде 8 таңба.", "New password must be at least 8 characters."), error: true);
        if (newPassword != confirmPassword)
            return Done("security", L.T("Пароли не совпадают.", "Құпиясөздер сәйкес емес.", "Passwords do not match."), error: true);

        user.PasswordHash = hasher.HashPassword(user, newPassword!);
        await db.SaveChangesAsync();
        return Done("security", L.T("Пароль обновлён.", "Құпиясөз жаңартылды.", "Password updated."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string? password)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        if (!string.IsNullOrEmpty(user.PasswordHash) &&
            hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? "") == PasswordVerificationResult.Failed)
            return Done("security", L.T("Неверный пароль — аккаунт не удалён.", "Құпиясөз қате — аккаунт жойылмады.", "Wrong password — account not deleted."), error: true);
        if (user.Role == "admin" && await db.Users.CountAsync(u => u.Role == "admin" && u.Status == "active") <= 1)
            return Done("security", L.T("Нельзя удалить последнего администратора.", "Соңғы әкімшіні жоюға болмайды.", "The last administrator cannot be deleted."), error: true);

        DeleteAvatarFile(user.AvatarUrl);
        db.Users.Remove(user);   // external logins are removed by cascade
        await db.SaveChangesAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private async Task<User?> CurrentUserAsync() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? await db.Users.FindAsync(id) : null;

    private IActionResult Done(string tab, string message, bool error = false)
    {
        TempData[error ? "Error" : "Success"] = message;
        TempData["Tab"] = tab;
        var back = Request.HasFormContentType ? Request.Form["returnUrl"].ToString() : "";
        return Url.IsLocalUrl(back) ? LocalRedirect(back) : RedirectToAction(nameof(Index));
    }

    /// <summary>Returns the file extension for JPEG / PNG / WebP by magic bytes, otherwise null.</summary>
    private static string? DetectImage(byte[] b)
    {
        if (b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
        if (b.Length > 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (b.Length > 12 && b.AsSpan(0, 4).SequenceEqual("RIFF"u8) && b.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }

    private void DeleteAvatarFile(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/avatars/")) return;
        var path = Path.Combine(env.WebRootPath, "uploads", "avatars", Path.GetFileName(url));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }
}
