// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportColumn.cs
// PROPÓSITO: Entidad para columnas del resultado del reporte
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Definición de columna para mostrar en el resultado del reporte
    /// </summary>
    [Table("report_column", Schema = "admin")]
    public class ReportColumn : IAuditableEntity
    {
        [Key]
        [Column("id_column")]
        public int Id { get; set; }

        [Column("id_report")]
        public int ReportId { get; set; }

        // Identificación de la columna
        [Column("column_key")]
        [Required]
        [MaxLength(100)]
        public string ColumnKey { get; set; } = string.Empty; // Nombre del campo en SQL

        [Column("column_name")]
        [Required]
        [MaxLength(150)]
        public string ColumnName { get; set; } = string.Empty; // Nombre para mostrar

        [Column("column_description")]
        [MaxLength(300)]
        public string? ColumnDescription { get; set; }

        // Tipo de datos y formato
        [Column("data_type")]
        [MaxLength(30)]
        public string DataType { get; set; } = "STRING";
        // STRING, NUMBER, INTEGER, DECIMAL, CURRENCY, DATE, DATETIME, 
        // BOOLEAN, PERCENTAGE, BADGE, LINK, IMAGE, ACTION

        [Column("format_pattern")]
        [MaxLength(100)]
        public string? FormatPattern { get; set; } // "N2", "C2", "yyyy-MM-dd"

        // Configuración visual
        [Column("width")]
        [MaxLength(20)]
        public string? Width { get; set; } // "100px", "auto", "15%"

        [Column("min_width")]
        [MaxLength(20)]
        public string? MinWidth { get; set; }

        [Column("text_align")]
        [MaxLength(20)]
        public string TextAlign { get; set; } = "LEFT"; // LEFT, CENTER, RIGHT

        [Column("css_class")]
        [MaxLength(100)]
        public string? CssClass { get; set; }

        // Para tipo BADGE
        [Column("badge_config", TypeName = "jsonb")]
        public string? BadgeConfig { get; set; }

        // Para tipo LINK
        [Column("link_template")]
        [MaxLength(500)]
        public string? LinkTemplate { get; set; }

        [Column("link_target")]
        [MaxLength(20)]
        public string LinkTarget { get; set; } = "_self";

        // Comportamiento
        [Column("is_sortable")]
        public bool IsSortable { get; set; } = true;

        [Column("is_filterable")]
        public bool IsFilterable { get; set; }

        [Column("is_visible")]
        public bool IsVisible { get; set; } = true;

        [Column("is_exportable")]
        public bool IsExportable { get; set; } = true;

        // Agregaciones
        [Column("show_total")]
        public bool ShowTotal { get; set; }

        [Column("aggregation_type")]
        [MaxLength(20)]
        public string? AggregationType { get; set; } // SUM, AVG, COUNT, MIN, MAX

        // Orden
        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("default_sort_direction")]
        [MaxLength(4)]
        public string? DefaultSortDirection { get; set; } // ASC, DESC

        // Estado
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navegación
        [ForeignKey("ReportId")]
        public virtual ReportDefinition? Report { get; set; }

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
