using System.Security.Claims;
using Game1.Data;
using Game1.Models;
using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game1.Controllers;

/// <summary>Admin area. Users are managed in SQL Server; the rest is still demo UI.</summary>
[Authorize(Roles = "admin")]
public class AdminController(IEduDataService data, AppDbContext db, IPasswordHasher<User> hasher, LangService L,
    EmailSender emailSender, AssignmentStore assignments, ILogger<AdminController> log) : Controller
{
    private static readonly string[] AllowedRoles = ["student", "teacher", "admin"];

    private int MyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    public IActionResult Dashboard() => View(data);

    public IActionResult Users() => View(data.GetUsers());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!AllowedRoles.Contains(model.Role)) model.Role = "student";
        if (!ModelState.IsValid)
            return Flash(L.T("Заполните все поля, пароль — минимум 8 символов.", "Барлық өрістерді толтырыңыз, құпиясөз — кемінде 8 таңба.", "Fill in all fields; password must be at least 8 characters."), error: true);

        var email = model.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Flash(L.T("Этот email уже зарегистрирован.", "Бұл email тіркелген.", "This email is already registered."), error: true);

        var user = new User { FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(), Email = email, Role = model.Role, EmailConfirmed = true };   // created by an admin → active, no approval
        user.PasswordHash = hasher.HashPassword(user, model.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Flash(L.T("Пользователь создан.", "Пайдаланушы жасалды.", "User created."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int id, string role)
    {
        if (id == MyId)
            return Flash(L.T("Нельзя изменить свою роль.", "Өз рөліңізді өзгертуге болмайды.", "You can't change your own role."), error: true);
        if (!AllowedRoles.Contains(role)) return BadRequest();

        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Role = role;
        await db.SaveChangesAsync();
        return Flash(L.T("Роль изменена.", "Рөл өзгертілді.", "Role updated."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        if (id == MyId)
            return Flash(L.T("Нельзя заблокировать себя.", "Өзіңізді бұғаттауға болмайды.", "You can't block yourself."), error: true);

        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Status = user.Status == "blocked" ? "active" : "blocked";
        await db.SaveChangesAsync();
        return Flash(user.Status == "blocked"
            ? L.T("Пользователь заблокирован.", "Пайдаланушы бұғатталды.", "User blocked.")
            : L.T("Пользователь разблокирован.", "Пайдаланушы бұғаттан шығарылды.", "User unblocked."));
    }

    /// <summary>A self-registered teacher → active; the teacher is told by email.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Status != "pending") return Flash(L.T("Пользователь не ждёт одобрения.", "Пайдаланушы мақұлдауды күтпейді.", "This user isn't waiting for approval."), error: true);
        user.Status = "active";
        await db.SaveChangesAsync();

        var login = Url.Action("Login", "Account", null, Request.Scheme)!;
        var hello = $"{L.T("Здравствуйте", "Сәлеметсіз бе", "Hello")}, {user.FirstName}!";
        var text = L.T("Администратор одобрил ваш аккаунт преподавателя EDU AI. Теперь можно войти:", "Әкімші EDU AI оқытушы аккаунтыңызды мақұлдады. Енді кіре аласыз:", "An administrator approved your EDU AI teacher account. You can sign in now:");
        static string E(string s) => System.Net.WebUtility.HtmlEncode(s);
        try
        {
            await emailSender.SendAsync(user.Email, L.T("Аккаунт преподавателя одобрен", "Оқытушы аккаунты мақұлданды", "Teacher account approved"),
                $@"<div style=""font-family:Arial,sans-serif;font-size:15px;color:#1f2937""><p>{E(hello)}</p><p>{E(text)}</p><p><a href=""{E(login)}"">{E(login)}</a></p></div>",
                $"{hello}\n\n{text}\n{login}\n\n— EDU AI");
        }
        catch (Exception ex) { log.LogError(ex, "Approval email to {Email} failed", user.Email); }

        return Flash(L.T($"{user.Email} одобрен — кабинет преподавателя открыт.", $"{user.Email} мақұлданды — оқытушы кабинеті ашылды.", $"{user.Email} approved — the teacher account is open."));
    }

    /// <summary>The user can't get the code (typo in the mail server, spam filter…) — the admin vouches for the address.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.EmailConfirmed = true;
        await db.SaveChangesAsync();
        await db.EmailCodes.Where(c => c.UserId == id && c.Purpose == EmailCode.Confirm).ExecuteDeleteAsync();
        return Flash(L.T($"Email {user.Email} подтверждён — пользователь может войти.", $"{user.Email} расталды — пайдаланушы кіре алады.", $"{user.Email} confirmed — the user can sign in now."));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id == MyId)
            return Flash(L.T("Нельзя удалить себя.", "Өзіңізді жоюға болмайды.", "You can't delete yourself."), error: true);

        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        await assignments.DeleteUserFilesAsync(user.Id);
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return Flash(L.T("Пользователь удалён.", "Пайдаланушы жойылды.", "User deleted."));
    }

    public IActionResult Roles() => View();

    public IActionResult Import() => View();

    public IActionResult Audit() => View(data.GetUsers());

    public IActionResult Center() => View(data);

    public IActionResult Courses() => View(data.GetCourses());

    public IActionResult Settings() => View();

    private IActionResult Flash(string message, bool error = false)
    {
        TempData[error ? "Error" : "Success"] = message;
        return RedirectToAction(nameof(Users));
    }
}
