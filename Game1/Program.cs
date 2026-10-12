using System.Text.Encodings.Web;
using System.Text.Unicode;
using Game1.Data;
using Game1.Models;
using Game1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Output Cyrillic / Kazakh text as-is instead of HTML entities.
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// UI language (KZ / RU / EN) — stored in a cookie, RU by default.
builder.Services.AddScoped<LangService>();

// Users / profile from SQL Server; course content is still mock data (see SqlEduDataService).
builder.Services.AddScoped<IEduDataService, SqlEduDataService>();

// Game zone catalog (hub cards, game of the day).
builder.Services.AddScoped<GameCatalog>();

// SQL Server: users / accounts.
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Passwords are stored only as salted PBKDF2 hashes.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// "Forgot password": signed one-hour links, sent by email (SMTP from user-secrets).
builder.Services.AddSingleton<PasswordResetTokens>();
builder.Services.AddScoped<EmailSender>();

// Cookie-based sign-in.
var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
auth.AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/Denied";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromDays(14);
        o.SlidingExpiration = true;

        // On every request re-check the account in the database:
        // a blocked / deleted user, or one whose role was changed, is signed out immediately.
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var id = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var role = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var user = int.TryParse(id, out var uid) ? await db.Users.FindAsync(uid) : null;
            if (user == null || user.Status != "active" || user.Role != role)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

// Google / Microsoft sign-in. Each provider is enabled only when its keys are configured
// (user-secrets: Authentication:Google:ClientId / ClientSecret, Authentication:Microsoft:ClientId / ClientSecret).
// The provider result goes to a short-lived "External" cookie; AccountController then maps it to a local account.
auth.AddCookie(AuthSchemes.External, o =>
{
    o.Cookie.Name = ".Game1.External";
    o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
});
if (builder.Configuration["Authentication:Google:ClientId"] is { Length: > 0 } googleId)
    auth.AddGoogle(o =>
    {
        o.SignInScheme = AuthSchemes.External;
        o.ClientId = googleId;
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        UseHttpFriendlyCorrelationCookie(o);
    });
if (builder.Configuration["Authentication:Microsoft:ClientId"] is { Length: > 0 } msId)
    auth.AddMicrosoftAccount(o =>
    {
        o.SignInScheme = AuthSchemes.External;
        o.ClientId = msId;
        o.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        UseHttpFriendlyCorrelationCookie(o);
    });

var app = builder.Build();

// The provider redirects back with a top-level GET, so a Lax cookie is sent; the default
// (SameSite=None + Secure) is dropped by browsers on plain http://localhost → "Correlation failed".
static void UseHttpFriendlyCorrelationCookie(Microsoft.AspNetCore.Authentication.RemoteAuthenticationOptions o)
{
    o.CorrelationCookie.SameSite = SameSiteMode.Lax;
    o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
}

// Create / upgrade the database (EF migrations) and make sure an administrator exists.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Admin credentials come from user-secrets / environment (SeedAdmin:Email, SeedAdmin:Password),
    // never from source code.
    var adminEmail = builder.Configuration["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
    var adminPassword = builder.Configuration["SeedAdmin:Password"];
    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword)
        && !db.Users.Any(u => u.Role == "admin"))
    {
        var admin = new User { FirstName = "Admin", LastName = "", Email = adminEmail, Role = "admin" };
        admin.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>().HashPassword(admin, adminPassword);
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Friendly 404 / 403 pages (Views/Shared/Error.cshtml).
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
