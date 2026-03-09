// ================================================================================
// ARCHIVO: CMS.Data/Services/AuditService.cs
// PROPÓSITO: Implementación del servicio de auditoría
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using CMS.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services;

/// <summary>
/// Servicio de auditoría para registrar cambios en tablas configuradas.
/// Usa caché en memoria para optimizar las verificaciones de configuración.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuditService> _logger;
    
    private const string CacheKeyPrefix = "AuditConfig_";
    private const string CacheKeyAllConfigs = "AuditConfig_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public AuditService(
        AppDbContext dbContext,
        IMemoryCache cache,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AuditTableConfig?> GetAuditConfigAsync(string databaseName, string schemaName, string tableName)
    {
        var cacheKey = $"{CacheKeyPrefix}{databaseName}_{schemaName}_{tableName}";

        if (_cache.TryGetValue(cacheKey, out AuditTableConfig? config))
        {
            return config;
        }

        config = await _dbContext.AuditTableConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.DatabaseName == databaseName &&
                c.SchemaName == schemaName &&
                c.TableName == tableName &&
                c.IsActive);

        if (config != null)
        {
            _cache.Set(cacheKey, config, CacheDuration);
        }

        return config;
    }

    /// <inheritdoc/>
    public async Task RecordUpdateAsync(
        string databaseName,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        string primaryKeyValue,
        Dictionary<string, (string? OldValue, string? NewValue)> changes,
        string userName,
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? requestPath = null)
    {
        try
        {
            var config = await GetAuditConfigAsync(databaseName, schemaName, tableName);

            if (config == null || !config.AuditUpdate)
            {
                return; // No está configurada o no audita UPDATEs
            }

            var excludedColumns = config.GetExcludedColumnsList();
            var auditLogs = new List<AuditLog>();

            foreach (var change in changes)
            {
                // Saltar columnas excluidas
                if (excludedColumns.Contains(change.Key.ToLower()))
                    continue;

                // Solo registrar si realmente cambió el valor
                if (change.Value.OldValue == change.Value.NewValue)
                    continue;

                auditLogs.Add(new AuditLog
                {
                    DatabaseName = databaseName,
                    SchemaName = schemaName,
                    TableName = tableName,
                    PrimaryKeyColumn = primaryKeyColumn,
                    PrimaryKeyValue = primaryKeyValue,
                    ColumnName = change.Key,
                    OldValue = change.Value.OldValue,
                    NewValue = change.Value.NewValue,
                    EventType = AuditEventType.Update,
                    UserName = userName,
                    IdUser = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RequestPath = requestPath,
                    CreateDate = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow,
                    CreatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName,
                    UpdatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName
                });
            }

            if (auditLogs.Any())
            {
                await _dbContext.AuditLogs.AddRangeAsync(auditLogs);
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug(
                    "Auditoría UPDATE: {Table} PK={PK}, {Count} campos cambiados por {User}",
                    $"{databaseName}.{schemaName}.{tableName}",
                    primaryKeyValue,
                    auditLogs.Count,
                    userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error registrando auditoría UPDATE para {Table} PK={PK}",
                $"{databaseName}.{schemaName}.{tableName}",
                primaryKeyValue);
            // No lanzar excepción para no afectar la operación principal
        }
    }

    /// <inheritdoc/>
    public async Task RecordDeleteAsync(
        string databaseName,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        string primaryKeyValue,
        Dictionary<string, string?> entityValues,
        string userName,
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? requestPath = null)
    {
        try
        {
            var config = await GetAuditConfigAsync(databaseName, schemaName, tableName);

            if (config == null || !config.AuditDelete)
            {
                return;
            }

            var excludedColumns = config.GetExcludedColumnsList();
            var auditLogs = new List<AuditLog>();

            foreach (var field in entityValues)
            {
                if (excludedColumns.Contains(field.Key.ToLower()))
                    continue;

                // En DELETE, old_value y new_value son iguales (el valor que se eliminó)
                auditLogs.Add(new AuditLog
                {
                    DatabaseName = databaseName,
                    SchemaName = schemaName,
                    TableName = tableName,
                    PrimaryKeyColumn = primaryKeyColumn,
                    PrimaryKeyValue = primaryKeyValue,
                    ColumnName = field.Key,
                    OldValue = field.Value,
                    NewValue = field.Value, // En DELETE es igual
                    EventType = AuditEventType.Delete,
                    UserName = userName,
                    IdUser = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RequestPath = requestPath,
                    CreateDate = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow,
                    CreatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName,
                    UpdatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName
                });
            }

            if (auditLogs.Any())
            {
                await _dbContext.AuditLogs.AddRangeAsync(auditLogs);
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug(
                    "Auditoría DELETE: {Table} PK={PK}, {Count} campos registrados por {User}",
                    $"{databaseName}.{schemaName}.{tableName}",
                    primaryKeyValue,
                    auditLogs.Count,
                    userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error registrando auditoría DELETE para {Table} PK={PK}",
                $"{databaseName}.{schemaName}.{tableName}",
                primaryKeyValue);
        }
    }

    /// <inheritdoc/>
    public async Task RecordInsertAsync(
        string databaseName,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        string primaryKeyValue,
        Dictionary<string, string?> entityValues,
        string userName,
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? requestPath = null)
    {
        try
        {
            var config = await GetAuditConfigAsync(databaseName, schemaName, tableName);

            if (config == null || !config.AuditInsert)
            {
                return;
            }

            var excludedColumns = config.GetExcludedColumnsList();
            var auditLogs = new List<AuditLog>();

            foreach (var field in entityValues)
            {
                if (excludedColumns.Contains(field.Key.ToLower()))
                    continue;

                auditLogs.Add(new AuditLog
                {
                    DatabaseName = databaseName,
                    SchemaName = schemaName,
                    TableName = tableName,
                    PrimaryKeyColumn = primaryKeyColumn,
                    PrimaryKeyValue = primaryKeyValue,
                    ColumnName = field.Key,
                    OldValue = null, // En INSERT no hay valor anterior
                    NewValue = field.Value,
                    EventType = AuditEventType.Insert,
                    UserName = userName,
                    IdUser = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RequestPath = requestPath,
                    CreateDate = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow,
                    CreatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName,
                    UpdatedBy = userName.Length > 30 ? userName.Substring(0, 30) : userName
                });
            }

            if (auditLogs.Any())
            {
                await _dbContext.AuditLogs.AddRangeAsync(auditLogs);
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug(
                    "Auditoría INSERT: {Table} PK={PK}, {Count} campos registrados por {User}",
                    $"{databaseName}.{schemaName}.{tableName}",
                    primaryKeyValue,
                    auditLogs.Count,
                    userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error registrando auditoría INSERT para {Table} PK={PK}",
                $"{databaseName}.{schemaName}.{tableName}",
                primaryKeyValue);
        }
    }

    /// <inheritdoc/>
    public async Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(
        string? databaseName = null,
        string? schemaName = null,
        string? tableName = null,
        string? eventType = null,
        string? userName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? primaryKeyValue = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(databaseName))
            query = query.Where(a => a.DatabaseName == databaseName);

        if (!string.IsNullOrWhiteSpace(schemaName))
            query = query.Where(a => a.SchemaName == schemaName);

        if (!string.IsNullOrWhiteSpace(tableName))
            query = query.Where(a => a.TableName == tableName);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(a => a.EventType == eventType);

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName.Contains(userName));

        if (fromDate.HasValue)
            query = query.Where(a => a.CreateDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreateDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(primaryKeyValue))
            query = query.Where(a => a.PrimaryKeyValue == primaryKeyValue);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.CreateDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    /// <inheritdoc/>
    public async Task<List<AuditTableConfig>> GetAllAuditConfigsAsync(bool onlyActive = true)
    {
        if (onlyActive && _cache.TryGetValue(CacheKeyAllConfigs, out List<AuditTableConfig>? configs))
        {
            return configs ?? new List<AuditTableConfig>();
        }

        var query = _dbContext.AuditTableConfigs.AsNoTracking();
        
        if (onlyActive)
            query = query.Where(c => c.IsActive);

        configs = await query
            .OrderBy(c => c.DatabaseName)
            .ThenBy(c => c.SchemaName)
            .ThenBy(c => c.TableName)
            .ToListAsync();

        if (onlyActive)
        {
            _cache.Set(CacheKeyAllConfigs, configs, CacheDuration);
        }

        return configs;
    }

    /// <inheritdoc/>
    public async Task<AuditTableConfig> UpsertAuditConfigAsync(
        string databaseName,
        string schemaName,
        string tableName,
        bool auditUpdate = true,
        bool auditDelete = true,
        bool auditInsert = false,
        string? excludedColumns = null,
        string? description = null,
        string userName = "SYSTEM")
    {
        var existing = await _dbContext.AuditTableConfigs
            .FirstOrDefaultAsync(c =>
                c.DatabaseName == databaseName &&
                c.SchemaName == schemaName &&
                c.TableName == tableName);

        if (existing != null)
        {
            existing.AuditUpdate = auditUpdate;
            existing.AuditDelete = auditDelete;
            existing.AuditInsert = auditInsert;
            existing.ExcludedColumns = excludedColumns;
            existing.Description = description;
            existing.RecordDate = DateTime.UtcNow;
            existing.UpdatedBy = userName;
        }
        else
        {
            existing = new AuditTableConfig
            {
                DatabaseName = databaseName,
                SchemaName = schemaName,
                TableName = tableName,
                AuditUpdate = auditUpdate,
                AuditDelete = auditDelete,
                AuditInsert = auditInsert,
                ExcludedColumns = excludedColumns,
                Description = description,
                IsActive = true,
                CreatedBy = userName,
                UpdatedBy = userName
            };
            _dbContext.AuditTableConfigs.Add(existing);
        }

        await _dbContext.SaveChangesAsync();
        await RefreshCacheAsync();

        return existing;
    }

    /// <inheritdoc/>
    public Task RefreshCacheAsync()
    {
        // Limpiar todas las cachés relacionadas con auditoría
        _cache.Remove(CacheKeyAllConfigs);
        
        // Las cachés individuales se invalidan por TTL
        _logger.LogInformation("Caché de auditoría refrescada");
        
        return Task.CompletedTask;
    }
}
