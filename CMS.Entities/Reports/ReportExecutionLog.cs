// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportExecutionLog.cs
// PROPÓSITO: Entidad para log de ejecución de reportes
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Registro de ejecuciones de reportes para auditoría
    /// </summary>
    [Table("report_execution_log", Schema = "admin")]
    public class ReportExecutionLog : IAuditableEntity
    {
        [Key]
        [Column("id_report_execution_log")]
        public int Id { get; set; }

        [Column("id_report_definition")]
        public int ReportId { get; set; }

        [Column("id_user")]
        public int UserId { get; set; }

        [Column("id_company")]
        public int CompanyId { get; set; }

        [Column("execution_date")]
        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;

        [Column("filters_used", TypeName = "jsonb")]
        public string? FiltersUsed { get; set; }

        [Column("rows_returned")]
        public int? RowsReturned { get; set; }

        [Column("execution_time_ms")]
        public int? ExecutionTimeMs { get; set; }

        [Column("export_type")]
        [MaxLength(20)]
        public string? ExportType { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "SUCCESS";

        [Column("error_message")]
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        [Column("ip_address")]
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Navegación
        [ForeignKey("ReportId")]
        public virtual ReportDefinition? Report { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        // IAuditableEntity
        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [Column("created_by")]
        [MaxLength(30)]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        [MaxLength(30)]
        public string? UpdatedBy { get; set; }
    }
}
