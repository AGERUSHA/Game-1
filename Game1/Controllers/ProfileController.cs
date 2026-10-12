using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

[Authorize]
public class ProfileController(IEduDataService data) : Controller
{
    public IActionResult Index() => View(data);
}
