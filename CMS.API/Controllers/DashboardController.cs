// ================================================================================
// ARCHIVO: CMS.API/Controllers/DashboardController.cs
// PROPÓSITO: Controlador para estadísticas del Dashboard
// DESCRIPCIÓN: Proporciona métricas y datos para el panel principal del sistema
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    /// <summary>
    /// Controlador para obtener estadísticas y datos del Dashboard
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext db, ILogger<DashboardController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las estadísticas del Dashboard para la compañía actual
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            try
            {
                var companyId = GetCurrentCompanyId();

                // Usuarios activos en la compañía
                var activeUsersQuery = _db.UserCompanies
                    .Where(uc => uc.IS_ACTIVE);

                if (companyId.HasValue)
                {
                    activeUsersQuery = activeUsersQuery.Where(uc => uc.ID_COMPANY == companyId.Value);
                }

                var activeUsers = await activeUsersQuery
                    .Select(uc => uc.ID_USER)
                    .Distinct()
                    .CountAsync();

                // Total de roles activos
                var totalRoles = await _db.Roles
                    .Where(r => r.IS_ACTIVE)
                    .CountAsync();

                // Total de permisos activos
                var totalPermissions = await _db.Permissions
                    .Where(p => p.IS_ACTIVE)
                    .CountAsync();

                // Módulos activos (menús de nivel 0)
                var activeModules = await _db.Menus
                    .Where(m => m.IS_ACTIVE && m.ID_PARENT == 0)
                    .CountAsync();

                // Obtener mensaje de bienvenida de la compañía
                string? welcomeMessage = null;
                if (companyId.HasValue)
                {
                    welcomeMessage = await _db.Companies
                        .Where(c => c.ID == companyId.Value)
                        .Select(c => c.DASHBOARD_WELCOME_MESSAGE)
                        .FirstOrDefaultAsync();
                }

                var stats = new DashboardStatsDto
                {
                    ActiveUsers = activeUsers,
                    TotalRoles = totalRoles,
                    TotalPermissions = totalPermissions,
                    ActiveModules = activeModules,
                    WelcomeMessage = welcomeMessage ?? "Bienvenido al Centro de Gestión. Aquí tienes un resumen de tu sistema."
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas del dashboard");
                return StatusCode(500, new { message = "Error obteniendo estadísticas" });
            }
        }

        /// <summary>
        /// Verifica si el usuario actual tiene un permiso específico
        /// </summary>
        [HttpGet("has-permission/{permissionKey}")]
        public ActionResult<bool> HasPermission(string permissionKey)
        {
            var permissions = User.FindAll("permissions")
                .Select(c => c.Value)
                .ToList();

            return Ok(permissions.Contains(permissionKey));
        }

        private int? GetCurrentCompanyId()
        {
            var companyIdClaim = User.FindFirstValue("company_id") ?? User.FindFirstValue("CompanyId");
            if (int.TryParse(companyIdClaim, out var companyId))
                return companyId;
            return null;
        }
    }

    /// <summary>
    /// DTO para estadísticas del Dashboard
    /// </summary>
    public class DashboardStatsDto
    {
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int ActiveModules { get; set; }
        public string WelcomeMessage { get; set; } = string.Empty;
    }
}
