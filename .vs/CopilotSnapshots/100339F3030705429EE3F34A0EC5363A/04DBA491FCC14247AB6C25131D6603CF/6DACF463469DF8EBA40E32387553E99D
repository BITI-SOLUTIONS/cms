using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    /// <summary>
    /// Entidad que representa permisos asignados directamente a usuarios.
    /// 
    /// Esta tabla permite asignar permisos específicos a usuarios de forma individual,
    /// independientemente de los permisos que hereden de sus roles.
    /// 
    /// Relación: Un usuario puede tener múltiples permisos directos.
    /// Clave primaria compuesta: (ID_USER, ID_PERMISSION)
    /// 
    /// Campos de auditoría automática:
    /// - RecordDate: Se actualiza automáticamente en UPDATE
    /// - CreateDate: Se establece automáticamente al INSERT
    /// - CreatedBy: Se establece automáticamente al INSERT
    /// - UpdatedBy: Se actualiza automáticamente en UPDATE
    /// - RowPointer: Identificador único GUID para integridad referencial
    /// </summary>
    [Table("user_permission", Schema = "admin")]
    public class UserPermission
    {
        /// <summary>
        /// ID del usuario al que se le asigna el permiso.
        /// Referencia a [ADMIN].[USER].[ID_USER]
        /// Parte de la clave primaria compuesta.
        /// </summary>
        [Key, Column("id_user", Order = 0)]
        public int UserId { get; set; }

        /// <summary>
        /// ID del permiso que se asigna al usuario.
        /// Referencia a [ADMIN].[PERMISSION].[ID_PERMISSION]
        /// Parte de la clave primaria compuesta.
        /// </summary>
        [Key, Column("id_permission", Order = 1)]
        public int PermissionId { get; set; }

        /// <summary>
        /// Indica si el permiso está permitido (true) o denegado (false).
        /// 
        /// Default: true (permiso permitido por defecto)
        /// 
        /// Valores:
        /// - true:  El usuario tiene permitido este permiso
        /// - false: El usuario tiene denegado este permiso (explícitamente)
        /// 
        /// Nota: Cuando IS_ALLOWED = false, se considera una denegación explícita
        /// del permiso, lo que puede anular permisos heredados de roles.
        /// </summary>
        [Column("is_allowed")]
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Fecha de la última modificación del registro.
        /// Se actualiza automáticamente en SQL mediante GETDATE() en UPDATE.
        /// Se establece automáticamente en INSERT mediante GETDATE().
        /// 
        /// Nota: Esta columna es importante para auditoría y tracking de cambios.
        /// </summary>
        [Column("record_date")]
        public DateTime RecordDate { get; set; }

        /// <summary>
        /// Identificador único del registro en formato GUID (Globally Unique Identifier).
        /// Generado automáticamente por SQL mediante NEWID().
        /// 
        /// Propósito:
        /// - Garantizar unicidad a nivel de base de datos
        /// - Facilitar sincronización y replicación de datos
        /// - Proporcionar referencia única para auditoría
        /// - Prevenir conflictos en sistemas distribuidos
        /// 
        /// Tiene una restricción UNIQUE NONCLUSTERED en la BD.
        /// </summary>
        [Column("rowpointer")]
        public Guid RowPointer { get; set; }

        /// <summary>
        /// Nombre del usuario que creó el registro.
        /// Se establece automáticamente por SQL mediante SUSER_SNAME() en INSERT.
        /// 
        /// Formato: Típicamente en formato "DOMAIN\USERNAME" o "usuario@dominio"
        /// Máximo: 30 caracteres
        /// 
        /// Propósito: Auditoría - Rastrear quién creó el registro.
        /// </summary>
        [Column("created_by")]
        [Required]
        [MaxLength(30)]
        public string CreatedBy { get; set; } = default!;

        /// <summary>
        /// Nombre del usuario que actualizó por última vez el registro.
        /// Se actualiza automáticamente por SQL mediante SUSER_SNAME() en UPDATE.
        /// 
        /// Formato: Típicamente en formato "DOMAIN\USERNAME" o "usuario@dominio"
        /// Máximo: 30 caracteres
        /// 
        /// Propósito: Auditoría - Rastrear quién modificó el registro.
        /// </summary>
        [Column("updated_by")]
        [Required]
        [MaxLength(30)]
        public string UpdatedBy { get; set; } = default!;

        /// <summary>
        /// Fecha y hora de creación del registro.
        /// Se establece automáticamente por SQL mediante GETDATE() en INSERT.
        /// 
        /// Propósito: Auditoría - Registrar cuándo se creó el permiso para este usuario.
        /// </summary>
        [Column("createdate")] [Required]
        public DateTime CreateDate { get; set; }

        // ===============================================================
        // PROPIEDADES DE NAVEGACIÓN (Lazy Loading)
        // ===============================================================

        /// <summary>
        /// Referencia de navegación al usuario propietario de este permiso.
        /// 
        /// Relación: Muchos-a-Uno (Many-to-One)
        /// - Muchos UserPermission pueden pertenecer a un Usuario
        /// - Cada UserPermission pertenece a exactamente un Usuario
        /// 
        /// Carga: Se carga bajo demanda (Lazy Loading) cuando se accede.
        /// Nullable: true (puede ser null si se carga sin incluir la relación)
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Referencia de navegación al permiso asignado.
        /// 
        /// Relación: Muchos-a-Uno (Many-to-One)
        /// - Muchos UserPermission pueden referenciar el mismo Permiso
        /// - Cada UserPermission referencia exactamente un Permiso
        /// 
        /// Carga: Se carga bajo demanda (Lazy Loading) cuando se accede.
        /// Nullable: true (puede ser null si se carga sin incluir la relación)
        /// </summary>
        public Permission? Permission { get; set; }
    }
}