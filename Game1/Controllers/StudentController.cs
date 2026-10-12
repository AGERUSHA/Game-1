using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

/// <summary>Student area. Views read data from IEduDataService (mock for now).</summary>
[Authorize(Roles = "student,admin")]
public class StudentController(IEduDataService data) : Controller
{
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

    public IActionResult Materials() => View(data.GetMaterials());

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
