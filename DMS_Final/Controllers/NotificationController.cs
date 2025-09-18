using DMS_Final.Attribute;
using DMS_Final.Services;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Final.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult GetUserNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized();

            var notifications = _notificationService.GetRecentNotifications(userId.Value, 10);
            return Json(notifications);
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult MarkNotificationsAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                _notificationService.MarkAllAsRead(userId.Value);
                return Ok();
            }
            return BadRequest("User not logged in");
        }


        [RoleAuthorize("Admin", "Developer")]
        public IActionResult AllNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized();

            var notifications = _notificationService.GetRecentNotifications(userId.Value, 50);
            return View(notifications);
        }
    }
}
