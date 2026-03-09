// ================================================================================
// ARCHIVO: CMS.Entities/Admin/AuditTableConfig.cs
// PROPÓSITO: Entidad para configuración de tablas a auditar
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin;

/// <summary>
/// Configuración de qué tablas deben ser auditadas en el sistema.
/// Esta tabla define qué tablas (de cualquier BD/schema) serán monitoreadas
/// para registrar cambios en audit_log.
/// </summary>
[Table("audit_table_config", Schema = "admin")]
public class AuditTableConfig
{
    [Key]
    [Column("id_audit_table_config")]
    public int Id { get; set; }

    // ===== Identificación de la tabla =====

    /// <summary>
    /// Nombre de la base de datos donde está la tabla (ej: 'cms', 'sinai')
    /// </summary>
    [Column("database_name")]
    [Required]
    [MaxLength(100)]
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del schema donde está la tabla (ej: 'admin', 'sinai')
    /// </summary>
    [Column("schema_name")]
    [Required]
    [MaxLength(100)]
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la tabla a auditar (ej: 'user', 'item')
    /// </summary>
    [Column("table_name")]
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    // ===== Configuración de qué auditar =====

    /// <summary>
    /// Indica si se deben auditar las operaciones UPDATE
    /// </summary>
    [Column("audit_update")]
    public bool AuditUpdate { get; set; } = true;

    /// <summary>
    /// Indica si se deben auditar las operaciones DELETE
    /// </summary>
    [Column("audit_delete")]
    public bool AuditDelete { get; set; } = true;

    /// <summary>
    /// Indica si se deben auditar las operaciones INSERT
    /// </summary>
    [Column("audit_insert")]
    public bool AuditInsert { get; set; } = false;

    /// <summary>
    /// Columnas a excluir de la auditoría, separadas por coma.
    /// Ej: 'record_date,rowpointer'
    /// </summary>
    [Column("excluded_columns")]
    public string? ExcludedColumns { get; set; }

    /// <summary>
    /// Descripción de la tabla auditada
    /// </summary>
    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Indica si esta configuración está activa
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

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
    /// Obtiene la lista de columnas excluidas
    /// </summary>
    public List<string> GetExcludedColumnsList()
    {
        if (string.IsNullOrWhiteSpace(ExcludedColumns))
            return new List<string>();

        return ExcludedColumns
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToLower())
            .ToList();
    }

    /// <summary>
    /// Verifica si una columna está excluida
    /// </summary>
    public bool IsColumnExcluded(string columnName)
    {
        return GetExcludedColumnsList().Contains(columnName.ToLower());
    }

    /// <summary>
    /// Obtiene el identificador completo de la tabla
    /// </summary>
    public string GetFullTableName()
    {
        return $"{DatabaseName}.{SchemaName}.{TableName}";
    }
}
