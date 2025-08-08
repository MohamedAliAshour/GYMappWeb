using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using GYMappWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace GYMappWeb.Helper
{
    public class SessionCheckFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

            if (allowAnonymous != null) return;

            var session = context.HttpContext.Session;
            var userSession = session.GetUserSession();

            if (userSession == null)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectResult($"/Identity/Account/Login?returnUrl={returnUrl}");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing after action executes
        }
    }
}
