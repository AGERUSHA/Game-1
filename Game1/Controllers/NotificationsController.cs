using Game1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game1.Controllers;

/// <summary>Notification center (all roles).</summary>
[Authorize]
public class NotificationsController(IEduDataService data) : Controller
{
    public IActionResult Index() => View(data.GetNotificationFeed());
}
