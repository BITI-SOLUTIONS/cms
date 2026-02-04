using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("billing_invoice", Schema = "admin")]
    public class BillingInvoice : IAuditableEntity
    {
        [Key]
        [Column("id_billing_invoice")]
        public int ID_BILLING_INVOICE { get; set; }

        [Column("id_subscription")]
        public int ID_SUBSCRIPTION { get; set; }

        [Column("billing_date")]
        public DateTime BILLING_DATE { get; set; }

        [Column("invoice_date")]
        public DateTime INVOICE_DATE { get; set; }

        [Column("due_date")]
        public DateTime DUE_DATE { get; set; }

        [Column("amount")]
        public decimal AMOUNT { get; set; }

        [Column("tax")]
        public decimal TAX { get; set; } = 0;

        [Column("total")]
        public decimal TOTAL { get; set; }

        [Column("id_currency")]
        public int ID_CURRENCY { get; set; } = 141;

        [Column("id_payment_status")]
        public int ID_PAYMENT_STATUS { get; set; } = 1;

        [Column("payment_date")]
        public DateTime? PAYMENT_DATE { get; set; }

        [Column("id_payment_method")]
        public int ID_PAYMENT_METHOD { get; set; } = 1;

        [Column("id_external_invoice")]
        public string? ID_EXTERNAL_INVOICE { get; set; }

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
        public virtual Currency Currency { get; set; } = default!;
        public virtual PaymentStatus PaymentStatus { get; set; } = default!;
        public virtual PaymentMethod PaymentMethod { get; set; } = default!;
    }
}