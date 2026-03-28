// ================================================================================
// ARCHIVO: CMS.Entities/Admin/FileCategory.cs
// PROPÓSITO: Entidad para categorías de archivos (BD Central)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Categoría de archivos (tabla admin.file_category).
    /// Las categorías son globales y reutilizables por todas las compañías.
    /// </summary>
    [Table("file_category", Schema = "admin")]
    public class FileCategory
    {
        [Key]
        [Column("id_file_category")]
        public int IdFileCategory { get; set; }

        /// <summary>
        /// Código único de la categoría (ej: DOC, LEGAL, FIN)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo de la categoría
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la categoría
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Icono Bootstrap Icons (ej: bi-folder)
        /// </summary>
        [MaxLength(50)]
        [Column("icon")]
        public string Icon { get; set; } = "bi-folder";

        /// <summary>
        /// Color hexadecimal (ej: #6366f1)
        /// </summary>
        [MaxLength(7)]
        [Column("color")]
        public string Color { get; set; } = "#6366f1";

        /// <summary>
        /// ID de la categoría padre (para jerarquía)
        /// </summary>
        [Column("parent_id")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Si la categoría está activa
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Auditoría estándar
        [Column("createdate")]
        public DateTime Createdate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "system";

        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "system";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        // Navegación
        [ForeignKey("ParentId")]
        public virtual FileCategory? Parent { get; set; }

        public virtual ICollection<FileCategory> Children { get; set; } = new List<FileCategory>();
    }
}
