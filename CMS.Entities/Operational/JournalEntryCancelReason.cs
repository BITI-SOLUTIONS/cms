// ================================================================================
// ARCHIVO: CMS.Entities/Operational/JournalEntryCancelReason.cs
// PROPÓSITO: Entidad para catálogo de razones de cancelación de asientos
// DESCRIPCIÓN: Catálogo maestro de razones predefinidas para cancelar asientos
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-XX
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Catálogo de razones de cancelación de asientos contables
    /// Tabla: {company_schema}.journal_entry_cancel_reason
    /// </summary>
    [Table("journal_entry_cancel_reason")]
    public class JournalEntryCancelReason
    {
        // ===== IDENTIFICADOR =====

        /// <summary>
        /// ID único de la razón de cancelación
        /// </summary>
        [Key]
        [Column("id_journal_entry_cancel_reason")]
        public int IdJournalEntryCancelReason { get; set; }

        // ===== CAMPOS PRINCIPALES =====

        /// <summary>
        /// Código único de la razón (ej: DATA_ERROR, DUPLICATE, etc.)
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo de la razón
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de la razón
        /// </summary>
        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        // ===== CONTROL =====

        /// <summary>
        /// Indica si la razón está activa y disponible para uso
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Orden de visualización en listas
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        // ===== AUDITORÍA =====

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última modificación
        /// </summary>
        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Usuario que modificó el registro por última vez
        /// </summary>
        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único global del registro (UUID)
        /// </summary>
        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        // ===== NAVEGACIÓN (EF Core) =====

        /// <summary>
        /// Asientos contables que fueron cancelados con esta razón
        /// (No mapeado en BD, solo para EF Core)
        /// </summary>
        public virtual ICollection<JournalEntry>? JournalEntries { get; set; }
    }

    // ===== CONSTANTES =====

    /// <summary>
    /// Códigos predefinidos de razones de cancelación
    /// </summary>
    public static class JournalEntryCancelReasonCodes
    {
        public const string DataError = "DATA_ERROR";
        public const string Duplicate = "DUPLICATE";
        public const string WrongAccount = "WRONG_ACCOUNT";
        public const string WrongAmount = "WRONG_AMOUNT";
        public const string WrongDate = "WRONG_DATE";
        public const string WrongPeriod = "WRONG_PERIOD";
        public const string WrongCostCenter = "WRONG_COST_CENTER";
        public const string NotApproved = "NOT_APPROVED";
        public const string PolicyChange = "POLICY_CHANGE";
        public const string ExternalDocCancelled = "EXTERNAL_DOC_CANCELLED";
        public const string AdjustmentNeeded = "ADJUSTMENT_NEEDED";
        public const string UserRequest = "USER_REQUEST";
        public const string SystemError = "SYSTEM_ERROR";
        public const string AuditRequirement = "AUDIT_REQUIREMENT";
        public const string Other = "OTHER";
    }
}
