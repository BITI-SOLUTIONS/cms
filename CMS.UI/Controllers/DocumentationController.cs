// ================================================================================
// ARCHIVO: CMS.UI/Controllers/DocumentationController.cs
// PROPÓSITO: Controller para mostrar la documentación del sistema
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-08
// ================================================================================

using CMS.UI.Filters;
using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    /// <summary>
    /// Controller para la sección de documentación del sistema.
    /// Muestra PDFs de ayuda organizados por categoría.
    /// </summary>
    [Authorize]
    [RequireValidSession]
    public class DocumentationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentationController> _logger;

        public DocumentationController(
            IHttpClientFactory httpClientFactory,
            ILogger<DocumentationController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Página principal de documentación
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Title"] = "Documentación del Sistema";
            return View();
        }

        /// <summary>
        /// Obtener documentación desde API
        /// </summary>
        [HttpGet("Documentation/Api/List")]
        public async Task<IActionResult> GetDocumentation()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync("/api/documentation");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }

                _logger.LogWarning("Error al obtener documentación: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentación del API");
                return StatusCode(500, new { error = "Error al cargar la documentación" });
            }
        }

        /// <summary>
        /// Proxy para descargar PDF desde API
        /// </summary>
        [HttpGet("Documentation/Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync($"/api/documentation/{id}/download");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";
                    var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "documento.pdf";
                    
                    return File(bytes, contentType, fileName);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar documento {Id}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Proxy para ver PDF en el navegador desde API
        /// </summary>
        [HttpGet("Documentation/View/{id:int}")]
        public async Task<IActionResult> ViewPdf(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                var response = await client.GetAsync($"/api/documentation/{id}/view");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";
                    
                    Response.Headers.Append("Content-Disposition", "inline");
                    return File(bytes, contentType);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al visualizar documento {Id}", id);
                return StatusCode(500);
            }
        }
    }
}
