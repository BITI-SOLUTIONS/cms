// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Location.cs
// PROPÓSITO: Entidad centralizada de localizaciones físicas
// DESCRIPCIÓN: Tabla única para almacenar la información de ubicación geográfica
//              de cualquier entidad del sistema (Bodega, Empleado, Cliente, Proveedor, etc.)
//              IdLocationType es FK lógica cross-DB → admin.location_type (BD central cms).
//              Se almacena en la BD de cada compañía (ej: sinai.location)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Localización física/geográfica de una entidad del sistema.
    /// Usada por Bodegas, Empleados, Clientes, Proveedores, etc.
    /// El campo IdLocationType define a qué categoría pertenece.
    /// </summary>
    [Table("location")]
    public class Location
    {
        [Key]
        [Column("id_location")]
        public int Id { get; set; }

        /// <summary>FK lógica cross-DB a admin.location_type (BD central cms)</summary>
        [Required]
        [Column("id_location_type")]
        public int IdLocationType { get; set; }

        // ── Jerarquía geográfica (FK lógicas a tablas admin.* en BD central cms) ──

        /// <summary>FK a admin.country</summary>
        [Column("id_country")]
        public int? IdCountry { get; set; }

        /// <summary>FK a admin.geographic_division1</summary>
        [Column("id_geographic_division1")]
        public int? IdGeographicDivision1 { get; set; }

        /// <summary>FK a admin.geographic_division2</summary>
        [Column("id_geographic_division2")]
        public int? IdGeographicDivision2 { get; set; }

        /// <summary>FK a admin.geographic_division3</summary>
        [Column("id_geographic_division3")]
        public int? IdGeographicDivision3 { get; set; }

        /// <summary>FK a admin.geographic_division4</summary>
        [Column("id_geographic_division4")]
        public int? IdGeographicDivision4 { get; set; }

        // ── Dirección ──

        /// <summary>Dirección completa (calle, número, zona)</summary>
        [MaxLength(500)]
        [Column("address")]
        public string? Address { get; set; }

        /// <summary>Dirección secundaria (apartamento, suite, edificio)</summary>
        [MaxLength(200)]
        [Column("address2")]
        public string? Address2 { get; set; }

        /// <summary>Código postal</summary>
        [MaxLength(20)]
        [Column("postal_code")]
        public string? PostalCode { get; set; }

        // ── Coordenadas GPS ──

        [Column("gps_latitude")]
        public double? GpsLatitude { get; set; }

        [Column("gps_longitude")]
        public double? GpsLongitude { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ID del registro del catálogo que usa esta localización (empleado, bodega, cliente, etc.).
        /// NULL = dirección disponible (sin asignar).
        /// Se actualiza automáticamente al crear/editar el registro propietario.
        /// Junto con IdLocationType identifica qué tipo de entidad es la propietaria.
        /// </summary>
        [Column("id_location_catalog")]
        public int? IdLocationCatalog { get; set; }

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

        // ── Navegación ──
        // RELACIÓN LÓGICA CROSS-DB: IdLocationType referencia admin.location_type (BD central cms).
        // No se puede declarar FK real ni navegación EF porque Location está en la BD de compañía.
        // Cargar por separado desde AppDbContext cuando se requiera.
        [NotMapped]
        public LocationType? LocationType { get; set; }
    }
}
