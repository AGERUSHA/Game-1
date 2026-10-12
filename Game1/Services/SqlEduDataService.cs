using System.Security.Claims;
using Game1.Data;
using Game1.Models;

namespace Game1.Services;

/// <summary>
/// SQL Server–backed data: users / accounts / current profile come from the database.
/// Course content (courses, tests, XP, …) is still served by <see cref="MockEduDataService"/>
/// until those tables exist.
/// </summary>
public class SqlEduDataService(LangService l, AppDbContext db, IHttpContextAccessor http) : MockEduDataService(l)
{
    private ClaimsPrincipal? Principal => http.HttpContext?.User;

    /// <summary>Signed-in account, or null for anonymous visitors.</summary>
    private User? CurrentUser =>
        int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? db.Users.Find(id) : null;

    // Real name / email on top of the demo stats.
    public override UserProfile GetStudent() => WithAccount(base.GetStudent());
    public override UserProfile GetTeacher() => WithAccount(base.GetTeacher());

    private UserProfile WithAccount(UserProfile p) =>
        CurrentUser is { } u
            ? p with
            {
                FirstName = u.FirstName, LastName = u.LastName, Email = u.Email, AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                RoleTitle = u.Role switch
                {
                    "teacher" => T("Преподаватель", "Оқытушы", "Teacher"),
                    "admin" => T("Администратор", "Әкімші", "Administrator"),
                    _ => T("Студент", "Студент", "Student"),
                },
            }
            : p;

    public override List<AdminUser> GetUsers() =>
        db.Users.OrderByDescending(u => u.CreatedAt).ToList()
            .Select(u => new AdminUser(
                $"{u.FirstName} {u.LastName}".Trim(), u.Email, u.Role, u.Status,
                u.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy"),
                u.LastLoginAt is { } t ? t.ToLocalTime().ToString("dd.MM.yyyy HH:mm") : T("Не входил", "Кірмеген", "Never"),
                u.Id))
            .ToList();

    public override ChartSpec GetRoleDistribution()
    {
        var counts = db.Users.GroupBy(u => u.Role).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        int C(string r) => counts.GetValueOrDefault(r);
        return new("doughnut",
            [T("Студенты", "Студенттер", "Students"), T("Преподаватели", "Оқытушылар", "Teachers"), T("Администраторы", "Әкімшілер", "Admins")],
            [new(T("Пользователи", "Пайдаланушылар", "Users"), [C("student"), C("teacher"), C("admin")])]);
    }
}
