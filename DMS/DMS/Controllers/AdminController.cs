using DMS.Attribute;
using DMS.Models;
using DMS.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers
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
            try
            {
                var createdBy = HttpContext.Session.GetString("UserName");
                user.CreatedBy = createdBy;
                user.CreatedOn = DateTime.Now;
                user.IsActive = true;

                _adminService.CreateUser(user);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("GetAllUsers", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(user);
            }
        }



        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult GetAllUsers(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var result = _adminService.GetAllUsers(page, pageSize, searchTerm, sortColumn, sortDirection);
                return Json(result);
            }

            // For initial page load, return View with empty model
            return View(new List<UserModel>());
        }


        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _adminService.GetUserById(id);
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

            // Handle AJAX vs normal requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            return RedirectToAction("GetAllUsers", "Admin");
        }


    }
}
