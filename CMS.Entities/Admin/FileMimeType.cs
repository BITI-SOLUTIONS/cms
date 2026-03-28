// ================================================================================
// ARCHIVO: CMS.Entities/Admin/FileMimeType.cs
// PROPÓSITO: Entidad para tipos MIME permitidos (BD Central)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Tipos MIME permitidos (tabla admin.file_mime_type).
    /// Define qué tipos de archivos se pueden subir y su configuración.
    /// </summary>
    [Table("file_mime_type", Schema = "admin")]
    public class FileMimeType
    {
        [Key]
        [Column("id_file_mime_type")]
        public int IdFileMimeType { get; set; }

        /// <summary>
        /// Extensión del archivo (ej: .pdf, .docx)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("extension")]
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME (ej: application/pdf)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("mime_type")]
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Descripción legible
        /// </summary>
        [MaxLength(100)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Icono Bootstrap Icons
        /// </summary>
        [MaxLength(50)]
        [Column("icon")]
        public string Icon { get; set; } = "bi-file";

        /// <summary>
        /// Categoría del tipo: document, image, video, audio, archive, code, other
        /// </summary>
        [MaxLength(30)]
        [Column("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Tamaño máximo permitido en MB
        /// </summary>
        [Column("max_size_mb")]
        public int MaxSizeMb { get; set; } = 50;

        /// <summary>
        /// Si este tipo de archivo está permitido
        /// </summary>
        [Column("is_allowed")]
        public bool IsAllowed { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ===== Auditoría estándar =====

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
    }
}
