// ================================================================================
// ARCHIVO: CMS.Entities/Operational/StockTransfer.cs
// PROPÓSITO: Entidad que representa un traslado de stock entre bodegas
// DESCRIPCIÓN: Encabezado del documento de transferencia interna.
//              Se almacena en la BD de cada compañía, schema {company_code}.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-12
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>Estados posibles de un traslado de stock.</summary>
    public static class StockTransferStatus
    {
        /// <summary>Solicitado, pendiente de aprobación/inicio</summary>
        public const string Pending = "Pending";
        /// <summary>Aprobado e iniciado — mercancía en tránsito</summary>
        public const string InProgress = "InProgress";
        /// <summary>Mercancía recibida en bodega destino</summary>
        public const string Completed = "Completed";
        /// <summary>Traslado cancelado</summary>
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// Encabezado de un traslado de stock entre bodegas de la misma compañía.
    /// </summary>
    public class StockTransfer
    {
        [Key]
        [Column("id_stock_transfer")]
        public int Id { get; set; }

        /// <summary>Número legible del traslado (ej: TRF-2026-00001)</summary>
        [Required]
        [MaxLength(30)]
        [Column("transfer_number")]
        public string TransferNumber { get; set; } = string.Empty;

        /// <summary>Referencia externa: factura, contenedor, orden de compra, etc.</summary>
        [MaxLength(100)]
        [Column("reference")]
        public string? Reference { get; set; }

        /// <summary>Notas generales del traslado</summary>
        [MaxLength(2000)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ===== BODEGAS =====

        /// <summary>FK lógica a {schema}.warehouse — bodega de origen</summary>
        [Column("id_warehouse_origin")]
        public int IdWarehouseOrigin { get; set; }

        /// <summary>FK lógica a {schema}.warehouse — bodega de destino</summary>
        [Column("id_warehouse_dest")]
        public int IdWarehouseDest { get; set; }

        // ===== ESTADO =====

        /// <summary>Estado: Pending | InProgress | Completed | Cancelled</summary>
        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = StockTransferStatus.Pending;

        // ===== FECHAS =====

        [Column("transfer_date")]
        public DateOnly TransferDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Column("expected_date")]
        public DateOnly? ExpectedDate { get; set; }

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [Column("cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        // ===== RESPONSABLES (FK lógica cross-DB a cms.admin.user) =====

        [Column("requested_by")]
        public int RequestedBy { get; set; }

        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        [Column("executed_by")]
        public int? ExecutedBy { get; set; }

        [MaxLength(500)]
        [Column("cancel_reason")]
        public string? CancelReason { get; set; }

        // ===== AUDITORÍA =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        // ===== NAVEGACIÓN (no mapeada a BD) =====

        [NotMapped]
        public List<StockTransferLine> Lines { get; set; } = [];

        [NotMapped]
        public string? OriginWarehouseName { get; set; }

        [NotMapped]
        public string? DestWarehouseName { get; set; }

        [NotMapped]
        public string? RequestedByName { get; set; }

        [NotMapped]
        public string? ApprovedByName { get; set; }

        [NotMapped]
        public string? ExecutedByName { get; set; }
    }
}
