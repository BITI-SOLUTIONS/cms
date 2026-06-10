// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Warehouse.cs
// PROPÓSITO: Entidad que representa una bodega (física o lógica) en el WMS
// DESCRIPCIÓN: Soporta jerarquía multinivel: Bodega → Zona → Pasillo → Rack → Bin
//              Se almacena en la BD de cada compañía, NO en la BD central.
//              Esquema: {company_code}.warehouse
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Tipo de bodega según su naturaleza operacional.
    /// </summary>
    public static class WarehouseType
    {
        /// <summary>Bodega física con ubicación real</summary>
        public const string Physical = "Physical";
        /// <summary>Bodega lógica/virtual (agrupación contable o de proceso)</summary>
        public const string Logical = "Logical";
        /// <summary>Bodega de tránsito (mercancía en movimiento)</summary>
        public const string Transit = "Transit";
        /// <summary>Cross-docking (recepción directa a despacho)</summary>
        public const string CrossDock = "CrossDock";
        /// <summary>Bodega de devoluciones</summary>
        public const string Returns = "Returns";
        /// <summary>Bodega de producción/manufactura</summary>
        public const string Manufacturing = "Manufacturing";
        /// <summary>Cuarentena (control de calidad)</summary>
        public const string Quarantine = "Quarantine";
        /// <summary>Bodega de dañados/merma</summary>
        public const string Damaged = "Damaged";
        /// <summary>Consignación / Stock de terceros</summary>
        public const string Consignment = "Consignment";
    }

    /// <summary>
    /// Nivel en la jerarquía de la bodega (multi-nivel WMS).
    /// </summary>
    public static class WarehouseLevel
    {
        public const int Warehouse = 0;  // Bodega principal
        public const int Zone = 1;       // Zona (ej: Fría, Seca, Electrónica)
        public const int Aisle = 2;      // Pasillo
        public const int Rack = 3;       // Rack / Estante
        public const int Bin = 4;        // Bin / Ubicación específica
    }

    /// <summary>
    /// Representa una bodega o ubicación dentro del sistema de gestión de almacenes (WMS).
    /// Soporta jerarquía: Bodega → Zona → Pasillo → Rack → Bin.
    /// Se almacena en la base de datos de cada compañía.
    /// </summary>
    public class Warehouse
    {
        [Key]
        [Column("id_warehouse")]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la bodega (ej: BG-01, ZONA-FRIA)
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo de la bodega
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada
        /// </summary>
        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        // ===== CLASIFICACIÓN =====

        /// <summary>
        /// Tipo de bodega: Physical, Logical, Transit, CrossDock, Returns, Manufacturing, Quarantine, Damaged, Consignment
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("warehouse_type")]
        public string WarehouseType { get; set; } = Operational.WarehouseType.Physical;

        /// <summary>
        /// Nivel jerárquico: 0=Bodega, 1=Zona, 2=Pasillo, 3=Rack, 4=Bin
        /// </summary>
        [Column("warehouse_level")]
        public int WarehouseLevel { get; set; } = Operational.WarehouseLevel.Warehouse;

        /// <summary>
        /// ID de la bodega padre (para jerarquía). NULL = bodega raíz.
        /// </summary>
        [Column("id_parent_warehouse")]
        public int? IdParentWarehouse { get; set; }

        // ===== COMPORTAMIENTO OPERACIONAL =====

        /// <summary>
        /// Indica si es la bodega predeterminada para nuevas operaciones
        /// </summary>
        [Column("is_default")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Permite stock negativo en esta bodega
        /// </summary>
        [Column("allows_negative_stock")]
        public bool AllowsNegativeStock { get; set; } = false;

        /// <summary>
        /// Requiere especificar ubicación exacta (bin) al hacer movimientos
        /// </summary>
        [Column("requires_location")]
        public bool RequiresLocation { get; set; } = false;

        /// <summary>
        /// Requiere número de lote/serie al recibir mercancía
        /// </summary>
        [Column("requires_lot_tracking")]
        public bool RequiresLotTracking { get; set; } = false;

        /// <summary>
        /// Requiere fecha de vencimiento al recibir mercancía
        /// </summary>
        [Column("requires_expiry_date")]
        public bool RequiresExpiryDate { get; set; } = false;

        /// <summary>
        /// Bodega gestionada (WMS activo: cada movimiento pasa por flujo de WMS)
        /// </summary>
        [Column("is_managed")]
        public bool IsManaged { get; set; } = false;

        // ===== CAPACIDAD =====

        /// <summary>
        /// Capacidad máxima (en unidades de capacidad)
        /// </summary>
        [Column("max_capacity", TypeName = "decimal(18,4)")]
        public decimal? MaxCapacity { get; set; }

        /// <summary>
        /// Unidad de capacidad (m2, m3, pallets, kg, etc.)
        /// </summary>
        [MaxLength(20)]
        [Column("capacity_unit")]
        public string? CapacityUnit { get; set; }

        // ===== UBICACIÓN FÍSICA =====

        /// <summary>
        /// FK a location.id_location — centraliza la dirección/GPS/contacto de la bodega.
        /// Permite reutilizar la misma tabla de localizaciones para bodegas, empleados,
        /// clientes, proveedores, etc.
        /// </summary>
        [Column("id_location")]
        public int? IdLocation { get; set; }

        // ── Datos de ubicación resueltos desde la tabla location (NotMapped) ──
        [NotMapped]
        public string? LocationAddress { get; set; }

        [NotMapped]
        public string? LocationCity { get; set; }

        [NotMapped]
        public string? LocationCountryCode { get; set; }

        [NotMapped]
        public decimal? LocationGpsLatitude { get; set; }

        [NotMapped]
        public decimal? LocationGpsLongitude { get; set; }

        [NotMapped]
        public Location? Location { get; set; }

        // ===== RESPONSABLE =====

        /// <summary>
        /// ID del usuario responsable de la bodega.
        /// FK LÓGICA CROSS-DB → cms.admin.user.id_user
        /// No se puede declarar FK real porque esta tabla vive en la BD de la compañía
        /// (ej: sinai) y admin.user está en la BD central (cms).
        /// La integridad referencial se mantiene a nivel de aplicación:
        ///   - WarehouseController valida que el usuario exista antes de guardar.
        ///   - WarehouseService resuelve el nombre/email del usuario consultando AppDbContext.
        /// </summary>
        [Column("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }

        // ===== DATOS RESUELTOS DEL RESPONSABLE (NO mapeados a BD) =====
        // Se populan en WarehouseService consultando la BD central (cms.admin.user)

        /// <summary>Nombre completo del responsable, resuelto desde cms.admin.user.</summary>
        [NotMapped]
        public string? ResponsibleName { get; set; }

        /// <summary>Email del responsable, resuelto desde cms.admin.user.</summary>
        [NotMapped]
        public string? ResponsibleEmail { get; set; }

        /// <summary>Teléfono del responsable, resuelto desde cms.admin.user.</summary>
        [NotMapped]
        public string? ResponsiblePhone { get; set; }

        // ===== ESTADO =====

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Notas adicionales
        /// </summary>
        [MaxLength(2000)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ===== AUDITORÍA =====

        [Column("createdate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("record_date")]
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(200)]
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        // ===== NAVEGACIÓN (no mapeado a BD) =====

        [NotMapped]
        public Warehouse? Parent { get; set; }

        [NotMapped]
        public List<Warehouse> Children { get; set; } = new();
    }
}
