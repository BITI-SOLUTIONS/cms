// ================================================================================
// ARCHIVO: CMS.API/Controllers/MenuController.cs
// PROPÓSITO: Controlador REST para gestión de menús
// DESCRIPCIÓN: REQUIERE JWT, devuelve menús filtrados por permisos del token
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-11
// ================================================================================

using CMS.Data;
using CMS.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    /// <summary>
    /// Controlador REST para la gestión de menús de navegación.
    /// REQUIERE autenticación JWT y devuelve menús filtrados por permisos.
    /// </summary>
    [Authorize]  // ⭐ REQUIERE JWT
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<MenuController> _logger;

        public MenuController(AppDbContext db, ILogger<MenuController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los menús disponibles para el usuario autenticado.
        /// Filtra según los permisos incluidos en el JWT.
        /// </summary>
        /// <returns>
        /// JSON:
        /// {
        ///   "success": true,
        ///   "count": 10,
        ///   "data": [ array de MenuDto en camelCase ]
        /// }
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // =====================================================================
                // 1. EXTRAER INFO DEL TOKEN JWT
                // =====================================================================
                var userIdClaim = User.FindFirst("userId")?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("❌ Token sin userId claim");
                    return Unauthorized(new { success = false, message = "Token inválido" });
                }

                // Obtener permisos del token (ya vienen en el JWT como claims)
                var permissions = User.FindAll("permission")
                                     .Select(c => c.Value)
                                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation("👤 Usuario: {UserName} (ID: {UserId}), Permisos: {Count}",
                    userName, userIdClaim, permissions.Count);

                // =====================================================================
                // 2. CARGAR TODOS LOS MENÚS ACTIVOS
                // =====================================================================
                var allMenus = await _db.Menus
                    .Where(m => m.IS_ACTIVE)
                    .OrderBy(m => m.ID_PARENT)
                    .ThenBy(m => m.ORDER)
                    .ToListAsync();

                _logger.LogInformation("📋 Menús activos: {Count}", allMenus.Count);

                // =====================================================================
                // 3. FILTRAR MENÚS SEGÚN PERMISOS DEL TOKEN
                // =====================================================================
                var filteredMenus = allMenus
                    .Where(m => string.IsNullOrEmpty(m.PERMISSION_KEY) ||
                               permissions.Contains(m.PERMISSION_KEY))
                    .ToList();

                _logger.LogInformation("🔒 Menús filtrados: {Filtered}/{Total}",
                    filteredMenus.Count, allMenus.Count);

                // =====================================================================
                // 4. CONVERTIR A DTOs (camelCase)
                // =====================================================================
                var menuDtos = filteredMenus.Select(m => new MenuDto
                {
                    IdMenu = m.ID_MENU,
                    IdParent = m.ID_PARENT,
                    Name = m.NAME,
                    Url = m.URL,
                    Icon = m.ICON,
                    Order = m.ORDER,
                    PermissionKey = m.PERMISSION_KEY,
                    IsActive = m.IS_ACTIVE
                }).ToList();

                // =====================================================================
                // 5. RESPUESTA
                // =====================================================================
                return Ok(new
                {
                    success = true,
                    count = menuDtos.Count,
                    data = menuDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo menús");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }
    }
}