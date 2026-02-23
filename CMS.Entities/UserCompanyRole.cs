// ================================================================================
// ARCHIVO: CMS.Entities/UserCompanyRole.cs
// PROPÓSITO: Entidad que relaciona usuarios con roles POR compañía
// DESCRIPCIÓN: Un usuario puede tener diferentes roles en diferentes compañías
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Entidad que representa los roles de un usuario en una compañía específica.
    /// PK compuesta: (ID_USER, ID_COMPANY, ID_ROLE)
    /// 
    /// Permite que un usuario tenga:
    /// - Rol de Admin en Compañía A
    /// - Rol de Usuario en Compañía B
    /// - Múltiples roles en la misma compañía
    /// </summary>
    [Table("user_company_role", Schema = "admin")]
    public class UserCompanyRole : IAuditableEntity
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        [Column("id_user")]
        public int ID_USER { get; set; }

        /// <summary>
        /// ID de la compañía donde aplica este rol
        /// </summary>
        [Column("id_company")]
        public int ID_COMPANY { get; set; }

        /// <summary>
        /// ID del rol asignado
        /// </summary>
        [Column("id_role")]
        public int ID_ROLE { get; set; }

        /// <summary>
        /// Indica si esta asignación de rol está activa
        /// </summary>
        [Column("is_active")]
        public bool IS_ACTIVE { get; set; } = true;

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
        public virtual User? User { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Role? Role { get; set; }
    }
}
