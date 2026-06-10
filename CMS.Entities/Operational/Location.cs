// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Location.cs
// PROPÓSITO: Entidad centralizada de localizaciones físicas
// DESCRIPCIÓN: Tabla única para almacenar la información de ubicación geográfica
//              de cualquier entidad del sistema (Bodega, Empleado, Cliente, Proveedor, etc.)
//              Relacionada con LocationType para clasificar su uso.
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

        /// <summary>FK a location_type — define el uso de la localización</summary>
        [Required]
        [Column("id_location_type")]
        public int IdLocationType { get; set; }

        // ── Jerarquía geográfica (FK lógicas a tablas admin.* en BD central cms) ──

        /// <summary>FK a admin.country</summary>
        [Column("id_country")]
        public int? IdCountry { get; set; }

        /// <summary>FK a admin.geographic_division1</summary>
        [Column("id_province")]
        public int? IdProvince { get; set; }

        /// <summary>FK a admin.geographic_division2</summary>
        [Column("id_canton")]
        public int? IdCanton { get; set; }

        /// <summary>FK a admin.geographic_division3</summary>
        [Column("id_district")]
        public int? IdDistrict { get; set; }

        /// <summary>FK a admin.geographic_division4</summary>
        [Column("id_neighborhood")]
        public int? IdNeighborhood { get; set; }

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

        // ── Auditoría ──

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        // ── Navegación ──

        [ForeignKey("IdLocationType")]
        public LocationType? LocationType { get; set; }
    }
}
