using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("country", Schema = "admin")]
    public class Country : IAuditableEntity
    {
        [Key]
        [Column("id_country")]
        public int ID_COUNTRY { get; set; }

        [Column("iso2_code")]
        [Required]
        [MaxLength(2)]
        public string ISO2_CODE { get; set; } = default!;

        [Column("iso3_code")]
        [Required]
        [MaxLength(3)]
        public string ISO3_CODE { get; set; } = default!;

        [Column("numeric_code")]
        [Required]
        [MaxLength(3)]
        public string NUMERIC_CODE { get; set; } = default!;

        [Column("name")]
        [Required]
        [MaxLength(150)]
        public string NAME { get; set; } = default!;

        [Column("name_native")]
        [Required]
        [MaxLength(150)]
        public string NAME_NATIVE { get; set; } = default!;

        [Column("official_name")]
        [Required]
        [MaxLength(150)]
        public string OFFICIAL_NAME { get; set; } = default!;

        [Column("continent")]
        [Required]
        [MaxLength(50)]
        public string CONTINENT { get; set; } = default!;

        [Column("region")]
        [Required]
        [MaxLength(100)]
        public string REGION { get; set; } = default!;

        [Column("sub_region")]
        [Required]
        [MaxLength(100)]
        public string SUB_REGION { get; set; } = default!;

        [Column("id_currency")]
        public int ID_CURRENCY { get; set; }

        [Column("phone_prefix")]
        [Required]
        [MaxLength(10)]
        public string PHONE_PREFIX { get; set; } = default!;

        [Column("time_zone")]
        [Required]
        [MaxLength(50)]
        public string TIME_ZONE { get; set; } = default!;

        [Column("date_format")]
        [Required]
        [MaxLength(30)]
        public string DATE_FORMAT { get; set; } = default!;

        [Column("id_language")]
        public int ID_LANGUAGE { get; set; }

        [Column("has_electronic_invoice")]
        public bool HAS_ELECTRONIC_INVOICE { get; set; }

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

        public virtual Language Language { get; set; } = default!;
        public virtual Currency Currency { get; set; } = default!;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}