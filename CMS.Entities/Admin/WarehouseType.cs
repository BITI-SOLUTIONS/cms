// ================================================================================
// ARCHIVO: CMS.Entities/Admin/WarehouseType.cs
// PROPÓSITO: Entidad que representa un tipo de bodega (tabla CENTRAL)
// DESCRIPCIÓN: Se almacena en la BD central (cms) en el schema admin.
//              Tabla: admin.warehouse_type
//              Es compartida por TODAS las compañías.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-01
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    [Table("warehouse_type", Schema = "admin")]
    public class WarehouseType
    {
        [Key]
        [Column("id_warehouse_type")]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Column("icon")]
        public string? Icon { get; set; }

        [MaxLength(20)]
        [Column("color")]
        public string? Color { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required]
        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();
    }
}
