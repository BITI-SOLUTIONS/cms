using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("role_permission", Schema = "admin")]
    public class RolePermission : IAuditableEntity
    {
        [Key, Column("id_role", Order = 0)]
        public int RoleId { get; set; }

        [Key, Column("id_permission", Order = 1)]
        public int PermissionId { get; set; }

        [Column("is_allowed")]
        public bool IsAllowed { get; set; } = true;

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

        public virtual Role Role { get; set; } = default!;
        public virtual Permission Permission { get; set; } = default!;
    }
}