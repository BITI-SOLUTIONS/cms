// ================================================================================
// ARCHIVO: CMS.Entities/Admin/FileShare.cs
// PROPÓSITO: Entidad para compartir archivos entre usuarios/compañías (BD Central)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Compartir archivos entre usuarios (tabla admin.file_share).
    /// Permite compartir archivos de una compañía con usuarios de la misma o diferentes compañías.
    /// </summary>
    [Table("file_share", Schema = "admin")]
    public class FileShare
    {
        [Key]
        [Column("id_file_share")]
        public int IdFileShare { get; set; }

        // ===== Referencia al archivo =====

        /// <summary>
        /// ID de la compañía donde está el archivo
        /// </summary>
        [Column("source_company_id")]
        public int SourceCompanyId { get; set; }

        /// <summary>
        /// ID del archivo en la BD de la compañía
        /// </summary>
        [Column("file_id")]
        public int FileId { get; set; }

        /// <summary>
        /// Código de negocio del archivo para referencia rápida
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("file_code")]
        public string FileCode { get; set; } = string.Empty;

        // ===== Compartido con =====

        /// <summary>
        /// Usuario con quien se comparte
        /// </summary>
        [Column("shared_with_user_id")]
        public int SharedWithUserId { get; set; }

        /// <summary>
        /// Compañía con quien se comparte
        /// </summary>
        [Column("shared_with_company_id")]
        public int SharedWithCompanyId { get; set; }

        // ===== Permisos =====

        [Column("can_view")]
        public bool CanView { get; set; } = true;

        [Column("can_download")]
        public bool CanDownload { get; set; } = true;

        [Column("can_edit")]
        public bool CanEdit { get; set; } = false;

        [Column("can_delete")]
        public bool CanDelete { get; set; } = false;

        [Column("can_share")]
        public bool CanShare { get; set; } = false;

        // ===== Configuración =====

        /// <summary>
        /// Tipo de compartir: 'user', 'company', 'public_link'
        /// </summary>
        [MaxLength(20)]
        [Column("share_type")]
        public string ShareType { get; set; } = "user";

        /// <summary>
        /// Token único para enlaces públicos
        /// </summary>
        [MaxLength(100)]
        [Column("public_link_token")]
        public string? PublicLinkToken { get; set; }

        /// <summary>
        /// Fecha de expiración del share
        /// </summary>
        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Hash de la contraseña (opcional para enlaces protegidos)
        /// </summary>
        [MaxLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Notificar al propietario cuando se accede
        /// </summary>
        [Column("notify_on_access")]
        public bool NotifyOnAccess { get; set; } = false;

        // ===== Auditoría de share =====

        /// <summary>
        /// Usuario que compartió el archivo
        /// </summary>
        [Column("shared_by_user_id")]
        public int SharedByUserId { get; set; }

        [Column("shared_at")]
        public DateTime SharedAt { get; set; } = DateTime.UtcNow;

        [Column("last_accessed_at")]
        public DateTime? LastAccessedAt { get; set; }

        [Column("access_count")]
        public int AccessCount { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [Column("revoked_by")]
        public int RevokedBy { get; set; }

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

        [ForeignKey("SourceCompanyId")]
        public virtual Company? SourceCompany { get; set; }

        [ForeignKey("SharedWithUserId")]
        public virtual User? SharedWithUser { get; set; }

        [ForeignKey("SharedWithCompanyId")]
        public virtual Company? SharedWithCompany { get; set; }

        [ForeignKey("SharedByUserId")]
        public virtual User? SharedByUser { get; set; }
    }
}
