// ================================================================================
// ARCHIVO: CMS.Entities/Operational/FileFolder.cs
// PROPÓSITO: Entidad para carpetas de archivos (BD Operacional)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Carpeta de archivos (tabla {schema}.file_folder).
    /// Permite organizar archivos en una estructura jerárquica.
    /// </summary>
    public class FileFolder
    {
        [Key]
        [Column("id_file_folder")]
        public int IdFileFolder { get; set; }

        // ===== Identificación =====

        /// <summary>
        /// Código único de la carpeta (código de negocio)
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la carpeta
        /// </summary>
        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la carpeta
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        // ===== Jerarquía =====

        /// <summary>
        /// ID de la carpeta padre
        /// </summary>
        [Column("parent_id")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Path completo (ej: /ROOT/DOCUMENTS/2024)
        /// </summary>
        [MaxLength(1000)]
        [Column("path")]
        public string? Path { get; set; }

        /// <summary>
        /// Nivel de profundidad (0 = raíz)
        /// </summary>
        [Column("level")]
        public int Level { get; set; } = 0;

        // ===== Categorización =====

        /// <summary>
        /// Código de categoría (referencia a admin.file_category)
        /// </summary>
        [MaxLength(20)]
        [Column("category_code")]
        public string? CategoryCode { get; set; }

        // ===== Apariencia =====

        /// <summary>
        /// Icono Bootstrap Icons
        /// </summary>
        [MaxLength(50)]
        [Column("icon")]
        public string Icon { get; set; } = "bi-folder";

        /// <summary>
        /// Color hexadecimal
        /// </summary>
        [MaxLength(7)]
        [Column("color")]
        public string Color { get; set; } = "#fbbf24";

        // ===== Configuración =====

        /// <summary>
        /// Si es carpeta del sistema (no se puede eliminar)
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Si es carpeta privada (solo visible para el creador)
        /// </summary>
        [Column("is_private")]
        public bool IsPrivate { get; set; } = false;

        /// <summary>
        /// Si permite acceso público
        /// </summary>
        [Column("allow_public_access")]
        public bool AllowPublicAccess { get; set; } = false;

        /// <summary>
        /// Orden de visualización
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        // ===== Estado =====

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        // ===== Auditoría estándar =====

        [Column("createdate")]
        public DateTime Createdate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "system";

        [MaxLength(100)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "system";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        // ===== Navegación =====

        [ForeignKey("ParentId")]
        public virtual FileFolder? Parent { get; set; }

        public virtual ICollection<FileFolder> Children { get; set; } = new List<FileFolder>();

        public virtual ICollection<FileDocument> Files { get; set; } = new List<FileDocument>();

        // ===== Propiedades calculadas =====

        /// <summary>
        /// Cantidad de archivos directos en esta carpeta
        /// </summary>
        [NotMapped]
        public int FileCount { get; set; }

        /// <summary>
        /// Cantidad de subcarpetas
        /// </summary>
        [NotMapped]
        public int SubfolderCount { get; set; }

        /// <summary>
        /// Tamaño total en bytes
        /// </summary>
        [NotMapped]
        public long TotalSizeBytes { get; set; }
    }
}
