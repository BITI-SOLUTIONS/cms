using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class DebugController : Controller
    {
        [HttpGet("/debug/claims")]
        public IActionResult Claims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).OrderBy(c => c.Type).ToList();

            return Json(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Name = User.Identity?.Name,
                OID = User.FindFirst("oid")?.Value,  // ⭐ Tu OID aquí
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("preferred_username")?.Value,
                AllClaims = claims
            });
        }
    }
}