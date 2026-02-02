//using CMS.UI.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace CMS.UI.Controllers
//{
//    [Authorize]
//    public class HomeController : Controller
//    {
//        private readonly MenuApiService _api;

//        public HomeController(MenuApiService api)
//        {
//            _api = api;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var menus = await _api.GetMenusAsync();
//            return View(menus);
//        }
//    }
//}
// ================================================================================
// ARCHIVO: CMS.UI/Controllers/HomeController.cs
// PROPÓSITO: Controller principal de la interfaz web del Sistema CMS
// DESCRIPCIÓN: Maneja la página de inicio (Dashboard) y la página de errores.
//              La página de inicio muestra el menú de navegación y sirve como
//              punto de entrada principal después del login con Azure AD.
//              La acción Error se invoca automáticamente cuando ocurren excepciones
//              o códigos de estado HTTP (404, 500, etc.) según la configuración
//              en Program.cs (UseExceptionHandler, UseStatusCodePagesWithReExecute).
// AUTOR: System CMS - BITI Solutions S.A
// CREADO: 2024
// ================================================================================

using CMS.UI.Models;
using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CMS.UI.Controllers
{
    /// <summary>
    /// Controller principal del sistema que maneja la página de inicio y errores.
    /// Requiere autenticación para todas las acciones (atributo [Authorize] a nivel de clase).
    /// </summary>
    [Authorize] // Todas las acciones requieren autenticación con Azure AD
    public class HomeController : Controller
    {
        private readonly MenuApiService _api;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        /// <param name="api">Servicio para obtener los menús desde la API REST</param>
        /// <param name="logger">Logger para registrar eventos y errores</param>
        public HomeController(MenuApiService api, ILogger<HomeController> logger)
        {
            _api = api;
            _logger = logger;
        }

        /// <summary>
        /// Acción principal que muestra el Dashboard o página de inicio del sistema.
        /// Obtiene los menús desde la API REST para renderizarlos en la vista.
        /// 
        /// RUTA: GET /Home/Index o GET /
        /// 
        /// FLUJO:
        /// 1. El usuario se autentica exitosamente con Azure AD
        /// 2. Es redirigido a esta acción (página de inicio)
        /// 3. Se obtienen los menús filtrados por permisos del usuario
        /// 4. Se renderizan en la vista Index.cshtml
        /// 
        /// SEGURIDAD:
        /// - Requiere autenticación (atributo [Authorize] heredado de la clase)
        /// - Los menús ya vienen filtrados por permisos desde el backend
        /// </summary>
        /// <returns>
        /// Vista Index.cshtml con la lista de menús del usuario autenticado.
        /// Si falla la API, devuelve una lista vacía para evitar errores de vista.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener menús desde la API (ya filtrados por permisos del usuario)
                var menus = await _api.GetMenusAsync();

                // Log informativo para debugging
                _logger.LogInformation(
                    "Usuario {User} accedió al Dashboard con {MenuCount} menús disponibles",
                    User.Identity?.Name ?? "Desconocido",
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
                // El usuario verá el dashboard sin menús
                return View(new List<CMS.Application.DTOs.MenuDto>());
            }
        }

        /// <summary>
        /// Acción que maneja los errores HTTP y excepciones no controladas del sistema.
        /// Se invoca automáticamente según la configuración de Program.cs:
        /// - app.UseExceptionHandler("/Home/Error") → Excepciones no controladas
        /// - app.UseStatusCodePagesWithReExecute("/Home/Error/{0}") → Códigos HTTP (404, 500, etc.)
        /// 
        /// RUTA: GET /Home/Error/{statusCode?}
        /// 
        /// FLUJO:
        /// 1. Ocurre un error en la aplicación (excepción o código HTTP no exitoso)
        /// 2. El middleware de excepciones captura el error
        /// 3. Redirige a esta acción con el código de estado (opcional)
        /// 4. Se crea un ErrorViewModel con información contextual
        /// 5. Se renderiza la vista Error.cshtml
        /// 
        /// EJEMPLOS DE USO:
        /// - Usuario intenta acceder a /SomeController/NonExistent → 404 → /Home/Error/404
        /// - Excepción de base de datos → 500 → /Home/Error/500
        /// - Timeout en llamada a API → 500 → /Home/Error/500
        /// 
        /// SEGURIDAD:
        /// - NO expone detalles técnicos en producción (stack trace, mensajes internos)
        /// - Muestra RequestId para correlacionar errores en logs
        /// - Permite al soporte técnico rastrear problemas específicos
        /// 
        /// NOTAS:
        /// - [AllowAnonymous]: Permite acceso sin autenticación para mostrar errores de login
        /// - [ResponseCache]: Evita que los navegadores cacheen las páginas de error
        /// </summary>
        /// <param name="statusCode">
        /// Código de estado HTTP opcional (404, 500, 403, etc.)
        /// Si es null, se trata de una excepción no controlada sin código específico.
        /// </param>
        /// <returns>
        /// Vista Error.cshtml con un ErrorViewModel que contiene:
        /// - RequestId: Para rastreo en logs
        /// - StatusCode: Código HTTP si está disponible
        /// - ErrorMessage: Mensaje amigable según el código
        /// - RequestPath: URL que causó el error
        /// </returns>
        [AllowAnonymous] // Permite acceso sin autenticación (ej: errores en login)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
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
                    "Error HTTP {StatusCode} en ruta {Path}. RequestId: {RequestId}. Usuario: {User}",
                    statusCode,
                    requestPath,
                    requestId,
                    User.Identity?.Name ?? "Anónimo"
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

            // Retornar vista de error con el modelo
            return View(model);
        }

        // ========================================================================
        // ACCIONES ADICIONALES OPCIONALES (Descomenta si las necesitas)
        // ========================================================================

        /// <summary>
        /// Página Acerca de / About del sistema.
        /// GET: /Home/About
        /// </summary>
        /*
        public IActionResult About()
        {
            return View();
        }
        */

        /// <summary>
        /// Página de Contacto del sistema.
        /// GET: /Home/Contact
        /// </summary>
        /*
        public IActionResult Contact()
        {
            return View();
        }
        */

        /// <summary>
        /// Página de Política de Privacidad.
        /// GET: /Home/Privacy
        /// </summary>
        /*
        [AllowAnonymous] // Puede verse sin autenticación
        public IActionResult Privacy()
        {
            return View();
        }
        */
    }
}