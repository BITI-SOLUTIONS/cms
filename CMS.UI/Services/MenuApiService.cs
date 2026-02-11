// ================================================================================
// ARCHIVO: CMS.UI/Services/MenuApiService.cs
// PROPÓSITO: Servicio para consumir el endpoint de menús de la API REST
// DESCRIPCIÓN: Obtiene los menús del sistema desde la API con autenticación JWT
//              Los menús se filtran por permisos del usuario en el backend
// ACTUALIZADO: 2026-02-11
// ================================================================================

using System.Net.Http.Json;
using CMS.Application.DTOs;

namespace CMS.UI.Services
{
    /// <summary>
    /// Servicio encargado de obtener los menús del sistema desde la API REST.
    /// REQUIERE que el usuario esté autenticado (JWT en sesión).
    /// Los menús se filtran automáticamente por permisos del usuario en el backend.
    /// La API devuelve una lista PLANA de menús, NO jerárquica.
    /// </summary>
    public class MenuApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MenuApiService> _logger;

        public MenuApiService(IHttpClientFactory factory, ILogger<MenuApiService> logger)
        {
            // ⭐ Usar "cmsapi-authenticated" (con JWT del MessageHandler)
            // El MessageHandler automáticamente agrega el JWT del session storage
            _http = factory.CreateClient("cmsapi-authenticated");
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista PLANA de menús disponibles para el usuario autenticado.
        /// Los menús se filtran según los permisos incluidos en el JWT.
        /// 
        /// Endpoint: GET /api/menu
        /// Autenticación: Bearer JWT (agregado automáticamente por MessageHandler)
        /// </summary>
        /// <returns>
        /// Lista PLANA de menús (NO jerárquica) o lista vacía en caso de error.
        /// La construcción de la jerarquía se hace en MenuViewComponent.
        /// </returns>
        public async Task<List<MenuDto>> GetMenusAsync()
        {
            try
            {
                _logger.LogInformation("📋 Obteniendo menús del API (con JWT)...");

                var response = await _http.GetAsync("/api/menu");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Menu API error: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);

                    // Si es 401, el usuario necesita re-login
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("⚠️ Usuario no autenticado - Necesita login");
                    }

                    return new List<MenuDto>();
                }

                // La API devuelve: { "success": true, "count": 10, "data": [...] }
                var json = await response.Content.ReadFromJsonAsync<MenuApiResponse>();

                if (json?.Success != true || json.Data == null || json.Data.Count == 0)
                {
                    _logger.LogInformation("ℹ️ Menu API devolvió respuesta vacía o sin éxito");
                    return new List<MenuDto>();
                }

                _logger.LogInformation("✅ Menús obtenidos exitosamente: {Count} ítems", json.Count);
                return json.Data;
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
        /// Usa PascalCase porque el API serializa con camelCase automáticamente.
        /// </summary>
        private class MenuApiResponse
        {
            public bool Success { get; set; }
            public int Count { get; set; }
            public List<MenuDto> Data { get; set; } = new();
        }
    }
}