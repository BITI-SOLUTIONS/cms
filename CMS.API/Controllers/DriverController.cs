// ================================================================================
// ARCHIVO: CMS.API/Controllers/DriverController.cs
// PROPÓSITO: API REST CRUD para conductores de la flota (por compañía)
// DESCRIPCIÓN: Gestión de conductores vinculados a usuarios activos del sistema.
//              Tabla en la BD de cada compañía, schema {company_code}.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

using CMS.Data;
using CMS.Data.Services;
using CMS.Entities;
using CMS.Entities.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/drivers")]
    public class DriverController : ControllerBase
    {
        private readonly ICompanyDbContextFactory _factory;
        private readonly AppDbContext _adminDb;
        private readonly ILogger<DriverController> _logger;

        public DriverController(
            ICompanyDbContextFactory factory,
            AppDbContext adminDb,
            ILogger<DriverController> logger)
        {
            _factory = factory;
            _adminDb = adminDb;
            _logger = logger;
        }

        private int GetCurrentCompanyId()
        {
            var v = User.FindFirst("companyId")?.Value ?? User.FindFirst("CompanyId")?.Value;
            if (int.TryParse(v, out var id)) return id;
            throw new UnauthorizedAccessException("companyId no encontrado en el token JWT");
        }

        private string GetCurrentUser() =>
            User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "system";

        // ============================================================
        // GET /api/drivers
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                await using var db = await _factory.CreateDbContextAsync(companyId);

                var q = db.Drivers.AsQueryable();
                if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.ToLower();
                    q = q.Where(x =>
                        x.FullName.ToLower().Contains(s) ||
                        x.Code.ToLower().Contains(s) ||
                        x.IdNumber.ToLower().Contains(s) ||
                        (x.Email != null && x.Email.ToLower().Contains(s)) ||
                        (x.LicenseNumber != null && x.LicenseNumber.ToLower().Contains(s)));
                }

                var total = await q.CountAsync();
                var items = await q.OrderBy(x => x.FullName)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

                // Enriquecer con nombre de usuario del sistema si aplica
                var userIds = items.Where(x => x.IdSystemUser.HasValue)
                                   .Select(x => x.IdSystemUser!.Value)
                                   .Distinct()
                                   .ToList();
                var users = await _adminDb.Users
                    .Where(u => userIds.Contains(u.ID_USER))
                    .Select(u => new { Id = u.ID_USER, Name = u.DISPLAY_NAME, Email = u.EMAIL })
                    .ToListAsync();

                var result = items.Select(d => new
                {
                    d.Id,
                    d.Code,
                    d.FirstName,
                    d.LastName,
                    d.SecondLastName,
                    d.FullName,
                    d.IdNumber,
                    d.IdType,
                    d.Phone,
                    d.Mobile,
                    d.Email,
                    d.Address,
                    d.LicenseNumber,
                    d.LicenseCategory,
                    d.LicenseExpiryDate,
                    d.HireDate,
                    d.Position,
                    d.EmergencyContactName,
                    d.EmergencyContactPhone,
                    d.IsActive,
                    d.Notes,
                    d.IdSystemUser,
                    SystemUserName = users.FirstOrDefault(u => u.Id == d.IdSystemUser)?.Name,
                    SystemUserEmail = users.FirstOrDefault(u => u.Id == d.IdSystemUser)?.Email,
                    d.CreateDate,
                    d.RecordDate
                });

                return Ok(new { total, page, pageSize, items = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conductores");
                return StatusCode(500, new { message = "Error al obtener conductores." });
            }
        }

        // ============================================================
        // GET /api/drivers/{id}
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                await using var db = await _factory.CreateDbContextAsync(companyId);

                var driver = await db.Drivers.FindAsync(id);
                if (driver == null) return NotFound(new { message = "Conductor no encontrado." });

                User? sysUser = null;
                if (driver.IdSystemUser.HasValue)
                    sysUser = await _adminDb.Users.FindAsync(driver.IdSystemUser.Value);

                return Ok(new
                {
                    driver.Id,
                    driver.Code,
                    driver.FirstName,
                    driver.LastName,
                    driver.SecondLastName,
                    driver.FullName,
                    driver.IdNumber,
                    driver.IdType,
                    driver.Phone,
                    driver.Mobile,
                    driver.Email,
                    driver.Address,
                    driver.LicenseNumber,
                    driver.LicenseCategory,
                    driver.LicenseExpiryDate,
                    driver.HireDate,
                    driver.Position,
                    driver.EmergencyContactName,
                    driver.EmergencyContactPhone,
                    driver.IsActive,
                    driver.Notes,
                    driver.IdSystemUser,
                    SystemUserName = sysUser?.DISPLAY_NAME,
                    SystemUserEmail = sysUser?.EMAIL
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conductor {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el conductor." });
            }
        }

        // ============================================================
        // GET /api/drivers/system-users — Usuarios activos de la compañía
        // ============================================================
        [HttpGet("system-users")]
        public async Task<IActionResult> GetSystemUsers()
        {
            try
            {
                var companyId = GetCurrentCompanyId();

                var users = await _adminDb.UserCompanies
                    .Where(uc => uc.ID_COMPANY == companyId && uc.IS_ACTIVE)
                    .Join(_adminDb.Users,
                          uc => uc.ID_USER,
                          u => u.ID_USER,
                          (uc, u) => new { Id = u.ID_USER, Name = u.DISPLAY_NAME, Email = u.EMAIL, IsActive = u.IS_ACTIVE })
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios del sistema");
                return StatusCode(500, new { message = "Error al obtener usuarios del sistema." });
            }
        }

        // ============================================================
        // POST /api/drivers
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Driver dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var companyId = GetCurrentCompanyId();
                await using var db = await _factory.CreateDbContextAsync(companyId);

                if (await db.Drivers.AnyAsync(x => x.Code == dto.Code))
                    return Conflict(new { message = $"El código '{dto.Code}' ya existe." });

                if (await db.Drivers.AnyAsync(x => x.IdNumber == dto.IdNumber))
                    return Conflict(new { message = $"El número de identificación '{dto.IdNumber}' ya existe." });

                // Validar que el usuario del sistema exista y esté activo
                if (dto.IdSystemUser.HasValue)
                {
                    var sysUser = await _adminDb.Users.FindAsync(dto.IdSystemUser.Value);
                    if (sysUser == null || !sysUser.IS_ACTIVE)
                        return BadRequest(new { message = "El usuario del sistema especificado no existe o no está activo." });
                }

                dto.FullName = $"{dto.FirstName} {dto.LastName} {dto.SecondLastName}".Trim();
                dto.CreatedBy = GetCurrentUser();
                dto.UpdatedBy = GetCurrentUser();
                dto.CreateDate = DateTime.UtcNow;
                dto.RecordDate = DateTime.UtcNow;
                dto.Rowpointer = Guid.NewGuid();

                db.Drivers.Add(dto);
                await db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new { dto.Id, dto.Code, dto.FullName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conductor");
                return StatusCode(500, new { message = "Error al crear el conductor." });
            }
        }

        // ============================================================
        // PUT /api/drivers/{id}
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Driver dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var companyId = GetCurrentCompanyId();
                await using var db = await _factory.CreateDbContextAsync(companyId);

                var entity = await db.Drivers.FindAsync(id);
                if (entity == null) return NotFound(new { message = "Conductor no encontrado." });

                if (await db.Drivers.AnyAsync(x => x.Code == dto.Code && x.Id != id))
                    return Conflict(new { message = $"El código '{dto.Code}' ya existe en otro conductor." });

                if (await db.Drivers.AnyAsync(x => x.IdNumber == dto.IdNumber && x.Id != id))
                    return Conflict(new { message = $"El número de identificación '{dto.IdNumber}' ya existe en otro conductor." });

                if (dto.IdSystemUser.HasValue)
                {
                    var sysUser = await _adminDb.Users.FindAsync(dto.IdSystemUser.Value);
                    if (sysUser == null || !sysUser.IS_ACTIVE)
                        return BadRequest(new { message = "El usuario del sistema especificado no existe o no está activo." });
                }

                entity.Code = dto.Code;
                entity.FirstName = dto.FirstName;
                entity.LastName = dto.LastName;
                entity.SecondLastName = dto.SecondLastName;
                entity.FullName = $"{dto.FirstName} {dto.LastName} {dto.SecondLastName}".Trim();
                entity.IdNumber = dto.IdNumber;
                entity.IdType = dto.IdType;
                entity.Phone = dto.Phone;
                entity.Mobile = dto.Mobile;
                entity.Email = dto.Email;
                entity.Address = dto.Address;
                entity.LicenseNumber = dto.LicenseNumber;
                entity.LicenseCategory = dto.LicenseCategory;
                entity.LicenseExpiryDate = dto.LicenseExpiryDate;
                entity.HireDate = dto.HireDate;
                entity.Position = dto.Position;
                entity.EmergencyContactName = dto.EmergencyContactName;
                entity.EmergencyContactPhone = dto.EmergencyContactPhone;
                entity.IsActive = dto.IsActive;
                entity.Notes = dto.Notes;
                entity.IdSystemUser = dto.IdSystemUser;
                entity.UpdatedBy = GetCurrentUser();
                entity.RecordDate = DateTime.UtcNow;

                await db.SaveChangesAsync();
                return Ok(new { entity.Id, entity.Code, entity.FullName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar conductor {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el conductor." });
            }
        }

        // ============================================================
        // DELETE /api/drivers/{id}
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                await using var db = await _factory.CreateDbContextAsync(companyId);

                var entity = await db.Drivers.FindAsync(id);
                if (entity == null) return NotFound(new { message = "Conductor no encontrado." });

                entity.IsActive = false;
                entity.UpdatedBy = GetCurrentUser();
                entity.RecordDate = DateTime.UtcNow;

                await db.SaveChangesAsync();
                return Ok(new { message = "Conductor desactivado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar conductor {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el conductor." });
            }
        }
    }
}
