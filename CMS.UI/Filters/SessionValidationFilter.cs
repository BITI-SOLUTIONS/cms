// ================================================================================
// ARCHIVO: CMS.UI/Filters/SessionValidationFilter.cs
// PROPÓSITO: Filtros para validar sesión y permisos del usuario
// DESCRIPCIÓN: Verifica que exista compañía seleccionada, JWT en sesión y permisos
//              Se aplica a todas las acciones que requieren autenticación
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ACTUALIZADO: 2026-03-02 - Agregado filtro de verificación de permisos
// ================================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace CMS.UI.Filters
{
    /// <summary>
    /// Filtro que valida que la sesión del usuario tenga los datos requeridos:
    /// - Compañía seleccionada
    /// - JWT de API
    /// </summary>
    public class SessionValidationFilter : IActionFilter
    {
        private readonly ILogger<SessionValidationFilter> _logger;

        public SessionValidationFilter(ILogger<SessionValidationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Verificar si la acción tiene [SkipSessionValidation]
            var skipValidation = context.ActionDescriptor.EndpointMetadata
                .OfType<SkipSessionValidationAttribute>()
                .Any();

            if (skipValidation)
            {
                return;
            }

            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Si el usuario no está autenticado, dejar que [Authorize] maneje
            if (user.Identity?.IsAuthenticated != true)
            {
                return;
            }

            // Verificar que hay compañía en sesión
            var companyId = httpContext.Session.GetInt32("SelectedCompanyId");
            var companySchema = httpContext.Session.GetString("SelectedCompanySchema");
            var apiToken = httpContext.Session.GetString("ApiToken");

            // Si falta alguno, redirigir a SelectCompany
            if (companyId == null || string.IsNullOrEmpty(companySchema) || string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning(
                    "⚠️ Sesión inválida: CompanyId={CompanyId}, Schema={Schema}, HasToken={HasToken}. Usuario: {User}",
                    companyId?.ToString() ?? "null",
                    companySchema ?? "null",
                    !string.IsNullOrEmpty(apiToken),
                    user.Identity?.Name ?? "desconocido");

                // Limpiar sesión
                httpContext.Session.Clear();

                // Redirigir a SelectCompany con forceLogout
                context.Result = new RedirectToActionResult(
                    "SelectCompany", 
                    "Account", 
                    new { forceLogout = true });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No se necesita hacer nada después de la acción
        }
    }

    /// <summary>
    /// Filtro que verifica que el usuario tenga un permiso específico.
    /// Extrae los permisos del JWT en sesión.
    /// </summary>
    public class RequirePermissionFilter : IActionFilter
    {
        private readonly ILogger<RequirePermissionFilter> _logger;
        private readonly string _permission;

        public RequirePermissionFilter(ILogger<RequirePermissionFilter> logger, string permission)
        {
            _logger = logger;
            _permission = permission;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Si el usuario no está autenticado, dejar que [Authorize] maneje
            if (user.Identity?.IsAuthenticated != true)
            {
                return;
            }

            // Obtener token de sesión
            var apiToken = httpContext.Session.GetString("ApiToken");
            if (string.IsNullOrEmpty(apiToken))
            {
                // Sin token, redirigir a login
                context.Result = new RedirectToActionResult(
                    "SelectCompany", 
                    "Account", 
                    new { forceLogout = true });
                return;
            }

            // Extraer permisos del JWT
            var permissions = GetPermissionsFromJwt(apiToken);

            _logger.LogInformation(
                "🔑 Verificando permiso {RequiredPermission} para {Path}. Usuario tiene {PermissionCount} permisos: [{Permissions}]",
                _permission,
                httpContext.Request.Path,
                permissions.Count,
                string.Join(", ", permissions.Take(5)) + (permissions.Count > 5 ? "..." : ""));

            // Verificar si tiene el permiso requerido
            if (!permissions.Contains(_permission))
            {
                _logger.LogWarning(
                    "🔒 Acceso denegado: Usuario {User} intentó acceder a {Path} sin permiso {Permission}. Permisos disponibles: [{Permissions}]",
                    user.Identity?.Name ?? "desconocido",
                    httpContext.Request.Path,
                    _permission,
                    string.Join(", ", permissions));

                // Redirigir a página de acceso denegado
                context.Result = new RedirectToActionResult(
                    "AccessDenied", 
                    "Home", 
                    new { permission = _permission });
            }
            else
            {
                _logger.LogInformation("✅ Permiso {Permission} verificado para {User}", _permission, user.Identity?.Name);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No se necesita hacer nada después de la acción
        }

        private List<string> GetPermissionsFromJwt(string token)
        {
            var permissions = new List<string>();

            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    // Los permisos están en claims con tipo "permission" (singular)
                    permissions = jwtToken.Claims
                        .Where(c => c.Type == "permission")
                        .Select(c => c.Value)
                        .ToList();
                }
            }
            catch
            {
                // Error decodificando JWT, retornar lista vacía
            }

            return permissions;
        }
    }

    /// <summary>
    /// Atributo para marcar controladores/acciones que requieren sesión válida
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireValidSessionAttribute : TypeFilterAttribute
    {
        public RequireValidSessionAttribute() : base(typeof(SessionValidationFilter))
        {
        }
    }

    /// <summary>
    /// Atributo para marcar controladores/acciones que requieren un permiso específico.
    /// Ejemplo: [RequirePermission("Admin.Users.View")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permission) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    /// <summary>
    /// Atributo para excluir una acción o controlador de la validación de sesión
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkipSessionValidationAttribute : Attribute
    {
    }
}
