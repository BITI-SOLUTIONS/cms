// ================================================================================
// ARCHIVO: CMS.UI/Services/UserSettingsApiService.cs
// PROPÓSITO: Servicio para gestión de configuración de usuario vía API
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CMS.UI.Services
{
    public class UserSettingsApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserSettingsApiService> _logger;

        public UserSettingsApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserSettingsApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la configuración del usuario actual
        /// </summary>
        public async Task<UserSettingsDto?> GetSettingsAsync()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/usersettings");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserSettingsDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("❌ Error obteniendo configuración: {Status}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo configuración de usuario");
                return null;
            }
        }

        /// <summary>
        /// Guarda la configuración del usuario
        /// </summary>
        public async Task<bool> SaveSettingsAsync(UserSettingsDto settings)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var json = JsonSerializer.Serialize(settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/usersettings", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Configuración guardada correctamente");
                    return true;
                }

                _logger.LogWarning("❌ Error guardando configuración: {Status}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error guardando configuración de usuario");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el historial de actividad del usuario (paginado)
        /// </summary>
        public async Task<ActivityLogPagedResult?> GetActivityLogAsync(int page = 1)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync($"api/usersettings/activity?page={page}&pageSize=10");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ActivityLogPagedResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("❌ Error obteniendo historial: {Status}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo historial de actividad");
                return null;
            }
        }

        /// <summary>
        /// Registra una actividad del usuario
        /// </summary>
        public async Task LogActivityAsync(string activityType, string description)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var request = new { ActivityType = activityType, Description = description };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await client.PostAsync("api/usersettings/activity", content);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ No se pudo registrar actividad");
            }
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }

    // =====================================================
    // DTOs
    // =====================================================

    public class UserSettingsDto
    {
        public string Theme { get; set; } = "dark";
        public bool SidebarCompact { get; set; } = true;
        public bool NotifyEmail { get; set; } = true;
        public bool NotifyBrowser { get; set; } = true;
        public bool NotifySound { get; set; }
        public string Language { get; set; } = "es";
        public string Timezone { get; set; } = "America/Costa_Rica";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string TimeFormat { get; set; } = "24h";
    }

    public class ActivityLogDto
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime ActivityDate { get; set; }
    }

    public class ActivityLogPagedResult
    {
        public List<ActivityLogDto> Activities { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
