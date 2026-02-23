// ================================================================================
// ARCHIVO: CMS.UI/Middleware/SessionValidationMiddleware.cs
// PROPÓSITO: Middleware para validar la sesión en cada request
// DESCRIPCIÓN: Verifica que si el usuario está autenticado, tenga una sesión válida
//              Si no, lo redirige a SelectCompany
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

namespace CMS.UI.Middleware
{
    /// <summary>
    /// Middleware que valida que usuarios autenticados tengan una sesión válida
    /// </summary>
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        // Rutas que no requieren validación de sesión
        private static readonly string[] _excludedPaths =
        [
            "/Account",
            "/Home/Error",
            "/_framework",
            "/_vs",
            "/css",
            "/js",
            "/img",
            "/lib",
            "/favicon"
        ];

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Verificar si la ruta está excluida
            var isExcluded = _excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            // Solo validar si NO está excluida Y el usuario está autenticado
            if (!isExcluded && context.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Verificar que la sesión esté disponible antes de acceder
                    if (context.Session.IsAvailable)
                    {
                        var companyId = context.Session.GetInt32("SelectedCompanyId");
                        var companySchema = context.Session.GetString("SelectedCompanySchema");
                        var apiToken = context.Session.GetString("ApiToken");

                        if (companyId == null || string.IsNullOrEmpty(companySchema) || string.IsNullOrEmpty(apiToken))
                        {
                            _logger.LogWarning(
                                "🔒 Sesión inválida detectada. Path: {Path}, User: {User}. Redirigiendo a SelectCompany.",
                                path,
                                context.User.Identity?.Name ?? "desconocido");

                            // Limpiar sesión
                            context.Session.Clear();

                            // Redirigir a SelectCompany
                            context.Response.Redirect("/Account/SelectCompany?forceLogout=true");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ Error en validación de sesión: {Message}", ex.Message);
                    // Continuar sin redirigir para evitar loops
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware
    /// </summary>
    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionValidationMiddleware>();
        }
    }
}
