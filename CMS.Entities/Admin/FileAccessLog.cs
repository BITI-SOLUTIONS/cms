// ================================================================================
// ARCHIVO: CMS.Entities/Admin/FileAccessLog.cs
// PROPÓSITO: Entidad para log de acceso a archivos (BD Central)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Log de acceso a archivos (tabla admin.file_access_log).
    /// Registra todas las acciones realizadas sobre archivos para auditoría.
    /// </summary>
    [Table("file_access_log", Schema = "admin")]
    public class FileAccessLog
    {
        [Key]
        [Column("id_file_access_log")]
        public long IdFileAccessLog { get; set; }

        /// <summary>
        /// ID de la compañía donde está el archivo
        /// </summary>
        [Column("id_company")]
        public int IdCompany { get; set; }

        /// <summary>
        /// ID del archivo
        /// </summary>
        [Column("file_id")]
        public int FileId { get; set; }

        /// <summary>
        /// Código del archivo
        /// </summary>
        [MaxLength(50)]
        [Column("file_code")]
        public string? FileCode { get; set; }

        /// <summary>
        /// ID de la versión específica (si aplica)
        /// </summary>
        [Column("version_id")]
        public int? VersionId { get; set; }

        // ===== Quién accedió =====

        [Column("id_user")]
        public int IdUser { get; set; }

        [MaxLength(100)]
        [Column("user_name")]
        public string? UserName { get; set; }

        [MaxLength(45)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        // ===== Tipo de acción =====

        /// <summary>
        /// Acción realizada: view, download, edit, delete, share, lock, unlock, restore, comment
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Detalles adicionales en formato JSON
        /// </summary>
        [Column("action_details", TypeName = "jsonb")]
        public string? ActionDetails { get; set; }

        // ===== Contexto =====

        /// <summary>
        /// Cómo se accedió: direct, share_link, api
        /// </summary>
        [MaxLength(20)]
        [Column("accessed_via")]
        public string AccessedVia { get; set; } = "direct";

        /// <summary>
        /// ID del share si fue accedido via enlace compartido
        /// </summary>
        [Column("id_file_share")]
        public int IdFileShare { get; set; }

        [Column("accessed_at")]
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

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

        [ForeignKey("IdCompany")]
        public virtual Company? Company { get; set; }

        [ForeignKey("IdUser")]
        public virtual User? User { get; set; }

        [ForeignKey("IdFileShare")]
        public virtual FileShare? Share { get; set; }
    }

    /// <summary>
    /// Tipos de acción para el log
    /// </summary>
    public static class FileAccessActions
    {
        public const string View = "view";
        public const string Download = "download";
        public const string Edit = "edit";
        public const string Delete = "delete";
        public const string Restore = "restore";
        public const string PermanentDelete = "permanent_delete";
        public const string Share = "share";
        public const string Unshare = "unshare";
        public const string Lock = "lock";
        public const string Unlock = "unlock";
        public const string Comment = "comment";
        public const string VersionCreate = "version_create";
        public const string VersionRestore = "version_restore";
        public const string Move = "move";
        public const string Rename = "rename";
        public const string TagAdd = "tag_add";
        public const string TagRemove = "tag_remove";
    }
}
