using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

/// <summary>Teacher area (UI prototype on mock data).</summary>
[Authorize(Roles = "teacher,admin")]
public class TeacherController(IEduDataService data) : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    public IActionResult Dashboard() => View(data);

    public IActionResult Students() => View(data.GetStudents());

    public IActionResult Courses() => View(data.GetTeacherCourses());

    public IActionResult CreateCourse() => View();

    public IActionResult Analytics() => View(data);

    public IActionResult Reports() => View(data.GetReports());

    /// <summary>Grading queue: list on the left, selected work on the right.</summary>
    public IActionResult Review(int id = 0)
    {
        var queue = data.GetSubmissions();
        var current = data.GetSubmission(id > 0 ? id : queue.First(s => s.Status is "new" or "review").Id);
        if (current is null) return NotFound();
        ViewData["Queue"] = queue;
        return View(current);
    }
}
