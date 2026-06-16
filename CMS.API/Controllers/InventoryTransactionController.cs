// ================================================================================
// ARCHIVO: CMS.API/Controllers/InventoryTransactionController.cs
// PROPÓSITO: API REST para gestión de movimientos de inventario
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-13
// ================================================================================

using CMS.Data.Services;
using CMS.Entities.Operational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionService _service;
        private readonly ILogger<InventoryTransactionController> _logger;

        public InventoryTransactionController(
            IInventoryTransactionService service,
            ILogger<InventoryTransactionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ================================================================
        // HELPERS
        // ================================================================

        private int GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value ?? User.FindFirst("CompanyId")?.Value;
            if (int.TryParse(claim, out var id)) return id;
            throw new UnauthorizedAccessException("companyId no encontrado en el token JWT");
        }

        private string GetCurrentUser() =>
            User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "system";

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("userId")?.Value ?? User.FindFirst("UserId")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // ================================================================
        // GET /api/inventorytransaction
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? search = null,
            [FromQuery] string? movementType = null,
            [FromQuery] string? status = null,
            [FromQuery] int? warehouseOriginId = null,
            [FromQuery] int? warehouseDestId = null,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var companyId = GetCurrentCompanyId();

                DateOnly? from = null, to = null;
                if (DateOnly.TryParse(dateFrom, out var df)) from = df;
                if (DateOnly.TryParse(dateTo, out var dt)) to = dt;

                var (items, total) = await _service.GetTransactionsAsync(
                    companyId, search, movementType, status,
                    warehouseOriginId, warehouseDestId, from, to, page, pageSize);

                return Ok(new
                {
                    items = items.Select(t => MapToDto(t)),
                    totalCount = total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo movimientos");
                return StatusCode(500, new { error = "Error al obtener movimientos", detail = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/{id}
        // ================================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var txn = await _service.GetByIdAsync(companyId, id);
                if (txn == null) return NotFound(new { error = "Movimiento no encontrado" });

                var lines = await _service.GetLinesAsync(companyId, id);
                txn.Lines = lines;

                return Ok(MapToDto(txn, includeLines: true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/{id}/lines
        // ================================================================
        [HttpGet("{id:int}/lines")]
        public async Task<IActionResult> GetLines(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var lines = await _service.GetLinesAsync(companyId, id);
                return Ok(lines.Select(MapLineToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo líneas del movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/next-number
        // ================================================================
        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextNumber()
        {
            try
            {
                var number = await _service.GenerateTransactionNumberAsync(GetCurrentCompanyId());
                return Ok(new { transactionNumber = number });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/check-number
        // ================================================================
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckNumber([FromQuery] string number, [FromQuery] int? excludeId = null)
        {
            var exists = await _service.TransactionNumberExistsAsync(GetCurrentCompanyId(), number, excludeId);
            return Ok(new { exists });
        }

        // ================================================================
        // GET /api/inventorytransaction/check-seal
        // ================================================================
        [HttpGet("check-seal")]
        public async Task<IActionResult> CheckSeal([FromQuery] string seal, [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(seal)) return Ok(new { exists = false });
            var exists = await _service.SecuritySealExistsAsync(GetCurrentCompanyId(), seal.Trim(), excludeId);
            return Ok(new { exists });
        }

        // ================================================================
        // GET /api/inventorytransaction/check-any-seal
        // ================================================================
        [HttpGet("check-any-seal")]
        public async Task<IActionResult> CheckAnySeal([FromQuery] string seal, [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(seal)) return Ok(new { exists = false });
            var exists = await _service.AnySealExistsAsync(GetCurrentCompanyId(), seal.Trim(), excludeId);
            return Ok(new { exists });
        }

        // ================================================================
        // GET /api/inventorytransaction/transit-warehouse/{warehouseId}/busy
        // ================================================================
        [HttpGet("transit-warehouse/{warehouseId:int}/busy")]
        public async Task<IActionResult> CheckTransitWarehouseBusy(int warehouseId, [FromQuery] int? excludeId = null)
        {
            try
            {
                var (isBusy, transactionNumber) = await _service.CheckTransitWarehouseBusyAsync(
                    GetCurrentCompanyId(), warehouseId, excludeId);
                return Ok(new { isBusy, transactionNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/existence/warehouse/{warehouseId}
        // ================================================================
        [HttpGet("existence/warehouse/{warehouseId:int}")]
        public async Task<IActionResult> GetExistenceByWarehouse(int warehouseId)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var items = await _service.GetExistencesByWarehouseAsync(companyId, warehouseId);
                return Ok(items.Select(e => new
                {
                    e.IdItem,
                    e.ItemCode,
                    e.IdWarehouse,
                    e.QtyOnHand,
                    e.QtyReserved,
                    e.QtyInTransit,
                    e.QtyAvailable,
                    e.AverageCost,
                    e.LastCost,
                    e.LastMovementDate
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // GET /api/inventorytransaction/existence/item/{itemId}
        // ================================================================
        [HttpGet("existence/item/{itemId:int}")]
        public async Task<IActionResult> GetExistenceByItem(int itemId)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var items = await _service.GetExistencesByItemAsync(companyId, itemId);
                return Ok(items.Select(e => new
                {
                    e.IdItem,
                    e.ItemCode,
                    e.IdWarehouse,
                    e.QtyOnHand,
                    e.QtyReserved,
                    e.QtyInTransit,
                    e.QtyAvailable,
                    e.AverageCost,
                    e.LastCost,
                    e.LastMovementDate
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // POST /api/inventorytransaction
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryTransactionDto dto)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();
                var userId = GetCurrentUserId();

                var txn = MapFromDto(dto);
                var lines = dto.Lines.Select(MapLineFromDto).ToList();

                var created = await _service.CreateAsync(companyId, txn, lines, user, userId);
                var result = await _service.GetByIdAsync(companyId, created.Id);
                result!.Lines = await _service.GetLinesAsync(companyId, created.Id);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(result, true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando movimiento");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PUT /api/inventorytransaction/{id}
        // ================================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryTransactionDto dto)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                var txn = new InventoryTransaction
                {
                    Id = id,
                    MovementType = dto.MovementType,
                    IdWarehouseOrigin = dto.IdWarehouseOrigin,
                    IdWarehouseDest = dto.IdWarehouseDest,
                    Reference = dto.Reference,
                    Notes = dto.Notes,
                    SecuritySeal = string.IsNullOrWhiteSpace(dto.SecuritySeal) ? null : dto.SecuritySeal.Trim(),
                    TransactionDate = dto.TransactionDate,
                    ExpectedArrivalDate = dto.ExpectedArrivalDate,
                    IsTransitTransfer = dto.IsTransitTransfer,
                    DepartureTime = TimeOnly.TryParse(dto.DepartureTime, out var dtU) ? dtU : null,
                    OdometerOut   = dto.OdometerOut
                };

                var updated = await _service.UpdateAsync(companyId, txn, user);
                return Ok(MapToDto(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PUT /api/inventorytransaction/{id}/lines
        // ================================================================
        [HttpPut("{id:int}/lines")]
        public async Task<IActionResult> SaveLines(int id, [FromBody] List<InventoryTransactionLineDto> dtos)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();
                var lines = dtos.Select(MapLineFromDto).ToList();
                await _service.SaveLinesAsync(companyId, id, lines, user);
                var result = await _service.GetLinesAsync(companyId, id);
                return Ok(result.Select(MapLineToDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando líneas del movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PATCH /api/inventorytransaction/{id}/confirm
        // ================================================================
        [HttpPatch("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var txn = await _service.ConfirmAsync(companyId, id, GetCurrentUser(), GetCurrentUserId());
                return Ok(MapToDto(txn));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PATCH /api/inventorytransaction/{id}/receive
        // ================================================================
        [HttpPatch("{id:int}/receive")]
        public async Task<IActionResult> ReceiveLines(int id, [FromBody] ReceiveLinesDto dto)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var lineQtysDict = dto.LineQtys?.Count > 0
                    ? dto.LineQtys.ToDictionary(lq => lq.LineId, lq => lq.Qty)
                    : null;
                var txn = await _service.ReceiveLinesAsync(
                    companyId, id, dto.LineIds,
                    GetCurrentUserId(), GetCurrentUser(),
                    dto.ArrivalTime, dto.DepartureTime, dto.OdometerOut,
                    dto.DestSeal, dto.NextWarehouseId,
                    lineQtysDict, dto.Signature);
                var lines = await _service.GetLinesAsync(companyId, id);
                txn.Lines = lines;
                return Ok(MapToDto(txn, true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recibiendo líneas del movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PATCH /api/inventorytransaction/{id}/complete
        // ================================================================
        [HttpPatch("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var txn = await _service.CompleteAsync(companyId, id, GetCurrentUser(), GetCurrentUserId());
                return Ok(MapToDto(txn));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completando movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // PATCH /api/inventorytransaction/{id}/cancel
        // ================================================================
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelTransactionDto dto)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var txn = await _service.CancelAsync(companyId, id, dto.Reason ?? "", GetCurrentUser(), GetCurrentUserId());
                return Ok(MapToDto(txn));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando movimiento {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ================================================================
        // MAPPERS
        // ================================================================

        private static object MapToDto(InventoryTransaction t, bool includeLines = false)
        {
            var dto = new
            {
                t.Id,
                t.TransactionNumber,
                t.MovementType,
                t.Status,
                t.IdWarehouseOrigin,
                t.IdWarehouseDest,
                t.Reference,
                t.Notes,
                t.SecuritySeal,
                TransactionDate = t.TransactionDate.ToString("yyyy-MM-dd"),
                ExpectedArrivalDate = t.ExpectedArrivalDate?.ToString("yyyy-MM-dd"),
                DepartureTime = t.DepartureTime?.ToString("HH:mm"),
                t.OdometerOut,
                t.ConfirmedDate,
                t.CompletedDate,
                t.CancelledDate,
                t.CancelReason,
                t.CreatedByUserId,
                t.ConfirmedByUserId,
                t.CancelledByUserId,
                t.IsTransitTransfer,
                t.AffectsStock,
                t.CreateDate,
                t.CreatedBy,
                t.UpdatedBy,
                Lines = includeLines ? t.Lines.Select(MapLineToDto) : null
            };
            return dto;
        }

        private static object MapLineToDto(InventoryTransactionLine l) => new
        {
            l.Id,
            l.IdInventoryTransaction,
            l.LineNumber,
            l.IdItem,
            l.ItemCode,
            l.ItemName,
            l.QtyRequested,
            l.QtyDispatched,
            l.QtyReceived,
            l.IdWarehouseOriginLine,
            l.IdWarehouseDestLine,
            l.LineStatus,
            l.ReceivedDate,
            l.ReceivedByUserId,
            l.IdUnitOfMeasure,
            l.UnitOfMeasureCode,
            l.UnitCost,
            l.TotalCost,
            l.LotNumber,
            ExpiryDate = l.ExpiryDate?.ToString("yyyy-MM-dd"),
            l.Notes,
            l.DestSecuritySeal,
            DepartureTime = l.DepartureTime?.ToString("HH:mm"),
            ArrivalTime = l.ArrivalTime?.ToString("HH:mm"),
            l.OdometerOut,
            l.Signature
        };

        private static InventoryTransaction MapFromDto(CreateInventoryTransactionDto dto) => new()
        {
            TransactionNumber = dto.TransactionNumber ?? string.Empty,
            MovementType = dto.MovementType,
            IdWarehouseOrigin = dto.IdWarehouseOrigin,
            IdWarehouseDest = dto.IdWarehouseDest,
            Reference = dto.Reference,
            Notes = dto.Notes,
            SecuritySeal = string.IsNullOrWhiteSpace(dto.SecuritySeal) ? null : dto.SecuritySeal.Trim(),
            TransactionDate = dto.TransactionDate,
            ExpectedArrivalDate = dto.ExpectedArrivalDate,
            IsTransitTransfer = dto.IsTransitTransfer,
            DepartureTime = TimeOnly.TryParse(dto.DepartureTime, out var dt0) ? dt0 : null,
            OdometerOut  = dto.OdometerOut
        };

        private static InventoryTransactionLine MapLineFromDto(InventoryTransactionLineDto dto) => new()
        {
            IdItem = dto.IdItem,
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            QtyRequested = dto.QtyRequested,
            IdWarehouseOriginLine = dto.IdWarehouseOriginLine,
            IdWarehouseDestLine = dto.IdWarehouseDestLine,
            IdUnitOfMeasure = dto.IdUnitOfMeasure,
            UnitOfMeasureCode = dto.UnitOfMeasureCode,
            UnitCost = dto.UnitCost,
            LotNumber = dto.LotNumber,
            ExpiryDate = dto.ExpiryDate,
            Notes = dto.Notes,
            DestSecuritySeal = string.IsNullOrWhiteSpace(dto.DestSecuritySeal) ? null : dto.DestSecuritySeal.Trim(),
            DepartureTime = TimeOnly.TryParse(dto.DepartureTime, out var dtL) ? dtL : null,
            ArrivalTime   = TimeOnly.TryParse(dto.ArrivalTime,   out var atL) ? atL : null,
            OdometerOut   = dto.OdometerOut
        };
    }

    // ================================================================
    // DTOs
    // ================================================================

    public class CreateInventoryTransactionDto
    {
        public string? TransactionNumber { get; set; }
        public string MovementType { get; set; } = InventoryMovementType.Transfer;
        public int IdWarehouseOrigin { get; set; }
        public int? IdWarehouseDest { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public string? SecuritySeal { get; set; }
        public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly? ExpectedArrivalDate { get; set; }
        public bool IsTransitTransfer { get; set; } = false;
        public string? DepartureTime { get; set; }
        public decimal? OdometerOut { get; set; }
        public List<InventoryTransactionLineDto> Lines { get; set; } = new();
    }

    public class UpdateInventoryTransactionDto
    {
        public string MovementType { get; set; } = InventoryMovementType.Transfer;
        public int IdWarehouseOrigin { get; set; }
        public int? IdWarehouseDest { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public string? SecuritySeal { get; set; }
        public DateOnly TransactionDate { get; set; }
        public DateOnly? ExpectedArrivalDate { get; set; }
        public bool IsTransitTransfer { get; set; } = false;
        public string? DepartureTime { get; set; }
        public decimal? OdometerOut { get; set; }
    }

    public class InventoryTransactionLineDto
    {
        public int IdItem { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal QtyRequested { get; set; }
        public int? IdWarehouseOriginLine { get; set; }
        public int? IdWarehouseDestLine { get; set; }
        public int? IdUnitOfMeasure { get; set; }
        public string? UnitOfMeasureCode { get; set; }
        public decimal? UnitCost { get; set; }
        public string? LotNumber { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public string? DestSecuritySeal { get; set; }
        public string? DepartureTime { get; set; }
        public string? ArrivalTime { get; set; }
        public decimal? OdometerOut { get; set; }
    }

    public class ReceiveLineQtyDto
    {
        public int LineId { get; set; }
        public decimal Qty { get; set; }
    }

    public class ReceiveLinesDto
    {
        public List<int> LineIds { get; set; } = new();
        /// <summary>Cantidades recibidas por línea (LineId → Qty)</summary>
        public List<ReceiveLineQtyDto> LineQtys { get; set; } = new();
        /// <summary>Hora de llegada al grupo de bodegas destino que se está recibiendo (HH:mm)</summary>
        public string? ArrivalTime { get; set; }
        /// <summary>Hora de salida del vehículo hacia el siguiente destino (HH:mm)</summary>
        public string? DepartureTime { get; set; }
        /// <summary>Km de salida del vehículo hacia el siguiente destino</summary>
        public decimal? OdometerOut { get; set; }
        /// <summary>Sello destino de la bodega que se está recibiendo en este momento</summary>
        public string? DestSeal { get; set; }
        /// <summary>Id de la siguiente bodega destino (para asociar el sello)</summary>
        public int? NextWarehouseId { get; set; }
        /// <summary>Firma digital del receptor (base64 PNG data URI)</summary>
        public string? Signature { get; set; }
    }

    public class CancelTransactionDto
    {
        public string? Reason { get; set; }
    }
}
