using System.Security.Claims;
using Game1.Data;
using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game1.Controllers;

/// <summary>Student area. Courses, lessons and materials come from SQL Server; the rest is still mock data (IEduDataService).</summary>
[Authorize(Roles = "student,admin")]
public class StudentController(IEduDataService data, CourseStore store, TestStore tests, AssignmentStore assignments,
    AppDbContext db, LangService L) : Controller
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
        if (page.TestId != null || page.AssignmentId != null) return RedirectToAction(nameof(Lesson), new { id });   // pass the test / hand in the work

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

    /// <summary>Opens (or resumes) an attempt. The page gets the questions without the right answers.</summary>
    public async Task<IActionResult> TakeTest(int id)
    {
        var test = await db.Tests.FindAsync(id);
        var item = data.GetTest(id);
        if (test == null || item == null || data.GetCourse(test.CourseId) is not { Enrolled: true }) return NotFound();
        if (item.Status == "locked")
        {
            TempData["Error"] = L.T("Тест откроется после предыдущих уроков курса.", "Тест курстың алдыңғы сабақтарынан кейін ашылады.", "The test opens after the earlier lessons of the course.");
            return RedirectToAction(nameof(Tests));
        }
        var attempt = await tests.StartOrResumeAsync(MyId, test);
        var view = data.GetAttempt(attempt.Id)!;
        ViewData["Test"] = item;
        ViewData["AttemptId"] = attempt.Id;
        ViewData["Answers"] = view.Answers;
        ViewData["SecondsLeft"] = Math.Max(0, test.Minutes * 60 - (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds);
        return View(view.Questions);
    }

    /// <summary>One answer (fetch from TakeTest). The first choice is final; the reply reveals the right option.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Answer(int attemptId, int index, int option)
    {
        var r = await tests.AnswerAsync(MyId, attemptId, index, option);
        return r.Error == null ? Json(new { ok = r.Ok, correct = r.Correct, explanation = r.Explanation })
                               : Json(new { error = r.Error });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishTest(int attemptId)
    {
        var a = await db.TestAttempts.Include(x => x.Test).FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == MyId);
        if (a == null) return NotFound();
        await tests.FinishAsync(a, a.Test);
        return RedirectToAction(nameof(TestResult), new { id = a.Id });
    }

    /// <summary>Result + per-question review of one of the student's own attempts.</summary>
    public IActionResult TestResult(int id)
    {
        var a = data.GetAttempt(id);
        if (a == null) return NotFound();
        if (!a.Finished) return RedirectToAction(nameof(TakeTest), new { id = a.Test.Id });
        ViewData["Test"] = a.Test;
        ViewData["Answers"] = a.Answers;
        ViewData["Seconds"] = a.Seconds;
        ViewData["AttemptId"] = a.Id;
        return View(a.Questions);
    }

    public IActionResult Assignments() => View(data.GetAssignments());

    public IActionResult Assignment(int id)
    {
        var a = data.GetAssignment(id);
        return a is null ? NotFound() : View(a);
    }

    /// <summary>Upload of the work (files + comment). Up to 10 files of the assignment's size limit.</summary>
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(300_000_000), RequestFormLimits(MultipartBodyLengthLimit = 300_000_000)]
    public async Task<IActionResult> SubmitAssignment(int id, string? comment, List<IFormFile> files)
    {
        var a = await db.Assignments.FindAsync(id);
        var enrolledAt = await db.Enrollments.Where(e => e.UserId == MyId && e.CourseId == (a == null ? 0 : a.CourseId)).Select(e => (DateTime?)e.EnrolledAt).FirstOrDefaultAsync();
        if (a == null || enrolledAt == null) return NotFound();

        var r = await assignments.SubmitAsync(MyId, a, comment, files, SqlEduDataService.AssignmentDue(a, enrolledAt));
        if (r.Ok)
            TempData["Success"] = L.T("Работа отправлена на проверку", "Жұмыс тексеруге жіберілді", "Work submitted for review");
        else
            TempData["Error"] = r.Error switch
            {
                "no-files" => L.T("Прикрепите хотя бы один файл.", "Кемінде бір файл тіркеңіз.", "Attach at least one file."),
                "too-many" => L.T($"Не больше {AssignmentStore.MaxFiles} файлов.", $"{AssignmentStore.MaxFiles} файлдан аспауы керек.", $"At most {AssignmentStore.MaxFiles} files."),
                "bad-type" => L.T("Неподдерживаемый формат файла.", "Файл пішіміне қолдау көрсетілмейді.", "Unsupported file format."),
                "too-big" => L.T($"Файл больше {a.MaxMb} MB.", $"Файл {a.MaxMb} MB-тан үлкен.", $"A file is larger than {a.MaxMb} MB."),
                "empty" => L.T("Пустой файл.", "Бос файл.", "Empty file."),
                _ => L.T("Работа уже отправлена — изменить её можно, только если преподаватель вернёт её на доработку.", "Жұмыс жіберілген — оқытушы қайтарса ғана өзгертуге болады.", "Already submitted — you can change it only if the teacher returns it."),
            };
        return RedirectToAction(nameof(Assignment), new { id });
    }

    /// <summary>Download of a file from the student's own submission.</summary>
    public async Task<IActionResult> SubmissionFile(int id)
    {
        var f = await db.SubmissionFiles.Include(x => x.Submission).FirstOrDefaultAsync(x => x.Id == id && x.Submission.UserId == MyId);
        if (f == null || !System.IO.File.Exists(assignments.PathOf(f))) return NotFound();
        return PhysicalFile(assignments.PathOf(f), "application/octet-stream", f.FileName);
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
