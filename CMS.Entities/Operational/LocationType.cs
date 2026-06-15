// ================================================================================
// ARCHIVO: CMS.Entities/Operational/LocationType.cs
// PROPÓSITO: Entidad que define los tipos de localización del sistema
// DESCRIPCIÓN: Tabla de catálogo CENTRAL que clasifica las localizaciones por su uso:
//              Bodega, Empleado, Cliente, Proveedor, etc.
//              Se almacena en la BD CENTRAL (cms, schema admin): admin.location_type
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Catálogo de tipos de localización.
    /// Permite clasificar una Location según el uso: Bodega, Empleado, Cliente, etc.
    /// </summary>
    [Table("location_type", Schema = "admin")]
    public class LocationType
    {
        [Key]
        [Column("id_location_type")]
        public int Id { get; set; }

        /// <summary>Código único del tipo (ej: WAREHOUSE, EMPLOYEE, CUSTOMER)</summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre descriptivo del tipo</summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Descripción del tipo de localización</summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>Ícono Bootstrap Icons (ej: bi-building, bi-person, bi-truck)</summary>
        [MaxLength(60)]
        [Column("icon")]
        public string? Icon { get; set; }

        /// <summary>Color hexadecimal para representación visual (ej: #6366f1)</summary>
        [MaxLength(20)]
        [Column("color")]
        public string? Color { get; set; }

        /// <summary>Orden de visualización en listas</summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ── Auditoría ──
        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        // Nota: la colección de Location no se expone como navegación EF
        // porque Location reside en la BD de compañía (cross-DB).
        // La relación es lógica y se resuelve a nivel de aplicación.
    }
}
