// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportFilter.cs
// PROPÓSITO: Entidad para filtros dinámicos de cada reporte
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Filtro dinámico configurable para un reporte
    /// </summary>
    [Table("report_filter", Schema = "admin")]
    public class ReportFilter : IAuditableEntity
    {
        [Key]
        [Column("id_report_filter")]
        public int Id { get; set; }

        [Column("id_report_definition")]
        public int ReportId { get; set; }

        // Identificación del filtro
        [Column("filter_key")]
        [Required]
        [MaxLength(50)]
        public string FilterKey { get; set; } = string.Empty; // Nombre del parámetro (@startDate)

        [Column("filter_name")]
        [Required]
        [MaxLength(100)]
        public string FilterName { get; set; } = string.Empty; // Nombre para mostrar

        [Column("filter_description")]
        [MaxLength(300)]
        public string? FilterDescription { get; set; }

        // Tipo de control
        [Column("filter_type")]
        [MaxLength(30)]
        public string FilterType { get; set; } = "TEXT";
        // TEXT, NUMBER, DATE, DATETIME, DATERANGE, SELECT, MULTISELECT, 
        // CHECKBOX, RADIO, AUTOCOMPLETE, COMPANY, USER

        // Configuración del control
        [Column("data_source")]
        [MaxLength(1000)]
        public string? DataSource { get; set; } // Para SELECT/MULTISELECT

        [Column("default_value")]
        [MaxLength(500)]
        public string? DefaultValue { get; set; }

        [Column("placeholder")]
        [MaxLength(200)]
        public string? Placeholder { get; set; }

        // Validaciones
        [Column("is_required")]
        public bool IsRequired { get; set; }

        [Column("min_value")]
        [MaxLength(100)]
        public string? MinValue { get; set; }

        [Column("max_value")]
        [MaxLength(100)]
        public string? MaxValue { get; set; }

        [Column("regex_pattern")]
        [MaxLength(300)]
        public string? RegexPattern { get; set; }

        // Layout
        [Column("col_span")]
        public int ColSpan { get; set; } = 3; // Columnas en grid de 12

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("group_name")]
        [MaxLength(50)]
        public string? GroupName { get; set; }

        // Estado
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_visible")]
        public bool IsVisible { get; set; } = true;

        // Navegación
        [ForeignKey("ReportId")]
        public virtual ReportDefinition? Report { get; set; }

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
