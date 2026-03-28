// ================================================================================
// ARCHIVO: CMS.Entities/Operational/FileTag.cs
// PROPÓSITO: Entidad para etiquetas personalizadas de archivos (BD Operacional)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Etiqueta personalizada (tabla {schema}.file_tag).
    /// </summary>
    [Table("file_tag")]
    public class FileTag
    {
        [Key]
        [Column("id_file_tag")]
        public int IdFileTag { get; set; }

        /// <summary>
        /// Nombre de la etiqueta
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color hexadecimal
        /// </summary>
        [MaxLength(7)]
        [Column("color")]
        public string Color { get; set; } = "#6366f1";

        /// <summary>
        /// Descripción de la etiqueta
        /// </summary>
        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Contador de uso
        /// </summary>
        [Column("usage_count")]
        public int UsageCount { get; set; } = 0;

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

    /// <summary>
    /// Relación archivo-etiqueta (tabla {schema}.file_file_tag).
    /// </summary>
    [Table("file_file_tag")]
    public class FileFileTag
    {
        [Column("id_file")]
        public int IdFile { get; set; }

        [Column("id_file_tag")]
        public int IdFileTag { get; set; }

        [Column("added_at")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("added_by")]
        public string? AddedBy { get; set; }

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

        // ===== Navegación =====

        [ForeignKey("IdFile")]
        public virtual FileDocument? File { get; set; }

        [ForeignKey("IdFileTag")]
        public virtual FileTag? Tag { get; set; }
    }
}
