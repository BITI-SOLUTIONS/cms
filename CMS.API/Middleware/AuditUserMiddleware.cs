using System.Security.Claims;
using CMS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMS.API.Middleware
{
    public class AuditUserMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var oid = context.User.FindFirst("oid")?.Value;

                if (!string.IsNullOrEmpty(oid) && Guid.TryParse(oid, out var azureOid))
                {
                    var user = await db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.AZURE_OID == azureOid);

                    if (user != null)
                    {
                        var identity = context.User.Identity as ClaimsIdentity;

                        if (!identity!.HasClaim(c => c.Type == "cms_username"))
                        {
                            identity.AddClaim(new Claim("cms_username", user.USER_NAME));
                            identity.AddClaim(new Claim("cms_user_id", user.ID_USER.ToString()));
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
