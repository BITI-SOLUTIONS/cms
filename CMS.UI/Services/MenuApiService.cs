// ================================================================================
// ARCHIVO: CMS.UI/Services/MenuApiService.cs
// PROPÓSITO: Servicio para consumir el endpoint de menús de la API REST
// DESCRIPCIÓN: Obtiene los menús del sistema desde la API y los deserializa
//              usando el DTO compartido de CMS.Application.DTOs
//              La API devuelve los menús en formato plano, la jerarquía se
//              construye en el ViewComponent.
// ================================================================================

using System.Net.Http.Json;
using CMS.Application.DTOs;
using Microsoft.Identity.Web; // ✅ AGREGAR ESTA LÍNEA

namespace CMS.UI.Services
{
    /// <summary>
    /// Servicio encargado de obtener los menús del sistema desde la API REST.
    /// Los menús se filtran automáticamente por permisos del usuario en el backend.
    /// La API devuelve una lista PLANA de menús, NO jerárquica.
    /// </summary>
    public class MenuApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MenuApiService> _logger;

        public MenuApiService(IHttpClientFactory factory, ILogger<MenuApiService> logger)
        {
            _http = factory.CreateClient("cmsapi-authenticated");
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista PLANA de menús disponibles para el usuario actual.
        /// La API devuelve solo los menús activos y filtrados por permisos.
        /// Endpoint: GET /api/menu
        /// </summary>
        /// <returns>
        /// Lista PLANA de menús (NO jerárquica) o lista vacía en caso de error.
        /// La construcción de la jerarquía se hace en MenuViewComponent.
        /// </returns>
        public async Task<List<MenuDto>> GetMenusAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/menu");

                // ✅ Si no está autenticado, devolver lista vacía (sin error)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogInformation("ℹ️ Usuario no autenticado - sin menús disponibles");
                    return new List<MenuDto>();
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("❌ Menu API error: {StatusCode}", response.StatusCode);
                    return new List<MenuDto>();
                }

                var json = await response.Content.ReadFromJsonAsync<MenuApiResponse>();

                if (json?.data == null || json.data.Count == 0)
                {
                    _logger.LogWarning("⚠️ Menu API devolvió respuesta vacía o nula");
                    return new List<MenuDto>();
                }

                _logger.LogInformation("✅ Menús obtenidos exitosamente: {Count} ítems", json.count);
                return json.data;
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                // Usuario no autenticado - devolver lista vacía sin error
                _logger.LogInformation("ℹ️ Usuario requiere autenticación - sin menús");
                return new List<MenuDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión al obtener menús");
                return new List<MenuDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Excepción al obtener menús");
                return new List<MenuDto>();
            }
        }

        /// <summary>
        /// Clase interna para deserializar la respuesta de la API.
        /// Coincide con la estructura devuelta por MenuController en CMS.API.
        /// </summary>
        private class MenuApiResponse
        {
            public bool success { get; set; }
            public int count { get; set; }
            public List<MenuDto> data { get; set; } = new();
        }
    }
}