using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("payment_status", Schema = "admin")]
    public class PaymentStatus : IAuditableEntity
    {
        [Key]
        [Column("id_payment_status")]
        public int ID_PAYMENT_STATUS { get; set; }

        [Column("description")]
        [Required]
        [MaxLength(100)]
        public string DESCRIPTION { get; set; } = default!;

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

        public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();
    }
}