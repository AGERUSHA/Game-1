using System.Security.Claims;
using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

/// <summary>Student area. Courses, lessons and materials come from SQL Server; the rest is still mock data (IEduDataService).</summary>
[Authorize(Roles = "student,admin")]
public class StudentController(IEduDataService data, CourseStore store, LangService L) : Controller
{
    private int MyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    public IActionResult Dashboard() => View(data);

    public IActionResult Courses() => View(data);

    public IActionResult CourseDetails(int id = 1)
    {
        var course = data.GetCourse(id);
        if (course is null) return NotFound();
        ViewData["Curriculum"] = data.GetCurriculum(id);
        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        if (!await store.EnrollAsync(MyId, id)) return NotFound();
        TempData["Success"] = L.T("Вы записались на курс — начинайте с первого урока!", "Курсқа жазылдыңыз — бірінші сабақтан бастаңыз!", "You're enrolled — start with the first lesson!");
        var first = data.GetCurriculum(id).SelectMany(m => m.Lessons).FirstOrDefault();
        return first != null ? RedirectToAction(nameof(Lesson), new { id = first.Id }) : RedirectToAction(nameof(CourseDetails), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RateCourse(int id, int stars)
    {
        await store.RateAsync(MyId, id, stars);
        TempData["Success"] = L.T("Спасибо за оценку!", "Бағаңызға рахмет!", "Thanks for rating!");
        return RedirectToAction(nameof(CourseDetails), new { id });
    }

    /// <summary>A lesson is open once the previous ones are done (completed lessons stay open for review).</summary>
    public async Task<IActionResult> Lesson(int id)
    {
        var page = data.GetLesson(id);
        if (page == null) return NotFound();
        if (!page.Course.Enrolled)
        {
            TempData["Error"] = L.T("Сначала запишитесь на курс.", "Алдымен курсқа жазылыңыз.", "Enroll in the course first.");
            return RedirectToAction(nameof(CourseDetails), new { id = page.Course.Id });
        }
        if (page.Lesson.Status == "locked")
        {
            TempData["Error"] = L.T("Сначала пройдите предыдущие уроки.", "Алдымен алдыңғы сабақтарды өтіңіз.", "Finish the previous lessons first.");
            return RedirectToAction(nameof(CourseDetails), new { id = page.Course.Id });
        }
        await store.TouchAsync(MyId, page.Course.Id);
        return View(page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteLesson(int id)
    {
        var page = data.GetLesson(id);
        if (page == null) return NotFound();
        if (!page.Course.Enrolled || page.Lesson.Status == "locked") return RedirectToAction(nameof(CourseDetails), new { id = page.Course.Id });

        await store.CompleteLessonAsync(MyId, id, page.Course.Id);
        if (page.NextId is int next) return RedirectToAction(nameof(Lesson), new { id = next });
        TempData["Success"] = L.T("Курс пройден! 🎉 Все уроки завершены.", "Курс аяқталды! 🎉 Барлық сабақ өтілді.", "Course completed! 🎉 All lessons done.");
        return RedirectToAction(nameof(CourseDetails), new { id = page.Course.Id });
    }

    public IActionResult Materials() => View(data.GetMaterials());

    public async Task<IActionResult> Material(int id)
    {
        var page = data.GetMaterial(id);
        if (page == null) return NotFound();
        await store.MarkViewedAsync(MyId, id);
        return View(page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMaterial(int id, string? returnUrl)
    {
        if (data.GetMaterial(id) == null) return NotFound();
        var saved = await store.ToggleSavedAsync(MyId, id);
        TempData["Success"] = saved
            ? L.T("Сохранено в закладки", "Бетбелгілерге сақталды", "Saved to bookmarks")
            : L.T("Убрано из закладок", "Бетбелгілерден алынды", "Removed from bookmarks");
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Materials));
    }

    public IActionResult Tests() => View(data.GetTests());

    public IActionResult TakeTest(int id = 1)
    {
        ViewData["Test"] = data.GetTests().FirstOrDefault(t => t.Id == id) ?? data.GetTests()[0];
        return View(data.GetQuestions(id));
    }

    /// <summary>Result + per-question breakdown. a = chosen option per question ("1,0,-1,…", -1 = skipped), t = seconds spent.</summary>
    public IActionResult TestResult(int id = 1, string? a = null, int? t = null)
    {
        var test = data.GetTests().FirstOrDefault(x => x.Id == id);
        if (test is null) return NotFound();
        var questions = data.GetQuestions(id);
        var answers = (a ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var n) && n >= -1 && n <= 3 ? n : -1).ToList();
        if (answers.Count != questions.Count)   // opened directly: show a sample attempt
            answers = questions.Select((q, i) => i is 1 or 6 ? (q.Correct + 1) % q.Options.Length : q.Correct).ToList();
        ViewData["Test"] = test;
        ViewData["Answers"] = answers;
        ViewData["Seconds"] = t is > 0 ? t : test.Minutes * 45 + 17;   // sample: ~¾ of the time limit
        return View(questions);
    }

    public IActionResult Assignments() => View(data.GetAssignments());

    public IActionResult Assignment(int id = 1)
    {
        var a = data.GetAssignment(id);
        return a is null ? NotFound() : View(a);
    }

    public IActionResult Ai() => View(data);

    public IActionResult AiAnalysis() => View(data);

    public IActionResult Xp() => View(data);

    public IActionResult League() => View(data);

    public IActionResult Achievements() => View(data.GetAchievements());

    public IActionResult Progress() => View(data);

    public IActionResult Flashcards(int deck = 1)
    {
        var decks = data.GetDecks();
        var current = decks.FirstOrDefault(d => d.Id == deck) ?? decks[0];
        ViewData["Decks"] = decks;
        ViewData["Cards"] = data.GetFlashcards(current.Id);
        return View(current);
    }

    public IActionResult Focus() => View(data);

    /// <summary>Month view; m = month offset from the current one (events exist for the current month only in mock data).</summary>
    public IActionResult Calendar(int m = 0)
    {
        ViewData["Month"] = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(Math.Clamp(m, -12, 12));
        ViewData["Offset"] = m;
        return View(m == 0 ? data.GetCalendar() : new List<Game1.Models.CalendarEvent>());
    }

    public IActionResult Quests() => View(data);
}
