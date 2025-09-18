using DMS_Final.Attribute;
using DMS_Final.Models;
using DMS_Final.Services.User;
using DMS_Final.Services.Admin; // Add this using
using Microsoft.AspNetCore.Mvc;
using DMS_Final.Services;

namespace DMS_Final.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentStatusHistoryService _documentStatusHistoryService;
        private readonly INotificationService _notificationService;


        public UserController(IUserService userService, IAdminService adminService, IDocumentService documentService, IDocumentStatusHistoryService documentStatusHistoryService, INotificationService notificationService) // Update constructor
        {
            _userService = userService;
            _adminService = adminService;
            _documentService = documentService;
            _documentStatusHistoryService = documentStatusHistoryService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _userService.Authenticate(username, password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserRole", user.Roles);
                HttpContext.Session.SetString("UserName", user.UserName);

                // ✅ Save login time
                _userService.SetLastLoginTime(user.Id);

                HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (user.Roles == "Admin")
                    return RedirectToAction("AdminDashboard", "User");
                else if (user.Roles == "Manager")
                    return RedirectToAction("ManagerDashboard", "User");
                else if (user.Roles == "IT")
                    return RedirectToAction("ITDashboard", "User");
                else if (user.Roles == "Viewer")
                    return RedirectToAction("ViewerDashboard", "User");
                else if (user.Roles == "Developer")
                    return RedirectToAction("DeveloperDashboard", "User");
                else
                    return Content($"Welcome {user.Name}, Role: {user.Roles}");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }


        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            ViewBag.DocumentDetailsCount = _adminService.GetDocumentDetailsCount();
            ViewBag.DocumentpendingDetailsCount = _adminService.GetPendingDocumentDetailsCount();
            ViewBag.DocumentapprovedDetailsCount = _adminService.GetApprovedDocumentDetailsCount();
            ViewBag.ActiveUserCount = _adminService.GetActiveUserCount();

            ViewBag.MonthlyPending = _documentService.GetMonthlyPendingCount();
            ViewBag.MonthlyApproved = _documentService.GetMonthlyApprovedCount();
            ViewBag.MonthlyRejected = _documentService.GetMonthlyRejectedCount();

            ViewBag.RecentDocuments = _documentService.GetRecentApprovedOrPendingDocuments(5);

            ViewBag.RecentActivities = _documentStatusHistoryService.GetRecentActivities(3);

            // --- NEW NOTIFICATION LOGIC ---
            // You need to get the current user's ID to fetch their notifications.
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId.HasValue)
            {
                ViewBag.UnreadNotificationCount = _notificationService.GetUnreadNotificationCount(currentUserId.Value);
                ViewBag.RecentNotifications = _notificationService.GetRecentNotifications(currentUserId.Value, 5);
            }
            else
            {
                // Handle case where user ID is not in session
                ViewBag.UnreadNotificationCount = 0;
                ViewBag.RecentNotifications = new List<NotificationModel>();
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime");
            return View("~/Views/Admin/AdminDashboard.cshtml");
        }









        [RoleAuthorize("IT")]
        public IActionResult ItDashboard()
        {
            return View("~/Views/IT/ITDashboard.cshtml");
        }

        [RoleAuthorize("Manager")]
        public IActionResult ManagerDashboard()
        {
            return View("~/Views/Manager/ManagerDashboard.cshtml");
        }


        [HttpGet]
        [RoleAuthorize("Developer")]
        public IActionResult DeveloperDashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            ViewBag.RecentDocumentsUser = _documentService.GetRecentApprovedOrPendingDocumentsUser(userName, 5);


            
            ViewBag.DocumentApprovedDetailsCountUser = _userService.GetApprovedDocumentDetailsCountUser(userName);
            ViewBag.DocumentPendingDetailsCountUser = _userService.GetPendingDocumentDetailsCountUser(userName);
            ViewBag.DocumentRejectedDetailsCountUser = _userService.GetRejectedDocumentDetailsCountUser(userName);


            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId.HasValue)
            {
                ViewBag.UnreadNotificationCount = _notificationService.GetUnreadNotificationCount(currentUserId.Value);
                ViewBag.RecentNotifications = _notificationService.GetRecentNotifications(currentUserId.Value, 5);
            }
            else
            {
                // Handle case where user ID is not in session
                ViewBag.UnreadNotificationCount = 0;
                ViewBag.RecentNotifications = new List<NotificationModel>();
            }
            return View("~/Views/Developer/DeveloperDashboard.cshtml");
        }

        [RoleAuthorize("Viewer")]
        public IActionResult ViewerDashboard()
        {
            return View("~/Views/Viewer/ViewerDashboard.cshtml");
        }


        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                // ✅ Save logout time
                _userService.SetLastLogoutTime(userId.Value);
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            

            if (!ModelState.IsValid)
                return View(model);

            
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
           
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            bool result = _userService.ChangePassword(userId.Value, model.CurrentPassword, model.NewPassword);

            if (result)
            {
                if (role == "Admin")
                    return RedirectToAction("AdminDashboard", "User");
                else if (role == "Manager")
                    return RedirectToAction("ManagerDashboard", "User");
                else if (role == "IT")
                    return RedirectToAction("ITDashboard", "User");
                else if (role == "Viewer")
                    return RedirectToAction("ViewerDashboard", "User");
                else if (role == "Developer")
                    return RedirectToAction("DeveloperDashboard", "User");

                return RedirectToAction("Login", "User");
            }
            else
            {
                ViewBag.Error = "Current password is incorrect.";
                return View(model);
            }
        }

    }
}
