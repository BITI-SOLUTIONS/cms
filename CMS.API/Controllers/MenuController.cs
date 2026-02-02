using CMS.Data.Services;
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.API.Controllers
{
    /// <summary>
    /// Controlador REST para la gestión de menús de navegación del sistema.
    /// Proporciona endpoints para obtener menús filtrados por permisos del usuario.
    /// 
    /// Características:
    /// - Devuelve solo menús activos (IS_ACTIVE = true)
    /// - Filtra menús según permisos del usuario autenticado
    /// - Si no hay usuario autenticado, devuelve todos los menús activos (Swagger)
    /// - Ordena menús por padre y luego por orden
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PermissionService _permService;
        private readonly ILogger<MenuController> _logger;

        /// <summary>
        /// Constructor del controlador de menús.
        /// </summary>
        /// <param name="db">Contexto de base de datos</param>
        /// <param name="permService">Servicio de cálculo de permisos</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public MenuController(AppDbContext db, PermissionService permService, ILogger<MenuController> logger)
        {
            _db = db;
            _permService = permService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los menús disponibles para el usuario actual.
        /// 
        /// Proceso:
        /// 1. Obtener usuario autenticado desde Azure AD (OID)
        /// 2. Cargar menús ACTIVOS (IS_ACTIVE = true)
        /// 3. Si no hay usuario → devolver todos los menús (Swagger/pruebas)
        /// 4. Si hay usuario → filtrar según permisos
        /// 5. Ordenar por padre y orden
        /// 
        /// Endpoint: GET /api/menu
        /// Autenticación: Requerida (pero funciona sin ella para Swagger)
        /// </summary>
        /// <returns>
        /// Respuesta JSON con estructura:
        /// {
        ///   "success": true,
        ///   "count": 15,
        ///   "data": [ { menús filtrados } ]
        /// }
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                /////////////////////////////////////////////////////////////////
                // 1) Intentar obtener usuario autenticado (si viene desde UI)
                /////////////////////////////////////////////////////////////////
                var oid = User.FindFirst("oid")?.Value;
                int? userId = null;

                if (!string.IsNullOrEmpty(oid) && Guid.TryParse(oid, out Guid azureOid))
                {
                    var user = await _db.Users
                        .FirstOrDefaultAsync(u => u.AZURE_OID == azureOid);

                    if (user != null)
                        userId = user.ID_USER;
                }

                /////////////////////////////////////////////////////////////////
                // 2) Cargar menú ACTIVO y ordenado
                /////////////////////////////////////////////////////////////////
                var menus = await _db.Menus
                    .Where(m => m.IS_ACTIVE == true)  // ✅ CORREGIDO: ACTIVE → IS_ACTIVE
                    .OrderBy(m => m.ID_PARENT)
                    .ThenBy(m => m.ORDER)
                    .ToListAsync();

                _logger.LogInformation("📋 Menús activos cargados: {Count}", menus.Count);

                /////////////////////////////////////////////////////////////////
                // 3) Si NO hay usuario → devolver TODO (Swagger / pruebas)
                /////////////////////////////////////////////////////////////////
                if (userId == null)
                {
                    _logger.LogInformation("⚠️ Sin usuario autenticado - Devolviendo todos los menús activos");
                    return Ok(new
                    {
                        success = true,
                        count = menus.Count,
                        data = menus
                    });
                }

                /////////////////////////////////////////////////////////////////
                // 4) Obtener permisos del usuario
                /////////////////////////////////////////////////////////////////
                var perms = await _permService.GetUserPermissionsAsync(userId.Value);
                _logger.LogInformation("🔑 Permisos del usuario {UserId}: {Count}", userId, perms.Count);

                /////////////////////////////////////////////////////////////////
                // 5) Filtrar según permisos
                /////////////////////////////////////////////////////////////////
                var filtered = menus
                    .Where(m =>
                        string.IsNullOrEmpty(m.PERMISSION_KEY) ||
                        perms.Contains(m.PERMISSION_KEY))
                    .ToList();

                _logger.LogInformation("✅ Menús filtrados por permisos: {Count}/{Total}", filtered.Count, menus.Count);

                /////////////////////////////////////////////////////////////////
                // 6) RESPUESTA FILTRADA
                /////////////////////////////////////////////////////////////////
                return Ok(new
                {
                    success = true,
                    count = filtered.Count,
                    data = filtered
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener menús");
                return StatusCode(500, new { message = "Error al obtener menús" });
            }
        }
    }
}