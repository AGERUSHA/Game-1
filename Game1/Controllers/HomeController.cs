using Game1.Models;
using Game1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers
{
    public class HomeController(IEduDataService data) : Controller
    {
        // Landing page (before sign-in)
        public IActionResult Index()
        {
            ViewData["Student"] = data.GetStudent();
            return View();
        }

        // Design System reference page
        public IActionResult DesignSystem() => View();

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>Friendly page for 404 / 403 / … (UseStatusCodePagesWithReExecute).</summary>
        [Route("/Home/Status/{code:int}")]
        public IActionResult Status(int code) =>
            View("Error", new ErrorViewModel { StatusCode = code, RequestId = HttpContext.TraceIdentifier });
    }
}
