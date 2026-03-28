// ================================================================================
// ARCHIVO: CMS.Entities/Operational/FileDocument.cs
// PROPÓSITO: Entidad principal para archivos (BD Operacional)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Archivo/Documento (tabla {schema}.file).
    /// Entidad principal del sistema de gestión de archivos.
    /// </summary>
    [Table("file")]
    public class FileDocument
    {
        [Key]
        [Column("id_file")]
        public int IdFile { get; set; }

        // ===== Identificación única =====

        /// <summary>
        /// Código de negocio único (para búsqueda rápida del cliente)
        /// Formato: FILE-000001
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// UUID global único (para compartir entre sistemas)
        /// </summary>
        [Column("uuid")]
        public Guid Uuid { get; set; } = Guid.NewGuid();

        // ===== Información básica =====

        /// <summary>
        /// Nombre del archivo (como se muestra al usuario)
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Título descriptivo (puede ser diferente al nombre)
        /// </summary>
        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Descripción del archivo
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        // ===== Ubicación =====

        /// <summary>
        /// ID de la carpeta donde está el archivo
        /// </summary>
        [Column("id_file_folder")]
        public int IdFileFolder { get; set; }

        /// <summary>
        /// Path completo incluyendo nombre de archivo
        /// </summary>
        [MaxLength(1000)]
        [Column("path")]
        public string? Path { get; set; }

        // ===== Categorización =====

        /// <summary>
        /// Código de categoría (referencia a admin.file_category)
        /// </summary>
        [MaxLength(20)]
        [Column("category_code")]
        public string? CategoryCode { get; set; }

        /// <summary>
        /// Etiquetas para búsqueda
        /// </summary>
        [Column("tags")]
        public string[]? Tags { get; set; }

        // ===== Metadatos del archivo =====

        /// <summary>
        /// Extensión del archivo (ej: .pdf)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("file_extension")]
        public string FileExtension { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME (ej: application/pdf)
        /// </summary>
        [MaxLength(100)]
        [Column("mime_type")]
        public string? MimeType { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        [Column("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Nombre original del archivo cuando fue subido
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("original_filename")]
        public string OriginalFilename { get; set; } = string.Empty;

        // ===== Contenido =====

        /// <summary>
        /// Contenido binario del archivo
        /// </summary>
        [Required]
        [Column("file_content")]
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Hash SHA-256 para detectar duplicados
        /// </summary>
        [MaxLength(64)]
        [Column("file_hash")]
        public string? FileHash { get; set; }

        // ===== Preview/Thumbnail =====

        /// <summary>
        /// Thumbnail del archivo (para imágenes, PDFs)
        /// </summary>
        [Column("thumbnail")]
        public byte[]? Thumbnail { get; set; }

        /// <summary>
        /// Tipo MIME del thumbnail
        /// </summary>
        [MaxLength(50)]
        [Column("thumbnail_mime_type")]
        public string? ThumbnailMimeType { get; set; }

        // ===== Versionamiento =====

        /// <summary>
        /// Versión actual del archivo
        /// </summary>
        [Column("current_version")]
        public int CurrentVersion { get; set; } = 1;

        /// <summary>
        /// Total de versiones del archivo
        /// </summary>
        [Column("version_count")]
        public int VersionCount { get; set; } = 1;

        // ===== Estado de bloqueo (Check-out/Check-in) =====

        /// <summary>
        /// Si el archivo está bloqueado para edición
        /// </summary>
        [Column("is_locked")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// ID del usuario que tiene el bloqueo
        /// </summary>
        [Column("locked_by_user_id")]
        public int? LockedByUserId { get; set; }

        /// <summary>
        /// Nombre del usuario que tiene el bloqueo
        /// </summary>
        [MaxLength(100)]
        [Column("locked_by_user_name")]
        public string? LockedByUserName { get; set; }

        /// <summary>
        /// Fecha/hora del bloqueo
        /// </summary>
        [Column("locked_at")]
        public DateTime? LockedAt { get; set; }

        /// <summary>
        /// Fecha/hora de expiración automática del bloqueo
        /// </summary>
        [Column("lock_expires_at")]
        public DateTime? LockExpiresAt { get; set; }

        /// <summary>
        /// Razón del bloqueo
        /// </summary>
        [MaxLength(255)]
        [Column("lock_reason")]
        public string? LockReason { get; set; }

        // ===== Favoritos y destacados =====

        /// <summary>
        /// Si está marcado como favorito
        /// </summary>
        [Column("is_favorite")]
        public bool IsFavorite { get; set; } = false;

        /// <summary>
        /// Si está fijado/anclado (pinned)
        /// </summary>
        [Column("is_pinned")]
        public bool IsPinned { get; set; } = false;

        // ===== Configuración de acceso =====

        /// <summary>
        /// Si es archivo privado (solo visible para el creador)
        /// </summary>
        [Column("is_private")]
        public bool IsPrivate { get; set; } = false;

        /// <summary>
        /// Si permite descarga
        /// </summary>
        [Column("allow_download")]
        public bool AllowDownload { get; set; } = true;

        /// <summary>
        /// Si permite impresión
        /// </summary>
        [Column("allow_print")]
        public bool AllowPrint { get; set; } = true;

        // ===== Fechas importantes =====

        /// <summary>
        /// Fecha del documento (diferente a created_at)
        /// </summary>
        [Column("document_date")]
        public DateOnly? DocumentDate { get; set; }

        /// <summary>
        /// Fecha de expiración/vigencia del documento
        /// </summary>
        [Column("expiry_date")]
        public DateOnly? ExpiryDate { get; set; }

        // ===== Relación con otras entidades =====

        /// <summary>
        /// Tipo de entidad relacionada: item, customer, vendor, invoice, etc.
        /// </summary>
        [MaxLength(50)]
        [Column("related_entity_type")]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// ID de la entidad relacionada
        /// </summary>
        [Column("related_entity_id")]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Código de la entidad relacionada
        /// </summary>
        [MaxLength(50)]
        [Column("related_entity_code")]
        public string? RelatedEntityCode { get; set; }

        // ===== Estado =====

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de eliminación (soft delete)
        /// </summary>
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        /// <summary>
        /// Razón de eliminación
        /// </summary>
        [MaxLength(255)]
        [Column("delete_reason")]
        public string? DeleteReason { get; set; }

        /// <summary>
        /// Fecha programada para eliminación permanente
        /// </summary>
        [Column("permanent_delete_scheduled_at")]
        public DateTime? PermanentDeleteScheduledAt { get; set; }

        // ===== Estadísticas =====

        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("download_count")]
        public int DownloadCount { get; set; } = 0;

        [Column("last_viewed_at")]
        public DateTime? LastViewedAt { get; set; }

        [Column("last_downloaded_at")]
        public DateTime? LastDownloadedAt { get; set; }

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

        [ForeignKey("IdFileFolder")]
        public virtual FileFolder? Folder { get; set; }

        public virtual ICollection<FileVersion> Versions { get; set; } = new List<FileVersion>();

        public virtual ICollection<FileComment> Comments { get; set; } = new List<FileComment>();

        // ===== Propiedades calculadas =====

        /// <summary>
        /// Tamaño formateado (KB, MB, GB)
        /// </summary>
        [NotMapped]
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

        /// <summary>
        /// Si el documento está expirado
        /// </summary>
        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.Today);

        /// <summary>
        /// Si el bloqueo ha expirado
        /// </summary>
        [NotMapped]
        public bool IsLockExpired => IsLocked && LockExpiresAt.HasValue && LockExpiresAt.Value < DateTime.UtcNow;

        /// <summary>
        /// Icono según la extensión
        /// </summary>
        [NotMapped]
        public string FileIcon => GetFileIcon(FileExtension);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string GetFileIcon(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "bi-file-earmark-pdf",
                ".doc" or ".docx" => "bi-file-earmark-word",
                ".xls" or ".xlsx" => "bi-file-earmark-excel",
                ".ppt" or ".pptx" => "bi-file-earmark-ppt",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "bi-file-earmark-image",
                ".mp4" or ".avi" or ".mov" or ".wmv" => "bi-file-earmark-play",
                ".mp3" or ".wav" or ".ogg" => "bi-file-earmark-music",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "bi-file-earmark-zip",
                ".json" or ".xml" or ".html" or ".css" or ".js" or ".sql" => "bi-file-earmark-code",
                ".txt" => "bi-file-earmark-text",
                _ => "bi-file-earmark"
            };
        }
    }
}
