using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CS4760GrantApplication.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DeptChairAttribute : Attribute, IAuthorizationFilter
    {

        public DeptChairAttribute()
        {

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

            var deptChair = session.GetString("IsDeptChair");
            if (deptChair != "True")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

        }
    }
}
