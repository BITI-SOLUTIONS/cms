// ================================================================================
// ARCHIVO: CMS.Data/Services/IAuditService.cs
// PROPÓSITO: Interface para el servicio de auditoría
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using CMS.Entities.Admin;

namespace CMS.Data.Services;

/// <summary>
/// Interface para el servicio de auditoría de cambios en tablas
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Verifica si una tabla está configurada para auditoría
    /// </summary>
    Task<AuditTableConfig?> GetAuditConfigAsync(string databaseName, string schemaName, string tableName);

    /// <summary>
    /// Registra cambios de UPDATE en una entidad
    /// </summary>
    /// <param name="databaseName">Nombre de la base de datos</param>
    /// <param name="schemaName">Nombre del schema</param>
    /// <param name="tableName">Nombre de la tabla</param>
    /// <param name="primaryKeyColumn">Nombre de la columna PK</param>
    /// <param name="primaryKeyValue">Valor de la PK</param>
    /// <param name="changes">Diccionario de cambios: NombreColumna -> (ValorAnterior, ValorNuevo)</param>
    /// <param name="userName">Usuario que realizó el cambio</param>
    /// <param name="userId">ID del usuario (requerido)</param>
    /// <param name="ipAddress">IP del cliente (opcional)</param>
    /// <param name="userAgent">User agent (opcional)</param>
    /// <param name="requestPath">Ruta HTTP (opcional)</param>
    Task RecordUpdateAsync(
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
        string? requestPath = null);

    /// <summary>
    /// Registra un DELETE en una entidad
    /// </summary>
    /// <param name="databaseName">Nombre de la base de datos</param>
    /// <param name="schemaName">Nombre del schema</param>
    /// <param name="tableName">Nombre de la tabla</param>
    /// <param name="primaryKeyColumn">Nombre de la columna PK</param>
    /// <param name="primaryKeyValue">Valor de la PK</param>
    /// <param name="entityValues">Diccionario con todos los valores de la entidad eliminada</param>
    /// <param name="userName">Usuario que realizó el cambio</param>
    /// <param name="userId">ID del usuario (requerido)</param>
    /// <param name="ipAddress">IP del cliente (opcional)</param>
    /// <param name="userAgent">User agent (opcional)</param>
    /// <param name="requestPath">Ruta HTTP (opcional)</param>
    Task RecordDeleteAsync(
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
        string? requestPath = null);

    /// <summary>
    /// Registra un INSERT en una entidad (si está configurado)
    /// </summary>
    Task RecordInsertAsync(
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
        string? requestPath = null);

    /// <summary>
    /// Obtiene el historial de auditoría con filtros
    /// </summary>
    Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(
        string? databaseName = null,
        string? schemaName = null,
        string? tableName = null,
        string? eventType = null,
        string? userName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? primaryKeyValue = null,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Obtiene todas las configuraciones de tablas auditadas
    /// </summary>
    Task<List<AuditTableConfig>> GetAllAuditConfigsAsync(bool onlyActive = true);

    /// <summary>
    /// Agrega o actualiza una configuración de auditoría
    /// </summary>
    Task<AuditTableConfig> UpsertAuditConfigAsync(
        string databaseName,
        string schemaName,
        string tableName,
        bool auditUpdate = true,
        bool auditDelete = true,
        bool auditInsert = false,
        string? excludedColumns = null,
        string? description = null,
        string userName = "SYSTEM");

    /// <summary>
    /// Recarga la caché de configuraciones de auditoría
    /// </summary>
    Task RefreshCacheAsync();
}
