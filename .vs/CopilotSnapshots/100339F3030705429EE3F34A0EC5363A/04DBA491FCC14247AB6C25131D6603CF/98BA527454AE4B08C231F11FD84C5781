using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("user_role", Schema = "admin")]
    public class UserRole : IAuditableEntity
    {
        [Key, Column("id_user", Order = 0)]
        public int UserId { get; set; }

        [Key, Column("id_role", Order = 1)]
        public int RoleId { get; set; }

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

        public virtual User User { get; set; } = default!;
        public virtual Role Role { get; set; } = default!;
    }
}