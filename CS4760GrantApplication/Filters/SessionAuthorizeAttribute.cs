using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {

        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;
            var userId = session.GetInt32("UserID");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
                return;
            }

            if (_requiredRole != null)
            {
                var userRole = session.GetString("UserRole");
                if (userRole != _requiredRole)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }
            }

        }
    }
}
