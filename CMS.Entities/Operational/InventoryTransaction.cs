// ================================================================================
// ARCHIVO: CMS.Entities/Operational/InventoryTransaction.cs
// PROPÓSITO: Entidad que representa un movimiento de inventario (entrada, salida,
//            traslado, ajuste, recepción de ruta de distribución, etc.)
// DESCRIPCIÓN: Centraliza TODOS los movimientos de inventario de la compañía.
//              Se almacena en la BD de cada compañía, schema {company_code}.
//              Soporta el flujo de "bodega a bodega con tránsito":
//               - Origen puede ser cualquier bodega
//               - Si el destino es una bodega tipo Transit (vehículo), las líneas
//                 pueden tener bodegas destino individuales diferentes
//               - El responsable de cada bodega destino confirma la recepción
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-13
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    // ============================================================
    // TIPOS DE MOVIMIENTO
    // ============================================================

    /// <summary>Tipo de movimiento de inventario.</summary>
    public static class InventoryMovementType
    {
        /// <summary>Traslado entre bodegas (estándar)</summary>
        public const string Transfer = "Transfer";
        /// <summary>Traslado vía bodega de tránsito (vehículo)</summary>
        public const string TransitTransfer = "TransitTransfer";
        /// <summary>Entrada por compra / recepción</summary>
        public const string PurchaseReceipt = "PurchaseReceipt";
        /// <summary>Salida por venta / despacho</summary>
        public const string SaleIssue = "SaleIssue";
        /// <summary>Ajuste positivo de inventario</summary>
        public const string AdjustmentIn = "AdjustmentIn";
        /// <summary>Ajuste negativo de inventario</summary>
        public const string AdjustmentOut = "AdjustmentOut";
        /// <summary>Devolución de cliente</summary>
        public const string CustomerReturn = "CustomerReturn";
        /// <summary>Devolución a proveedor</summary>
        public const string SupplierReturn = "SupplierReturn";
        /// <summary>Traspaso a merma / dañados</summary>
        public const string WriteOff = "WriteOff";
        /// <summary>Inventario físico / conteo</summary>
        public const string PhysicalCount = "PhysicalCount";
    }

    /// <summary>Estados del documento de movimiento.</summary>
    public static class InventoryTransactionStatus
    {
        /// <summary>Borrador / en creación</summary>
        public const string Draft = "Draft";
        /// <summary>Confirmado — mercancía despachada desde origen</summary>
        public const string Confirmed = "Confirmed";
        /// <summary>En tránsito (aplica para TransitTransfer)</summary>
        public const string InTransit = "InTransit";
        /// <summary>Parcialmente recibido (algunas líneas confirmadas)</summary>
        public const string PartiallyReceived = "PartiallyReceived";
        /// <summary>Completado — toda la mercancía recibida</summary>
        public const string Completed = "Completed";
        /// <summary>Cancelado</summary>
        public const string Cancelled = "Cancelled";
    }

    // ============================================================
    // ENCABEZADO DE MOVIMIENTO
    // ============================================================

    /// <summary>
    /// Encabezado de un movimiento de inventario.
    /// Registra TODOS los movimientos: traslados simples, vía tránsito, ajustes, entradas, salidas.
    /// </summary>
    [Table("inventory_transaction")]
    public class InventoryTransaction
    {
        [Key]
        [Column("id_inventory_transaction")]
        public int Id { get; set; }

        /// <summary>Número legible del movimiento (ej: INV-2026-00001)</summary>
        [Required]
        [MaxLength(30)]
        [Column("transaction_number")]
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>Tipo de movimiento: Transfer, TransitTransfer, PurchaseReceipt, etc.</summary>
        [Required]
        [MaxLength(30)]
        [Column("movement_type")]
        public string MovementType { get; set; } = InventoryMovementType.Transfer;

        /// <summary>Estado: Draft, Confirmed, InTransit, PartiallyReceived, Completed, Cancelled</summary>
        [Required]
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = InventoryTransactionStatus.Draft;

        // ===== BODEGAS =====

        /// <summary>FK lógica a {schema}.warehouse — bodega de origen</summary>
        [Column("id_warehouse_origin")]
        public int IdWarehouseOrigin { get; set; }

        /// <summary>
        /// FK lógica a {schema}.warehouse — bodega de destino principal.
        /// En movimientos TransitTransfer, esta es la bodega de tránsito (vehículo).
        /// Las bodegas destino finales se especifican en cada línea.
        /// </summary>
        [Column("id_warehouse_dest")]
        public int? IdWarehouseDest { get; set; }

        // ===== REFERENCIAS =====



        /// <summary>Referencia externa: factura, OC, guía de remisión, etc.</summary>
        [MaxLength(100)]
        [Column("reference")]
        public string? Reference { get; set; }

        /// <summary>Notas generales</summary>
        [MaxLength(2000)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ===== FECHAS =====

        [Column("transaction_date")]
        public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Column("expected_arrival_date")]
        public DateOnly? ExpectedArrivalDate { get; set; }

        [Column("confirmed_date")]
        public DateTime? ConfirmedDate { get; set; }

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [Column("cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        // ===== RESPONSABLES (FK lógica cross-DB a cms.admin.user) =====

        [Column("created_by_user_id")]
        public int CreatedByUserId { get; set; }

        [Column("confirmed_by_user_id")]
        public int? ConfirmedByUserId { get; set; }

        [Column("cancelled_by_user_id")]
        public int? CancelledByUserId { get; set; }

        [MaxLength(500)]
        [Column("cancel_reason")]
        public string? CancelReason { get; set; }

        // ===== FLAGS =====

        /// <summary>True si es un traslado con bodega de tránsito (vehículo)</summary>
        [Column("is_transit_transfer")]
        public bool IsTransitTransfer { get; set; } = false;

        /// <summary>True si el movimiento ya afectó el saldo de existencias</summary>
        [Column("affects_stock")]
        public bool AffectsStock { get; set; } = false;

        // ===== SELLO DE SEGURIDAD (solo TransitTransfer) =====

        /// <summary>Sello de seguridad único para traslados vía tránsito. No puede repetirse entre transacciones.</summary>
        [MaxLength(50)]
        [Column("security_seal")]
        public string? SecuritySeal { get; set; }

        // ===== TRAMO ORIGEN → BODEGA TRÁNSITO (solo TransitTransfer) =====

        /// <summary>Hora de salida desde la bodega origen hacia la bodega de tránsito.</summary>
        [Column("departure_time")]
        public TimeOnly? DepartureTime { get; set; }



        /// <summary>Kilometraje al salir de la bodega origen.</summary>
        [Column("odometer_out")]
        public decimal? OdometerOut { get; set; }

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

        // ===== NAVEGACIÓN =====

        [NotMapped]
        public List<InventoryTransactionLine> Lines { get; set; } = new();
    }

    // ============================================================
    // LÍNEA DE MOVIMIENTO
    // ============================================================

    /// <summary>
    /// Línea de artículo dentro de un movimiento de inventario.
    /// En movimientos TransitTransfer, cada línea puede tener su propia bodega destino
    /// (distinta a la bodega de tránsito del encabezado) y su propio estado de recepción.
    /// </summary>
    [Table("inventory_transaction_line")]
    public class InventoryTransactionLine
    {
        [Key]
        [Column("id_inventory_transaction_line")]
        public int Id { get; set; }

        [Column("id_inventory_transaction")]
        public int IdInventoryTransaction { get; set; }

        [Column("line_number")]
        public int LineNumber { get; set; } = 1;

        // ===== ARTÍCULO =====

        /// <summary>FK lógica a {schema}.item</summary>
        [Column("id_item")]
        public int IdItem { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("item_code")]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        // ===== CANTIDADES =====

        [Column("qty_requested")]
        public decimal QtyRequested { get; set; } = 0;

        [Column("qty_dispatched")]
        public decimal QtyDispatched { get; set; } = 0;

        [Column("qty_received")]
        public decimal QtyReceived { get; set; } = 0;

        // ===== BODEGAS POR LÍNEA (para TransitTransfer) =====

        /// <summary>
        /// Bodega de origen de esta línea específica.
        /// Null = hereda del encabezado.
        /// </summary>
        [Column("id_warehouse_origin_line")]
        public int? IdWarehouseOriginLine { get; set; }

        /// <summary>
        /// Bodega destino FINAL de esta línea.
        /// En TransitTransfer: bodega física destino (diferente a la bodega de tránsito del encabezado).
        /// En Transfer simple: null (hereda del encabezado).
        /// </summary>
        [Column("id_warehouse_dest_line")]
        public int? IdWarehouseDestLine { get; set; }

        // ===== ESTADO DE RECEPCIÓN POR LÍNEA =====

        /// <summary>Estado de esta línea: Pending, InTransit, Received, Rejected</summary>
        [Required]
        [MaxLength(20)]
        [Column("line_status")]
        public string LineStatus { get; set; } = "Pending";

        /// <summary>Fecha y hora en que se confirmó la recepción de esta línea</summary>
        [Column("received_date")]
        public DateTime? ReceivedDate { get; set; }

        /// <summary>FK lógica cross-DB a cms.admin.user — quien confirmó la recepción</summary>
        [Column("received_by_user_id")]
        public int? ReceivedByUserId { get; set; }

        // ===== UNIDAD DE MEDIDA =====

        [Column("id_unit_of_measure")]
        public int? IdUnitOfMeasure { get; set; }

        [MaxLength(10)]
        [Column("unit_of_measure_code")]
        public string? UnitOfMeasureCode { get; set; }

        // ===== COSTO / VALOR =====

        [Column("unit_cost")]
        public decimal? UnitCost { get; set; }

        [Column("total_cost")]
        public decimal? TotalCost { get; set; }

        // ===== LOTE / VENCIMIENTO =====

        [MaxLength(50)]
        [Column("lot_number")]
        public string? LotNumber { get; set; }

        [Column("expiry_date")]
        public DateOnly? ExpiryDate { get; set; }

        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ===== TRAMO BODEGA TRÁNSITO → BODEGA DESTINO N (solo TransitTransfer) =====

        /// <summary>Sello de seguridad específico para el tramo hacia esta bodega destino final.</summary>
        [MaxLength(50)]
        [Column("dest_security_seal")]
        public string? DestSecuritySeal { get; set; }

        /// <summary>Hora de salida hacia esta bodega destino desde la bodega de tránsito.</summary>
        [Column("departure_time")]
        public TimeOnly? DepartureTime { get; set; }

        /// <summary>Hora de llegada a esta bodega destino.</summary>
        [Column("arrival_time")]
        public TimeOnly? ArrivalTime { get; set; }

        /// <summary>Kilometraje al salir hacia esta bodega destino.</summary>
        [Column("odometer_out")]
        public decimal? OdometerOut { get; set; }

        /// <summary>Firma digital del receptor (base64 PNG data URI) capturada en dispositivo táctil.</summary>
        [Column("signature")]
        public string? Signature { get; set; }

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

    // ============================================================
    // SALDO DE EXISTENCIAS POR BODEGA
    // ============================================================

    /// <summary>
    /// Saldo de inventario de un artículo en una bodega específica.
    /// Se actualiza cada vez que se completa un movimiento de inventario.
    /// </summary>
    [Table("existence_warehouse")]
    public class ExistenceWarehouse
    {
        [Key]
        [Column("id_existence_warehouse")]
        public int Id { get; set; }

        /// <summary>FK lógica a {schema}.item</summary>
        [Column("id_item")]
        public int IdItem { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("item_code")]
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>FK lógica a {schema}.warehouse</summary>
        [Column("id_warehouse")]
        public int IdWarehouse { get; set; }

        // ===== SALDOS =====

        /// <summary>Stock disponible (confirmado, no reservado)</summary>
        [Column("qty_on_hand")]
        public decimal QtyOnHand { get; set; } = 0;

        /// <summary>Stock reservado (comprometido en pedidos, traslados en proceso)</summary>
        [Column("qty_reserved")]
        public decimal QtyReserved { get; set; } = 0;

        /// <summary>Stock en tránsito (enviado pero no recibido)</summary>
        [Column("qty_in_transit")]
        public decimal QtyInTransit { get; set; } = 0;

        /// <summary>Disponible = OnHand - Reserved</summary>
        [Column("qty_available")]
        public decimal QtyAvailable { get; set; } = 0;

        // ===== COSTOS =====

        [Column("average_cost")]
        public decimal AverageCost { get; set; } = 0;

        [Column("last_cost")]
        public decimal LastCost { get; set; } = 0;

        // ===== CONTROL =====

        /// <summary>Referencia al último movimiento que actualizó este registro</summary>
        [Column("last_transaction_id")]
        public int? LastTransactionId { get; set; }

        [Column("last_movement_date")]
        public DateTime? LastMovementDate { get; set; }

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
