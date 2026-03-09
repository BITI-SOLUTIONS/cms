// ================================================================================
// ARCHIVO: CMS.Entities/UserSettings.cs
// PROPÓSITO: Entidad para configuración/preferencias personales del usuario
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Configuración/preferencias personales del usuario
    /// Tabla: admin.user_settings
    /// </summary>
    [Table("user_settings", Schema = "admin")]
    public class UserSettings : IAuditableEntity
    {
        [Key]
        [Column("id_user_settings")]
        public int ID_USER_SETTINGS { get; set; }

        [Column("id_user")]
        [Required]
        public int ID_USER { get; set; }

        // =====================================================
        // Apariencia
        // =====================================================

        /// <summary>
        /// Tema de la interfaz: dark, light
        /// </summary>
        [Column("theme")]
        [Required]
        [MaxLength(20)]
        public string THEME { get; set; } = "dark";

        /// <summary>
        /// Contraer menú lateral automáticamente
        /// </summary>
        [Column("sidebar_compact")]
        public bool SIDEBAR_COMPACT { get; set; } = true;

        // =====================================================
        // Notificaciones
        // =====================================================

        [Column("notify_email")]
        public bool NOTIFY_EMAIL { get; set; } = true;

        [Column("notify_browser")]
        public bool NOTIFY_BROWSER { get; set; } = true;

        [Column("notify_sound")]
        public bool NOTIFY_SOUND { get; set; } = false;

        // =====================================================
        // Regional
        // =====================================================

        /// <summary>
        /// Idioma: es, en
        /// </summary>
        [Column("language")]
        [Required]
        [MaxLength(10)]
        public string LANGUAGE { get; set; } = "es";

        /// <summary>
        /// Zona horaria: America/Costa_Rica, etc.
        /// </summary>
        [Column("timezone")]
        [Required]
        [MaxLength(50)]
        public string TIMEZONE { get; set; } = "America/Costa_Rica";

        /// <summary>
        /// Formato de fecha: dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd
        /// </summary>
        [Column("date_format")]
        [Required]
        [MaxLength(20)]
        public string DATE_FORMAT { get; set; } = "dd/MM/yyyy";

        /// <summary>
        /// Formato de hora: 24h, 12h
        /// </summary>
        [Column("time_format")]
        [Required]
        [MaxLength(10)]
        public string TIME_FORMAT { get; set; } = "24h";

        // =====================================================
        // Auditoría (IAuditableEntity)
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
    }
}
