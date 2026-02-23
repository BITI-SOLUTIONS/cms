// ================================================================================
// ARCHIVO: CMS.Entities/UserCompanyPermission.cs
// PROPÓSITO: Entidad para permisos directos/denegaciones por usuario-compañía
// DESCRIPCIÓN: Permite otorgar o denegar permisos específicos a un usuario
//              en una compañía, independientemente de sus roles
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Entidad que representa permisos directos o denegaciones para un usuario
    /// en una compañía específica.
    /// 
    /// PK compuesta: (ID_USER, ID_COMPANY, ID_PERMISSION)
    /// 
    /// Casos de uso:
    /// - Usuario tiene rol "Contador" pero se le DENIEGA "Billing.CreditNotes.Edit"
    /// - Usuario tiene rol "Usuario" pero se le OTORGA "Reports.Financial.View"
    /// 
    /// Regla: is_allowed = false SIEMPRE tiene prioridad sobre permisos de roles
    /// </summary>
    [Table("user_company_permission", Schema = "admin")]
    public class UserCompanyPermission : IAuditableEntity
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        [Column("id_user")]
        public int ID_USER { get; set; }

        /// <summary>
        /// ID de la compañía donde aplica este permiso
        /// </summary>
        [Column("id_company")]
        public int ID_COMPANY { get; set; }

        /// <summary>
        /// ID del permiso
        /// </summary>
        [Column("id_permission")]
        public int ID_PERMISSION { get; set; }

        /// <summary>
        /// Indica si el permiso está permitido o denegado.
        /// 
        /// true  = Permiso OTORGADO (adicional a roles)
        /// false = Permiso DENEGADO (anula permisos de roles)
        /// 
        /// IMPORTANTE: false tiene prioridad sobre true de roles
        /// </summary>
        [Column("is_allowed")]
        public bool IS_ALLOWED { get; set; } = true;

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
        public virtual Permission? Permission { get; set; }
    }
}
