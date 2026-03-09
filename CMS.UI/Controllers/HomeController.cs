// ================================================================================
// ARCHIVO: CMS.UI/Controllers/HomeController.cs
// PROPÓSITO: Controller principal de la interfaz web del Sistema CMS
// DESCRIPCIÓN: Maneja la página de inicio (Dashboard) con JWT autenticado
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-14 - Agregado filtro de validación de sesión
// ================================================================================

using CMS.UI.Filters;
using CMS.UI.Models;
using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace CMS.UI.Controllers
{
    /// <summary>
    /// Controller principal del sistema que maneja la página de inicio y errores.
    /// Requiere autenticación y sesión válida (compañía + JWT).
    /// </summary>
    [Authorize]
    [RequireValidSession] // Valida que hay compañía y JWT en sesión
    public class HomeController : Controller
    {
        private readonly MenuApiService _api;
        private readonly DashboardApiService _dashboardApi;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        public HomeController(
            MenuApiService api, 
            DashboardApiService dashboardApi,
            ILogger<HomeController> logger)
        {
            _api = api;
            _dashboardApi = dashboardApi;
            _logger = logger;
        }

        /// <summary>
        /// Acción principal que muestra el Dashboard o página de inicio del sistema.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // ⭐ VERIFICAR AUTENTICACIÓN
                if (User.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("⚠️ Usuario no autenticado. Redirigiendo a SelectCompany.");
                    return RedirectToAction("SelectCompany", "Account", new { forceLogout = true });
                }

                // ⭐ VERIFICAR QUE HAY UNA COMPAÑÍA VÁLIDA EN LA SESIÓN
                var companyId = HttpContext.Session.GetInt32("SelectedCompanyId");
                var companySchema = HttpContext.Session.GetString("SelectedCompanySchema");

                if (companyId == null || string.IsNullOrEmpty(companySchema))
                {
                    _logger.LogWarning("⚠️ Sesión sin compañía válida. Redirigiendo a SelectCompany.");
                    return RedirectToAction("SelectCompany", "Account", new { forceLogout = true });
                }

                // ⭐ VERIFICAR SI HAY JWT EN SESIÓN
                var token = HttpContext.Session.GetString("ApiToken");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No hay JWT en sesión. Redirigiendo a SelectCompany.");
                    return RedirectToAction("SelectCompany", "Account", new { forceLogout = true });
                }

                // ⭐ Obtener información del usuario autenticado
                var userEmail = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("preferred_username")
                    ?? "Usuario desconocido";

                var displayName = User.FindFirstValue("name") 
                    ?? User.FindFirstValue("display_name")
                    ?? userEmail;

                // Obtener menús desde la API (con JWT en el header via AuthenticatedApiMessageHandler)
                var menus = await _api.GetMenusAsync();

                // ⭐ Obtener estadísticas del Dashboard
                var dashboardStats = await _dashboardApi.GetDashboardStatsAsync();

                // Pasar datos a la vista
                ViewData["UserName"] = displayName;
                ViewData["UserEmail"] = userEmail;
                ViewData["DashboardStats"] = dashboardStats;

                // ⭐ Verificar permisos para accesos rápidos
                // Los permisos vienen del JWT que está en sesión, NO del User.Claims de la cookie
                var permissions = GetPermissionsFromJwt(token);
                ViewData["HasUsersPermission"] = permissions.Contains("Admin.Users.View");
                ViewData["HasRolesPermission"] = permissions.Contains("Admin.Roles.View");
                // Admin.Menus.Edit es el permiso correcto (no Admin.Menus.View que no existe)
                ViewData["HasMenusPermission"] = permissions.Contains("Admin.Menus.Edit");
                ViewData["HasCompaniesPermission"] = permissions.Contains("System.ViewAllCompanies");

                // ⭐ Datos para el link de "Roles y Permisos"
                // Obtener userId del JWT
                var userIdFromJwt = GetUserIdFromJwt(token);
                ViewData["CurrentUserId"] = userIdFromJwt;
                ViewData["CurrentCompanyId"] = companyId;
                // Usar roleId = 1 (Admin) como default para ManagePermissions
                ViewData["DefaultRoleId"] = 1;

                _logger.LogInformation("🔐 Permisos del usuario: {PermissionCount} ({HasUsers}/{HasRoles}/{HasMenus}/{HasCompanies})",
                    permissions.Count,
                    ViewData["HasUsersPermission"],
                    ViewData["HasRolesPermission"],
                    ViewData["HasMenusPermission"],
                    ViewData["HasCompaniesPermission"]);

                // Log informativo para debugging
                _logger.LogInformation(
                    "Usuario {User} accedió al Dashboard con {MenuCount} menús disponibles",
                    userEmail,
                    menus?.Count ?? 0
                );

                // Retornar vista con los menús (o lista vacía si falló la API)
                return View(menus ?? new List<CMS.Application.DTOs.MenuDto>());
            }
            catch (Exception ex)
            {
                // Log de error crítico
                _logger.LogError(ex, "Error al cargar la página de inicio para el usuario {User}",
                    User.Identity?.Name ?? "Desconocido");

                // Retornar vista con lista vacía para evitar crash
                return View(new List<CMS.Application.DTOs.MenuDto>());
            }
        }

        /// <summary>
        /// Acción que maneja los errores HTTP y excepciones no controladas del sistema.
        /// </summary>
        [AllowAnonymous] // Los errores deben mostrarse incluso sin autenticación
        [SkipSessionValidation] // No validar sesión para la página de errores
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            // ⭐ Verificar si hay sesión válida para decidir el layout
            var hasValidSession = HttpContext.Session.GetInt32("SelectedCompanyId") != null &&
                                  !string.IsNullOrEmpty(HttpContext.Session.GetString("ApiToken"));

            // Obtener RequestId único para correlacionar con logs
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Obtener la ruta que causó el error
            var requestPath = HttpContext.Request.Path.Value;

            // Mensaje de error según el código de estado
            string? errorMessage = statusCode switch
            {
                400 => "Solicitud inválida. Por favor, verifica los datos enviados.",
                401 => "No tiene autorización para acceder a este recurso. Inicie sesión.",
                403 => "Acceso prohibido. No tiene permisos suficientes para esta acción.",
                404 => "La página solicitada no existe o fue movida.",
                408 => "La solicitud tardó demasiado tiempo. Intente nuevamente.",
                500 => "Error interno del servidor. Nuestro equipo ha sido notificado.",
                502 => "El servidor no está disponible temporalmente. Intente más tarde.",
                503 => "Servicio no disponible. El sistema está en mantenimiento.",
                _ => statusCode.HasValue
                    ? $"Ocurrió un error inesperado (Código: {statusCode})."
                    : "Ha ocurrido un error inesperado al procesar su solicitud."
            };

            // Log del error para análisis posterior
            if (statusCode.HasValue && statusCode >= 400)
            {
                _logger.LogWarning(
                    "Error HTTP {StatusCode} en ruta {Path}. RequestId: {RequestId}. Usuario: {User}. SesiónVálida: {HasSession}",
                    statusCode,
                    requestPath,
                    requestId,
                    User.Identity?.Name ?? "Anónimo",
                    hasValidSession
                );
            }
            else
            {
                _logger.LogError(
                    "Excepción no controlada. RequestId: {RequestId}. Ruta: {Path}. Usuario: {User}",
                    requestId,
                    requestPath,
                    User.Identity?.Name ?? "Anónimo"
                );
            }

            // Crear modelo para la vista de error
            var model = new ErrorViewModel
            {
                RequestId = requestId,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                RequestPath = requestPath
            };

            // ⭐ Si no hay sesión válida, usar vista standalone
            if (!hasValidSession)
            {
                return View("ErrorStandalone", model);
            }

            // Retornar vista de error con el modelo
            return View(model);
        }

        /// <summary>
        /// Extrae los permisos del JWT almacenado en sesión
        /// </summary>
        private List<string> GetPermissionsFromJwt(string token)
        {
            var permissions = new List<string>();

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error decodificando JWT para extraer permisos");
            }

            return permissions;
        }

        /// <summary>
        /// Extrae el userId del JWT almacenado en sesión
        /// </summary>
        private int GetUserIdFromJwt(string token)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    // El userId puede estar en "sub", "nameid" o "userId"
                    var userIdClaim = jwtToken.Claims
                        .FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid" || c.Type == "userId");

                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        return userId;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error decodificando JWT para extraer userId");
            }

            return 1; // Default al usuario 1 si no se puede obtener
        }

        /// <summary>
        /// Página de acceso denegado cuando el usuario no tiene permisos suficientes.
        /// </summary>
        [AllowAnonymous]
        [SkipSessionValidation]
        public IActionResult AccessDenied(string? permission = null, string? returnUrl = null)
        {
            _logger.LogWarning(
                "🔒 Acceso denegado para usuario {User}. Permiso requerido: {Permission}. URL: {ReturnUrl}",
                User.Identity?.Name ?? "Anónimo",
                permission ?? "desconocido",
                returnUrl ?? Request.Headers["Referer"].FirstOrDefault() ?? "desconocido");

            // Mapear permisos a nombres amigables
            var permissionNames = new Dictionary<string, string>
            {
                { "Admin.Users.View", "Gestión de Usuarios" },
                { "Admin.Users.Edit", "Edición de Usuarios" },
                { "Admin.Users.Create", "Creación de Usuarios" },
                { "Admin.Users.Delete", "Eliminación de Usuarios" },
                { "Admin.Roles.View", "Gestión de Roles" },
                { "Admin.Roles.Edit", "Edición de Roles" },
                { "Admin.Menus.View", "Gestión de Menús" },
                { "Admin.Menus.Edit", "Edición de Menús" },
                { "Admin.Permissions.View", "Gestión de Permisos" },
                { "Admin.Permissions.Edit", "Edición de Permisos" },
                { "Admin.Companies.View", "Gestión de Compañías" },
                { "Admin.Dashboard.View", "Dashboard de Administración" },
                { "Admin.Audit.View", "Registro de Auditoría" },
                { "Admin.Logs.View", "Logs del Sistema" },
                { "Admin.APIKeys.Edit", "Gestión de API Keys" },
                { "Admin.Jobs.View", "Programador de Tareas" },
                { "Admin.Backup.Execute", "Backup y Restauración" },
                { "Admin.Health.View", "Estado del Sistema" },
                { "System.ViewAllCompanies", "Ver Todas las Compañías" }
            };

            var permissionDisplayName = permission != null && permissionNames.TryGetValue(permission, out var name)
                ? name
                : permission ?? "esta funcionalidad";

            ViewData["Permission"] = permission;
            ViewData["PermissionDisplayName"] = permissionDisplayName;
            ViewData["ReturnUrl"] = returnUrl ?? "/Home";

            return View();
        }

        #region Profile & Settings

        /// <summary>
        /// Página de perfil del usuario actual.
        /// Muestra información personal y permite editar datos básicos.
        /// </summary>
        public IActionResult Profile()
        {
            var token = HttpContext.Session.GetString("ApiToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SelectCompany", "Account", new { forceLogout = true });
            }

            // Obtener información del usuario desde claims y JWT
            var userInfo = GetUserInfoFromJwt(token);

            ViewData["Title"] = "Mi Perfil";
            ViewData["UserInfo"] = userInfo;
            ViewData["CompanyName"] = HttpContext.Session.GetString("SelectedCompanyName") ?? "Sin compañía";
            ViewData["CompanySchema"] = HttpContext.Session.GetString("SelectedCompanySchema") ?? "";

            return View();
        }

        /// <summary>
        /// Página de configuración personal del usuario.
        /// Permite ajustar preferencias como tema, idioma, notificaciones, etc.
        /// </summary>
        public IActionResult Settings()
        {
            var token = HttpContext.Session.GetString("ApiToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SelectCompany", "Account", new { forceLogout = true });
            }

            var userInfo = GetUserInfoFromJwt(token);

            ViewData["Title"] = "Configuración";
            ViewData["UserInfo"] = userInfo;
            ViewData["CompanyName"] = HttpContext.Session.GetString("SelectedCompanyName") ?? "Sin compañía";

            return View();
        }

        /// <summary>
        /// Extrae información del usuario desde el JWT
        /// </summary>
        private UserProfileInfo GetUserInfoFromJwt(string token)
        {
            var info = new UserProfileInfo();

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    info.UserId = int.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid" || c.Type == "userId")?.Value, out var uid) ? uid : 0;
                    info.UserName = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value ?? "";
                    info.Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
                    info.DisplayName = jwtToken.Claims.FirstOrDefault(c => c.Type == "display_name" || c.Type == "name")?.Value ?? info.UserName;
                    info.Roles = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
                    info.Permissions = jwtToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                    info.CompanyId = int.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == "companyId")?.Value, out var cid) ? cid : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extrayendo información del JWT");
            }

            return info;
        }

        #endregion
    }

    /// <summary>
    /// Información del perfil del usuario extraída del JWT
    /// </summary>
    public class UserProfileInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}