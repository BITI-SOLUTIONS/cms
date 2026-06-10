// ================================================================================
// ARCHIVO: CMS.UI/Controllers/LocalizationController.cs
// PROPÓSITO: Controlador de vistas para Settings/Localization y Settings/LocalizationType
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class LocalizationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LocalizationController> _logger;
        private readonly IConfiguration _configuration;

        public LocalizationController(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LocalizationController> logger,
            IConfiguration configuration)
        {
            _httpClient           = httpClient;
            _httpContextAccessor  = httpContextAccessor;
            _logger               = logger;
            _configuration        = configuration;
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

        private void SetViewBagCommon()
        {
            ConfigureAuthHeader();
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken   = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                              ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                              ?? string.Empty;
        }

        // ============================================================
        // GET /Settings/LocalizationType
        // ============================================================
        [HttpGet]
        public IActionResult LocalizationType()
        {
            SetViewBagCommon();
            return View();
        }

        // ============================================================
        // GET /Settings/Locations  (antes /Warehouse/Locations)
        // ============================================================
        [HttpGet]
        [Route("Settings/Locations")]
        public IActionResult Locations()
        {
            SetViewBagCommon();
            ViewBag.CompanyCountryId = _httpContextAccessor.HttpContext?.Session.GetInt32("SelectedCompanyCountryId") ?? 0;
            return View();
        }
    }
}
