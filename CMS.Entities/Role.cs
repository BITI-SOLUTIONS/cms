using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("role", Schema = "admin")]
    public class Role : IAuditableEntity
    {
        [Key]
        [Column("id_role")]
        public int ID_ROLE { get; set; }

        [Column("role_name")]
        [Required]
        [MaxLength(100)]
        public string ROLE_NAME { get; set; } = default!;

        [Column("description")]
        [Required]
        [MaxLength(255)]
        public string DESCRIPTION { get; set; } = default!;

        [Column("is_system")]
        public bool IS_SYSTEM { get; set; }

        [Column("is_active")]
        public bool IS_ACTIVE { get; set; } = true;

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

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
