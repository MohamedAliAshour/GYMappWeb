using GYMappWeb.Models;
using Microsoft.AspNetCore.Localization;

namespace GYMappWeb.Helper
{
    public class CookieRequestCultureProvider : RequestCultureProvider
    {
        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Try to get culture from cookie
            var cultureCookie = httpContext.Request.Cookies[".AspNetCore.Culture"];
            if (!string.IsNullOrEmpty(cultureCookie))
            {
                if (cultureCookie.Contains("ar-SA"))
                    return new ProviderCultureResult("ar-SA");
                if (cultureCookie.Contains("en-US"))
                    return new ProviderCultureResult("en-US");
            }

            // Fallback to default culture
            return new ProviderCultureResult("en-US");
        }
    }
}
