// ================================================================================
// ARCHIVO: CMS.UI/Controllers/ReportsController.cs
// PROPÓSITO: Controlador MVC para las vistas de reportes
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportsController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportsController(
            IConfiguration configuration, 
            ILogger<ReportsController> logger,
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
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
            return View();
        }

        // GET: /Reports/View/{id} - Ver/Ejecutar un reporte específico
        [Route("Reports/View/{id}")]
        public IActionResult View(int id)
        {
            ViewData["ReportId"] = id;
            return View();
        }

        // GET: /Reports/Admin - Administración de reportes (solo admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        // GET: /Reports/Edit/{id} - Editar un reporte (solo admin)
        [Authorize(Roles = "Admin")]
        [Route("Reports/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            ViewData["ReportId"] = id;
            return View();
        }

        // GET: /Reports/Create - Crear nuevo reporte (solo admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        #region API Proxy Endpoints

        // GET: /Reports/Api/Categories - Proxy para obtener categorías
        [HttpGet("Reports/Api/Categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync("/api/report/categories");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error obteniendo categorías: {Status}", response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy de categorías");
                return StatusCode(500, new { error = "Error de conexión con el API" });
            }
        }

        // GET: /Reports/Api/List - Proxy para obtener lista de reportes
        [HttpGet("Reports/Api/List")]
        public async Task<IActionResult> GetReportsList()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync("/api/report");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error obteniendo reportes: {Status}", response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy de reportes");
                return StatusCode(500, new { error = "Error de conexión con el API" });
            }
        }

        // GET: /Reports/Api/Detail/{id} - Proxy para obtener detalle de un reporte
        [HttpGet("Reports/Api/Detail/{id}")]
        public async Task<IActionResult> GetReportDetail(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync($"/api/report/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error obteniendo reporte {Id}: {Status}", id, response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy de reporte {Id}", id);
                return StatusCode(500, new { error = "Error de conexión con el API" });
            }
        }

        // POST: /Reports/Api/Execute - Proxy para ejecutar un reporte
        [HttpPost("Reports/Api/Execute")]
        public async Task<IActionResult> ExecuteReport([FromBody] object request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("/api/report/execute", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error ejecutando reporte: {Status}", response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy de ejecución de reporte");
                return StatusCode(500, new { error = "Error de conexión con el API" });
            }
        }

        // POST: /Reports/Api/Favorite/{id} - Proxy para marcar/desmarcar favorito
        [HttpPost("Reports/Api/Favorite/{id}")]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.PostAsync($"/api/report/{id}/favorite", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error en favorito {Id}: {Status}", id, response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy de favorito {Id}", id);
                return StatusCode(500, new { error = "Error de conexión con el API" });
            }
        }

        #endregion
    }
}
