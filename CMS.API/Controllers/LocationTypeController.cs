// ================================================================================
// ARCHIVO: CMS.API/Controllers/LocationTypeController.cs
// PROPÓSITO: API REST para mantenimiento de tipos de localización
// DESCRIPCIÓN: LocationType es un catálogo CENTRAL (admin.location_type, BD cms).
//              Las operaciones CRUD no requieren companyId; solo autenticación.
//              GetLocationCount sí recibe companyId para buscar en BD de compañía.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// MODIFICADO: 2026-07-04 — Migrado a BD central (admin.location_type)
// ================================================================================

using CMS.Data.Services;
using CMS.Entities.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LocationTypeController : ControllerBase
    {
        private readonly ILocationTypeService _service;
        private readonly ILogger<LocationTypeController> _logger;

        public LocationTypeController(ILocationTypeService service, ILogger<LocationTypeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCompanyId()
        {
            var val = User.FindFirstValue("companyId")
                   ?? User.FindFirstValue("CompanyId")
                   ?? User.FindFirstValue("company_id");
            return int.TryParse(val, out var id) ? id : 0;
        }

        private string GetUserName() =>
            User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("preferred_username") ?? "system";

        // GET /api/locationtype
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
        {
            var items = await _service.GetAllAsync(0, isActive);
            return Ok(items.Select(MapToDto));
        }

        // GET /api/locationtype/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(0, id);
            return item == null ? NotFound() : Ok(MapToDto(item));
        }

        // GET /api/locationtype/check-code?code=WAREHOUSE&excludeId=1
        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCode([FromQuery] string code, [FromQuery] int? excludeId = null)
        {
            var exists = await _service.CodeExistsAsync(0, code, excludeId);
            return Ok(new { exists });
        }

        // POST /api/locationtype
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LocationTypeUpsertDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _service.CodeExistsAsync(0, dto.Code))
                return BadRequest(new { message = $"El código '{dto.Code.ToUpper()}' ya existe." });

            var entity = MapFromDto(dto);
            var created = await _service.CreateAsync(0, entity, GetUserName());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        // PUT /api/locationtype/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationTypeUpsertDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _service.CodeExistsAsync(0, dto.Code, id))
                return BadRequest(new { message = $"El código '{dto.Code.ToUpper()}' ya está en uso." });

            var entity = MapFromDto(dto);
            entity.Id = id;

            try
            {
                var updated = await _service.UpdateAsync(0, entity, GetUserName());
                return Ok(MapToDto(updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PATCH /api/locationtype/{id}/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _service.DeactivateAsync(0, id, GetUserName());
            return ok ? Ok(new { message = "Tipo desactivado." }) : NotFound();
        }

        // PATCH /api/locationtype/{id}/activate
        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var ok = await _service.ActivateAsync(0, id, GetUserName());
            return ok ? Ok(new { message = "Tipo activado." }) : NotFound();
        }

        // DELETE /api/locationtype/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCompanyId();
            // Verificar si tiene localizaciones asociadas en la compañía activa del usuario
            if (companyId > 0)
            {
                var count = await _service.GetLocationCountAsync(companyId, id);
                if (count > 0)
                    return BadRequest(new { message = $"No se puede eliminar: tiene {count} localización(es) asociada(s)." });
            }
            var ok = await _service.DeleteAsync(0, id);
            return ok ? Ok(new { message = "Tipo eliminado." }) : NotFound();
        }

        // ── Mappers ──

        private static LocationTypeDto MapToDto(LocationType x) => new()
        {
            Id          = x.Id,
            Code        = x.Code,
            Name        = x.Name,
            Description = x.Description,
            Icon        = x.Icon,
            Color       = x.Color,
            SortOrder   = x.SortOrder,
            IsActive    = x.IsActive,
            CreatedAt   = x.CreateDate,
            CreatedBy   = x.CreatedBy,
            UpdatedAt   = x.RecordDate,
            UpdatedBy   = x.UpdatedBy
        };

        private static LocationType MapFromDto(LocationTypeUpsertDto dto) => new()
        {
            Code        = dto.Code.Trim().ToUpper(),
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Icon        = dto.Icon?.Trim(),
            Color       = dto.Color?.Trim(),
            SortOrder   = dto.SortOrder,
            IsActive    = dto.IsActive
        };
    }

    // ── DTOs ──

    public class LocationTypeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public int LocationCount { get; set; }
    }

    public class LocationTypeUpsertDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(60)]
        public string? Icon { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? Color { get; set; }

        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
