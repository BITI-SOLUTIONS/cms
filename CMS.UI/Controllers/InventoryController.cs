// ================================================================================
// ARCHIVO: CMS.UI/Controllers/InventoryController.cs
// PROPÓSITO: Controlador para las vistas de Inventory incluyendo Label Items
// DESCRIPCIÓN: Maneja las vistas de gestión de artículos
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-19
// ================================================================================

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<InventoryController> _logger;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public InventoryController(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<InventoryController> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        private void ConfigureAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        private string GetApiBaseUrl()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var baseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"];
            return baseUrl ?? (environment == "Production" 
                ? "https://cms.biti-solutions.com" 
                : "https://localhost:7001");
        }

        #region Label Items

        /// <summary>
        /// Pantalla de impresión de etiquetas
        /// GET: /Inventory/LabelItems
        /// </summary>
        public async Task<IActionResult> LabelItems(string? search = null, int page = 1)
        {
            try
            {
                ConfigureAuthHeader();

                var url = $"{GetApiBaseUrl()}/api/item/labels?page={page}&pageSize=20";
                if (!string.IsNullOrEmpty(search))
                    url += $"&search={Uri.EscapeDataString(search)}";

                var response = await _httpClient.GetAsync(url);

                var viewModel = new LabelItemsViewModel
                {
                    Search = search,
                    CurrentPage = page
                };

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ItemListResponse>(json, JsonOptions);

                    if (result != null)
                    {
                        viewModel.Items = result.Items;
                        viewModel.TotalCount = result.TotalCount;
                        viewModel.TotalPages = result.TotalPages;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error obteniendo artículos de etiqueta: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    TempData["Error"] = "Error al cargar los artículos de etiqueta.";
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LabelItems");
                TempData["Error"] = "Error al cargar los artículos de etiqueta";
                return View(new LabelItemsViewModel());
            }
        }

        /// <summary>
        /// Obtener datos de un artículo para mostrar en el panel de etiqueta
        /// GET: /Inventory/LabelItems/GetItem/{id}
        /// </summary>
        [HttpGet("Inventory/LabelItems/GetItem/{id:int}")]
        public async Task<IActionResult> GetLabelItem(int id)
        {
            try
            {
                ConfigureAuthHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/item/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Artículo no encontrado" });
                }

                var json = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<ItemDto>(json, JsonOptions);

                return Json(new { success = true, item });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículo {Id}", id);
                return Json(new { success = false, message = "Error al cargar el artículo" });
            }
        }

        /// <summary>
        /// Guardar datos de etiqueta de un artículo
        /// POST: /Inventory/LabelItems/SaveLabel
        /// </summary>
        [HttpPost("Inventory/LabelItems/SaveLabel")]
        [IgnoreAntiforgeryToken] // AJAX call with JWT authentication, no need for antiforgery token
        public async Task<IActionResult> SaveLabel([FromBody] SaveLabelRequest request)
        {
            try
            {
                ConfigureAuthHeader();

                var payload = JsonSerializer.Serialize(new
                {
                    labelItem = request.LabelItem,
                    labelPrice = request.LabelPrice,
                    labelItemBarcode = request.LabelItemBarcode,
                    printLabelName = request.PrintLabelName,
                    printLabelPrice = request.PrintLabelPrice,
                    printLabelBarcode = request.PrintLabelBarcode,
                    labelWidthCm = request.LabelWidthCm,
                    labelHeightCm = request.LabelHeightCm,
                    labelOrientation = request.LabelOrientation,
                    printLabelBorder = request.PrintLabelBorder,
                    labelBorderColor = request.LabelBorderColor,
                    labelNameColor = request.LabelNameColor,
                    labelPriceColor = request.LabelPriceColor,
                    labelBarcodeColor = request.LabelBarcodeColor,
                    labelFontSize = request.LabelFontSize,
                    labelFontFamily = request.LabelFontFamily,
                    labelPriceDecimals = request.LabelPriceDecimals,
                    labelThousandSeparator = request.LabelThousandSeparator,
                    labelCurrencySymbol = request.LabelCurrencySymbol,
                    printCurrencySymbol = request.PrintCurrencySymbol
                });

                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/item/{request.ItemId}/label", content);

                if (response.IsSuccessStatusCode)
                {
                    // Devolver también el item actualizado para refrescar la lista
                    var json = await response.Content.ReadAsStringAsync();
                    var updatedItem = JsonSerializer.Deserialize<ItemDto>(json, JsonOptions);
                    return Json(new { success = true, message = "Etiqueta guardada exitosamente", item = updatedItem });
                }

                var error = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Error al guardar: {error}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando etiqueta del artículo {Id}", request.ItemId);
                return Json(new { success = false, message = "Error al guardar la etiqueta" });
            }
        }

        /// <summary>
        /// Registra una impresión de etiqueta en el historial
        /// POST: /Inventory/LabelItems/RecordPrint
        /// </summary>
        [HttpPost("Inventory/LabelItems/RecordPrint")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RecordPrint([FromBody] RecordPrintRequest request)
        {
            try
            {
                ConfigureAuthHeader();

                var payload = JsonSerializer.Serialize(new
                {
                    labelItem = request.LabelItem,
                    labelPrice = request.LabelPrice,
                    labelItemBarcode = request.LabelItemBarcode,
                    printLabelName = request.PrintLabelName,
                    printLabelPrice = request.PrintLabelPrice,
                    printLabelBarcode = request.PrintLabelBarcode,
                    printLabelBorder = request.PrintLabelBorder,
                    printCurrencySymbol = request.PrintCurrencySymbol,
                    labelWidthCm = request.LabelWidthCm,
                    labelHeightCm = request.LabelHeightCm,
                    labelOrientation = request.LabelOrientation,
                    labelBorderColor = request.LabelBorderColor,
                    labelNameColor = request.LabelNameColor,
                    labelPriceColor = request.LabelPriceColor,
                    labelBarcodeColor = request.LabelBarcodeColor,
                    labelFontSize = request.LabelFontSize,
                    labelFontFamily = request.LabelFontFamily,
                    labelPriceDecimals = request.LabelPriceDecimals,
                    labelThousandSeparator = request.LabelThousandSeparator,
                    labelCurrencySymbol = request.LabelCurrencySymbol,
                    formattedPrice = request.FormattedPrice,
                    quantityPrinted = request.QuantityPrinted,
                    printerName = request.PrinterName,
                    printNotes = request.PrintNotes
                });

                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/item/{request.ItemId}/print", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Impresión registrada exitosamente" });
                }

                var error = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Error al registrar impresión: {error}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando impresión del artículo {Id}", request.ItemId);
                return Json(new { success = false, message = "Error al registrar la impresión" });
            }
        }

        #endregion

        #region Item Master (Mantenimiento)

        /// <summary>
        /// Lista de artículos (Mantenimiento)
        /// GET: /Inventory/Items
        /// </summary>
        public async Task<IActionResult> Items(string? search = null, string? category = null, int page = 1)
        {
            try
            {
                ConfigureAuthHeader();

                var url = $"{GetApiBaseUrl()}/api/item?page={page}&pageSize=20";
                if (!string.IsNullOrEmpty(search))
                    url += $"&search={Uri.EscapeDataString(search)}";
                if (!string.IsNullOrEmpty(category))
                    url += $"&category={Uri.EscapeDataString(category)}";

                var response = await _httpClient.GetAsync(url);

                var viewModel = new ItemListViewModel
                {
                    Search = search,
                    Category = category,
                    CurrentPage = page
                };

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ItemListResponse>(json, JsonOptions);

                    if (result != null)
                    {
                        viewModel.Items = result.Items;
                        viewModel.TotalCount = result.TotalCount;
                        viewModel.TotalPages = result.TotalPages;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error obteniendo artículos: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    TempData["Error"] = "Error al cargar los artículos. Verifique que la base de datos de la compañía existe.";
                }

                // Obtener categorías para el filtro
                try
                {
                    var categoriesResponse = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/item/categories");
                    if (categoriesResponse.IsSuccessStatusCode)
                    {
                        var categoriesJson = await categoriesResponse.Content.ReadAsStringAsync();
                        viewModel.Categories = JsonSerializer.Deserialize<List<string>>(categoriesJson, JsonOptions) ?? new();
                    }
                }
                catch { /* Ignorar error de categorías */ }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Items");
                TempData["Error"] = "Error al cargar los artículos";
                return View(new ItemListViewModel());
            }
        }

        /// <summary>
        /// Detalle de un artículo
        /// GET: /Inventory/Items/{id}
        /// </summary>
        [HttpGet("Inventory/Items/{id:int}")]
        public async Task<IActionResult> ItemDetails(int id)
        {
            try
            {
                ConfigureAuthHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/item/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Artículo no encontrado";
                    return RedirectToAction(nameof(Items));
                }

                var json = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<ItemDto>(json, JsonOptions);

                return View("ItemDetails", item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle de artículo {Id}", id);
                TempData["Error"] = "Error al cargar el artículo";
                return RedirectToAction(nameof(Items));
            }
        }

        /// <summary>
        /// Formulario para crear artículo
        /// GET: /Inventory/Items/Create
        /// </summary>
        [HttpGet("Inventory/Items/Create")]
        public IActionResult CreateItem()
        {
            return View("CreateItem", new CreateItemViewModel());
        }

        /// <summary>
        /// Crear artículo
        /// POST: /Inventory/Items/Create
        /// </summary>
        [HttpPost("Inventory/Items/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateItem", model);
            }

            try
            {
                ConfigureAuthHeader();

                var payload = JsonSerializer.Serialize(model);
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{GetApiBaseUrl()}/api/item", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Artículo creado exitosamente";
                    return RedirectToAction(nameof(Items));
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al crear artículo: {error}";
                return View("CreateItem", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando artículo");
                TempData["Error"] = "Error al crear el artículo";
                return View("CreateItem", model);
            }
        }

        /// <summary>
        /// Formulario para editar artículo
        /// GET: /Inventory/Items/Edit/{id}
        /// </summary>
        [HttpGet("Inventory/Items/Edit/{id:int}")]
        public async Task<IActionResult> EditItem(int id)
        {
            try
            {
                ConfigureAuthHeader();

                var response = await _httpClient.GetAsync($"{GetApiBaseUrl()}/api/item/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Artículo no encontrado";
                    return RedirectToAction(nameof(Items));
                }

                var json = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<EditItemViewModel>(json, JsonOptions);

                return View("EditItem", item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo artículo para editar {Id}", id);
                TempData["Error"] = "Error al cargar el artículo";
                return RedirectToAction(nameof(Items));
            }
        }

        /// <summary>
        /// Actualizar artículo
        /// POST: /Inventory/Items/Edit/{id}
        /// </summary>
        [HttpPost("Inventory/Items/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, EditItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditItem", model);
            }

            try
            {
                ConfigureAuthHeader();

                var payload = JsonSerializer.Serialize(model);
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GetApiBaseUrl()}/api/item/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Artículo actualizado exitosamente";
                    return RedirectToAction(nameof(Items));
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al actualizar artículo: {error}";
                return View("EditItem", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando artículo {Id}", id);
                TempData["Error"] = "Error al actualizar el artículo";
                return View("EditItem", model);
            }
        }

        /// <summary>
        /// Eliminar artículo
        /// POST: /Inventory/Items/Delete/{id}
        /// </summary>
        [HttpPost("Inventory/Items/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                ConfigureAuthHeader();

                var response = await _httpClient.DeleteAsync($"{GetApiBaseUrl()}/api/item/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Artículo eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar el artículo";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando artículo {Id}", id);
                TempData["Error"] = "Error al eliminar el artículo";
            }

            return RedirectToAction(nameof(Items));
        }

        #endregion
    }

    // ===== VIEW MODELS =====

    public class ItemListViewModel
    {
        public List<ItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public string? Category { get; set; }
        public List<string> Categories { get; set; } = new();
    }

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

    public class CreateItemViewModel
    {
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
        public bool IsActive { get; set; } = true;
        public bool IsSellable { get; set; } = true;
        public bool IsPurchasable { get; set; } = true;
        public bool TrackLots { get; set; }
        public bool TrackSerialNumbers { get; set; }

        // Label Item fields
        public string? LabelItem { get; set; }
        public decimal LabelPrice { get; set; }
        public string? LabelItemBarcode { get; set; }
        public bool IsLabelItem { get; set; }
        public bool PrintLabelName { get; set; } = true;
        public bool PrintLabelPrice { get; set; } = true;
        public bool PrintLabelBarcode { get; set; } = true;
    }

    public class EditItemViewModel : CreateItemViewModel
    {
        public int Id { get; set; }
    }

    // ===== LABEL ITEMS VIEW MODELS =====

    public class LabelItemsViewModel
    {
        public List<ItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public ItemDto? SelectedItem { get; set; }
    }

    public class SaveLabelRequest
    {
        public int ItemId { get; set; }
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

    public class RecordPrintRequest
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
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
}
