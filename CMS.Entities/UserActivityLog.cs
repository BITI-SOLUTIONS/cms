// ================================================================================
// ARCHIVO: CMS.Entities/UserActivityLog.cs
// PROPÓSITO: Entidad para historial de actividad del usuario
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Historial de actividad del usuario (login, accesos, cambios de configuración)
    /// Tabla: admin.user_activity_log
    /// </summary>
    [Table("user_activity_log", Schema = "admin")]
    public class UserActivityLog : IAuditableEntity
    {
        [Key]
        [Column("id_user_activity_log")]
        public int ID_USER_ACTIVITY_LOG { get; set; }

        [Column("id_user")]
        [Required]
        public int ID_USER { get; set; }

        [Column("id_company")]
        public int? ID_COMPANY { get; set; }

        /// <summary>
        /// Tipo de actividad: LOGIN, LOGOUT, PAGE_ACCESS, PASSWORD_CHANGE, SETTINGS_CHANGE, COMPANY_SWITCH, SUPPORT_REQUEST
        /// </summary>
        [Column("activity_type")]
        [Required]
        [MaxLength(50)]
        public string ACTIVITY_TYPE { get; set; } = default!;

        [Column("activity_description")]
        [Required]
        [MaxLength(500)]
        public string ACTIVITY_DESCRIPTION { get; set; } = default!;

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IP_ADDRESS { get; set; }

        [Column("user_agent")]
        [MaxLength(500)]
        public string? USER_AGENT { get; set; }

        [Column("device_info")]
        [MaxLength(100)]
        public string? DEVICE_INFO { get; set; }

        /// <summary>
        /// Metadatos adicionales en formato JSON
        /// </summary>
        [Column("metadata", TypeName = "jsonb")]
        public string? METADATA { get; set; }

        [Column("is_success")]
        public bool IS_SUCCESS { get; set; } = true;

        [Column("error_message")]
        [MaxLength(500)]
        public string? ERROR_MESSAGE { get; set; }

        [Column("activity_date")]
        public DateTime ACTIVITY_DATE { get; set; } = DateTime.UtcNow;

        // =====================================================
        // Campos de auditoría (IAuditableEntity)
        // =====================================================

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [MaxLength(30)]
        public string? CreatedBy { get; set; } = "SYSTEM";

        [Column("updated_by")]
        [MaxLength(30)]
        public string? UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        // Navigation properties
        [ForeignKey("ID_USER")]
        public virtual User? User { get; set; }

        [ForeignKey("ID_COMPANY")]
        public virtual Company? Company { get; set; }
    }

    /// <summary>
    /// Tipos de actividad predefinidos
    /// </summary>
    public static class ActivityTypes
    {
        public const string LOGIN = "LOGIN";
        public const string LOGOUT = "LOGOUT";
        public const string LOGIN_FAILED = "LOGIN_FAILED";
        public const string PAGE_ACCESS = "PAGE_ACCESS";
        public const string PASSWORD_CHANGE = "PASSWORD_CHANGE";
        public const string SETTINGS_CHANGE = "SETTINGS_CHANGE";
        public const string COMPANY_SWITCH = "COMPANY_SWITCH";
        public const string SUPPORT_REQUEST = "SUPPORT_REQUEST";
        public const string PROFILE_VIEW = "PROFILE_VIEW";
    }
}
