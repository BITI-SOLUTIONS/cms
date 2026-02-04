using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("currency", Schema = "admin")]
    public class Currency : IAuditableEntity
    {
        [Key]
        [Column("id_currency")]
        public int ID_CURRENCY { get; set; }

        [Column("currency_code")]
        [Required]
        [MaxLength(3)]
        public string CURRENCY_CODE { get; set; } = default!;

        [Column("currency_name")]
        [Required]
        [MaxLength(100)]
        public string CURRENCY_NAME { get; set; } = default!;

        [Column("currency_symbol")]
        [Required]
        [MaxLength(10)]
        public string CURRENCY_SYMBOL { get; set; } = default!;

        [Column("minor_unit")]
        public short MINOR_UNIT { get; set; } = 2;

        [Column("rounding_increment")]
        public decimal ROUNDING_INCREMENT { get; set; } = 0;

        [Column("is_crypto")]
        public bool IS_CRYPTO { get; set; }

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

        public virtual ICollection<Country> Countries { get; set; } = new List<Country>();
        public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();
    }
}