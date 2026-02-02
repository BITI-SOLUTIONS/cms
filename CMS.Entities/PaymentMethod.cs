using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("payment_method", Schema = "admin")]
    public class PaymentMethod : IAuditableEntity
    {
        [Key]
        [Column("id_payment_method")]
        public int ID_PAYMENT_METHOD { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string NAME { get; set; } = default!;

        [Column("description")]
        [Required]
        public string DESCRIPTION { get; set; } = default!;

        [Column("id_payment_category")]
        public int ID_PAYMENT_CATEGORY { get; set; }

        [Column("id_payment_provider")]
        public int? ID_PAYMENT_PROVIDER { get; set; }

        [Column("is_online")]
        public bool IS_ONLINE { get; set; }

        [Column("requires_reference")]
        public bool REQUIRES_REFERENCE { get; set; }

        [Column("requires_confirmation")]
        public bool REQUIRES_CONFIRMATION { get; set; }

        [Column("allows_partial_payments")]
        public bool ALLOWS_PARTIAL_PAYMENTS { get; set; }

        [Column("allows_refunds")]
        public bool ALLOWS_REFUNDS { get; set; }

        [Column("config")]
        [Required]
        public string CONFIG { get; set; } = default!;

        [Column("sort_order")]
        public int SORT_ORDER { get; set; }

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

        public virtual PaymentCategory PaymentCategory { get; set; } = default!;
        public virtual PaymentProvider? PaymentProvider { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();
    }
}