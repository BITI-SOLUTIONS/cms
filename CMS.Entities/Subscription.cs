using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("subscription", Schema = "admin")]
    public class Subscription : IAuditableEntity
    {
        [Key]
        [Column("id_subscription")]
        public int ID_SUBSCRIPTION { get; set; }

        [Column("id_company")]
        public int ID_COMPANY { get; set; }

        [Column("id_license_plan")]
        public int ID_LICENSE_PLAN { get; set; }

        [Column("id_subscription_status")]
        public int ID_SUBSCRIPTION_STATUS { get; set; } = 3;

        [Column("id_billing_cycle")]
        public int ID_BILLING_CYCLE { get; set; }

        [Column("current_period_start")]
        public DateTime CURRENT_PERIOD_START { get; set; }

        [Column("current_period_end")]
        public DateTime CURRENT_PERIOD_END { get; set; }

        [Column("next_billing_date")]
        public DateTime? NEXT_BILLING_DATE { get; set; }

        [Column("id_payment_method")]
        public int ID_PAYMENT_METHOD { get; set; }

        [Column("total_amount_paid_usd")]
        public decimal TOTAL_AMOUNT_PAID_USD { get; set; } = 0;

        [Column("auto_renew")]
        public bool AUTO_RENEW { get; set; } = true;

        [Column("cancel_at_period_end")]
        public bool CANCEL_AT_PERIOD_END { get; set; } = false;

        [Column("cancelled_at")]
        public DateTime? CANCELLED_AT { get; set; }

        [Column("cancel_reason")]
        [MaxLength(255)]
        public string? CANCEL_REASON { get; set; }

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

        public virtual Company Company { get; set; } = default!;
        public virtual LicensePlan LicensePlan { get; set; } = default!;
        public virtual SubscriptionStatus SubscriptionStatus { get; set; } = default!;
        public virtual BillingCycle BillingCycle { get; set; } = default!;
        public virtual PaymentMethod PaymentMethod { get; set; } = default!;
        public virtual ICollection<SubscriptionFeature> SubscriptionFeatures { get; set; } = new List<SubscriptionFeature>();
        public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();
    }
}