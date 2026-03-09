// ================================================================================
// ARCHIVO: CMS.UI/Controllers/ApiProxyController.cs
// PROPÓSITO: Proxy local para llamadas a la API desde el frontend
// DESCRIPCIÓN: Permite que el frontend llame a rutas locales (/api/*) que se
//              reenvían a la API con el token JWT de la sesión
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CMS.UI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ApiProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiProxyController> _logger;

        public ApiProxyController(
            IHttpClientFactory httpClientFactory,
            ILogger<ApiProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // =====================================================
        // USER SETTINGS
        // =====================================================

        /// <summary>
        /// GET /api/usersettings - Obtiene configuración del usuario
        /// </summary>
        [HttpGet("usersettings")]
        public async Task<IActionResult> GetUserSettings()
        {
            return await ProxyGetAsync("api/usersettings");
        }

        /// <summary>
        /// PUT /api/usersettings - Actualiza configuración del usuario
        /// </summary>
        [HttpPut("usersettings")]
        public async Task<IActionResult> UpdateUserSettings()
        {
            return await ProxyPutAsync("api/usersettings");
        }

        /// <summary>
        /// GET /api/usersettings/activity - Historial de actividad
        /// </summary>
        [HttpGet("usersettings/activity")]
        public async Task<IActionResult> GetActivityLog([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await ProxyGetAsync($"api/usersettings/activity?page={page}&pageSize={pageSize}");
        }

        /// <summary>
        /// POST /api/usersettings/activity - Registra actividad
        /// </summary>
        [HttpPost("usersettings/activity")]
        public async Task<IActionResult> LogActivity()
        {
            return await ProxyPostAsync("api/usersettings/activity");
        }

        // =====================================================
        // SUPPORT
        // =====================================================

        /// <summary>
        /// POST /api/support/request - Envía solicitud de soporte
        /// </summary>
        [HttpPost("support/request")]
        public async Task<IActionResult> SendSupportRequest()
        {
            return await ProxyPostAsync("api/support/request");
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private async Task<IActionResult> ProxyGetAsync(string path)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync(path);

                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, 
                    TryParseJson(content) ?? content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy GET {Path}", path);
                return StatusCode(500, new { message = "Error de conexión con el servidor" });
            }
        }

        private async Task<IActionResult> ProxyPostAsync(string path)
        {
            try
            {
                var client = GetAuthenticatedClient();
                
                // Leer el body del request
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(path, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, 
                    TryParseJson(responseContent) ?? responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy POST {Path}", path);
                return StatusCode(500, new { message = "Error de conexión con el servidor" });
            }
        }

        private async Task<IActionResult> ProxyPutAsync(string path)
        {
            try
            {
                var client = GetAuthenticatedClient();
                
                // Leer el body del request
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(path, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, 
                    TryParseJson(responseContent) ?? responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proxy PUT {Path}", path);
                return StatusCode(500, new { message = "Error de conexión con el servidor" });
            }
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
            var token = HttpContext.Session.GetString("ApiToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private static object? TryParseJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(content);
            }
            catch
            {
                return null;
            }
        }
    }
}
