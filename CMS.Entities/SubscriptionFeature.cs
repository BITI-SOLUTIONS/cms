using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("subscription_feature", Schema = "admin")]
    public class SubscriptionFeature : IAuditableEntity
    {
        [Key]
        [Column("id_subscription_feature")]
        public int ID_SUBSCRIPTION_FEATURE { get; set; }

        [Column("id_subscription")]
        public int ID_SUBSCRIPTION { get; set; }

        [Column("description")]
        [Required]
        [MaxLength(100)]
        public string DESCRIPTION { get; set; } = default!;

        [Column("is_active")]
        public bool IS_ACTIVE { get; set; } = true;

        [Column("usage_current")]
        public long USAGE_CURRENT { get; set; } = 0;

        [Column("usage_limit")]
        public long USAGE_LIMIT { get; set; } = 1;

        [Column("usage_percentage")]
        public decimal USAGE_PERCENTAGE { get; set; } = 0;

        [Column("notes")]
        public string? NOTES { get; set; }

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

        public virtual Subscription Subscription { get; set; } = default!;
    }
}