// ================================================================================
// ARCHIVO: CMS.Entities/Admin/InventoryTransactionStatus.cs
// PROPÓSITO: Entidad del catálogo central de estados de movimiento de inventario
// DESCRIPCIÓN: Representa un registro de admin.inventory_transaction_status en la
//              BD central (cms). Compartido por todas las compañías.
//              El campo Code coincide con los valores que se usaban en
//              sinai.inventory_transaction.status antes de la migración.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-05
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    [Table("inventory_transaction_status", Schema = "admin")]
    public class InventoryTransactionStatus
    {
        [Key]
        [Column("id_inventory_transaction_status")]
        public int Id { get; set; }

        /// <summary>Código interno del estado (Draft, Confirmed, InTransit, etc.). Inmutable.</summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre legible para mostrar en la UI (ej: "En Tránsito")</summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Descripción del estado para tooltips o documentación.</summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        // ===== PRESENTACIÓN VISUAL =====

        /// <summary>Clase de Bootstrap Icon (ej: bi-truck)</summary>
        [MaxLength(50)]
        [Column("icon")]
        public string? Icon { get; set; }

        /// <summary>Emoji corto para uso en tablas y badges (ej: 🚛)</summary>
        [MaxLength(10)]
        [Column("emoji")]
        public string? Emoji { get; set; }

        /// <summary>Clase CSS del badge de estado (ej: st-InTransit)</summary>
        [MaxLength(60)]
        [Column("css_class")]
        public string? CssClass { get; set; }

        // ===== COMPORTAMIENTO DEL FLUJO =====

        /// <summary>True para Completed y Cancelled — estados terminales de los que no se puede volver.</summary>
        [Column("is_final_state")]
        public bool IsFinalState { get; set; } = false;

        /// <summary>True solo para Draft — es el único estado editable.</summary>
        [Column("allows_edit")]
        public bool AllowsEdit { get; set; } = false;

        // ===== ESTADO =====

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
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }
}
