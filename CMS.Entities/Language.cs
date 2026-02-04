using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("language", Schema = "admin")]
    public class Language : IAuditableEntity
    {
        [Key]
        [Column("id_language")]
        public int ID_LANGUAGE { get; set; }

        [Column("language_code")]
        [Required]
        [MaxLength(15)]
        public string LANGUAGE_CODE { get; set; } = default!;

        [Column("language_name")]
        [Required]
        [MaxLength(200)]
        public string LANGUAGE_NAME { get; set; } = default!;

        [Column("language_name_native")]
        [Required]
        [MaxLength(200)]
        public string LANGUAGE_NAME_NATIVE { get; set; } = default!;

        [Column("is_iso_639_3")]
        public bool IS_ISO_639_3 { get; set; }

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
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}