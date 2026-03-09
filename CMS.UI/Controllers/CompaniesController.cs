// ================================================================================
// ARCHIVO: CMS.UI/Controllers/CompaniesController.cs
// PROPÓSITO: Controlador para gestión de compañías en la UI
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ACTUALIZADO: 2026-03-02 - Agregada verificación de permisos
// ================================================================================

using CMS.UI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CMS.UI.Controllers
{
    [Authorize]
    [RequirePermission("System.ViewAllCompanies")]
    [Route("[controller]")]
    public class CompaniesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompaniesController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompaniesController(
            IHttpClientFactory httpClientFactory,
            ILogger<CompaniesController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("cmsapi");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Lista de compañías
        /// GET: /Companies
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("api/company");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var companies = JsonSerializer.Deserialize<List<CompanyListViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<CompanyListViewModel>();

                    _logger.LogInformation("📋 Compañías obtenidas: {Count}", companies.Count);
                    return View(companies);
                }
                else
                {
                    _logger.LogWarning("⚠️ Error obteniendo compañías: {Status}", response.StatusCode);
                    TempData["Error"] = "No se pudieron cargar las compañías";
                    return View(new List<CompanyListViewModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañías");
                TempData["Error"] = "Error al cargar las compañías";
                return View(new List<CompanyListViewModel>());
            }
        }

        /// <summary>
        /// Detalle de una compañía
        /// GET: /Companies/Details/{id}
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"api/company/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var company = JsonSerializer.Deserialize<CompanyDetailViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (company != null)
                    {
                        return View(company);
                    }
                }

                TempData["Error"] = "Compañía no encontrada";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle de compañía {Id}", id);
                TempData["Error"] = "Error al cargar la compañía";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Formulario de creación
        /// GET: /Companies/Create
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await LoadCountriesViewBag();
            return View(new CompanyCreateViewModel());
        }

        /// <summary>
        /// Procesar creación
        /// POST: /Companies/Create
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCountriesViewBag();
                return View(model);
            }

            try
            {
                SetAuthHeader();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/company", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Compañía creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al crear la compañía: {error}";
                    await LoadCountriesViewBag();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando compañía");
                TempData["Error"] = "Error al crear la compañía";
                await LoadCountriesViewBag();
                return View(model);
            }
        }

        /// <summary>
        /// Formulario de edición
        /// GET: /Companies/Edit/{id}
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync($"api/company/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var company = JsonSerializer.Deserialize<CompanyEditViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (company != null)
                    {
                        await LoadCountriesViewBag();
                        return View(company);
                    }
                }

                TempData["Error"] = "Compañía no encontrada";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañía para editar {Id}", id);
                TempData["Error"] = "Error al cargar la compañía";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesar edición
        /// POST: /Companies/Edit/{id}
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCountriesViewBag();
                return View(model);
            }

            try
            {
                SetAuthHeader();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/company/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Compañía actualizada exitosamente";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar: {error}";
                    await LoadCountriesViewBag();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando compañía {Id}", id);
                TempData["Error"] = "Error al actualizar la compañía";
                await LoadCountriesViewBag();
                return View(model);
            }
        }

        /// <summary>
        /// Activar/Desactivar compañía
        /// POST: /Companies/ToggleActive/{id}
        /// </summary>
        [HttpPost("ToggleActive/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.PatchAsync($"api/company/{id}/toggle-active", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Estado de la compañía actualizado";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado de compañía {Id}", id);
                TempData["Error"] = "Error al cambiar el estado";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCountriesViewBag()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("api/catalog/countries");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<List<CatalogItem>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<CatalogItem>();
                    ViewBag.Countries = countries;
                }
                else
                {
                    ViewBag.Countries = new List<CatalogItem>();
                }
            }
            catch
            {
                ViewBag.Countries = new List<CatalogItem>();
            }
        }

        // ================================================================================
        // ViewModels
        // ================================================================================

        public class CompanyListViewModel
        {
            public int Id { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public string CompanySchema { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public bool IsAdminCompany { get; set; }
            public string? ContactEmail { get; set; }
            public string? ContactPhone { get; set; }
            public string? Website { get; set; }
            public string? LogoUrl { get; set; }
            /// <summary>
            /// Logo en formato Data URI (base64)
            /// </summary>
            public string? LogoData { get; set; }
            public DateTime? CreateDate { get; set; }

            /// <summary>
            /// Obtiene la fuente del logo válida.
            /// Prioridad: LogoData (base64) > LogoUrl (si es URL completa o data URI)
            /// Retorna null si no hay logo válido (para mostrar icono por defecto)
            /// </summary>
            public string? GetLogoSource()
            {
                // Prioridad 1: LogoData (Data URI base64)
                if (!string.IsNullOrEmpty(LogoData) && LogoData.StartsWith("data:"))
                {
                    return LogoData;
                }

                // Prioridad 2: LogoUrl (si es URL completa con protocolo)
                if (!string.IsNullOrEmpty(LogoUrl))
                {
                    // Solo aceptar URLs completas (http://, https://) o data URIs
                    if (LogoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        LogoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        LogoUrl.StartsWith("data:"))
                    {
                        return LogoUrl;
                    }
                }

                // No hay logo válido
                return null;
            }
        }

        public class CompanyDetailViewModel : CompanyListViewModel
        {
            public string? CompanyTaxId { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public int? CountryId { get; set; }
            public string? CountryName { get; set; }
            public string? ManagerName { get; set; }
            public string? Industry { get; set; }
            public bool IsTenant { get; set; }
            public bool IsProduction { get; set; }
            public bool UsesAzureAd { get; set; }
            public string? DashboardWelcomeMessage { get; set; }
            public int MaxFailedLoginAttempts { get; set; }
            public int LockoutDurationMinutes { get; set; }
            public DateTime? RecordDate { get; set; }
            public int UserCount { get; set; }
        }

        public class CompanyCreateViewModel
        {
            public string CompanyName { get; set; } = string.Empty;
            public string CompanySchema { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? CompanyTaxId { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public int? CountryId { get; set; }
            public string? ContactEmail { get; set; }
            public string? ContactPhone { get; set; }
            public string? Website { get; set; }
            public string? LogoUrl { get; set; }
            public string? ManagerName { get; set; }
            public string? Industry { get; set; }
        }

        public class CompanyEditViewModel : CompanyCreateViewModel
        {
            public int Id { get; set; }
            public string? DashboardWelcomeMessage { get; set; }
            public int MaxFailedLoginAttempts { get; set; } = 5;
            public int LockoutDurationMinutes { get; set; } = 15;
        }

        public class CatalogItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Code { get; set; }
        }
    }
}
