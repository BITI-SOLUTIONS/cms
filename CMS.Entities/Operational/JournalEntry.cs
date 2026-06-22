// ================================================================================
// ARCHIVO: CMS.Entities/Operational/JournalEntry.cs
// PROPÓSITO: Entidades para Asientos de Diario (Journal Entries)
// DESCRIPCIÓN: Encabezado y líneas de asientos contables. Basado en mejores
//              prácticas de SAP FI, Oracle Financials, y otros ERP reconocidos.
//              Soporta multi-moneda, reversiones, aprobaciones, y trazabilidad.
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-20
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    // ============================================================
    // ENCABEZADO DE ASIENTO DE DIARIO
    // ============================================================

    /// <summary>
    /// Encabezado de asiento de diario (journal entry header).
    /// Registra todas las transacciones contables del sistema.
    /// </summary>
    [Table("journal_entry")]
    public class JournalEntry
    {
        // ===== PK + IDENTIFICACIÓN =====

        [Key]
        [Column("id_journal_entry")]
        public int IdJournalEntry { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("entry_number")]
        public string EntryNumber { get; set; } = string.Empty;

        /// <summary>Tipo: Manual, Automatic, Reversal, Adjustment, Closing, Opening</summary>
        [Required]
        [MaxLength(30)]
        [Column("entry_type")]
        public string EntryType { get; set; } = "Manual";

        // ===== REFERENCIA =====

        /// <summary>Referencia del documento origen (factura, recibo, etc.)</summary>
        [MaxLength(100)]
        [Column("reference")]
        public string? Reference { get; set; }

        // ===== FECHAS =====

        [Column("entry_date")]
        public DateOnly EntryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Column("posting_date")]
        public DateOnly PostingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        // ===== MONEDA Y CONVERSIÓN =====

        [Required]
        [MaxLength(3)]
        [Column("currency_code")]
        public string CurrencyCode { get; set; } = "CRC";

        [Column("exchange_rate")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        // ===== ESTADO Y CONTROL =====

        /// <summary>Estado: Draft, Posted, Reversed, Cancelled</summary>
        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Draft";

        [Column("is_reversing")]
        public bool IsReversing { get; set; } = false;

        /// <summary>FK al asiento que está siendo revertido</summary>
        [Column("id_reversed_entry")]
        public int? IdReversedEntry { get; set; }

        [Column("reversal_date")]
        public DateOnly? ReversalDate { get; set; }

        // ===== TOTALES =====

        [Column("debit_total")]
        public decimal DebitTotal { get; set; } = 0.00m;

        [Column("credit_total")]
        public decimal CreditTotal { get; set; } = 0.00m;

        // ===== CONTROL DE APROBACIÓN =====

        [Column("requires_approval")]
        public bool RequiresApproval { get; set; } = false;

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        /// <summary>FK lógica cross-DB a cms.admin.user</summary>
        [Column("approved_by_user_id")]
        public int? ApprovedByUserId { get; set; }

        [MaxLength(500)]
        [Column("approval_notes")]
        public string? ApprovalNotes { get; set; }

        // ===== AUDITORÍA Y CONTROL =====

        [Column("posted_date")]
        public DateTime? PostedDate { get; set; }

        /// <summary>FK lógica cross-DB a cms.admin.user</summary>
        [Column("posted_by_user_id")]
        public int? PostedByUserId { get; set; }

        [Column("cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        /// <summary>FK lógica cross-DB a cms.admin.user</summary>
        [Column("cancelled_by_user_id")]
        public int? CancelledByUserId { get; set; }

        /// <summary>FK a journal_entry_cancel_reason</summary>
        [Column("id_journal_entry_cancel_reason")]
        public int? IdJournalEntryCancelReason { get; set; }

        // ===== CAMPOS DE AUDITORÍA ESTÁNDAR =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        // ===== NAVEGACIÓN =====

        /// <summary>Líneas del asiento (detalle)</summary>
        [NotMapped]
        public List<JournalEntryLine> Lines { get; set; } = new();

        /// <summary>Razón de cancelación (si fue cancelado)</summary>
        public virtual JournalEntryCancelReason? CancelReason { get; set; }
    }

    // ============================================================
    // LÍNEA DE ASIENTO DE DIARIO
    // ============================================================

    /// <summary>
    /// Línea de asiento de diario (journal entry line).
    /// Detalle de las partidas contables (débitos y créditos).
    /// PK compuesta: (IdJournalEntry, IdJournalEntryLine)
    /// IdJournalEntryLine es el número de línea secuencial dentro del asiento (1, 2, 3...)
    /// </summary>
    [Table("journal_entry_line")]
    public class JournalEntryLine
    {
        // ===== PK COMPUESTA + RELACIÓN CON ENCABEZADO =====

        [Key]
        [Column("id_journal_entry", Order = 0)]
        public int IdJournalEntry { get; set; }

        [Key]
        [Column("id_journal_entry_line", Order = 1)]
        public int IdJournalEntryLine { get; set; }

        // ===== CUENTA CONTABLE =====

        /// <summary>FK a sinai.chart_of_accounts (REQUERIDO)</summary>
        [Required]
        [Column("id_chart_of_accounts")]
        public int IdChartOfAccounts { get; set; }

        // ===== DESCRIPCIÓN Y REFERENCIA =====

        [Required]
        [MaxLength(500)]
        [Column("line_description")]
        public string LineDescription { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("reference")]
        public string? Reference { get; set; }

        // ===== DÉBITO / CRÉDITO =====

        [Column("debit_amount")]
        public decimal DebitAmount { get; set; } = 0.00m;

        [Column("credit_amount")]
        public decimal CreditAmount { get; set; } = 0.00m;

        // ===== MONEDA Y CONVERSIÓN =====

        [Required]
        [MaxLength(3)]
        [Column("currency_code")]
        public string CurrencyCode { get; set; } = "CRC";

        [Column("exchange_rate")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        [Column("debit_amount_base")]
        public decimal DebitAmountBase { get; set; } = 0.00m;

        [Column("credit_amount_base")]
        public decimal CreditAmountBase { get; set; } = 0.00m;

        // ===== DIMENSIONES ANALÍTICAS =====

        [MaxLength(30)]
        [Column("cost_center_code")]
        public string? CostCenterCode { get; set; }

        [MaxLength(200)]
        [Column("cost_center_name")]
        public string? CostCenterName { get; set; }

        [MaxLength(30)]
        [Column("project_code")]
        public string? ProjectCode { get; set; }

        [MaxLength(200)]
        [Column("project_name")]
        public string? ProjectName { get; set; }

        [MaxLength(30)]
        [Column("department_code")]
        public string? DepartmentCode { get; set; }

        [MaxLength(200)]
        [Column("department_name")]
        public string? DepartmentName { get; set; }

        // ===== BUSINESS PARTNER =====

        /// <summary>Tipo: Customer, Supplier, Employee, Other</summary>
        [MaxLength(20)]
        [Column("business_partner_type")]
        public string? BusinessPartnerType { get; set; }

        [MaxLength(50)]
        [Column("business_partner_code")]
        public string? BusinessPartnerCode { get; set; }

        [MaxLength(200)]
        [Column("business_partner_name")]
        public string? BusinessPartnerName { get; set; }

        // ===== FECHA DE VENCIMIENTO =====

        [Column("due_date")]
        public DateOnly? DueDate { get; set; }

        // ===== IMPUESTOS =====

        [MaxLength(20)]
        [Column("tax_code")]
        public string? TaxCode { get; set; }

        [Column("tax_rate")]
        public decimal? TaxRate { get; set; }

        [Column("tax_amount")]
        public decimal? TaxAmount { get; set; }

        // ===== RECONCILIACIÓN =====

        [Column("is_reconciled")]
        public bool IsReconciled { get; set; } = false;

        [Column("reconciliation_date")]
        public DateOnly? ReconciliationDate { get; set; }

        [MaxLength(100)]
        [Column("reconciliation_ref")]
        public string? ReconciliationRef { get; set; }

        // ===== CAMPOS DE AUDITORÍA ESTÁNDAR =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }

    // ============================================================
    // CONSTANTES
    // ============================================================

    /// <summary>Tipos de asiento de diario</summary>
    public static class JournalEntryType
    {
        public const string Manual = "Manual";              // Asiento manual
        public const string Automatic = "Automatic";        // Asiento automático
        public const string Reversal = "Reversal";          // Asiento de reversión
        public const string Adjustment = "Adjustment";      // Asiento de ajuste
        public const string Closing = "Closing";            // Asiento de cierre
        public const string Opening = "Opening";            // Asiento de apertura
    }

    /// <summary>Estados de asiento de diario</summary>
    public static class JournalEntryStatus
    {
        public const string Draft = "Draft";                // Borrador
        public const string Posted = "Posted";              // Contabilizado
        public const string Reversed = "Reversed";          // Revertido
        public const string Cancelled = "Cancelled";        // Cancelado
    }

    /// <summary>Tipos de socio de negocio</summary>
    public static class BusinessPartnerType
    {
        public const string Customer = "Customer";          // Cliente
        public const string Supplier = "Supplier";          // Proveedor
        public const string Employee = "Employee";          // Empleado
        public const string Other = "Other";                // Otro
    }

    /// <summary>Módulos fuente de asientos automáticos</summary>
    public static class JournalEntrySourceModule
    {
        public const string Sales = "Sales";                // Ventas
        public const string Purchasing = "Purchasing";      // Compras
        public const string Inventory = "Inventory";        // Inventario
        public const string Payroll = "Payroll";            // Nómina
        public const string FixedAssets = "FixedAssets";    // Activos Fijos
        public const string Banking = "Banking";            // Banca
        public const string Accounting = "Accounting";      // Contabilidad
    }

    /// <summary>Tipos de documento fuente</summary>
    public static class JournalEntrySourceDocumentType
    {
        public const string Invoice = "Invoice";            // Factura
        public const string Payment = "Payment";            // Pago
        public const string Receipt = "Receipt";            // Recibo
        public const string Adjustment = "Adjustment";      // Ajuste
        public const string Depreciation = "Depreciation";  // Depreciación
        public const string Transfer = "Transfer";          // Traslado
    }
}
