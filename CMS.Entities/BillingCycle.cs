using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("billing_cycle", Schema = "admin")]
    public class BillingCycle : IAuditableEntity
    {
        [Key]
        [Column("id_billing_cycle")]
        public int ID_BILLING_CYCLE { get; set; }

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

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}