using System.Security.Claims;
using Game1.Data;
using Game1.Models;
using Microsoft.EntityFrameworkCore;

namespace Game1.Services;

/// <summary>
/// SQL Server–backed data: accounts, the current profile, and course content
/// (courses, modules, lessons, materials) with the signed-in student's progress.
/// Everything else (tests, XP, league, …) is still served by <see cref="MockEduDataService"/>
/// until those tables exist.
/// </summary>
public class SqlEduDataService(LangService l, AppDbContext db, IHttpContextAccessor http) : MockEduDataService(l)
{
    private ClaimsPrincipal? Principal => http.HttpContext?.User;

    private int? UserId => int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>Signed-in account, or null for anonymous visitors.</summary>
    private User? CurrentUser => UserId is int id ? db.Users.Find(id) : null;

    // Real name / email / course counts on top of the demo stats.
    public override UserProfile GetStudent() => WithAccount(base.GetStudent());
    public override UserProfile GetTeacher() => WithAccount(base.GetTeacher());

    private UserProfile WithAccount(UserProfile p)
    {
        if (CurrentUser is not { } u) return p;
        var mine = GetCourses().Where(c => c.Enrolled).ToList();
        return p with
        {
            FirstName = u.FirstName, LastName = u.LastName, Email = u.Email, AvatarUrl = u.AvatarUrl,
            Role = u.Role,
            RoleTitle = u.Role switch
            {
                "teacher" => T("Преподаватель", "Оқытушы", "Teacher"),
                "admin" => T("Администратор", "Әкімші", "Administrator"),
                _ => T("Студент", "Студент", "Student"),
            },
            Courses = mine.Count,
            CompletedCourses = mine.Count(c => c.Progress == 100),
        };
    }

    // ------------------------------------------------------------------ courses

    public override List<string> GetCategories() =>
        [T("Все", "Барлығы", "All"), .. db.Courses.Where(c => c.Status == "published").Select(c => c.Category).Distinct().OrderBy(c => c).ToList()];

    /// <summary>Asked several times per page (sidebar, profile stats…) — read once per request.</summary>
    private List<Course>? catalog;

    public override List<Course> GetCourses() => catalog ??= Query(db.Courses.Where(c => c.Status == "published").OrderBy(c => c.Id));

    /// <summary>Published courses for everyone; drafts only for their author and admins.</summary>
    public override Course? GetCourse(int id)
    {
        var uid = UserId;
        var admin = Principal?.IsInRole("admin") == true;
        return Query(db.Courses.Where(c => c.Id == id && (c.Status == "published" || admin || (uid != null && c.TeacherId == uid)))).FirstOrDefault();
    }

    public override Course? GetContinueCourse()
    {
        var uid = UserId;
        var id = db.Enrollments.Where(e => e.UserId == uid && e.Course.Status == "published")
            .OrderByDescending(e => e.LastActivityAt).Select(e => (int?)e.CourseId).FirstOrDefault();
        return id is int cid ? GetCourse(cid) : null;
    }

    private List<Course> Query(IQueryable<CourseEntity> courses)
    {
        var uid = UserId;
        var rows = courses.Select(c => new
        {
            c,
            Lessons = c.Modules.SelectMany(m => m.Lessons).Count(),
            Students = db.Enrollments.Count(e => e.CourseId == c.Id),
            Rating = db.Enrollments.Where(e => e.CourseId == c.Id && e.Rating != null).Average(e => (double?)e.Rating),
            Done = db.LessonCompletions.Count(lc => lc.UserId == uid && lc.Lesson.Module.CourseId == c.Id),
            Mine = db.Enrollments.Where(e => e.UserId == uid && e.CourseId == c.Id).Select(e => new { e.Rating }).FirstOrDefault(),
        }).ToList();

        return rows.Select(r => new Course(
            r.c.Id, r.c.Title, r.c.Subtitle, r.c.Category, r.c.Description, r.c.TeacherName,
            r.Lessons, r.Lessons == 0 ? 0 : Math.Min(100, r.Done * 100 / r.Lessons), Math.Round(r.Rating ?? 0, 1), r.Students,
            r.c.Cover, r.c.Icon, T($"{r.c.Hours} ч", $"{r.c.Hours} сағ", $"{r.c.Hours} h"), r.c.Level, r.c.Status,
            Enrolled: r.Mine != null)
        {
            Outcomes = r.c.Outcomes.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            MyRating = r.Mine?.Rating,
        }).ToList();
    }

    /// <summary>
    /// Modules and lessons with the student's status: completed, the first unfinished one is "current",
    /// the rest are locked. Not enrolled → everything locked.
    /// </summary>
    public override List<CourseModule> GetCurriculum(int courseId)
    {
        var uid = UserId;
        var modules = db.Modules.Where(m => m.CourseId == courseId).OrderBy(m => m.Order)
            .Select(m => new { m.Title, Lessons = m.Lessons.OrderBy(x => x.Order).Select(x => new { x.Id, x.Title, x.Type, x.Duration }).ToList() })
            .ToList();
        var enrolled = db.Enrollments.Any(e => e.UserId == uid && e.CourseId == courseId);
        var done = db.LessonCompletions.Where(lc => lc.UserId == uid && lc.Lesson.Module.CourseId == courseId)
            .Select(lc => lc.LessonId).ToHashSet();

        var currentGiven = false;
        string Status(int id)
        {
            if (done.Contains(id)) return "completed";
            if (enrolled && !currentGiven) { currentGiven = true; return "current"; }
            return "locked";
        }
        return modules.Select(m => new CourseModule(m.Title,
            m.Lessons.Select(x => new Lesson(x.Title, x.Type, x.Duration, Status(x.Id), x.Id)).ToList())).ToList();
    }

    public override LessonPage? GetLesson(int lessonId)
    {
        var row = db.Lessons.Where(x => x.Id == lessonId)
            .Select(x => new { x.Body, x.VideoUrl, CourseId = x.Module.CourseId, Module = x.Module.Title }).FirstOrDefault();
        if (row == null || GetCourse(row.CourseId) is not { } course) return null;

        var flat = GetCurriculum(row.CourseId).SelectMany(m => m.Lessons).ToList();
        var i = flat.FindIndex(x => x.Id == lessonId);
        return new LessonPage(course, row.Module, flat[i], row.Body, VideoLinks.Embed(row.VideoUrl),
            i > 0 ? flat[i - 1].Id : null, i < flat.Count - 1 ? flat[i + 1].Id : null, i + 1, flat.Count);
    }

    // ------------------------------------------------------------------ materials

    public override List<Material> GetMaterials()
    {
        var uid = UserId;
        return db.Materials.Where(m => m.Course.Status == "published" && db.Enrollments.Any(e => e.UserId == uid && e.CourseId == m.CourseId))
            .OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id)
            .Select(m => new { m, Course = m.Course.Title, Mark = db.MaterialMarks.FirstOrDefault(k => k.UserId == uid && k.MaterialId == m.Id) })
            .ToList()
            .Select(r => ToMaterial(r.m, r.Course, r.Mark))
            .ToList();
    }

    /// <summary>A material of a course the student is enrolled in (any material for admins / the author).</summary>
    public override MaterialPage? GetMaterial(int materialId)
    {
        var uid = UserId;
        var r = db.Materials.Where(m => m.Id == materialId)
            .Select(m => new { m, Course = m.Course.Title, Mark = db.MaterialMarks.FirstOrDefault(k => k.UserId == uid && k.MaterialId == m.Id) })
            .FirstOrDefault();
        if (r == null || GetCourse(r.m.CourseId) is not { } course) return null;
        var privileged = Principal?.IsInRole("admin") == true || db.Courses.Any(c => c.Id == course.Id && c.TeacherId == uid);
        if (!course.Enrolled && !privileged) return null;
        return new MaterialPage(ToMaterial(r.m, r.Course, r.Mark), course.Id, r.m.Body, VideoLinks.Embed(r.m.VideoUrl), r.m.FileUrl);
    }

    private Material ToMaterial(MaterialEntity m, string course, MaterialMark? mark) =>
        new(m.Title, m.Type, course, m.Meta, Ago(m.CreatedAt), m.Id, mark?.Saved == true, mark?.ViewedAt != null);

    /// <summary>"2 дня назад" / "2 күн бұрын" / "2 days ago".</summary>
    private string Ago(DateTime utc)
    {
        var d = (int)(DateTime.UtcNow - utc).TotalDays;
        return d switch
        {
            < 1 => T("Сегодня", "Бүгін", "Today"),
            1 => T("Вчера", "Кеше", "Yesterday"),
            < 7 => T($"{d} дн. назад", $"{d} күн бұрын", $"{d} days ago"),
            < 30 => T($"{d / 7} нед. назад", $"{d / 7} апта бұрын", $"{d / 7} wk ago"),
            _ => utc.ToLocalTime().ToString("dd.MM.yyyy"),
        };
    }

    // ------------------------------------------------------------------ admin

    public override List<AdminUser> GetUsers() =>
        db.Users.OrderByDescending(u => u.CreatedAt).ToList()
            .Select(u => new AdminUser(
                $"{u.FirstName} {u.LastName}".Trim(), u.Email, u.Role, u.Status,
                u.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy"),
                u.LastLoginAt is { } t ? t.ToLocalTime().ToString("dd.MM.yyyy HH:mm") : T("Не входил", "Кірмеген", "Never"),
                u.Id, u.EmailConfirmed))
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
