using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

/// <summary>Game zone: hub + playable games. Game content is generated client-side and changes every round.</summary>
[Authorize(Roles = "student,admin")]
public class GamesController(GameCatalog games) : Controller
{
    public IActionResult Index() => View(games.All());

    [Route("/Games/Play/{id}")]
    public IActionResult Play(string id)
    {
        var game = games.Find(id);
        if (game is not { Playable: true }) return NotFound();
        ViewData["GameOfDay"] = games.GameOfDay().Id == id;
        return View(game);
    }
}
