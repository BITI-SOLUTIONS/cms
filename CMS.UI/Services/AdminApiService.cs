// ================================================================================
// ARCHIVO: CMS.UI/Services/AdminApiService.cs
// PROPÓSITO: Servicio para comunicación con la API de administración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

using CMS.UI.Controllers;
using CMS.UI.Models.Admin;
using System.Net.Http.Json;
using System.Text.Json;

namespace CMS.UI.Services
{
    public class AdminApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminApiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AdminApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AdminApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("cmsapi");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        #region Health Check

        public async Task<HealthCheckViewModel> GetHealthCheckAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/admin/health");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<HealthCheckApiResponse>(JsonOptions);
                    return MapToHealthViewModel(result);
                }

                _logger.LogWarning("Error obteniendo health check: {Status}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo health check");
            }

            return new HealthCheckViewModel
            {
                LastCheck = DateTime.UtcNow,
                OverallStatus = "Unknown",
                Services = new List<ServiceHealthViewModel>(),
                Metrics = new SystemMetricsViewModel()
            };
        }

        private HealthCheckViewModel MapToHealthViewModel(HealthCheckApiResponse? response)
        {
            if (response == null)
                return new HealthCheckViewModel { OverallStatus = "Unknown" };

            return new HealthCheckViewModel
            {
                LastCheck = response.Timestamp,
                OverallStatus = response.Status,
                Services = response.Services?.Select(s => new ServiceHealthViewModel
                {
                    Name = s.Name,
                    Status = s.Status,
                    ResponseTime = s.ResponseTime,
                    Message = s.Message,
                    Endpoint = s.Endpoint,
                    LastCheck = response.Timestamp
                }).ToList() ?? new List<ServiceHealthViewModel>(),
                Metrics = new SystemMetricsViewModel
                {
                    CpuUsage = response.Metrics?.CpuUsage ?? 0,
                    MemoryUsage = response.Metrics?.MemoryUsage ?? 0,
                    MemoryUsed = response.Metrics?.MemoryUsed ?? "N/A",
                    MemoryTotal = response.Metrics?.MemoryTotal ?? "N/A",
                    DiskUsage = response.Metrics?.DiskUsage ?? 0,
                    DiskUsed = response.Metrics?.DiskUsed ?? "N/A",
                    DiskTotal = response.Metrics?.DiskTotal ?? "N/A",
                    ActiveConnections = response.Metrics?.ActiveConnections ?? 0,
                    RequestsPerMinute = response.Metrics?.RequestsPerMinute ?? 0,
                    AverageResponseTime = response.Metrics?.AverageResponseTime ?? 0,
                    Uptime = TimeSpan.FromSeconds(response.Metrics?.UptimeSeconds ?? 0)
                }
            };
        }

        #endregion

        #region Audit Trail

        public async Task<AuditTrailViewModel> GetAuditTrailAsync(string? search, string? action, DateTime? from, DateTime? to, int page, int pageSize)
        {
            try
            {
                var client = CreateClient();
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(action))
                    queryParams.Add($"action={action}");
                if (from.HasValue)
                    queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue)
                    queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var url = $"api/admin/audit?{string.Join("&", queryParams)}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuditApiResponse>(JsonOptions);
                    return new AuditTrailViewModel
                    {
                        SearchTerm = search,
                        ActionFilter = action,
                        FromDate = from ?? DateTime.UtcNow.AddDays(-7),
                        ToDate = to ?? DateTime.UtcNow,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = result?.TotalCount ?? 0,
                        Entries = result?.Entries?.Select(e => new AuditEntryViewModel
                        {
                            Id = e.Id,
                            Timestamp = e.Timestamp,
                            User = e.User,
                            Action = e.Action,
                            Entity = e.Entity,
                            EntityId = e.EntityId,
                            Details = e.Details,
                            IpAddress = e.IpAddress
                        }).ToList() ?? new List<AuditEntryViewModel>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo audit trail");
            }

            return new AuditTrailViewModel
            {
                FromDate = from ?? DateTime.UtcNow.AddDays(-7),
                ToDate = to ?? DateTime.UtcNow
            };
        }

        #endregion

        #region System Logs

        public async Task<SystemLogsViewModel> GetSystemLogsAsync(string? level, string? source, DateTime? from, DateTime? to, int page, int pageSize)
        {
            try
            {
                var client = CreateClient();
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(level))
                    queryParams.Add($"level={level}");
                if (!string.IsNullOrEmpty(source))
                    queryParams.Add($"source={source}");
                if (from.HasValue)
                    queryParams.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
                if (to.HasValue)
                    queryParams.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var url = $"api/admin/logs?{string.Join("&", queryParams)}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LogsApiResponse>(JsonOptions);
                    return new SystemLogsViewModel
                    {
                        LevelFilter = level,
                        SourceFilter = source,
                        FromDate = from ?? DateTime.UtcNow.AddHours(-24),
                        ToDate = to ?? DateTime.UtcNow,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = result?.TotalCount ?? 0,
                        Entries = result?.Entries?.Select(e => new LogEntryViewModel
                        {
                            Id = e.Id,
                            Timestamp = e.Timestamp,
                            Level = e.Level,
                            Source = e.Source,
                            Message = e.Message,
                            CorrelationId = e.CorrelationId,
                            StackTrace = e.StackTrace
                        }).ToList() ?? new List<LogEntryViewModel>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo logs");
            }

            return new SystemLogsViewModel
            {
                FromDate = from ?? DateTime.UtcNow.AddHours(-24),
                ToDate = to ?? DateTime.UtcNow
            };
        }

        #endregion

        #region API Keys

        public async Task<ApiKeysViewModel> GetApiKeysAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/admin/apikeys");

                if (response.IsSuccessStatusCode)
                {
                    var keys = await response.Content.ReadFromJsonAsync<List<ApiKeyApiDto>>(JsonOptions);
                    return new ApiKeysViewModel
                    {
                        ApiKeys = keys?.Select(k => new ApiKeyViewModel
                        {
                            Id = k.Id,
                            Name = k.Name,
                            KeyPrefix = k.KeyPrefix,
                            Environment = k.Environment,
                            CreatedAt = k.CreatedAt,
                            LastUsed = k.LastUsed,
                            IsActive = k.IsActive,
                            Permissions = k.Permissions ?? Array.Empty<string>()
                        }).ToList() ?? new List<ApiKeyViewModel>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo API keys");
            }

            return new ApiKeysViewModel();
        }

        public async Task<(bool Success, string? ApiKey, string? Error)> CreateApiKeyAsync(CreateApiKeyViewModel model)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync("api/admin/apikeys", new
                {
                    name = model.Name,
                    environment = model.Environment,
                    permissions = model.Permissions
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>(JsonOptions);
                    return (true, result?.ApiKey, null);
                }

                return (false, null, "Error creando API Key");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando API key");
                return (false, null, ex.Message);
            }
        }

        public async Task<bool> RevokeApiKeyAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.DeleteAsync($"api/admin/apikeys/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revocando API key {Id}", id);
                return false;
            }
        }

        #endregion

        #region Jobs

        public async Task<JobSchedulerViewModel> GetJobsAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/admin/jobs");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JobsApiResponse>(JsonOptions);
                    return new JobSchedulerViewModel
                    {
                        Jobs = result?.Jobs?.Select(j => new ScheduledJobViewModel
                        {
                            Id = j.Id,
                            Name = j.Name,
                            Description = j.Description,
                            CronExpression = j.CronExpression,
                            NextRun = j.NextRun,
                            LastRun = j.LastRun,
                            LastStatus = j.LastStatus,
                            IsEnabled = j.IsEnabled
                        }).ToList() ?? new List<ScheduledJobViewModel>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo jobs");
            }

            return new JobSchedulerViewModel();
        }

        public async Task<bool> RunJobAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/admin/jobs/{id}/run", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando job {Id}", id);
                return false;
            }
        }

        public async Task<bool> ToggleJobAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/admin/jobs/{id}/toggle", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling job {Id}", id);
                return false;
            }
        }

        #endregion

        #region Backups

        public async Task<BackupViewModel> GetBackupsAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/admin/backups");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BackupsApiResponse>(JsonOptions);
                    return new BackupViewModel
                    {
                        StorageUsed = result?.StorageUsed ?? "N/A",
                        StorageLimit = result?.StorageLimit ?? "N/A",
                        StoragePercentage = result?.StoragePercentage ?? 0,
                        Backups = result?.Backups?.Select(b => new BackupEntryViewModel
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Type = b.Type,
                            CreatedAt = b.CreatedAt,
                            Size = b.Size,
                            Status = b.Status,
                            CreatedBy = b.CreatedBy
                        }).ToList() ?? new List<BackupEntryViewModel>()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo backups");
            }

            return new BackupViewModel();
        }

        public async Task<bool> CreateBackupAsync(string type = "full")
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync("api/admin/backups", new { type });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando backup");
                return false;
            }
        }

        public async Task<bool> RestoreBackupAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/admin/backups/{id}/restore", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restaurando backup {Id}", id);
                return false;
            }
        }

        #endregion

        #region DTOs para API

        private class HealthCheckApiResponse
        {
            public string Status { get; set; } = "Unknown";
            public DateTime Timestamp { get; set; }
            public List<ServiceHealthApi>? Services { get; set; }
            public MetricsApi? Metrics { get; set; }
        }

        private class ServiceHealthApi
        {
            public string Name { get; set; } = string.Empty;
            public string Status { get; set; } = "Unknown";
            public int ResponseTime { get; set; }
            public string? Message { get; set; }
            public string? Endpoint { get; set; }
        }

        private class MetricsApi
        {
            public double CpuUsage { get; set; }
            public double MemoryUsage { get; set; }
            public string? MemoryUsed { get; set; }
            public string? MemoryTotal { get; set; }
            public double DiskUsage { get; set; }
            public string? DiskUsed { get; set; }
            public string? DiskTotal { get; set; }
            public int ActiveConnections { get; set; }
            public int RequestsPerMinute { get; set; }
            public double AverageResponseTime { get; set; }
            public int UptimeSeconds { get; set; }
        }

        private class AuditApiResponse
        {
            public List<AuditEntryApi>? Entries { get; set; }
            public int TotalCount { get; set; }
        }

        private class AuditEntryApi
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string User { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string Entity { get; set; } = string.Empty;
            public string? EntityId { get; set; }
            public string? Details { get; set; }
            public string? IpAddress { get; set; }
        }

        private class LogsApiResponse
        {
            public List<LogEntryApi>? Entries { get; set; }
            public int TotalCount { get; set; }
        }

        private class LogEntryApi
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string? CorrelationId { get; set; }
            public string? StackTrace { get; set; }
        }

        private class ApiKeyApiDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string KeyPrefix { get; set; } = string.Empty;
            public string Environment { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime? LastUsed { get; set; }
            public bool IsActive { get; set; }
            public string[]? Permissions { get; set; }
        }

        private class CreateApiKeyResponse
        {
            public int Id { get; set; }
            public string? ApiKey { get; set; }
        }

        private class JobsApiResponse
        {
            public List<JobApi>? Jobs { get; set; }
        }

        private class JobApi
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string CronExpression { get; set; } = string.Empty;
            public DateTime? NextRun { get; set; }
            public DateTime? LastRun { get; set; }
            public string? LastStatus { get; set; }
            public bool IsEnabled { get; set; }
        }

        private class BackupsApiResponse
        {
            public List<BackupApi>? Backups { get; set; }
            public string? StorageUsed { get; set; }
            public string? StorageLimit { get; set; }
            public int StoragePercentage { get; set; }
        }

        private class BackupApi
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string Size { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? CreatedBy { get; set; }
        }

        #endregion

        #region System Settings

        public async Task<SystemSettingsViewModel> GetSystemSettingsAsync()
        {
            var model = new SystemSettingsViewModel();

            try
            {
                var client = CreateClient();

                // Obtener compañías del sistema
                var companiesResponse = await client.GetAsync("api/admin/system/companies");
                if (companiesResponse.IsSuccessStatusCode)
                {
                    var companies = await companiesResponse.Content.ReadFromJsonAsync<List<CompanySystemDto>>(JsonOptions);
                    model.Companies = companies?.Select(c => new CompanySystemItem
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Schema = c.Schema,
                        IsAdminCompany = c.IsAdminCompany,
                        IsActive = c.IsActive,
                        UsesAzureAD = c.UsesAzureAD,
                        UserCount = c.UserCount
                    }).ToList() ?? new List<CompanySystemItem>();

                    model.AdminCompany = model.Companies.FirstOrDefault(c => c.IsAdminCompany);
                    model.HasAdminCompany = model.AdminCompany != null;
                }

                // Obtener permisos del sistema
                var permissionsResponse = await client.GetAsync("api/admin/system/permissions");
                if (permissionsResponse.IsSuccessStatusCode)
                {
                    var permissions = await permissionsResponse.Content.ReadFromJsonAsync<List<SystemPermissionDto>>(JsonOptions);
                    model.SystemPermissions = permissions?.Select(p => new SystemPermissionItem
                    {
                        Id = p.Id,
                        Key = p.Key,
                        Name = p.Name,
                        Description = p.Description,
                        IsActive = p.IsActive
                    }).ToList() ?? new List<SystemPermissionItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuración del sistema");
            }

            return model;
        }

        public async Task<bool> SetAdminCompanyAsync(int companyId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync("api/admin/system/set-admin-company", new { companyId });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo compañía Admin");
                return false;
            }
        }

        private class CompanySystemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Schema { get; set; } = string.Empty;
            public bool IsAdminCompany { get; set; }
            public bool IsActive { get; set; }
            public bool UsesAzureAD { get; set; }
            public int UserCount { get; set; }
        }

        private class SystemPermissionDto
        {
            public int Id { get; set; }
            public string Key { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        #endregion
    }
}
