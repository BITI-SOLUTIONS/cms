// ================================================================================
// ARCHIVO: CMS.Entities/Admin/AuditLog.cs
// PROPÓSITO: Entidad para registro de auditoría de cambios
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin;

/// <summary>
/// Registro de auditoría de cambios en tablas configuradas.
/// Almacena el historial de todos los cambios (UPDATE, DELETE, INSERT)
/// realizados en las tablas configuradas en audit_table_config.
/// </summary>
[Table("audit_log", Schema = "admin")]
public class AuditLog
{
    [Key]
    [Column("id_audit_log")]
    public int Id { get; set; }

    // ===== Identificación de la tabla afectada =====

    /// <summary>
    /// Nombre de la base de datos
    /// </summary>
    [Column("database_name")]
    [Required]
    [MaxLength(100)]
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del schema
    /// </summary>
    [Column("schema_name")]
    [Required]
    [MaxLength(100)]
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la tabla
    /// </summary>
    [Column("table_name")]
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    // ===== Identificación del registro afectado =====

    /// <summary>
    /// Nombre de la columna que es clave primaria
    /// </summary>
    [Column("primary_key_column")]
    [MaxLength(100)]
    public string? PrimaryKeyColumn { get; set; }

    /// <summary>
    /// Valor de la clave primaria del registro afectado
    /// </summary>
    [Column("primary_key_value")]
    [MaxLength(500)]
    public string? PrimaryKeyValue { get; set; }

    // ===== Detalle del cambio =====

    /// <summary>
    /// Nombre del campo/columna modificado
    /// </summary>
    [Column("column_name")]
    [Required]
    [MaxLength(100)]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Valor anterior del campo.
    /// En DELETE es el valor que se eliminó.
    /// </summary>
    [Column("old_value")]
    public string? OldValue { get; set; }

    /// <summary>
    /// Valor nuevo del campo.
    /// En DELETE es igual a OldValue.
    /// </summary>
    [Column("new_value")]
    public string? NewValue { get; set; }

    // ===== Tipo de evento =====

    /// <summary>
    /// Tipo de operación: 'INSERT', 'UPDATE' o 'DELETE'
    /// </summary>
    [Column("event_type")]
    [Required]
    [MaxLength(10)]
    public string EventType { get; set; } = string.Empty;

    // ===== Usuario que realizó la acción =====

        /// <summary>
        /// Nombre de usuario del sistema que realizó la acción
        /// </summary>
        [Column("user_name")]
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        [Column("id_user")]
        [Required]
        public int IdUser { get; set; }

    // ===== Información adicional =====

    /// <summary>
    /// Dirección IP del cliente
    /// </summary>
    [Column("ip_address")]
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent del navegador
    /// </summary>
    [Column("user_agent")]
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Ruta de la solicitud HTTP
    /// </summary>
    [Column("request_path")]
    [MaxLength(500)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// Información adicional en formato JSON
    /// </summary>
    [Column("additional_info", TypeName = "jsonb")]
    public string? AdditionalInfo { get; set; }

    // ===== Campos de auditoría =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [MaxLength(30)]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Column("updated_by")]
        [MaxLength(30)]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

    // ===== Métodos auxiliares =====

    /// <summary>
    /// Obtiene el identificador completo de la tabla
    /// </summary>
    public string GetFullTableName()
    {
        return $"{DatabaseName}.{SchemaName}.{TableName}";
    }

    /// <summary>
    /// Verifica si es un evento de DELETE
    /// </summary>
    public bool IsDelete => EventType == AuditEventType.Delete;

    /// <summary>
    /// Verifica si es un evento de UPDATE
    /// </summary>
    public bool IsUpdate => EventType == AuditEventType.Update;

    /// <summary>
    /// Verifica si es un evento de INSERT
    /// </summary>
    public bool IsInsert => EventType == AuditEventType.Insert;
}

/// <summary>
/// Constantes para tipos de eventos de auditoría
/// </summary>
public static class AuditEventType
{
    public const string Insert = "INSERT";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
}
