using CMS.Data.Services;
using CMS.Entities.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CostCenterController : ControllerBase
{
    private readonly ICostCenterService _costCenterService;
    private readonly ILogger<CostCenterController> _logger;

    public CostCenterController(ICostCenterService costCenterService, ILogger<CostCenterController> logger)
    {
        _costCenterService = costCenterService;
        _logger = logger;
    }

    private int GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("CompanyId no encontrado en el token");
        }
        return companyId;
    }

    /// <summary>
    /// Obtener todos los centros de costo con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CostCenterDto>>> GetCostCenters(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? costCenterType = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isPostingAllowed = null)
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenters = await _costCenterService.GetCostCentersAsync(companyId, isActive, costCenterType, category, isPostingAllowed);
            var dtos = costCenters.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost centers");
            return StatusCode(500, new { message = "Error al obtener centros de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener jerarquía completa de centros de costo
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<List<CostCenterDto>>> GetHierarchy()
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenters = await _costCenterService.GetHierarchyAsync(companyId);
            var dtos = costCenters.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost center hierarchy");
            return StatusCode(500, new { message = "Error al obtener jerarquía de centros de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener solo centros de costo que permiten imputación directa
    /// </summary>
    [HttpGet("posting")]
    public async Task<ActionResult<List<CostCenterDto>>> GetPostingCostCenters()
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenters = await _costCenterService.GetPostingCostCentersAsync(companyId);
            var dtos = costCenters.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posting cost centers");
            return StatusCode(500, new { message = "Error al obtener centros de costo para imputación", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener centros de costo válidos a una fecha específica
    /// </summary>
    [HttpGet("valid")]
    public async Task<ActionResult<List<CostCenterDto>>> GetValidCostCenters([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenters = await _costCenterService.GetValidCostCentersAsync(companyId, asOfDate);
            var dtos = costCenters.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid cost centers");
            return StatusCode(500, new { message = "Error al obtener centros de costo válidos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un centro de costo por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CostCenterDto>> GetCostCenterById(int id)
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenter = await _costCenterService.GetCostCenterByIdAsync(companyId, id);

            if (costCenter == null)
                return NotFound(new { message = $"Centro de costo con ID {id} no encontrado" });

            return Ok(MapToDto(costCenter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost center {Id}", id);
            return StatusCode(500, new { message = "Error al obtener centro de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un centro de costo por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<CostCenterDto>> GetCostCenterByCode(string code)
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenter = await _costCenterService.GetCostCenterByCodeAsync(companyId, code);

            if (costCenter == null)
                return NotFound(new { message = $"Centro de costo con código '{code}' no encontrado" });

            return Ok(MapToDto(costCenter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost center by code {Code}", code);
            return StatusCode(500, new { message = "Error al obtener centro de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener centros hijos de un padre específico
    /// </summary>
    [HttpGet("{id}/children")]
    public async Task<ActionResult<List<CostCenterDto>>> GetChildren(int id)
    {
        try
        {
            var companyId = GetCompanyId();
            var children = await _costCenterService.GetChildrenAsync(companyId, id);
            var dtos = children.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting children for cost center {Id}", id);
            return StatusCode(500, new { message = "Error al obtener centros hijos", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear un nuevo centro de costo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CostCenterDto>> CreateCostCenter([FromBody] CostCenterDto dto)
    {
        try
        {
            var companyId = GetCompanyId();
            var costCenter = MapToEntity(dto);
            costCenter.CreatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            costCenter.UpdatedBy = costCenter.CreatedBy;

            var created = await _costCenterService.CreateCostCenterAsync(companyId, costCenter);
            return CreatedAtAction(nameof(GetCostCenterById), new { id = created.IdCostCenter }, MapToDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cost center");
            return StatusCode(500, new { message = "Error al crear centro de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un centro de costo existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CostCenterDto>> UpdateCostCenter(int id, [FromBody] CostCenterDto dto)
    {
        try
        {
            if (id != dto.IdCostCenter)
                return BadRequest(new { message = "El ID de la URL no coincide con el ID del objeto" });

            var companyId = GetCompanyId();
            var costCenter = MapToEntity(dto);
            costCenter.UpdatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            var updated = await _costCenterService.UpdateCostCenterAsync(companyId, costCenter);
            return Ok(MapToDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost center {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar centro de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar un centro de costo
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCostCenter(int id)
    {
        try
        {
            var companyId = GetCompanyId();
            var deleted = await _costCenterService.DeleteCostCenterAsync(companyId, id);

            if (!deleted)
                return NotFound(new { message = $"Centro de costo con ID {id} no encontrado" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cost center {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar centro de costo", error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar si un código ya existe
    /// </summary>
    [HttpGet("code-exists/{code}")]
    public async Task<ActionResult<bool>> CodeExists(string code, [FromQuery] int? excludeId = null)
    {
        try
        {
            var companyId = GetCompanyId();
            var exists = await _costCenterService.CodeExistsAsync(companyId, code, excludeId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if code exists");
            return StatusCode(500, new { message = "Error al verificar código", error = ex.Message });
        }
    }

    // ===== MAPEO ENTRE ENTIDAD Y DTO =====

    private static CostCenterDto MapToDto(CostCenter entity)
    {
        return new CostCenterDto
        {
            IdCostCenter = entity.IdCostCenter,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IdParentCostCenter = entity.IdParentCostCenter,
            HierarchyLevel = entity.HierarchyLevel,
            FullPath = entity.FullPath,
            CostCenterType = entity.CostCenterType,
            Category = entity.Category,
            ResponsibleUserId = entity.ResponsibleUserId,
            ResponsibleName = entity.ResponsibleName,
            Location = entity.Location,
            Department = entity.Department,
            Division = entity.Division,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            AnnualBudget = entity.AnnualBudget,
            BudgetCurrency = entity.BudgetCurrency,
            AllowOverBudget = entity.AllowOverBudget,
            IsPostingAllowed = entity.IsPostingAllowed,
            IsBlocked = entity.IsBlocked,
            IsActive = entity.IsActive,
            ProfitCenterCode = entity.ProfitCenterCode,
            BusinessAreaCode = entity.BusinessAreaCode,
            CompanyCode = entity.CompanyCode,
            Notes = entity.Notes,
            ParentCostCenterCode = entity.ParentCostCenter?.Code,
            ParentCostCenterName = entity.ParentCostCenter?.Name
        };
    }

    private static CostCenter MapToEntity(CostCenterDto dto)
    {
        return new CostCenter
        {
            IdCostCenter = dto.IdCostCenter,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            IdParentCostCenter = dto.IdParentCostCenter,
            HierarchyLevel = dto.HierarchyLevel,
            FullPath = dto.FullPath,
            CostCenterType = dto.CostCenterType,
            Category = dto.Category ?? string.Empty,
            ResponsibleUserId = dto.ResponsibleUserId,
            ResponsibleName = dto.ResponsibleName,
            Location = dto.Location,
            Department = dto.Department,
            Division = dto.Division,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            AnnualBudget = dto.AnnualBudget,
            BudgetCurrency = dto.BudgetCurrency,
            AllowOverBudget = dto.AllowOverBudget,
            IsPostingAllowed = dto.IsPostingAllowed,
            IsBlocked = dto.IsBlocked,
            IsActive = dto.IsActive,
            ProfitCenterCode = dto.ProfitCenterCode,
            BusinessAreaCode = dto.BusinessAreaCode,
            CompanyCode = dto.CompanyCode,
            Notes = dto.Notes
        };
    }
}

// ===== DTO =====

public class CostCenterDto
{
    public int IdCostCenter { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? IdParentCostCenter { get; set; }
    public int HierarchyLevel { get; set; }
    public string? FullPath { get; set; }
    public string CostCenterType { get; set; } = CostCenter.CostCenterTypes.Operational;
    public string? Category { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public decimal? AnnualBudget { get; set; }
    public string BudgetCurrency { get; set; } = "CRC";
    public bool AllowOverBudget { get; set; }
    public bool IsPostingAllowed { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsActive { get; set; }
    public string? ProfitCenterCode { get; set; }
    public string? BusinessAreaCode { get; set; }
    public string? CompanyCode { get; set; }
    public string? Notes { get; set; }

    // Información del padre (solo lectura)
    public string? ParentCostCenterCode { get; set; }
    public string? ParentCostCenterName { get; set; }
}
