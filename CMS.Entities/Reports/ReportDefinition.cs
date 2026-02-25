// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportDefinition.cs
// PROPÓSITO: Entidad principal para definición de reportes dinámicos
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Definición de un reporte dinámico con su fuente de datos
    /// </summary>
    [Table("report_definition", Schema = "admin")]
    public class ReportDefinition : IAuditableEntity
    {
        [Key]
        [Column("id_report")]
        public int Id { get; set; }

        [Column("report_code")]
        [Required]
        [MaxLength(50)]
        public string ReportCode { get; set; } = string.Empty;

        [Column("report_name")]
        [Required]
        [MaxLength(200)]
        public string ReportName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column("id_category")]
        public int CategoryId { get; set; }

        // Fuente de datos
        [Column("data_source_type")]
        [MaxLength(20)]
        public string DataSourceType { get; set; } = "SQL"; // SQL, TABLE, VIEW, STORED_PROCEDURE

        [Column("data_source")]
        [MaxLength(4000)]
        public string? DataSource { get; set; }

        [Column("connection_type")]
        [MaxLength(20)]
        public string ConnectionType { get; set; } = "ADMIN"; // ADMIN, COMPANY

        // Configuración visual
        [Column("icon")]
        [MaxLength(50)]
        public string Icon { get; set; } = "bi-file-earmark-bar-graph";

        [Column("default_page_size")]
        public int DefaultPageSize { get; set; } = 25;

        [Column("allow_export_excel")]
        public bool AllowExportExcel { get; set; } = true;

        [Column("allow_export_pdf")]
        public bool AllowExportPdf { get; set; } = true;

        [Column("allow_export_csv")]
        public bool AllowExportCsv { get; set; } = true;

        // Permisos
        [Column("required_permission")]
        [MaxLength(100)]
        public string? RequiredPermission { get; set; }

        // Estado
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("sort_order")]
        public int SortOrder { get; set; }

        // Navegación
        [ForeignKey("CategoryId")]
        public virtual ReportCategory? Category { get; set; }

        public virtual ICollection<ReportFilter> Filters { get; set; } = new List<ReportFilter>();
        public virtual ICollection<ReportColumn> Columns { get; set; } = new List<ReportColumn>();

        // IAuditableEntity
        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("row_pointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [Column("created_by")]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
