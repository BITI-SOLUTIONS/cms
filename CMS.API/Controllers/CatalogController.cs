// ================================================================================
// ARCHIVO: CMS.API/Controllers/CatalogController.cs
// PROPÓSITO: Endpoints para catálogos del sistema (países, géneros, etc.)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

using CMS.Data;
using CMS.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CompanyConfigService _companyService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(
            AppDbContext db, 
            CompanyConfigService companyService,
            ILogger<CatalogController> logger)
        {
            _db = db;
            _companyService = companyService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener lista de países desde la base de datos
        /// GET: api/catalog/countries
        /// </summary>
        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _db.Countries
                    .Where(c => c.IS_ACTIVE)
                    .OrderBy(c => c.NAME)
                    .Select(c => new CatalogItem
                    {
                        Id = c.ID_COUNTRY,
                        Name = c.NAME,
                        Code = c.ISO2_CODE
                    })
                    .ToListAsync();

                _logger.LogInformation("📋 Países obtenidos: {Count}", countries.Count);

                return Ok(countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo países de BD, usando fallback");

                // Fallback con datos estáticos
                var fallback = new List<CatalogItem>
                {
                    new() { Id = 1, Name = "Costa Rica", Code = "CR" },
                    new() { Id = 2, Name = "Estados Unidos", Code = "US" },
                    new() { Id = 3, Name = "México", Code = "MX" },
                };
                return Ok(fallback);
            }
        }

        /// <summary>
        /// Obtener lista de géneros desde la base de datos
        /// GET: api/catalog/genders
        /// </summary>
        [HttpGet("genders")]
        public async Task<IActionResult> GetGenders()
        {
            try
            {
                var genders = await _db.Genders
                    .Where(g => g.IS_ACTIVE)
                    .OrderBy(g => g.GENDER_CODE)
                    .Select(g => new CatalogItem
                    {
                        Id = g.ID_GENDER,
                        Name = g.DESCRIPTION,
                        Code = g.GENDER_CODE
                    })
                    .ToListAsync();

                _logger.LogInformation("📋 Géneros obtenidos: {Count}", genders.Count);

                return Ok(genders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo géneros de BD, usando fallback");

                // Fallback con datos estáticos
                var fallback = new List<CatalogItem>
                {
                    new() { Id = 1, Name = "Masculino", Code = "M" },
                    new() { Id = 2, Name = "Femenino", Code = "F" },
                    new() { Id = 3, Name = "Otro", Code = "O" },
                };
                return Ok(fallback);
            }
        }

        /// <summary>
        /// Obtener lista de idiomas desde la base de datos
        /// GET: api/catalog/languages
        /// </summary>
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            try
            {
                var languages = await _db.Languages
                    .Where(l => l.IS_ACTIVE)
                    .OrderBy(l => l.LANGUAGE_NAME)
                    .Select(l => new CatalogItem
                    {
                        Id = l.ID_LANGUAGE,
                        Name = l.LANGUAGE_NAME,
                        Code = l.LANGUAGE_CODE
                    })
                    .ToListAsync();

                _logger.LogInformation("📋 Idiomas obtenidos: {Count}", languages.Count);

                return Ok(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo idiomas de BD, usando fallback");

                var fallback = new List<CatalogItem>
                {
                    new() { Id = 1832, Name = "Español", Code = "es" },
                    new() { Id = 1833, Name = "English", Code = "en" },
                };
                return Ok(fallback);
            }
        }

        /// <summary>
        /// Obtener lista de roles activos
        /// GET: api/catalog/roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _db.Roles
                    .Where(r => r.IS_ACTIVE)
                    .OrderBy(r => r.ROLE_NAME)
                    .Select(r => new CatalogItem
                    {
                        Id = r.ID_ROLE,
                        Name = r.ROLE_NAME,
                        Code = r.IS_SYSTEM ? "SYSTEM" : "CUSTOM"
                    })
                    .ToListAsync();

                _logger.LogInformation("📋 Roles obtenidos: {Count}", roles.Count);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                return StatusCode(500, new { message = "Error obteniendo roles" });
            }
        }

        /// <summary>
        /// Obtener lista de zonas horarias
        /// GET: api/catalog/timezones
        /// </summary>
        [HttpGet("timezones")]
        public async Task<IActionResult> GetTimezones()
        {
            try
            {
                var timezones = new List<CatalogItem>
                {
                    new() { Id = 1, Name = "Costa Rica (UTC-6)", Code = "America/Costa_Rica" },
                    new() { Id = 2, Name = "México (UTC-6)", Code = "America/Mexico_City" },
                    new() { Id = 3, Name = "Nueva York (UTC-5)", Code = "America/New_York" },
                    new() { Id = 4, Name = "Los Ángeles (UTC-8)", Code = "America/Los_Angeles" },
                    new() { Id = 5, Name = "Madrid (UTC+1)", Code = "Europe/Madrid" },
                    new() { Id = 6, Name = "Londres (UTC+0)", Code = "Europe/London" },
                    new() { Id = 7, Name = "Bogotá (UTC-5)", Code = "America/Bogota" },
                    new() { Id = 8, Name = "Buenos Aires (UTC-3)", Code = "America/Argentina/Buenos_Aires" },
                    new() { Id = 9, Name = "Santiago (UTC-3)", Code = "America/Santiago" },
                    new() { Id = 10, Name = "Lima (UTC-5)", Code = "America/Lima" },
                };

                return Ok(timezones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo zonas horarias");
                return StatusCode(500, new { message = "Error obteniendo zonas horarias" });
            }
        }

        /// <summary>
        /// Obtener lista de estados de usuario
        /// GET: api/catalog/user-statuses
        /// </summary>
        [HttpGet("user-statuses")]
        public async Task<IActionResult> GetUserStatuses()
        {
            try
            {
                var statuses = new List<CatalogItem>
                {
                    new() { Id = 1, Name = "Activo", Code = "active" },
                    new() { Id = 2, Name = "Inactivo", Code = "inactive" },
                    new() { Id = 3, Name = "Bloqueado", Code = "locked" },
                    new() { Id = 4, Name = "Pendiente verificación", Code = "pending" },
                };

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estados de usuario");
                return StatusCode(500, new { message = "Error obteniendo estados" });
            }
        }

        /// <summary>
        /// Obtener lista de compañías visibles para el usuario actual.
        /// - Si el usuario tiene System.ViewAllCompanies en una compañía Admin → ve TODAS
        /// - Si no → solo ve las compañías que tiene asignadas
        /// GET: api/catalog/companies
        /// </summary>
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                // Obtener ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("⚠️ No se pudo obtener el ID del usuario del token");
                    return Unauthorized(new { message = "Usuario no identificado" });
                }

                // Obtener compañías visibles para este usuario
                var companies = await _companyService.GetVisibleCompaniesForUserAsync(userId);

                var result = companies.Select(c => new CatalogItem
                {
                    Id = c.ID,
                    Name = c.COMPANY_NAME,
                    Code = c.COMPANY_SCHEMA
                }).ToList();

                _logger.LogInformation("📋 Compañías visibles para usuario {UserId}: {Count}", userId, result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañías");
                return StatusCode(500, new { message = "Error obteniendo compañías" });
            }
        }

        /// <summary>
        /// Obtener TODAS las compañías activas (solo para Super Admins).
        /// GET: api/catalog/companies/all
        /// </summary>
        [HttpGet("companies/all")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                // Obtener ID del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Usuario no identificado" });
                }

                // Verificar que tenga permiso para ver todas las compañías
                var hasPermission = await _companyService.HasViewAllCompaniesPermissionAsync(userId);

                if (!hasPermission)
                {
                    _logger.LogWarning("⚠️ Usuario {UserId} intentó acceder a todas las compañías sin permiso", userId);
                    return Forbid();
                }

                var companies = await _companyService.GetAllActiveCompaniesAsync();

                var result = companies.Select(c => new CatalogItem
                {
                    Id = c.ID,
                    Name = c.COMPANY_NAME,
                    Code = c.COMPANY_SCHEMA
                }).ToList();

                _logger.LogInformation("📋 Todas las compañías obtenidas por Super Admin {UserId}: {Count}", userId, result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las compañías");
                return StatusCode(500, new { message = "Error obteniendo compañías" });
            }
        }

        public class CatalogItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Code { get; set; }
        }
    }
}
