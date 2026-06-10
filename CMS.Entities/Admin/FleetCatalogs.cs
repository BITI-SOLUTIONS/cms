// ================================================================================
// ARCHIVO: CMS.Entities/Admin/FleetCatalogs.cs
// PROPÓSITO: Catálogos centrales del módulo Fleet Management
// DESCRIPCIÓN: Tablas en BD central (cms, schema admin) compartidas por todas
//              las compañías: TipoUnidad, EstadoUnidad, Combustible, Marca, Modelo.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    // ============================================================
    // CATÁLOGO: TransportUnitTypeCatalog (Tipo de Unidad)
    // ============================================================
    [Table("transport_unit_type", Schema = "admin")]
    public class TransportUnitTypeCatalog
    {
        [Key]
        [Column("id_transport_unit_type")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        [MaxLength(50)][Column("icon")]
        public string? Icon { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }

    // ============================================================
    // CATÁLOGO: TransportUnitStatusCatalog (Estado de Unidad)
    // ============================================================
    [Table("transport_unit_status", Schema = "admin")]
    public class TransportUnitStatusCatalog
    {
        [Key]
        [Column("id_transport_unit_status")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        /// <summary>Color HEX para la insignia de estado (ej: #22c55e)</summary>
        [MaxLength(20)][Column("badge_color")]
        public string? BadgeColor { get; set; }

        [MaxLength(50)][Column("icon")]
        public string? Icon { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }

    // ============================================================
    // CATÁLOGO: FuelTypeCatalog (Tipo de Combustible)
    // ============================================================
    [Table("fuel_type", Schema = "admin")]
    public class FuelTypeCatalog
    {
        [Key]
        [Column("id_fuel_type")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        [MaxLength(50)][Column("icon")]
        public string? Icon { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }

    // ============================================================
    // CATÁLOGO: TransportUnitBrand (Marca de Unidad de Transporte)
    // ============================================================
    [Table("transport_unit_brand", Schema = "admin")]
    public class TransportUnitBrand
    {
        [Key]
        [Column("id_transport_unit_brand")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        /// <summary>FK a admin.country — país de origen de la marca (NOT NULL)</summary>
        [Required]
        [Column("id_country")]
        public int IdCountry { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(IdCountry))]
        public Country? Country { get; set; }   // nullable para no requerir Include en toda consulta

        public ICollection<TransportUnitModel> Models { get; set; } = new List<TransportUnitModel>();
    }

    // ============================================================
    // CATÁLOGO: TransportUnitModel (Modelo de Unidad de Transporte)
    // ============================================================
    [Table("transport_unit_model", Schema = "admin")]
    public class TransportUnitModel
    {
        [Key]
        [Column("id_transport_unit_model")]
        public int Id { get; set; }

        /// <summary>FK a la marca de unidad de transporte</summary>
        [Required]
        [Column("id_transport_unit_brand")]
        public int IdTransportUnitBrand { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        /// <summary>FK nullable a admin.transport_unit_type — tipo de unidad al que aplica este modelo</summary>
        [Column("id_transport_unit_type")]
        public int? IdTransportUnitType { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(IdTransportUnitBrand))]
        public TransportUnitBrand? Brand { get; set; }

        [ForeignKey(nameof(IdTransportUnitType))]
        public TransportUnitTypeCatalog? TransportUnitType { get; set; }
    }
}
