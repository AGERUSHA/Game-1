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
            i > 0 ? flat[i - 1].Id : null, i < flat.Count - 1 ? flat[i + 1].Id : null, i + 1, flat.Count,
            db.Tests.Where(t => t.LessonId == lessonId).Select(t => (int?)t.Id).FirstOrDefault(),
            db.Assignments.Where(a => a.LessonId == lessonId).Select(a => (int?)a.Id).FirstOrDefault());
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

    // ------------------------------------------------------------------ tests

    private string Difficulty(string d) => d switch
    {
        "easy" => T("Лёгкий", "Жеңіл", "Easy"),
        "hard" => T("Сложный", "Күрделі", "Hard"),
        _ => T("Средний", "Орташа", "Medium"),
    };

    /// <summary>lessonId → completed | current | locked for one course (cached per request).</summary>
    private readonly Dictionary<int, Dictionary<int, string>> lessonStatus = [];
    private Dictionary<int, string> LessonStatuses(int courseId)
    {
        if (!lessonStatus.TryGetValue(courseId, out var map))
            lessonStatus[courseId] = map = GetCurriculum(courseId).SelectMany(m => m.Lessons).ToDictionary(x => x.Id, x => x.Status);
        return map;
    }

    public override List<TestItem> GetTests() => QueryTests(db.Tests.Where(t =>
        t.Course.Status == "published" && db.Enrollments.Any(e => e.UserId == UserId && e.CourseId == t.CourseId)));

    public override TestItem? GetTest(int testId) => QueryTests(db.Tests.Where(t => t.Id == testId)).FirstOrDefault();

    private List<TestItem> QueryTests(IQueryable<TestEntity> tests)
    {
        var uid = UserId;
        var rows = tests.OrderBy(t => t.CourseId).ThenBy(t => t.Id).Select(t => new
        {
            t.Id, t.Title, Course = t.Course.Title, t.CourseId, t.LessonId, t.Minutes, t.Difficulty, t.PassScore,
            Questions = t.Questions.Count,
            Best = db.TestAttempts.Where(a => a.TestId == t.Id && a.UserId == uid && a.FinishedAt != null).Max(a => (int?)a.Score),
            Attempts = db.TestAttempts.Count(a => a.TestId == t.Id && a.UserId == uid && a.FinishedAt != null),
            Last = db.TestAttempts.Where(a => a.TestId == t.Id && a.UserId == uid && a.FinishedAt != null).OrderByDescending(a => a.FinishedAt).Select(a => (int?)a.Id).FirstOrDefault(),
        }).ToList();

        return rows.Select(r =>
        {
            var locked = r.LessonId is int lid && LessonStatuses(r.CourseId).GetValueOrDefault(lid, "locked") == "locked";
            var status = r.Best >= r.PassScore ? "passed" : r.Attempts > 0 ? "retake" : locked ? "locked" : "available";
            return new TestItem(r.Id, r.Title, r.Course, r.Questions, r.Minutes, r.Best, status, r.Attempts, Difficulty(r.Difficulty),
                r.PassScore, r.Last, r.CourseId);
        }).ToList();
    }

    public override List<Question> GetQuestions(int testId) =>
        db.Questions.Where(q => q.TestId == testId).OrderBy(q => q.Order).ToList()
            .Select(q => new Question(q.Text, q.Options.Split('\n'), q.Correct, q.Topic, q.Code, q.Explanation)).ToList();

    public override AttemptView? GetAttempt(int attemptId)
    {
        var a = db.TestAttempts.FirstOrDefault(x => x.Id == attemptId && x.UserId == UserId);
        if (a == null || GetTest(a.TestId) is not { } test) return null;
        var questions = GetQuestions(a.TestId);
        var answers = TestStore.ParseAnswers(a.Answers, questions.Count);
        var seconds = (int)((a.FinishedAt ?? DateTime.UtcNow) - a.StartedAt).TotalSeconds;
        return new AttemptView(a.Id, test, questions, answers, Math.Min(seconds, test.Minutes * 60), a.FinishedAt != null, a.Score, test.PassScore);
    }

    // ------------------------------------------------------------------ assignments

    public override List<Assignment> GetAssignments()
    {
        var uid = UserId;
        return QueryAssignments(db.Assignments.Where(x => x.Course.Status == "published" && db.Enrollments.Any(e => e.UserId == uid && e.CourseId == x.CourseId)))
            .Select(r => r.Info).OrderBy(a => a.Status is "graded" ? 2 : a.Status is "submitted" ? 1 : 0).ThenBy(a => a.DaysLeft).ToList();
    }

    public override AssignmentDetail? GetAssignment(int id)
    {
        var uid = UserId;
        var r = QueryAssignments(db.Assignments.Where(x => x.Id == id && db.Enrollments.Any(e => e.UserId == uid && e.CourseId == x.CourseId))).FirstOrDefault();
        if (r == null) return null;
        var files = r.Submission?.Files.Select(f => new SubmittedFile(f.FileName, Size(f.Size), FileIcon(f.FileName), f.Id)).ToList() ?? [];
        return new AssignmentDetail(r.Info, r.Module ?? "", r.Entity.Description, r.Entity.Xp,
            r.Entity.Formats.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), r.Entity.MaxMb,
            r.Submission?.GraderName ?? r.Teacher, r.Submission?.GraderName != null ? T("Проверил работу", "Жұмысты тексерді", "Reviewer") : T("Преподаватель курса", "Курс оқытушысы", "Course teacher"),
            r.Submission?.Feedback, null, r.Submission?.Comment, files,
            r.Submission?.GradedAt is { } g ? Ago(g) : null);
    }

    private record AssignmentRow(Assignment Info, AssignmentEntity Entity, SubmissionEntity? Submission, string Teacher, string? Module);

    private List<AssignmentRow> QueryAssignments(IQueryable<AssignmentEntity> q)
    {
        var uid = UserId;
        var rows = q.Select(x => new
        {
            x, Course = x.Course.Title, Teacher = x.Course.TeacherName,
            Module = db.Lessons.Where(l => l.Id == x.LessonId).Select(l => l.Module.Title).FirstOrDefault(),
            Enrolled = db.Enrollments.Where(e => e.UserId == uid && e.CourseId == x.CourseId).Select(e => (DateTime?)e.EnrolledAt).FirstOrDefault(),
            Sub = db.Submissions.Include(s => s.Files).FirstOrDefault(s => s.AssignmentId == x.Id && s.UserId == uid),
        }).ToList();

        return rows.Select(r =>
        {
            var due = AssignmentDue(r.x, r.Enrolled);
            var daysLeft = due is { } d ? (int)Math.Floor((d.ToLocalTime().Date - DateTime.Now.Date).TotalDays) : 999;
            var status = r.Sub?.Status switch
            {
                "graded" => "graded",
                "returned" => "returned",
                "submitted" => "submitted",
                _ => due < DateTime.UtcNow ? "overdue" : "todo",
            };
            var info = new Assignment(r.x.Title, r.Course, due is { } dd ? DueText(dd) : T("Без срока", "Мерзімсіз", "No deadline"),
                status, r.Sub?.Status == "graded" ? r.Sub.Score : null, r.x.MaxScore, daysLeft, r.x.Id);
            return new AssignmentRow(info, r.x, r.Sub, r.Teacher, r.Module);
        }).ToList();
    }

    /// <summary>Fixed date, or N days (end of day) after the student enrolled.</summary>
    public static DateTime? AssignmentDue(AssignmentEntity a, DateTime? enrolledAt) =>
        a.DueAt ?? (a.DueDays is int days && enrolledAt is { } e ? e.ToLocalTime().Date.AddDays(days + 1).AddMinutes(-1).ToUniversalTime() : null);

    /// <summary>"8 окт, 23:59" in the UI language.</summary>
    private string DueText(DateTime utc)
    {
        var d = utc.ToLocalTime();
        string[] months = l.Current switch
        {
            "kk" => ["қаң", "ақп", "нау", "сәу", "мам", "мау", "шіл", "там", "қыр", "қаз", "қар", "жел"],
            "en" => ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            _ => ["янв", "фев", "мар", "апр", "мая", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"],
        };
        return l.Current == "en" ? $"{months[d.Month - 1]} {d.Day}, {d:HH:mm}" : $"{d.Day} {months[d.Month - 1]}, {d:HH:mm}";
    }

    private static string Size(long b) => b > 1048576 ? $"{b / 1048576.0:0.0} MB" : $"{Math.Max(1, b / 1024)} KB";

    private static string FileIcon(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".zip" or ".rar" or ".7z" => "file-archive",
        ".sql" => "database",
        ".pdf" or ".doc" or ".docx" or ".txt" => "file-text",
        ".png" or ".jpg" or ".jpeg" or ".fig" => "image",
        _ => "file-code-2",
    };

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
