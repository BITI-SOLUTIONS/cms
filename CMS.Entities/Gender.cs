using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("gender", Schema = "admin")]
    public class Gender : IAuditableEntity
    {
        [Key]
        [Column("id_gender")]
        public int ID_GENDER { get; set; }

        [Column("gender_code")]
        [Required]
        [MaxLength(10)]
        public string GENDER_CODE { get; set; } = default!;

        [Column("description")]
        [Required]
        [MaxLength(50)]
        public string DESCRIPTION { get; set; } = default!;

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
    }
}