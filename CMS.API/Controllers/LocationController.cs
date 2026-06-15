// ================================================================================
// ARCHIVO: CMS.API/Controllers/LocationController.cs
// PROPÓSITO: API REST para mantenimiento de localizaciones
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Data;
using CMS.Data.Services;
using CMS.Entities.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _service;
        private readonly ILocationTypeService _typeService;
        private readonly AppDbContext _centralDb;
        private readonly ILogger<LocationController> _logger;

        public LocationController(
            ILocationService service,
            ILocationTypeService typeService,
            AppDbContext centralDb,
            ILogger<LocationController> logger)
        {
            _service     = service;
            _typeService = typeService;
            _centralDb   = centralDb;
            _logger      = logger;
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

        // GET /api/location?page=1&pageSize=20&search=&locationTypeId=&isActive=true
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page          = 1,
            [FromQuery] int pageSize      = 20,
            [FromQuery] string? search    = null,
            [FromQuery] int? locationTypeId = null,
            [FromQuery] bool? isActive    = null)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var (items, total) = await _service.GetPagedAsync(companyId, page, pageSize, search, locationTypeId, isActive);
            return Ok(new
            {
                items = items.Select(MapToDto),
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }

        // GET /api/location/by-type/{locationTypeId}?availableOnly=true&currentLocationId=5
        [HttpGet("by-type/{locationTypeId:int}")]
        public async Task<IActionResult> GetByType(
            int locationTypeId,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool availableOnly = false,
            [FromQuery] int? currentLocationId = null)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            IEnumerable<Location> items;
            if (availableOnly)
                items = await _service.GetAvailableByTypeAsync(companyId, locationTypeId, currentLocationId);
            else
                items = await _service.GetByTypeAsync(companyId, locationTypeId, isActive);

            return Ok(items.Select(MapToDto));
        }

        // GET /api/location/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var item = await _service.GetByIdAsync(companyId, id);
            return item == null ? NotFound() : Ok(MapToDto(item));
        }

        // POST /api/location
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LocationUpsertDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var locType = await _typeService.GetByIdAsync(companyId, dto.IdLocationType);
            if (locType == null)
                return BadRequest(new { message = "El tipo de localización no existe." });

            var entity = MapFromDto(dto);
            var created = await _service.CreateAsync(companyId, entity, GetUserName());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        // PUT /api/location/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationUpsertDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var locType = await _typeService.GetByIdAsync(companyId, dto.IdLocationType);
            if (locType == null)
                return BadRequest(new { message = "El tipo de localización no existe." });

            var entity = MapFromDto(dto);
            entity.Id = id;

            try
            {
                var updated = await _service.UpdateAsync(companyId, entity, GetUserName());
                return Ok(MapToDto(updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PATCH /api/location/{id}/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var ok = await _service.DeactivateAsync(companyId, id, GetUserName());
            return ok ? Ok(new { message = "Localización desactivada." }) : NotFound();
        }

        // PATCH /api/location/{id}/activate
        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var ok = await _service.ActivateAsync(companyId, id, GetUserName());
            return ok ? Ok(new { message = "Localización activada." }) : NotFound();
        }

        // DELETE /api/location/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var ok = await _service.DeleteAsync(companyId, id);
            return ok ? Ok(new { message = "Localización eliminada." }) : NotFound();
        }

        // GET /api/location/geo/countries?companyId=4
        // Retorna el país asignado a la compañía (filtrado por admin.company.id_country)
        [HttpGet("geo/countries")]
        public async Task<IActionResult> GetCountries([FromQuery] int? filterCountryId = null)
        {
            var query = _centralDb.Countries.Where(c => c.IS_ACTIVE);
            if (filterCountryId.HasValue)
                query = query.Where(c => c.ID_COUNTRY == filterCountryId.Value);
            var list = await query.OrderBy(c => c.NAME)
                .Select(c => new { id = c.ID_COUNTRY, name = c.NAME, iso2 = c.ISO2_CODE, iso3 = c.ISO3_CODE })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/location/geo/provinces?idCountry=50
        [HttpGet("geo/provinces")]
        public async Task<IActionResult> GetProvinces([FromQuery] int idCountry)
        {
            var list = await _centralDb.GeographicDivisions1
                .Where(p => p.IdCountry == idCountry && p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new { id = p.IdGeographicDivision1, name = p.Name, code = p.Code })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/location/geo/cantons?idProvince=1
        [HttpGet("geo/cantons")]
        public async Task<IActionResult> GetCantons([FromQuery] int idProvince)
        {
            var list = await _centralDb.GeographicDivisions2
                .Where(c => c.IdGeographicDivision1 == idProvince && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { id = c.IdGeographicDivision2, name = c.Name, code = c.Code })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/location/geo/districts?idCanton=1
        [HttpGet("geo/districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] int idCanton)
        {
            var list = await _centralDb.GeographicDivisions3
                .Where(d => d.IdGeographicDivision2 == idCanton && d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.IdGeographicDivision3, name = d.Name, code = d.Code })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/location/geo/neighborhoods?idDistrict=1
        [HttpGet("geo/neighborhoods")]
        public async Task<IActionResult> GetNeighborhoods([FromQuery] int idDistrict)
        {
            var list = await _centralDb.GeographicDivisions4
                .Where(n => n.IdGeographicDivision3 == idDistrict && n.IsActive)
                .OrderBy(n => n.Name)
                .Select(n => new { id = n.IdGeographicDivision4, name = n.Name, code = n.Code, postalCode = n.PostalCode })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/location/geo/postal-code?idCountry=50&idDivision1=1&idDivision2=1&idDivision3=1&idDivision4=1
        [HttpGet("geo/postal-code")]
        public async Task<IActionResult> GetPostalCode(
            [FromQuery] int idCountry, [FromQuery] int idDivision1,
            [FromQuery] int idDivision2, [FromQuery] int idDivision3,
            [FromQuery] int idDivision4)
        {
            var country = await _centralDb.Countries
                .Where(c => c.ID_COUNTRY == idCountry)
                .Select(c => c.NUMERIC_CODE)
                .FirstOrDefaultAsync();
            if (country == null) return BadRequest("País no encontrado");

            var div1 = await _centralDb.GeographicDivisions1
                .Where(x => x.IdGeographicDivision1 == idDivision1)
                .Select(x => x.Code).FirstOrDefaultAsync();
            var div2 = await _centralDb.GeographicDivisions2
                .Where(x => x.IdGeographicDivision2 == idDivision2)
                .Select(x => x.Code).FirstOrDefaultAsync();
            var div3 = await _centralDb.GeographicDivisions3
                .Where(x => x.IdGeographicDivision3 == idDivision3)
                .Select(x => x.Code).FirstOrDefaultAsync();
            var div4 = await _centralDb.GeographicDivisions4
                .Where(x => x.IdGeographicDivision4 == idDivision4)
                .Select(x => x.Code).FirstOrDefaultAsync();

            if (div1 == null || div2 == null || div3 == null || div4 == null)
                return BadRequest("División geográfica no encontrada");

            // Formato: {numeric_code}-{div1}{div2}{div3}-{div4}
            var postalCode = $"{country}-{div1}{div2}{div3}-{div4}";

            // Persistir en geographic_division4
            var div4Entity = await _centralDb.GeographicDivisions4
                .FirstOrDefaultAsync(x => x.IdGeographicDivision4 == idDivision4);
            if (div4Entity != null && div4Entity.PostalCode != postalCode)
            {
                div4Entity.PostalCode = postalCode;
                await _centralDb.SaveChangesAsync();
            }

            return Ok(new { postalCode });
        }



        private static LocationDto MapToDto(Location x) => new()
        {
            Id               = x.Id,
            IdLocationType   = x.IdLocationType,
            LocationTypeName = x.LocationType?.Name,
            LocationTypeIcon = x.LocationType?.Icon,
            LocationTypeColor = x.LocationType?.Color,
            IdCountry              = x.IdCountry,
            IdGeographicDivision1  = x.IdGeographicDivision1,
            IdGeographicDivision2  = x.IdGeographicDivision2,
            IdGeographicDivision3  = x.IdGeographicDivision3,
            IdGeographicDivision4  = x.IdGeographicDivision4,
            Address          = x.Address,
            Address2         = x.Address2,
            PostalCode       = x.PostalCode,
            GpsLatitude      = x.GpsLatitude,
            GpsLongitude     = x.GpsLongitude,
            IsActive         = x.IsActive,
            CreatedAt        = x.CreateDate,
            CreatedBy        = x.CreatedBy,
            UpdatedAt        = x.RecordDate,
            UpdatedBy        = x.UpdatedBy
        };

        private static Location MapFromDto(LocationUpsertDto dto) => new()
        {
            IdLocationType         = dto.IdLocationType,
            IdCountry              = dto.IdCountry,
            IdGeographicDivision1  = dto.IdGeographicDivision1,
            IdGeographicDivision2  = dto.IdGeographicDivision2,
            IdGeographicDivision3  = dto.IdGeographicDivision3,
            IdGeographicDivision4  = dto.IdGeographicDivision4,
            Address         = dto.Address?.Trim(),
            Address2        = dto.Address2?.Trim(),
            PostalCode      = dto.PostalCode?.Trim(),
            GpsLatitude     = dto.GpsLatitude,
            GpsLongitude    = dto.GpsLongitude,
            IsActive        = dto.IsActive
        };
    }

    // ── DTOs ──

    public class LocationDto
    {
        public int Id { get; set; }
        public int IdLocationType { get; set; }
        public string? LocationTypeName { get; set; }
        public string? LocationTypeIcon { get; set; }
        public string? LocationTypeColor { get; set; }
        public int? IdCountry { get; set; }
        public int? IdGeographicDivision1 { get; set; }
        public int? IdGeographicDivision2 { get; set; }
        public int? IdGeographicDivision3 { get; set; }
        public int? IdGeographicDivision4 { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? PostalCode { get; set; }
        public double? GpsLatitude { get; set; }
        public double? GpsLongitude { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class LocationUpsertDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int IdLocationType { get; set; }

        public int? IdCountry { get; set; }
        public int? IdGeographicDivision1 { get; set; }
        public int? IdGeographicDivision2 { get; set; }
        public int? IdGeographicDivision3 { get; set; }
        public int? IdGeographicDivision4 { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Address { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Address2 { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? GpsLatitude { get; set; }
        public double? GpsLongitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
