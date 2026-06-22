// ================================================================================
// ARCHIVO: CMS.UI/Controllers/AccountingController.cs
// PROPÓSITO: Controller UI para gestión del módulo de Contabilidad
// DESCRIPCIÓN: Maneja las vistas del módulo de contabilidad: Plan de Cuentas,
//              Asientos de Diario, etc. Pasa configuración API/Token a las vistas.
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-20
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class AccountingController : Controller
    {
        private readonly ILogger<AccountingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AccountingController(
            ILogger<AccountingController> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private string GetApiToken() =>
            _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
            ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
            ?? string.Empty;

        private string GetApiBaseUrl()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var baseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"];
            return baseUrl ?? (environment == "Production"
                ? "https://cms.biti-solutions.com"
                : "https://localhost:7001");
        }

        /// <summary>
        /// Vista principal del Plan de Cuentas (Chart of Accounts)
        /// GET: /Accounting/ChartOfAccounts
        /// </summary>
        public IActionResult ChartOfAccounts()
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken = GetApiToken();
            return View();
        }

        /// <summary>
        /// Vista de Asientos de Diario (Journal Entries) - Futuro
        /// GET: /Accounting/JournalEntries
        /// </summary>
        public IActionResult JournalEntries()
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken = GetApiToken();
            return View();
        }

        /// <summary>
        /// Vista de Centros de Costo (Cost Centers)
        /// GET: /Accounting/CostCenters
        /// </summary>
        public IActionResult CostCenters()
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken = GetApiToken();
            return View();
        }

        /// <summary>
        /// Vista de Catálogos Generales (General Catalogs)
        /// GET: /Accounting/GeneralCatalogs
        /// </summary>
        public IActionResult GeneralCatalogs()
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken = GetApiToken();
            return View();
        }
    }
}
