// ================================================================================
// ARCHIVO: CMS.Entities/Operational/DistributionRoute.cs
// PROPÓSITO: Entidad de rutas de distribución y sus paradas (WMS / Distribution)
// DESCRIPCIÓN: Se almacena en la BD de cada compañía, schema {company_code}.
//              Una ruta define el recorrido de reparto: origen → paradas → destino.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-10
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Estado operacional de una ruta de distribución.
    /// </summary>
    public static class RouteStatus
    {
        /// <summary>Activa y disponible para asignación</summary>
        public const string Active   = "Active";
        /// <summary>Inactiva / suspendida temporalmente</summary>
        public const string Inactive = "Inactive";
        /// <summary>En ejecución (en ruta actualmente)</summary>
        public const string InProgress = "InProgress";
    }

    /// <summary>
    /// Frecuencia de operación de la ruta.
    /// </summary>
    public static class RouteFrequency
    {
        public const string Daily    = "Daily";
        public const string Weekly   = "Weekly";
        public const string BiWeekly = "BiWeekly";
        public const string Monthly  = "Monthly";
        public const string OnDemand = "OnDemand";
    }

    /// <summary>
    /// Representa una ruta de distribución. Define el recorrido,
    /// la frecuencia, el vehículo asignado y la lista de paradas ordenadas.
    /// Se almacena en la base de datos de cada compañía.
    /// </summary>
    [Table("distribution_route")]
    public class DistributionRoute
    {
        [Key]
        [Column("id_distribution_route")]
        public int Id { get; set; }

        /// <summary>Código único de la ruta (ej: RT-001, RUTA-SUR)</summary>
        [Required]
        [MaxLength(30)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre descriptivo de la ruta</summary>
        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        // ── Estado y frecuencia ──────────────────────────────────────────
        [Required]
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = RouteStatus.Active;

        [Required]
        [MaxLength(30)]
        [Column("frequency")]
        public string Frequency { get; set; } = RouteFrequency.Daily;

        /// <summary>Días de operación como bitmask: 1=Lun, 2=Mar, 4=Mié, 8=Jue, 16=Vie, 32=Sáb, 64=Dom</summary>
        [Column("operation_days")]
        public int OperationDays { get; set; } = 31; // Lun-Vie por defecto

        [Column("departure_time")]
        public TimeOnly? DepartureTime { get; set; }

        [Column("estimated_duration_minutes")]
        public int? EstimatedDurationMinutes { get; set; }

        [Column("estimated_distance_km")]
        public decimal? EstimatedDistanceKm { get; set; }

        // ── Vehículo / conductor ─────────────────────────────────────────
        [MaxLength(100)]
        [Column("vehicle_plate")]
        public string? VehiclePlate { get; set; }

        [MaxLength(100)]
        [Column("vehicle_description")]
        public string? VehicleDescription { get; set; }

        [Column("driver_user_id")]
        public int? DriverUserId { get; set; }

        [MaxLength(100)]
        [NotMapped]
        public string? DriverName { get; set; }

        // ── Bodega origen ─────────────────────────────────────────────────
        // RELACIÓN LÓGICA CROSS-DB: id_origin_warehouse referencia {schema}.warehouse.id_warehouse
        [Column("id_origin_warehouse")]
        public int? IdOriginWarehouse { get; set; }

        [MaxLength(200)]
        [NotMapped]
        public string? OriginWarehouseName { get; set; }

        // ── Capacidad / métricas ─────────────────────────────────────────
        [Column("max_weight_kg")]
        public decimal? MaxWeightKg { get; set; }

        [Column("max_volume_m3")]
        public decimal? MaxVolumeM3 { get; set; }

        // ── Configuración ────────────────────────────────────────────────
        [Column("requires_signature")]
        public bool RequiresSignature { get; set; } = false;

        [Column("requires_photo")]
        public bool RequiresPhoto { get; set; } = false;

        [Column("allows_partial_delivery")]
        public bool AllowsPartialDelivery { get; set; } = true;

        // ── Estado ───────────────────────────────────────────────────────
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ── Auditoría ────────────────────────────────────────────────────
        [Column("createdate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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

        // ── Navegación ───────────────────────────────────────────────────
        [NotMapped]
        public List<DistributionRouteStop> Stops { get; set; } = new();
    }

    // ============================================================================
    // PARADA DE RUTA
    // ============================================================================

    /// <summary>
    /// Estado de una parada individual dentro de una ruta de distribución.
    /// </summary>
    public static class StopStatus
    {
        public const string Pending   = "Pending";
        public const string Completed = "Completed";
        public const string Skipped   = "Skipped";
        public const string Failed    = "Failed";
    }

    /// <summary>
    /// Representa una parada (cliente/punto de entrega) dentro de una ruta de distribución.
    /// </summary>
    [Table("distribution_route_stop")]
    public class DistributionRouteStop
    {
        [Key]
        [Column("id_distribution_route_stop")]
        public int Id { get; set; }

        [Column("id_distribution_route")]
        public int IdRoute { get; set; }

        /// <summary>Orden de visita dentro de la ruta (1-based)</summary>
        [Column("stop_order")]
        public int StopOrder { get; set; }

        // ── Cliente / destino ────────────────────────────────────────────
        [MaxLength(200)]
        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [MaxLength(500)]
        [Column("address")]
        public string? Address { get; set; }

        [MaxLength(100)]
        [Column("city")]
        public string? City { get; set; }

        [Column("gps_latitude")]
        public double? GpsLatitude { get; set; }

        [Column("gps_longitude")]
        public double? GpsLongitude { get; set; }

        // ── Contacto ─────────────────────────────────────────────────────
        [MaxLength(100)]
        [Column("contact_name")]
        public string? ContactName { get; set; }

        [MaxLength(30)]
        [Column("contact_phone")]
        public string? ContactPhone { get; set; }

        // ── Ventana de tiempo ────────────────────────────────────────────
        [Column("time_window_start")]
        public TimeOnly? TimeWindowStart { get; set; }

        [Column("time_window_end")]
        public TimeOnly? TimeWindowEnd { get; set; }

        [Column("estimated_service_minutes")]
        public int? EstimatedServiceMinutes { get; set; }

        // ── Estado ───────────────────────────────────────────────────────
        [Required]
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = StopStatus.Pending;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        // ── Auditoría ────────────────────────────────────────────────────
        [Column("createdate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
