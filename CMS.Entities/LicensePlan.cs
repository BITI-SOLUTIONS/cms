using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("license_plan", Schema = "admin")]
    public class LicensePlan : IAuditableEntity
    {
        [Key]
        [Column("id_license_plan")]
        public int ID_LICENSE_PLAN { get; set; }

        [Column("license_plan_name")]
        [Required]
        [MaxLength(100)]
        public string LICENSE_PLAN_NAME { get; set; } = default!;

        [Column("license_plan_description")]
        [Required]
        public string LICENSE_PLAN_DESCRIPTION { get; set; } = default!;

        [Column("max_users")]
        public int MAX_USERS { get; set; }

        [Column("max_subsidiaries")]
        public int MAX_SUBSIDIARIES { get; set; }

        [Column("max_storage_gb")]
        public int MAX_STORAGE_GB { get; set; }

        [Column("max_api_calls_per_month")]
        public long MAX_API_CALLS_PER_MONTH { get; set; }

        [Column("monthly_price")]
        public decimal MONTHLY_PRICE { get; set; }

        [Column("annual_price")]
        public decimal ANNUAL_PRICE { get; set; }

        [Column("features")]
        [Required]
        public string FEATURES { get; set; } = default!;

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

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}