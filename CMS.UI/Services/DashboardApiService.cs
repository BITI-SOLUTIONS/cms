// ================================================================================
// ARCHIVO: CMS.UI/Services/DashboardApiService.cs
// PROPÓSITO: Servicio para obtener datos del Dashboard desde la API
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using System.Net.Http.Headers;

namespace CMS.UI.Services
{
    /// <summary>
    /// Servicio para comunicarse con el endpoint del Dashboard en la API
    /// </summary>
    public class DashboardApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DashboardApiService> _logger;

        public DashboardApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DashboardApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("cmsapi-authenticated");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las estadísticas del Dashboard
        /// </summary>
        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync("api/dashboard/stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var stats = await response.Content.ReadFromJsonAsync<DashboardStatsViewModel>();
                    return stats ?? new DashboardStatsViewModel();
                }

                _logger.LogWarning("Error obteniendo stats del dashboard: {StatusCode}", response.StatusCode);
                return new DashboardStatsViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetDashboardStatsAsync");
                return new DashboardStatsViewModel();
            }
        }

        /// <summary>
        /// Verifica si el usuario tiene un permiso específico
        /// </summary>
        public async Task<bool> HasPermissionAsync(string permissionKey)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"api/dashboard/has-permission/{permissionKey}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando permiso {PermissionKey}", permissionKey);
                return false;
            }
        }
    }

    /// <summary>
    /// ViewModel para estadísticas del Dashboard
    /// </summary>
    public class DashboardStatsViewModel
    {
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int ActiveModules { get; set; }
        public string WelcomeMessage { get; set; } = "Bienvenido al Centro de Gestión. Aquí tienes un resumen de tu sistema.";
    }
}
