// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportCategory.cs
// PROPÓSITO: Entidad para categorías de reportes
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Categoría para agrupar reportes (Ventas, Inventario, Finanzas, etc.)
    /// </summary>
    [Table("report_category", Schema = "admin")]
    public class ReportCategory : IAuditableEntity
    {
        [Key]
        [Column("id_report_category")]
        public int Id { get; set; }

        [Column("category_code")]
        [Required]
        [MaxLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Column("category_name")]
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("icon")]
        [MaxLength(50)]
        public string Icon { get; set; } = "bi-folder";

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navegación
        public virtual ICollection<ReportDefinition> Reports { get; set; } = new List<ReportDefinition>();

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
