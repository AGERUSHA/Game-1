using System.Security.Claims;
using Game1.Data;
using Game1.Models;
using Game1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Game1.Controllers;

/// <summary>
/// Real sign-in / sign-up backed by SQL Server.
/// Passwords are hashed with PasswordHasher (PBKDF2 + salt) and never stored in plain text.
/// </summary>
public class AccountController(AppDbContext db, IPasswordHasher<User> hasher, LangService L, IAuthenticationSchemeProvider schemes,
    EmailSender emailSender, PasswordResetTokens resetTokens, EmailCodes emailCodes, ILogger<AccountController> log, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewData["DevLogin"] = DevLoginAllowed;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ------------------------------------------------------------ Quick sign-in for development

    /// <summary>
    /// Password-less sign-in as a demo student / teacher / admin.
    /// Only in the Development environment AND only for requests from this computer (loopback).
    /// The demo accounts have no password, so nobody can sign in to them from elsewhere.
    /// A tunnel (cloudflared, ngrok…) connects from loopback too, but adds forwarding headers — those requests are refused.
    /// </summary>
    private bool DevLoginAllowed =>
        env.IsDevelopment() && HttpContext.Connection.RemoteIpAddress is { } ip && System.Net.IPAddress.IsLoopback(ip)
        && !Request.Headers.ContainsKey("X-Forwarded-For") && !Request.Headers.ContainsKey("Cf-Connecting-IP")
        && !Request.Headers.ContainsKey("X-Forwarded-Host") && !Request.Headers.ContainsKey("Forwarded");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DevLogin(string role)
    {
        if (!DevLoginAllowed) return NotFound();
        if (role is not ("student" or "teacher" or "admin")) return BadRequest();

        var email = $"dev.{role}@eduai.local";
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Role = role,
                FirstName = role switch { "teacher" => "Демо", "admin" => "Демо", _ => "Акерке" },
                LastName = role switch { "teacher" => "Преподаватель", "admin" => "Админ", _ => "Студент" },
            };
            db.Users.Add(user);
        }
        user.Role = role;
        user.Status = "active";
        user.EmailConfirmed = true;
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await SignInAsync(user, rememberMe: true);
        return RedirectToDashboard(role);
    }

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return LoginFailed(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null) return LoginFailed(model);

        if (PasswordGuard.IsLocked(user))
        {
            var minutes = PasswordGuard.MinutesLeft(user);
            ViewData["Error"] = L.T($"Слишком много неверных попыток. Подождите {minutes} мин. или восстановите пароль.",
                $"Қате әрекеттер тым көп. {minutes} мин. күтіңіз немесе құпиясөзді қалпына келтіріңіз.",
                $"Too many wrong attempts. Wait {minutes} min or reset your password.");
            model.Password = "";
            return View(model);
        }

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            PasswordGuard.Failed(user);
            await db.SaveChangesAsync();
            return LoginFailed(model);
        }
        PasswordGuard.Reset(user);

        if (user.Status == "blocked")
        {
            await db.SaveChangesAsync();
            ViewData["Error"] = InactiveMessage(user);
            model.Password = "";
            return View(model);
        }

        // Upgrade the hash if hashing parameters became stronger since it was stored.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, model.Password);
        }

        // Right password, but the address was never confirmed → send a code instead of signing in.
        if (!user.EmailConfirmed)
        {
            await db.SaveChangesAsync();
            if (!await emailCodes.SentRecentlyAsync(user, EmailCode.Confirm)) await SendCodeAsync(user, EmailCode.Confirm);
            return RedirectToAction(nameof(ConfirmEmail), new { email = user.Email, returnUrl = model.ReturnUrl });
        }

        if (user.Status != "active")   // teacher waiting for approval
        {
            await db.SaveChangesAsync();
            ViewData["Error"] = InactiveMessage(user);
            model.Password = "";
            return View(model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await SignInAsync(user, model.RememberMe);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);
        return RedirectToDashboard(user.Role);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role is not ("student" or "teacher")) model.Role = "student";

        if (!ModelState.IsValid)
        {
            ViewData["Error"] = model.Password != model.ConfirmPassword
                ? L.T("Пароли не совпадают.", "Құпиясөздер сәйкес емес.", "Passwords do not match.")
                : L.T("Проверьте поля: все обязательны, пароль — минимум 8 символов.", "Өрістерді тексеріңіз: бәрі міндетті, құпиясөз — кемінде 8 таңба.", "Check the fields: all are required, password must be at least 8 characters.");
            return View(ClearPasswords(model));
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is { EmailConfirmed: true } or { Status: "blocked" })
        {
            ViewData["Error"] = L.T("Этот email уже зарегистрирован.", "Бұл email тіркелген.", "This email is already registered.");
            return View(ClearPasswords(model));
        }

        // An unconfirmed account was never proven to belong to anyone (someone may have typed
        // another person's address) — whoever signs up with the email now takes it over.
        if (user == null) db.Users.Add(user = new User { Email = email });
        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Role = model.Role;
        // Anyone could pick "teacher" — an administrator approves the account first (Admin → Users).
        user.Status = model.Role == "teacher" ? "pending" : "active";
        user.PasswordHash = hasher.HashPassword(user, model.Password);
        PasswordGuard.Reset(user);
        await db.SaveChangesAsync();

        await SendCodeAsync(user, EmailCode.Confirm);
        return RedirectToAction(nameof(ConfirmEmail), new { email });
    }

    // ------------------------------------------------------------ Confirm email

    [HttpGet]
    public IActionResult ConfirmEmail(string? email, string? returnUrl)
    {
        ViewData["Email"] = email?.Trim().ToLowerInvariant();
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>The 6-digit code from the sign-up email → account confirmed and signed in.</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ConfirmEmail(string? email, string? code, string? returnUrl)
    {
        var addr = email?.Trim().ToLowerInvariant() ?? "";
        var user = addr.Length > 0 ? await db.Users.SingleOrDefaultAsync(u => u.Email == addr) : null;
        if (user is { EmailConfirmed: true })
        {
            TempData["Success"] = L.T("Email уже подтверждён — войдите.", "Email расталған — кіріңіз.", "Email already confirmed — please sign in.");
            return RedirectToAction(nameof(Login));
        }
        if (user is not { Status: "active" or "pending" } || !await emailCodes.VerifyAsync(user, EmailCode.Confirm, code))
        {
            ViewData["Email"] = addr;
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Error"] = L.T("Неверный или устаревший код. Проверьте письмо или отправьте новый.", "Код қате немесе ескірген. Хатты тексеріңіз немесе жаңасын жіберіңіз.", "Wrong or expired code. Check the email or send a new one.");
            return View();
        }

        var firstVisit = user.LastLoginAt == null;
        user.EmailConfirmed = true;
        if (user.Status == "pending")
        {
            await db.SaveChangesAsync();
            TempData["Success"] = L.T("Email подтверждён. Кабинет преподавателя откроется после одобрения администратором — мы напишем на почту.",
                "Email расталды. Оқытушы кабинеті әкімші мақұлдағаннан кейін ашылады — поштаңызға хабарлаймыз.",
                "Email confirmed. The teacher account opens once an administrator approves it — we'll email you.");
            return RedirectToAction(nameof(Login));
        }
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await SignInAsync(user, rememberMe: true);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return firstVisit && user.Role == "student" ? RedirectToAction(nameof(Welcome)) : RedirectToDashboard(user.Role);
    }

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendConfirmation(string? email, string? returnUrl)
    {
        var addr = email?.Trim().ToLowerInvariant() ?? "";
        var user = addr.Length > 0 ? await db.Users.SingleOrDefaultAsync(u => u.Email == addr) : null;
        if (user is { EmailConfirmed: false, Status: "active" or "pending" } && !await emailCodes.SentRecentlyAsync(user, EmailCode.Confirm))
            await SendCodeAsync(user, EmailCode.Confirm);
        TempData["Success"] = L.T("Новый код отправлен (не чаще раза в минуту).", "Жаңа код жіберілді (минутына бір реттен жиі емес).", "A new code was sent (at most once a minute).");
        return RedirectToAction(nameof(ConfirmEmail), new { email = addr, returnUrl });
    }

    /// <summary>First-run onboarding for new students.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Welcome() => View();

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    // ------------------------------------------------------------ Forgot password

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(string? email)
    {
        var addr = email?.Trim().ToLowerInvariant() ?? "";
        var user = addr.Length > 0 ? await db.Users.SingleOrDefaultAsync(u => u.Email == addr && u.Status != "blocked") : null;
        if (user != null && !await emailCodes.SentRecentlyAsync(user, EmailCode.Reset))
        {
            var link = Url.Action(nameof(ResetPassword), "Account", new { token = resetTokens.Create(user) }, Request.Scheme)!;
            await SendCodeAsync(user, EmailCode.Reset, link);
        }
        // Same answer whether or not the email exists — don't reveal who is registered.
        ViewData["Sent"] = true;
        ViewData["Email"] = addr;
        return View();
    }

    /// <summary>The 6-digit code from the email → the same "new password" page as the link.</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyResetCode(string? email, string? code)
    {
        var addr = email?.Trim().ToLowerInvariant() ?? "";
        var user = addr.Length > 0 ? await db.Users.SingleOrDefaultAsync(u => u.Email == addr && u.Status != "blocked") : null;
        if (user == null || !await emailCodes.VerifyAsync(user, EmailCode.Reset, code))
        {
            ViewData["Sent"] = true;
            ViewData["Email"] = addr;
            ViewData["Error"] = L.T("Неверный или устаревший код. Проверьте письмо или запросите новый.", "Код қате немесе ескірген. Хатты тексеріңіз немесе жаңасын сұраңыз.", "Wrong or expired code. Check the email or request a new one.");
            return View(nameof(ForgotPassword));
        }
        return RedirectToAction(nameof(ResetPassword), new { token = resetTokens.Create(user) });
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token)
    {
        ViewData["Token"] = token;
        if (resetTokens.Read(token) == null)
            ViewData["Error"] = L.T("Ссылка недействительна или устарела. Запросите новую.", "Сілтеме жарамсыз немесе ескірген. Жаңасын сұраңыз.", "This link is invalid or expired. Request a new one.");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(string? token, string? newPassword, string? confirmPassword)
    {
        ViewData["Token"] = token;
        var data = resetTokens.Read(token);
        var user = data is { } d ? await db.Users.FindAsync(d.UserId) : null;
        if (user == null || !PasswordResetTokens.Matches(user, data!.Value.Stamp))
        {
            ViewData["Error"] = L.T("Ссылка недействительна или уже использована. Запросите новую.", "Сілтеме жарамсыз немесе қолданылған. Жаңасын сұраңыз.", "This link is invalid or already used. Request a new one.");
            return View();
        }
        if ((newPassword?.Length ?? 0) < 8 || newPassword != confirmPassword)
        {
            ViewData["Error"] = (newPassword?.Length ?? 0) < 8
                ? L.T("Пароль — минимум 8 символов.", "Құпиясөз — кемінде 8 таңба.", "Password must be at least 8 characters.")
                : L.T("Пароли не совпадают.", "Құпиясөздер сәйкес емес.", "Passwords do not match.");
            return View();
        }

        user.PasswordHash = hasher.HashPassword(user, newPassword!);   // also invalidates this link
        user.EmailConfirmed = true;   // the link / code came by email, so the address is proven
        PasswordGuard.Reset(user);
        await db.SaveChangesAsync();
        await db.EmailCodes.Where(c => c.UserId == user.Id && c.Purpose == EmailCode.Reset).ExecuteDeleteAsync();
        TempData["Success"] = L.T("Пароль изменён. Войдите с новым паролем.", "Құпиясөз өзгертілді. Жаңа құпиясөзбен кіріңіз.", "Password changed. Sign in with your new password.");
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Emails a fresh 6-digit code (sign-up confirmation or password reset; the reset one also carries the link).</summary>
    private async Task SendCodeAsync(User user, string purpose, string? link = null)
    {
        var code = await emailCodes.CreateAsync(user, purpose);
        var minutes = (int)EmailCodes.Lifetime(purpose).TotalMinutes;
        var confirm = purpose == EmailCode.Confirm;

        var subject = confirm
            ? L.T("Код подтверждения EDU AI", "EDU AI растау коды", "EDU AI confirmation code")
            : L.T("Код восстановления пароля EDU AI", "EDU AI құпиясөзін қалпына келтіру коды", "EDU AI password reset code");
        var hello = $"{L.T("Здравствуйте", "Сәлеметсіз бе", "Hello")}, {user.FirstName}!";
        var intro = confirm
            ? L.T($"Ваш код подтверждения email для EDU AI (действует {minutes} минут):", $"EDU AI-да email растау кодыңыз ({minutes} минут жарамды):", $"Your EDU AI email confirmation code (valid for {minutes} minutes):")
            : L.T($"Ваш код для восстановления пароля EDU AI (действует {minutes} минут):", $"EDU AI құпиясөзін қалпына келтіру кодыңыз ({minutes} минут жарамды):", $"Your EDU AI password reset code (valid for {minutes} minutes):");
        var linkLine = L.T("Или просто перейдите по ссылке (действует 1 час):", "Немесе сілтемеге өтіңіз (1 сағат жарамды):", "Or just open this link (valid for 1 hour):");
        var ignore = (confirm
            ? L.T("Если вы не регистрировались в EDU AI — просто проигнорируйте письмо.", "Егер сіз EDU AI-да тіркелмесеңіз — хатты елемеңіз.", "If you didn't sign up for EDU AI, just ignore this email.")
            : L.T("Если вы не запрашивали сброс — просто проигнорируйте письмо.", "Егер сіз қалпына келтіруді сұрамасаңыз — хатты елемеңіз.", "If you didn't request this, just ignore this email."))
            + " " + L.T("Никому не сообщайте этот код.", "Бұл кодты ешкімге айтпаңыз.", "Never share this code.");

        static string E(string s) => System.Net.WebUtility.HtmlEncode(s);
        var linkHtml = link == null ? "" : $@"<p>{E(linkLine)}<br><a href=""{E(link)}"">{E(link)}</a></p>";
        var html = $@"<div style=""font-family:Arial,sans-serif;font-size:15px;color:#1f2937"">
<p>{E(hello)}</p>
<p>{E(intro)}</p>
<p style=""font-size:32px;font-weight:700;letter-spacing:8px;margin:16px 0"">{code}</p>
{linkHtml}
<p style=""color:#6b7280"">{E(ignore)}</p>
</div>";
        var linkText = link == null ? "" : $"{linkLine}\n{link}\n\n";
        var text = $"{hello}\n\n{intro}\n\n    {code}\n\n{linkText}{ignore}\n\n— EDU AI";

        try { await emailSender.SendAsync(user.Email, subject, html, text); }
        catch (Exception ex) { log.LogError(ex, "Email ({Purpose}) to {Email} failed", purpose, user.Email); }
    }

    // ------------------------------------------------------------ Google / Microsoft

    /// <summary>Redirects to the provider's sign-in page.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl)
    {
        if (provider is not (AuthSchemes.Google or AuthSchemes.Microsoft) || await schemes.GetSchemeAsync(provider) == null)
        {
            TempData["Error"] = L.T("Этот способ входа ещё не настроен.", "Бұл кіру тәсілі әлі бапталмаған.", "This sign-in method is not configured yet.");
            return RedirectToAction(nameof(Login));
        }
        var redirect = Url.Action(nameof(ExternalCallback), new { returnUrl });
        return Challenge(new AuthenticationProperties { RedirectUri = redirect }, provider);
    }

    /// <summary>Provider sends the user back here: find the linked account, or create one.</summary>
    public async Task<IActionResult> ExternalCallback(string? returnUrl)
    {
        var ext = await ReadExternalAsync();
        if (ext == null) return ExternalFailed();

        // 1) Already linked → sign in.
        var link = await db.ExternalLogins.Include(l => l.User)
            .SingleOrDefaultAsync(l => l.Provider == ext.Provider && l.ProviderKey == ext.Key);
        if (link != null) return await FinishExternalAsync(link.User, returnUrl);

        if (string.IsNullOrEmpty(ext.Email))
            return ExternalFailed(L.T("Провайдер не передал email.", "Провайдер email жібермеді.", "The provider did not share an email address."));

        // 2) An account with this email already exists → owner must confirm with their password
        //    (prevents taking over an account just by having a provider identity with the same email).
        if (await db.Users.AnyAsync(u => u.Email == ext.Email))
            return View("LinkAccount", new LinkAccountViewModel { Provider = ext.Provider, Email = ext.Email, ReturnUrl = returnUrl });

        // 3) New person → create a student account without a local password.
        var user = new User
        {
            FirstName = ext.FirstName,
            LastName = ext.LastName,
            Email = ext.Email,
            Role = "student",
            EmailConfirmed = true,   // the provider has verified the address
        };
        db.Users.Add(user);
        db.ExternalLogins.Add(new ExternalLogin { Provider = ext.Provider, ProviderKey = ext.Key, User = user });
        await db.SaveChangesAsync();
        return await FinishExternalAsync(user, Url.IsLocalUrl(returnUrl) ? returnUrl : "/Account/Welcome");
    }

    /// <summary>Links Google / Microsoft to an existing account after its password is confirmed.</summary>
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> LinkAccount(LinkAccountViewModel model)
    {
        var ext = await ReadExternalAsync();
        if (ext == null || string.IsNullOrEmpty(ext.Email)) return ExternalFailed();

        model.Provider = ext.Provider;
        model.Email = ext.Email;
        var user = await db.Users.SingleAsync(u => u.Email == ext.Email);

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            ViewData["Error"] = L.T("У этого аккаунта нет пароля — войдите тем способом, которым регистрировались.", "Бұл аккаунттың құпиясөзі жоқ — тіркелген тәсілмен кіріңіз.", "This account has no password — sign in the way you registered.");
            return View(model);
        }
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password ?? "") == PasswordVerificationResult.Failed)
        {
            ViewData["Error"] = L.T("Неверный пароль.", "Құпиясөз қате.", "Wrong password.");
            model.Password = "";
            return View(model);
        }

        if (!await db.ExternalLogins.AnyAsync(l => l.UserId == user.Id && l.Provider == ext.Provider))
            db.ExternalLogins.Add(new ExternalLogin { Provider = ext.Provider, ProviderKey = ext.Key, UserId = user.Id });
        user.EmailConfirmed = true;   // knows the password AND owns the address at the provider
        await db.SaveChangesAsync();
        return await FinishExternalAsync(user, model.ReturnUrl);
    }

    private record ExternalIdentity(string Provider, string Key, string? Email, string FirstName, string LastName);

    private async Task<ExternalIdentity?> ReadExternalAsync()
    {
        var result = await HttpContext.AuthenticateAsync(AuthSchemes.External);
        var id = result.Principal?.FindFirst(ClaimTypes.NameIdentifier);
        if (!result.Succeeded || id == null) return null;

        // Claim issuer = the scheme that produced it ("Google" / "Microsoft") — set by the handler, not the user.
        var p = result.Principal!;
        var full = p.FindFirstValue(ClaimTypes.Name) ?? "";
        var first = p.FindFirstValue(ClaimTypes.GivenName) ?? full.Split(' ').FirstOrDefault() ?? "";
        var last = p.FindFirstValue(ClaimTypes.Surname) ?? string.Join(' ', full.Split(' ').Skip(1));
        return new ExternalIdentity(id.Issuer, id.Value, p.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant(), first, last);
    }

    private async Task<IActionResult> FinishExternalAsync(User user, string? returnUrl)
    {
        await HttpContext.SignOutAsync(AuthSchemes.External);
        if (user.Status != "active")
        {
            TempData["Error"] = InactiveMessage(user);
            return RedirectToAction(nameof(Login));
        }
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await SignInAsync(user, rememberMe: true);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToDashboard(user.Role);
    }

    private IActionResult ExternalFailed(string? message = null)
    {
        TempData["Error"] = message ?? L.T("Не удалось войти через внешний сервис. Попробуйте ещё раз.", "Сыртқы қызмет арқылы кіру сәтсіз аяқталды. Қайталап көріңіз.", "External sign-in failed. Please try again.");
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Signed in, but the page belongs to another role → back to own dashboard.</summary>
    public IActionResult Denied() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToDashboard(User.FindFirstValue(ClaimTypes.Role) ?? "student")
            : RedirectToAction(nameof(Login));

    /// <summary>Why a signed-in attempt is refused: blocked, or a teacher not approved yet.</summary>
    private string InactiveMessage(User user) => user.Status == "pending"
        ? L.T("Аккаунт преподавателя ждёт одобрения администратора. Мы напишем на почту, когда его откроют.",
              "Оқытушы аккаунты әкімшінің мақұлдауын күтуде. Ашылғанда поштаңызға хабарлаймыз.",
              "Your teacher account is waiting for administrator approval. We'll email you when it's open.")
        : L.T("Аккаунт заблокирован. Обратитесь к администратору.", "Аккаунт бұғатталған. Әкімшіге хабарласыңыз.", "Account is blocked. Contact the administrator.");

    private IActionResult LoginFailed(LoginViewModel model)
    {
        // Same message for unknown email and wrong password — don't reveal which one.
        ViewData["Error"] = L.T("Неверный email или пароль.", "Email немесе құпиясөз қате.", "Invalid email or password.");
        model.Password = "";
        return View(model);
    }

    private static RegisterViewModel ClearPasswords(RegisterViewModel model)
    {
        model.Password = model.ConfirmPassword = "";
        return model;
    }

    private Task SignInAsync(User user, bool rememberMe) => UserSession.SignInAsync(HttpContext, user, rememberMe);

    private IActionResult RedirectToDashboard(string role) => role switch
    {
        "teacher" => Redirect("/Teacher/Dashboard"),
        "admin" => Redirect("/Admin/Dashboard"),
        _ => Redirect("/Student/Dashboard"),
    };
}
