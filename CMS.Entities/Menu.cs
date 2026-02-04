using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("menu", Schema = "admin")]
    public class Menu : IAuditableEntity
    {
        [Key]
        [Column("id_menu")]
        public int ID_MENU { get; set; }

        [Column("id_parent")]
        public int ID_PARENT { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string NAME { get; set; } = default!;

        [Column("url")]
        [Required]
        [MaxLength(200)]
        public string URL { get; set; } = default!;

        [Column("icon")]
        [MaxLength(50)]
        public string? ICON { get; set; }

        [Column("menu_order")]
        public int ORDER { get; set; }

        [Column("permission_key")]
        [MaxLength(150)]
        public string? PERMISSION_KEY { get; set; }

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

        [NotMapped]
        public List<Menu> Children { get; set; } = new();
    }
}