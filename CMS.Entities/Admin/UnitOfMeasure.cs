// ================================================================================
// ARCHIVO: CMS.Entities/Admin/UnitOfMeasure.cs
// PROPÓSITO: Entidad que representa una unidad de medida (tabla CENTRAL)
// DESCRIPCIÓN: Esta entidad se almacena en la BD central (cms) en el schema admin
//              Tabla: admin.unit_of_measure
//              Es compartida por TODAS las compañías
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-02
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Representa una unidad de medida para los artículos.
    /// Se almacena en la base de datos CENTRAL (cms) en el schema admin.
    /// Es compartida por todas las compañías del sistema.
    /// </summary>
    [Table("unit_of_measure", Schema = "admin")]
    public class UnitOfMeasure
    {
        [Key]
        [Column("id_unit_of_measure")]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la unidad de medida (ej: "kg", "unidad", "litro")
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo de la unidad de medida (ej: "Kilogramo", "Unidad", "Litro")
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la unidad de medida
        /// </summary>
        [MaxLength(200)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Símbolo de la unidad de medida (ej: "kg", "u", "L")
        /// </summary>
        [MaxLength(10)]
        [Column("symbol")]
        public string? Symbol { get; set; }

        /// <summary>
        /// Indica si esta unidad permite decimales
        /// </summary>
        [Column("allows_decimals")]
        public bool AllowsDecimals { get; set; } = true;

        /// <summary>
        /// Indica si es la unidad por defecto
        /// </summary>
        [Column("is_default")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Indica si está activa
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Orden de visualización
        /// </summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; }

        // ===== AUDITORÍA =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required]
        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();
    }
}
