using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("page", Schema = "admin")]
    public class Page : IAuditableEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("page_url")]
        [Required]
        [MaxLength(200)]
        public string PageUrl { get; set; } = default!;

        [Column("title")]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Column("icon")]
        [MaxLength(100)]
        public string? Icon { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

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
    }
}