using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DMS_Final.Attribute
{
    public class RoleAuthorizeAttribute : TypeFilterAttribute
    {
        public RoleAuthorizeAttribute(params string[] roles) : base(typeof(RoleAuthorizeFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class RoleAuthorizeFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeFilter(string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || !_roles.Contains(role))
            {
                // Check if the request is AJAX
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new JsonResult(new { redirectTo = "/User/Login" }) { StatusCode = 401 };
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "User", null);
                }
            }
        }
    }

}
