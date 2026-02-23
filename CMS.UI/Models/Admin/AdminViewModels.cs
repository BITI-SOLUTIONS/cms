// ================================================================================
// ARCHIVO: CMS.UI/Models/Admin/AdminViewModels.cs
// PROPÓSITO: ViewModels para el módulo de administración del sistema
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Admin
{
    #region Audit Trail

    public class AuditTrailViewModel
    {
        public string? SearchTerm { get; set; }
        public string? ActionFilter { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public List<AuditEntryViewModel> Entries { get; set; } = new();

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class AuditEntryViewModel
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public string ActionBadgeClass => Action switch
        {
            "CREATE" => "bg-success",
            "UPDATE" => "bg-warning",
            "DELETE" => "bg-danger",
            "LOGIN" => "bg-info",
            "LOGOUT" => "bg-secondary",
            "EXPORT" => "bg-primary",
            _ => "bg-secondary"
        };
    }

    #endregion

    #region System Logs

    public class SystemLogsViewModel
    {
        public string? LevelFilter { get; set; }
        public string? SourceFilter { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public int TotalCount { get; set; }
        public List<LogEntryViewModel> Entries { get; set; } = new();
    }

    public class LogEntryViewModel
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string? StackTrace { get; set; }
        public string? AdditionalData { get; set; }

        public string LevelBadgeClass => Level switch
        {
            "ERROR" or "FATAL" => "level-error",
            "WARN" or "WARNING" => "level-warn",
            "INFO" => "level-info",
            "DEBUG" => "level-debug",
            "TRACE" => "level-trace",
            _ => "level-info"
        };

        public string LevelIcon => Level switch
        {
            "ERROR" or "FATAL" => "bi-x-circle-fill",
            "WARN" or "WARNING" => "bi-exclamation-triangle-fill",
            "INFO" => "bi-info-circle-fill",
            "DEBUG" => "bi-bug-fill",
            "TRACE" => "bi-activity",
            _ => "bi-circle"
        };
    }

    #endregion

    #region API Keys

    public class ApiKeysViewModel
    {
        public List<ApiKeyViewModel> ApiKeys { get; set; } = new();
    }

    public class ApiKeyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsActive { get; set; }
        public string[] Permissions { get; set; } = Array.Empty<string>();
    }

    public class CreateApiKeyViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Environment { get; set; } = "Production";

        public List<string> Permissions { get; set; } = new();

        public DateTime? ExpiresAt { get; set; }
    }

    #endregion

    #region Job Scheduler

    public class JobSchedulerViewModel
    {
        public List<ScheduledJobViewModel> Jobs { get; set; } = new();
        public List<JobExecutionViewModel> RecentExecutions { get; set; } = new();
    }

    public class ScheduledJobViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public DateTime? NextRun { get; set; }
        public DateTime? LastRun { get; set; }
        public string? LastStatus { get; set; }
        public bool IsEnabled { get; set; }

        public string StatusBadgeClass => LastStatus switch
        {
            "Success" => "status-success",
            "Failed" => "status-error",
            "Running" => "status-running",
            _ => "status-unknown"
        };
    }

    public class JobExecutionViewModel
    {
        public int Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string? Message { get; set; }
    }

    #endregion

    #region Backup & Restore

    public class BackupViewModel
    {
        public List<BackupEntryViewModel> Backups { get; set; } = new();
        public string StorageUsed { get; set; } = string.Empty;
        public string StorageLimit { get; set; } = string.Empty;
        public int StoragePercentage { get; set; }
    }

    public class BackupEntryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }

        public string TypeBadgeClass => Type switch
        {
            "Full" => "type-full",
            "Incremental" => "type-incremental",
            "Manual" => "type-manual",
            "Weekly" => "type-weekly",
            _ => "type-default"
        };
    }

    #endregion

    #region Health Check

    public class HealthCheckViewModel
    {
        public DateTime LastCheck { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public List<ServiceHealthViewModel> Services { get; set; } = new();
        public SystemMetricsViewModel Metrics { get; set; } = new();

        public string OverallStatusClass => OverallStatus switch
        {
            "Healthy" => "status-healthy",
            "Degraded" => "status-degraded",
            "Unhealthy" => "status-unhealthy",
            _ => "status-unknown"
        };
    }

    public class ServiceHealthViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ResponseTime { get; set; }
        public DateTime LastCheck { get; set; }
        public string? Message { get; set; }
        public string? Endpoint { get; set; }

        public string StatusClass => Status switch
        {
            "Healthy" => "status-healthy",
            "Degraded" => "status-degraded",
            "Unhealthy" => "status-unhealthy",
            _ => "status-unknown"
        };

        public string StatusIcon => Status switch
        {
            "Healthy" => "bi-check-circle-fill",
            "Degraded" => "bi-exclamation-circle-fill",
            "Unhealthy" => "bi-x-circle-fill",
            _ => "bi-question-circle"
        };
    }

    public class SystemMetricsViewModel
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public string MemoryUsed { get; set; } = string.Empty;
        public string MemoryTotal { get; set; } = string.Empty;
        public double DiskUsage { get; set; }
        public string DiskUsed { get; set; } = string.Empty;
        public string DiskTotal { get; set; } = string.Empty;
        public int ActiveConnections { get; set; }
        public int RequestsPerMinute { get; set; }
        public double AverageResponseTime { get; set; }
        public TimeSpan Uptime { get; set; }

        public string UptimeFormatted
        {
            get
            {
                var parts = new List<string>();
                if (Uptime.Days > 0) parts.Add($"{Uptime.Days}d");
                if (Uptime.Hours > 0) parts.Add($"{Uptime.Hours}h");
                if (Uptime.Minutes > 0) parts.Add($"{Uptime.Minutes}m");
                return string.Join(" ", parts);
            }
        }
    }

    #endregion
}
