using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("data_translation", Schema = "admin")]
    public class DataTranslation : IAuditableEntity
    {
        [Key]
        [Column("id_data_translation")]
        public int ID_TRANSLATION { get; set; }

        [Column("table_name")]
        [Required]
        [MaxLength(128)]
        public string TABLE_NAME { get; set; } = default!;

        [Column("pk_json")]
        [Required]
        public string PK_JSON { get; set; } = default!;

        [Column("field_name")]
        [Required]
        [MaxLength(128)]
        public string FIELD_NAME { get; set; } = default!;

        [Column("text_value")]
        [Required]
        public string TEXT_VALUE { get; set; } = default!;

        [Column("id_language")]
        public int ID_LANGUAGE { get; set; }

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
    }
}