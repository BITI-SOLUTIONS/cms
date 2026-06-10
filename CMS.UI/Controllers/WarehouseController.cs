// ================================================================================
// ARCHIVO: CMS.UI/Controllers/WarehouseController.cs
// PROPÓSITO: Controlador de vistas para el módulo Warehouse & Distribution
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WarehouseController> _logger;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WarehouseController(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<WarehouseController> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        private void ConfigureAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                     ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private string GetApiBaseUrl()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var baseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"];
            return baseUrl ?? (environment == "Production"
                ? "https://cms.biti-solutions.com"
                : "https://localhost:7001");
        }

        // ============================================================
        // GET /Warehouse/Warehouses
        // ============================================================
        [HttpGet]
        public IActionResult Warehouses()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();

            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;

            return View();
        }

        // ============================================================
        // GET /Warehouse/StockTransfers
        // ============================================================
        [HttpGet]
        public IActionResult StockTransfers()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();

            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;

            return View();
        }

        // ============================================================
        // GET /Warehouse/Locations  → redirige a la nueva ruta en Settings
        // ============================================================
        [HttpGet]
        public IActionResult Locations()
        {
            return RedirectToAction("Locations", "Localization");
        }

        // ============================================================
        // GET /Warehouse/DistributionRoutes  (alias: /Warehouse/Routes)
        // ============================================================
        [HttpGet]
        public IActionResult DistributionRoutes()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();

            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;

            return View("DistributionRoutes");
        }

        // Alias para compatibilidad con el menú que usa /Warehouse/Routes
        [HttpGet]
        public IActionResult Routes() => DistributionRoutes();

        // ============================================================
        // GET /Warehouse/InventoryMovements
        // ============================================================
        [HttpGet]
        public IActionResult InventoryMovements()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();

            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;

            return View("InventoryMovements");
        }

        // Alias: /Warehouse/Movements
        [HttpGet]
        public IActionResult Movements() => InventoryMovements();

        // ============================================================
        // GET /Warehouse/TransportUnits
        // ============================================================
        [HttpGet]
        public IActionResult TransportUnits()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();

            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;

            return View("TransportUnits");
        }

        // Alias para compatibilidad con links anteriores
        [HttpGet]
        public IActionResult Vehicles() => TransportUnits();

        // ============================================================
        // GET /Warehouse/Drivers
        // ============================================================
        [HttpGet]
        public IActionResult Drivers()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;
            return View("Drivers");
        }

        // ============================================================
        // GET /Warehouse/Insurers
        // ============================================================
        [HttpGet]
        public IActionResult Insurers()
        {
            ConfigureAuthHeader();
            var apiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiBaseUrl = apiBaseUrl;
            ViewBag.ApiToken = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                             ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                             ?? string.Empty;
            return View("Insurers");
        }
    }
}
