// ================================================================================
// ARCHIVO: CMS.UI/Controllers/ReportsController.cs
// PROPÓSITO: Controlador MVC para las vistas de reportes
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ReportsController(
            IConfiguration configuration, 
            ILogger<ReportsController> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        private string GetApiBaseUrl()
        {
            var env = _environment.IsDevelopment() ? "Development" : "Production";
            var baseUrl = _configuration[$"ApiSettings:{env}:BaseUrl"];
            _logger.LogDebug("API Base URL ({Env}): {BaseUrl}", env, baseUrl);
            return baseUrl ?? "";
        }

        // GET: /Reports - Redirige a /Reports/General
        public IActionResult Index()
        {
            return RedirectToAction(nameof(General));
        }

        // GET: /Reports/General - Lista de reportes
        public IActionResult General()
        {
            ViewData["ApiBaseUrl"] = GetApiBaseUrl();
            return View();
        }

        // GET: /Reports/View/{id} - Ver/Ejecutar un reporte específico
        [Route("Reports/View/{id}")]
        public IActionResult View(int id)
        {
            ViewData["ApiBaseUrl"] = GetApiBaseUrl();
            ViewData["ReportId"] = id;
            return View();
        }

        // GET: /Reports/Admin - Administración de reportes (solo admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            ViewData["ApiBaseUrl"] = GetApiBaseUrl();
            return View();
        }

        // GET: /Reports/Edit/{id} - Editar un reporte (solo admin)
        [Authorize(Roles = "Admin")]
        [Route("Reports/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewData["ApiBaseUrl"] = GetApiBaseUrl();
            ViewData["ReportId"] = id;
            return View();
        }

        // GET: /Reports/Create - Crear nuevo reporte (solo admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ApiBaseUrl"] = GetApiBaseUrl();
            return View();
        }
    }
}
