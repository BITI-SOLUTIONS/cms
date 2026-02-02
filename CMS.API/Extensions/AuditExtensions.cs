using System.Security.Claims;

namespace CMS.API.Extensions
{
    public static class AuditExtensions
    {
        public static string GetAuditUser(this HttpContext? context)
        {
            return context?.User?.FindFirstValue("cms_username") ?? "SYSTEM";
        }
    }
}