using DMS_Final.Attribute;
using DMS_Final.Models;
using DMS_Final.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Final.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult CreateUser(UserModel user)
        {
            var createtdBy = HttpContext.Session.GetString("UserName");
            user.CreatedBy = createtdBy;
            user.CreatedOn = DateTime.Now;
            user.IsActive = true;

            _adminService.CreateUser(user);
            return RedirectToAction("GetAllUsers", "Admin");
        }

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _adminService.GetAllUsers();
            return View(users);
        }

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _adminService.GetAllUsers().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult EditUser(UserModel user)
        {
            var LastUpdateBy = HttpContext.Session.GetString("UserName");
            user.LastUpdateBy = LastUpdateBy;
            user.LastUpdateOn = DateTime.Now;
            _adminService.UpdateUser(user);
            return RedirectToAction("GetAllUsers", "Admin");
        }


        [RoleAuthorize("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            _adminService.DeleteUser(id);
            return RedirectToAction("GetAllUsers", "Admin");
        }

    }
}
