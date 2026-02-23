// ================================================================================
// ARCHIVO: CMS.Entities/PasswordResetRequest.cs
// PROPÓSITO: Entidad para rastrear solicitudes de restablecimiento de contraseña
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ACTUALIZADO: 2026-02-13 - Agregados campos de auditoría
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Rastrea las solicitudes de restablecimiento de contraseña para auditoría y seguridad
    /// </summary>
    [Table("password_reset_request", Schema = "admin")]
    public class PasswordResetRequest : IAuditableEntity
    {
        [Key]
        [Column("id_password_reset_request")]
        public int ID { get; set; }

        [Column("id_user")]
        public int ID_USER { get; set; }

        [Column("token")]
        [Required]
        [MaxLength(255)]
        public string TOKEN { get; set; } = default!;

        [Column("token_hash")]
        [Required]
        [MaxLength(255)]
        public string TOKEN_HASH { get; set; } = default!;

        [Column("expires_at")]
        public DateTime EXPIRES_AT { get; set; }

        [Column("requested_at")]
        public DateTime REQUESTED_AT { get; set; }

        [Column("requested_ip")]
        [MaxLength(50)]
        public string? REQUESTED_IP { get; set; }

        [Column("is_used")]
        public bool IS_USED { get; set; } = false;

        [Column("used_at")]
        public DateTime? USED_AT { get; set; }

        [Column("used_ip")]
        [MaxLength(50)]
        public string? USED_IP { get; set; }

        // ===== AUDITORÍA =====
        [Column("rowpointer")]
        public Guid RowPointer { get; set; }

        [Column("record_date")]
        public DateTime RecordDate { get; set; }

        [Column("createdate")]
        public DateTime CreateDate { get; set; }

        [Column("created_by")]
        [Required]
        [MaxLength(30)]
        public string CreatedBy { get; set; } = default!;

        [Column("updated_by")]
        [Required]
        [MaxLength(30)]
        public string UpdatedBy { get; set; } = default!;

        // ===== NAVEGACIÓN =====
        public virtual User User { get; set; } = default!;
    }
}
