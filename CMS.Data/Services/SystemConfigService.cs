// ================================================================================
// ARCHIVO: CMS.Data/Services/SystemConfigService.cs
// PROPÓSITO: Servicio para obtener configuraciones globales del sistema
// DESCRIPCIÓN: Lee y cachea configuraciones de la tabla admin.system_config
//              Desencripta automáticamente valores marcados como is_encrypted
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ACTUALIZADO: 2026-02-13 - Soporte para desencriptación automática
// ================================================================================

using CMS.Entities;
using CMS.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio para acceder a las configuraciones globales del sistema
    /// </summary>
    public class SystemConfigService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SystemConfigService> _logger;
        private readonly EncryptionService? _encryptionService;
        private const string CACHE_KEY_PREFIX = "SystemConfig_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

        public SystemConfigService(
            AppDbContext context,
            IMemoryCache cache,
            ILogger<SystemConfigService> logger,
            EncryptionService? encryptionService = null)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Obtiene todas las configuraciones de una categoría (desencriptando valores encriptados)
        /// </summary>
        public async Task<Dictionary<string, string?>> GetCategoryConfigAsync(string category)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{category}";

            if (!_cache.TryGetValue(cacheKey, out Dictionary<string, string?>? configs))
            {
                var dbConfigs = await _context.SystemConfigs
                    .AsNoTracking()
                    .Where(c => c.CONFIG_CATEGORY == category && c.IS_ACTIVE)
                    .ToListAsync();

                configs = new Dictionary<string, string?>();

                foreach (var config in dbConfigs)
                {
                    var value = config.GetEffectiveValue();

                    // Si está encriptado, desencriptar
                    if (config.IS_ENCRYPTED && !string.IsNullOrEmpty(value) && _encryptionService != null)
                    {
                        try
                        {
                            value = _encryptionService.Decrypt(value);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error desencriptando {Category}.{Key}", 
                                config.CONFIG_CATEGORY, config.CONFIG_KEY);
                            value = null; // No exponer el valor encriptado si falla
                        }
                    }

                    configs[config.CONFIG_KEY] = value;
                }

                _cache.Set(cacheKey, configs, CACHE_DURATION);
                _logger.LogInformation("📋 Configuraciones '{Category}' cargadas de BD ({Count} items)", 
                    category, configs.Count);
            }

            return configs ?? new Dictionary<string, string?>();
        }

        /// <summary>
        /// Obtiene una configuración específica
        /// </summary>
        public async Task<string?> GetConfigValueAsync(string category, string key)
        {
            var configs = await GetCategoryConfigAsync(category);
            return configs.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Obtiene una configuración como entero
        /// </summary>
        public async Task<int> GetConfigIntAsync(string category, string key, int defaultValue = 0)
        {
            var value = await GetConfigValueAsync(category, key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Obtiene una configuración como booleano
        /// </summary>
        public async Task<bool> GetConfigBoolAsync(string category, string key, bool defaultValue = false)
        {
            var value = await GetConfigValueAsync(category, key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Obtiene la configuración SMTP completa
        /// </summary>
        public async Task<SmtpSettings> GetSmtpSettingsAsync()
        {
            var configs = await GetCategoryConfigAsync(SystemConfigCategories.SMTP);

            return new SmtpSettings
            {
                Host = configs.GetValueOrDefault(SmtpConfigKeys.HOST) ?? "smtp.office365.com",
                Port = int.TryParse(configs.GetValueOrDefault(SmtpConfigKeys.PORT), out var port) ? port : 587,
                Username = configs.GetValueOrDefault(SmtpConfigKeys.USERNAME),
                Password = configs.GetValueOrDefault(SmtpConfigKeys.PASSWORD),
                FromEmail = configs.GetValueOrDefault(SmtpConfigKeys.FROM_EMAIL),
                FromName = configs.GetValueOrDefault(SmtpConfigKeys.FROM_NAME) ?? "CMS Sistema",
                UseSsl = bool.TryParse(configs.GetValueOrDefault(SmtpConfigKeys.USE_SSL), out var ssl) && ssl,
                TimeoutSeconds = int.TryParse(configs.GetValueOrDefault(SmtpConfigKeys.TIMEOUT_SECONDS), out var timeout) ? timeout : 30
            };
        }

        /// <summary>
        /// Invalida el caché de una categoría
        /// </summary>
        public void InvalidateCategory(string category)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{category}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("🔄 Caché invalidado para categoría: {Category}", category);
        }

        /// <summary>
        /// Invalida todo el caché de configuraciones
        /// </summary>
        public void InvalidateAll()
        {
            InvalidateCategory(SystemConfigCategories.SMTP);
            InvalidateCategory(SystemConfigCategories.SECURITY);
            InvalidateCategory(SystemConfigCategories.GENERAL);
        }
    }

    /// <summary>
    /// DTO para configuración SMTP
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.office365.com";
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromEmail { get; set; }
        public string FromName { get; set; } = "CMS Sistema";
        public bool UseSsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Verifica si la configuración SMTP está completa
        /// </summary>
        public bool IsConfigured =>
            !string.IsNullOrEmpty(Host) &&
            Port > 0 &&
            !string.IsNullOrEmpty(Username) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(FromEmail);
    }
}
