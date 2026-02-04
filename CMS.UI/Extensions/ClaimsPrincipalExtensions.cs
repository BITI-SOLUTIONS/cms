using System.Security.Claims;

namespace CMS.UI.Extensions  // Asegúrate de usar el namespace de TU proyecto
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return "Usuario";

            // 1. Busca un claim personalizado "DisplayName"
            var displayName = principal.FindFirstValue("DisplayName")
                           ?? principal.FindFirstValue(ClaimTypes.GivenName)
                           ?? principal.FindFirstValue(ClaimTypes.Name);

            // 2. Si no hay, usa el nombre del Identity
            return displayName ?? principal.Identity?.Name ?? "Usuario";
        }
    }
}