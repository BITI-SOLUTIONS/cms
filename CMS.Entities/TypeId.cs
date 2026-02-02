using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("type_id", Schema = "admin")]
    public class TypeId : IAuditableEntity
    {
        [Key]
        [Column("id_type_id")]
        public int ID_TYPE_ID { get; set; }

        [Column("description")]
        [Required]
        [MaxLength(120)]
        public string DESCRIPTION { get; set; } = default!;

        [Column("is_tax_id")]
        public bool IS_TAX_ID { get; set; }

        [Column("is_legal_registration")]
        public bool IS_LEGAL_REGISTRATION { get; set; }

        [Column("number_characters")]
        public int NUMBER_CHARACTERS { get; set; }

        [Column("allow_letters")]
        public bool ALLOW_LETTERS { get; set; }

        [Column("format_validation")]
        [Required]
        public string FORMAT_VALIDATION { get; set; } = default!;

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

        public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}