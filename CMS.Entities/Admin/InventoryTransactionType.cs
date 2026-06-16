// ================================================================================
// ARCHIVO: CMS.Entities/Admin/InventoryTransactionType.cs
// PROPÓSITO: Entidad que representa un tipo de movimiento de inventario (tabla CENTRAL)
// DESCRIPCIÓN: Se almacena en la BD central (cms) en el schema admin.
//              Tabla: admin.inventory_transaction_type
//              Es compartida por TODAS las compañías.
//              El campo Code corresponde a los valores que se guardan en
//              sinai.inventory_transaction.movement_type.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-02
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    [Table("inventory_transaction_type", Schema = "admin")]
    public class InventoryTransactionType
    {
        [Key]
        [Column("id_inventory_transaction_type")]
        public int Id { get; set; }

        /// <summary>
        /// Código interno que coincide con los valores almacenados en
        /// sinai.inventory_transaction.movement_type
        /// (ej: Transfer, TransitTransfer, PurchaseReceipt, …)
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre descriptivo para mostrar en la UI</summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Descripción del comportamiento del tipo de movimiento</summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>Icono Bootstrap Icons (ej: bi-truck, bi-box-arrow-in-down)</summary>
        [MaxLength(50)]
        [Column("icon")]
        public string? Icon { get; set; }

        /// <summary>Emoji o símbolo visual corto (ej: 🚛, 📦)</summary>
        [MaxLength(10)]
        [Column("emoji")]
        public string? Emoji { get; set; }

        /// <summary>Clase CSS para el badge de tipo en la lista de movimientos</summary>
        [MaxLength(60)]
        [Column("css_class")]
        public string? CssClass { get; set; }

        /// <summary>
        /// Indica si este tipo genera un traslado vía bodega de tránsito (vehículo).
        /// Equivale a la lógica is_transit_transfer del encabezado.
        /// </summary>
        [Column("is_transit_transfer")]
        public bool IsTransitTransfer { get; set; } = false;

        /// <summary>Orden de aparición en los selectores</summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ===== AUDITORÍA =====

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
