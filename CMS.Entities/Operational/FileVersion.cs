// ================================================================================
// ARCHIVO: CMS.Entities/Operational/FileVersion.cs
// PROPÓSITO: Entidad para versiones de archivos (BD Operacional)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Versión de archivo (tabla {schema}.file_version).
    /// Almacena el historial de versiones para versionamiento.
    /// </summary>
    [Table("file_version")]
    public class FileVersion
    {
        [Key]
        [Column("id_file_version")]
        public int IdFileVersion { get; set; }

        /// <summary>
        /// ID del archivo al que pertenece esta versión
        /// </summary>
        [Column("id_file")]
        public int IdFile { get; set; }

        /// <summary>
        /// Número de versión (1, 2, 3...)
        /// </summary>
        [Column("version_number")]
        public int VersionNumber { get; set; }

        // ===== Contenido de la versión =====

        /// <summary>
        /// Contenido binario de esta versión
        /// </summary>
        [Required]
        [Column("file_content")]
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Tamaño de esta versión en bytes
        /// </summary>
        [Column("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Hash SHA-256 de esta versión
        /// </summary>
        [MaxLength(64)]
        [Column("file_hash")]
        public string? FileHash { get; set; }

        // ===== Metadatos =====

        /// <summary>
        /// Nombre del archivo en esta versión
        /// </summary>
        [MaxLength(500)]
        [Column("filename")]
        public string? Filename { get; set; }

        /// <summary>
        /// Tipo MIME
        /// </summary>
        [MaxLength(100)]
        [Column("mime_type")]
        public string? MimeType { get; set; }

        // ===== Información del cambio =====

        /// <summary>
        /// Resumen breve del cambio
        /// </summary>
        [MaxLength(500)]
        [Column("change_summary")]
        public string? ChangeSummary { get; set; }

        /// <summary>
        /// Descripción detallada del cambio
        /// </summary>
        [Column("change_details")]
        public string? ChangeDetails { get; set; }

        /// <summary>
        /// Tipo de cambio: create, update, restore, auto_save
        /// </summary>
        [MaxLength(20)]
        [Column("change_type")]
        public string ChangeType { get; set; } = "update";

        // ===== Almacenamiento =====

        /// <summary>
        /// Tamaño del diff (si se usa delta storage)
        /// </summary>
        [Column("diff_size_bytes")]
        public long? DiffSizeBytes { get; set; }

        /// <summary>
        /// Si es copia completa (true) o solo diff (false)
        /// </summary>
        [Column("is_full_copy")]
        public bool IsFullCopy { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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

        [ForeignKey("IdFile")]
        public virtual FileDocument? File { get; set; }

        // ===== Propiedades calculadas =====

        [NotMapped]
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Tipos de cambio de versión
    /// </summary>
    public static class FileChangeTypes
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Restore = "restore";
        public const string AutoSave = "auto_save";
    }
}
