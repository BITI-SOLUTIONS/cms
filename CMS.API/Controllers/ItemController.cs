// ================================================================================
// ARCHIVO: CMS.API/Controllers/ItemController.cs
// PROPÓSITO: API REST para gestionar artículos en las BD operacionales
// DESCRIPCIÓN: CRUD de artículos usando la BD de la compañía activa del usuario
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-19
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
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;

        public ItemController(
            IItemService itemService,
            ILogger<ItemController> logger)
        {
            _itemService = itemService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la compañía activa del token JWT
        /// </summary>
        private int GetCurrentCompanyId()
        {
            // El JWT usa "companyId" (minúsculas)
            var companyIdClaim = User.FindFirst("companyId")?.Value 
                              ?? User.FindFirst("CompanyId")?.Value;
            if (int.TryParse(companyIdClaim, out var companyId))
            {
                return companyId;
            }
            throw new UnauthorizedAccessException("companyId no encontrado en el token JWT");
        }

        /// <summary>
        /// Obtiene el usuario actual
        /// </summary>
        private string? GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? 
                   User.FindFirst("preferred_username")?.Value;
        }

        /// <summary>
        /// Lista artículos con filtros y paginación
        /// GET: api/item?search=xxx&category=xxx&isActive=true&page=1&pageSize=20
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ItemListResponse>> GetItems(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var (items, totalCount) = await _itemService.GetItemsAsync(
                    companyId, search, category, isActive, page, pageSize);

                return Ok(new ItemListResponse
                {
                    Items = items.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Lista SOLO artículos de etiqueta (is_label_item = true)
        /// GET: api/item/labels?search=xxx&page=1&pageSize=20
        /// </summary>
        [HttpGet("labels")]
        public async Task<ActionResult<ItemListResponse>> GetLabelItems(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var (items, totalCount) = await _itemService.GetLabelItemsAsync(
                    companyId, search, page, pageSize);

                return Ok(new ItemListResponse
                {
                    Items = items.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículos de etiqueta");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza SOLO los campos de etiqueta de un artículo
        /// PUT: api/item/{id}/label
        /// </summary>
        [HttpPut("{id}/label")]
        public async Task<ActionResult<ItemDto>> UpdateLabelInfo(int id, [FromBody] UpdateLabelRequest request)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                var updated = await _itemService.UpdateLabelInfoAsync(
                    companyId, id, 
                    request.LabelItem, 
                    request.LabelPrice, 
                    request.LabelItemBarcode,
                    request.PrintLabelName,
                    request.PrintLabelPrice,
                    request.PrintLabelBarcode,
                    request.LabelWidthCm,
                    request.LabelHeightCm,
                    request.LabelOrientation,
                    request.PrintLabelBorder,
                    request.LabelBorderColor,
                    request.LabelNameColor,
                    request.LabelPriceColor,
                    request.LabelBarcodeColor,
                    request.LabelFontSize,
                    request.LabelFontFamily,
                    request.LabelPriceDecimals,
                    request.LabelThousandSeparator,
                    request.LabelCurrencySymbol,
                    request.PrintCurrencySymbol,
                    user);

                if (updated == null)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando etiqueta del artículo {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un artículo por ID
        /// GET: api/item/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItem(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var item = await _itemService.GetItemByIdAsync(companyId, id);

                if (item == null)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                return Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículo {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un artículo por código
        /// GET: api/item/code/{code}
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<ItemDto>> GetItemByCode(string code)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var item = await _itemService.GetItemByCodeAsync(companyId, code);

                if (item == null)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                return Ok(MapToDto(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículo por código {Code}", code);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo artículo
        /// POST: api/item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemRequest request)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                // Verificar si ya existe un artículo con ese código
                var existing = await _itemService.GetItemByCodeAsync(companyId, request.Code);
                if (existing != null)
                {
                    return BadRequest(new { message = $"Ya existe un artículo con el código {request.Code}" });
                }

                var item = new Item
                {
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    Barcode = request.Barcode,
                    Category = request.Category,
                    Subcategory = request.Subcategory,
                    Brand = request.Brand,
                    UnitOfMeasure = request.UnitOfMeasure ?? "unidad",
                    CostPrice = request.CostPrice,
                    SalePrice = request.SalePrice,
                    TaxRate = request.TaxRate,
                    MinStock = request.MinStock,
                    MaxStock = request.MaxStock,
                    CurrentStock = request.CurrentStock,
                    ImageUrl = request.ImageUrl,
                    IsActive = request.IsActive ?? true,
                    IsSellable = request.IsSellable ?? true,
                    IsPurchasable = request.IsPurchasable ?? true,
                    TrackLots = request.TrackLots ?? false,
                    TrackSerialNumbers = request.TrackSerialNumbers ?? false,

                    // Label Item fields
                    LabelItem = request.LabelItem,
                    LabelPrice = request.LabelPrice,
                    LabelItemBarcode = request.LabelItemBarcode,
                    IsLabelItem = request.IsLabelItem
                };

                var created = await _itemService.CreateItemAsync(companyId, item, user);

                return CreatedAtAction(nameof(GetItem), new { id = created.Id }, MapToDto(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando artículo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un artículo
        /// PUT: api/item/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ItemDto>> UpdateItem(int id, [FromBody] UpdateItemRequest request)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                var item = new Item
                {
                    Id = id,
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    Barcode = request.Barcode,
                    Category = request.Category,
                    Subcategory = request.Subcategory,
                    Brand = request.Brand,
                    UnitOfMeasure = request.UnitOfMeasure ?? "unidad",
                    CostPrice = request.CostPrice,
                    SalePrice = request.SalePrice,
                    TaxRate = request.TaxRate,
                    MinStock = request.MinStock,
                    MaxStock = request.MaxStock,
                    CurrentStock = request.CurrentStock,
                    ImageUrl = request.ImageUrl,
                    IsActive = request.IsActive,
                    IsSellable = request.IsSellable,
                    IsPurchasable = request.IsPurchasable,
                    TrackLots = request.TrackLots,
                    TrackSerialNumbers = request.TrackSerialNumbers,

                    // Label Item fields
                    LabelItem = request.LabelItem,
                    LabelPrice = request.LabelPrice,
                    LabelItemBarcode = request.LabelItemBarcode,
                    IsLabelItem = request.IsLabelItem
                };

                var updated = await _itemService.UpdateItemAsync(companyId, item, user);

                if (updated == null)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                return Ok(MapToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando artículo {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina (desactiva) un artículo
        /// DELETE: api/item/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                var result = await _itemService.DeleteItemAsync(companyId, id, user);

                if (!result)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando artículo {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las categorías disponibles
        /// GET: api/item/categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var categories = await _itemService.GetCategoriesAsync(companyId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo categorías");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Registra una impresión de etiqueta en el historial
        /// POST: api/item/{id}/print
        /// </summary>
        [HttpPost("{id}/print")]
        public async Task<ActionResult<LabelPrintHistoryDto>> RecordLabelPrint(int id, [FromBody] RecordPrintRequest request)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var user = GetCurrentUser();

                // Obtener el artículo para validar y obtener datos
                var item = await _itemService.GetItemByIdAsync(companyId, id);
                if (item == null)
                {
                    return NotFound(new { message = "Artículo no encontrado" });
                }

                var printHistory = await _itemService.RecordLabelPrintAsync(
                    companyId,
                    id,
                    item.Code,
                    item.Name,
                    request.LabelItem ?? item.LabelItem ?? item.Name,
                    request.LabelPrice,
                    request.LabelItemBarcode,
                    request.PrintLabelName,
                    request.PrintLabelPrice,
                    request.PrintLabelBarcode,
                    request.PrintLabelBorder,
                    request.PrintCurrencySymbol,
                    request.LabelWidthCm,
                    request.LabelHeightCm,
                    request.LabelOrientation,
                    request.LabelBorderColor,
                    request.LabelNameColor,
                    request.LabelPriceColor,
                    request.LabelBarcodeColor,
                    request.LabelFontSize,
                    request.LabelFontFamily,
                    request.LabelPriceDecimals,
                    request.LabelThousandSeparator,
                    request.LabelCurrencySymbol,
                    request.FormattedPrice,
                    request.QuantityPrinted,
                    user,
                    request.PrinterName,
                    request.PrintNotes
                );

                return Ok(new LabelPrintHistoryDto
                {
                    Id = printHistory.Id,
                    IdItem = printHistory.IdItem,
                    ItemCode = printHistory.ItemCode,
                    ItemName = printHistory.ItemName,
                    LabelItem = printHistory.LabelItem,
                    LabelPrice = printHistory.LabelPrice,
                    FormattedPrice = printHistory.FormattedPrice,
                    QuantityPrinted = printHistory.QuantityPrinted,
                    PrintDate = printHistory.PrintDate,
                    PrintedBy = printHistory.PrintedBy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando impresión de etiqueta para artículo {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el historial de impresiones de etiquetas
        /// GET: api/item/print-history
        /// </summary>
        [HttpGet("print-history")]
        public async Task<ActionResult<List<LabelPrintHistoryDto>>> GetLabelPrintHistory(
            [FromQuery] int? itemId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? printedBy = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var history = await _itemService.GetLabelPrintHistoryAsync(
                    companyId, itemId, fromDate, toDate, printedBy, page, pageSize);

                var dtos = history.Select(h => new LabelPrintHistoryDto
                {
                    Id = h.Id,
                    IdItem = h.IdItem,
                    ItemCode = h.ItemCode,
                    ItemName = h.ItemName,
                    LabelItem = h.LabelItem,
                    LabelPrice = h.LabelPrice,
                    FormattedPrice = h.FormattedPrice,
                    QuantityPrinted = h.QuantityPrinted,
                    PrintDate = h.PrintDate,
                    PrintedBy = h.PrintedBy
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de impresiones");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // ===== MAPPERS Y DTOS =====

        private static ItemDto MapToDto(Item item) => new()
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                Description = item.Description,
                Barcode = item.Barcode,
                Category = item.Category,
                Subcategory = item.Subcategory,
                Brand = item.Brand,
                UnitOfMeasure = item.UnitOfMeasure,
                CostPrice = item.CostPrice,
                SalePrice = item.SalePrice,
                TaxRate = item.TaxRate,
                MinStock = item.MinStock,
                MaxStock = item.MaxStock,
                CurrentStock = item.CurrentStock,
                ImageUrl = item.ImageUrl,
                IsActive = item.IsActive,
                IsSellable = item.IsSellable,
                IsPurchasable = item.IsPurchasable,
                TrackLots = item.TrackLots,
                TrackSerialNumbers = item.TrackSerialNumbers,

                // Label Item fields
                LabelItem = item.LabelItem,
                LabelPrice = item.LabelPrice,
                LabelItemBarcode = item.LabelItemBarcode,
                IsLabelItem = item.IsLabelItem,
                PrintLabelName = item.PrintLabelName,
                PrintLabelPrice = item.PrintLabelPrice,
                PrintLabelBarcode = item.PrintLabelBarcode,

                // Label size and format
                LabelWidthCm = item.LabelWidthCm,
                LabelHeightCm = item.LabelHeightCm,
                LabelOrientation = item.LabelOrientation,
                PrintLabelBorder = item.PrintLabelBorder,
                LabelBorderColor = item.LabelBorderColor,
                LabelNameColor = item.LabelNameColor,
                LabelPriceColor = item.LabelPriceColor,
                LabelBarcodeColor = item.LabelBarcodeColor,

                // Label font and price format
                LabelFontSize = item.LabelFontSize,
                LabelFontFamily = item.LabelFontFamily,
                LabelPriceDecimals = item.LabelPriceDecimals,
                LabelThousandSeparator = item.LabelThousandSeparator,
                LabelCurrencySymbol = item.LabelCurrencySymbol,
                PrintCurrencySymbol = item.PrintCurrencySymbol,

                // Auditoría
                CreateDate = item.CreateDate,
                RecordDate = item.RecordDate,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy,
                RowPointer = item.RowPointer
            };
    }

    // ===== REQUEST/RESPONSE DTOS =====

    public class ItemListResponse
    {
        public List<ItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Brand { get; set; }
        public string UnitOfMeasure { get; set; } = "unidad";
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal CurrentStock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSellable { get; set; }
        public bool IsPurchasable { get; set; }
        public bool TrackLots { get; set; }
        public bool TrackSerialNumbers { get; set; }

        // Label Item fields
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool IsLabelItem { get; set; }
        public bool PrintLabelName { get; set; }
        public bool PrintLabelPrice { get; set; }
        public bool PrintLabelBarcode { get; set; }

        // Label size and format
        public decimal LabelWidthCm { get; set; }
        public decimal LabelHeightCm { get; set; }
        public string LabelOrientation { get; set; } = "horizontal";
        public bool PrintLabelBorder { get; set; }
        public string LabelBorderColor { get; set; } = "#000000";
        public string LabelNameColor { get; set; } = "#000000";
        public string LabelPriceColor { get; set; } = "#16a34a";
        public string LabelBarcodeColor { get; set; } = "#000000";

        // Label font and price format
        public decimal LabelFontSize { get; set; } = 14.0m;
        public string LabelFontFamily { get; set; } = "Arial";
        public int LabelPriceDecimals { get; set; } = 2;
        public string LabelThousandSeparator { get; set; } = ",";
        public string LabelCurrencySymbol { get; set; } = "₡";
        public bool PrintCurrencySymbol { get; set; } = true;

        // Auditoría
        public DateTime CreateDate { get; set; }
        public DateTime RecordDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public Guid RowPointer { get; set; }
    }

    public class CreateItemRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Brand { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal CurrentStock { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSellable { get; set; }
        public bool? IsPurchasable { get; set; }
        public bool? TrackLots { get; set; }
        public bool? TrackSerialNumbers { get; set; }

        // Label Item fields
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool IsLabelItem { get; set; }
        public bool PrintLabelName { get; set; } = true;
        public bool PrintLabelPrice { get; set; } = true;
        public bool PrintLabelBarcode { get; set; } = true;
    }

    public class UpdateItemRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Brand { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal CurrentStock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSellable { get; set; }
        public bool IsPurchasable { get; set; }
        public bool TrackLots { get; set; }
        public bool TrackSerialNumbers { get; set; }

        // Label Item fields
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool IsLabelItem { get; set; }
        public bool PrintLabelName { get; set; }
        public bool PrintLabelPrice { get; set; }
        public bool PrintLabelBarcode { get; set; }

        // Label size and format
        public decimal LabelWidthCm { get; set; } = 4.0m;
        public decimal LabelHeightCm { get; set; } = 2.0m;
        public string LabelOrientation { get; set; } = "horizontal";
        public bool PrintLabelBorder { get; set; } = true;
        public string LabelBorderColor { get; set; } = "#000000";
        public string LabelNameColor { get; set; } = "#000000";
        public string LabelPriceColor { get; set; } = "#16a34a";
        public string LabelBarcodeColor { get; set; } = "#000000";

        // Label font and price format
        public decimal LabelFontSize { get; set; } = 14.0m;
        public string LabelFontFamily { get; set; } = "Arial";
        public int LabelPriceDecimals { get; set; } = 2;
        public string LabelThousandSeparator { get; set; } = ",";
        public string LabelCurrencySymbol { get; set; } = "₡";
        public bool PrintCurrencySymbol { get; set; } = true;
    }

    /// <summary>
    /// Request para actualizar solo los datos de etiqueta de un artículo
    /// </summary>
    public class UpdateLabelRequest
    {
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool PrintLabelName { get; set; } = true;
        public bool PrintLabelPrice { get; set; } = true;
        public bool PrintLabelBarcode { get; set; } = true;

        // Label size and format
        public decimal LabelWidthCm { get; set; } = 4.0m;
        public decimal LabelHeightCm { get; set; } = 2.0m;
        public string LabelOrientation { get; set; } = "horizontal";
        public bool PrintLabelBorder { get; set; } = true;
        public string LabelBorderColor { get; set; } = "#000000";
        public string LabelNameColor { get; set; } = "#000000";
        public string LabelPriceColor { get; set; } = "#16a34a";
        public string LabelBarcodeColor { get; set; } = "#000000";

        // Label font and price format
        public decimal LabelFontSize { get; set; } = 14.0m;
        public string LabelFontFamily { get; set; } = "Arial";
        public int LabelPriceDecimals { get; set; } = 2;
        public string LabelThousandSeparator { get; set; } = ",";
        public string LabelCurrencySymbol { get; set; } = "₡";
        public bool PrintCurrencySymbol { get; set; } = true;
    }

    /// <summary>
    /// Request para registrar una impresión de etiqueta
    /// </summary>
    public class RecordPrintRequest
    {
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool PrintLabelName { get; set; } = true;
        public bool PrintLabelPrice { get; set; } = true;
        public bool PrintLabelBarcode { get; set; } = true;
        public bool PrintLabelBorder { get; set; } = true;
        public bool PrintCurrencySymbol { get; set; } = true;

        // Label size
        public decimal LabelWidthCm { get; set; } = 4.0m;
        public decimal LabelHeightCm { get; set; } = 2.0m;
        public string LabelOrientation { get; set; } = "horizontal";

        // Label colors
        public string LabelBorderColor { get; set; } = "#000000";
        public string LabelNameColor { get; set; } = "#000000";
        public string LabelPriceColor { get; set; } = "#16a34a";
        public string LabelBarcodeColor { get; set; } = "#000000";

        // Label font and price format
        public decimal LabelFontSize { get; set; } = 14.0m;
        public string LabelFontFamily { get; set; } = "Arial";
        public int LabelPriceDecimals { get; set; } = 2;
        public string LabelThousandSeparator { get; set; } = ",";
        public string LabelCurrencySymbol { get; set; } = "₡";

        // Formatted price as printed
        public string? FormattedPrice { get; set; }

        // Print info
        public int QuantityPrinted { get; set; } = 1;
        public string? PrinterName { get; set; }
        public string? PrintNotes { get; set; }
    }

    /// <summary>
    /// DTO para historial de impresiones
    /// </summary>
    public class LabelPrintHistoryDto
    {
        public int Id { get; set; }
        public int IdItem { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? FormattedPrice { get; set; }
        public int QuantityPrinted { get; set; }
        public DateTime PrintDate { get; set; }
        public string? PrintedBy { get; set; }
    }
}
