// ================================================================================
// ARCHIVO: CMS.Entities/Geographic.cs
// PROPÓSITO: Entidades de jerarquía geográfica genérica (4 niveles)
//            Division1 = Provincia/Estado, Division2 = Cantón/Municipio,
//            Division3 = Distrito, Division4 = Barrio/Colonia
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("geographic_division1", Schema = "admin")]
    public class GeographicDivision1
    {
        [Key]
        [Column("id_geographic_division1")]
        public int IdGeographicDivision1 { get; set; }

        [Required]
        [Column("id_country")]
        public int IdCountry { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [ForeignKey("IdCountry")]
        public Country? Country { get; set; }
    }

    [Table("geographic_division2", Schema = "admin")]
    public class GeographicDivision2
    {
        [Key]
        [Column("id_geographic_division2")]
        public int IdGeographicDivision2 { get; set; }

        [Required]
        [Column("id_country")]
        public int IdCountry { get; set; }

        [Required]
        [Column("id_geographic_division1")]
        public int IdGeographicDivision1 { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [ForeignKey("IdCountry")]
        public Country? Country { get; set; }

        [ForeignKey("IdGeographicDivision1")]
        public GeographicDivision1? Division1 { get; set; }
    }

    [Table("geographic_division3", Schema = "admin")]
    public class GeographicDivision3
    {
        [Key]
        [Column("id_geographic_division3")]
        public int IdGeographicDivision3 { get; set; }

        [Required]
        [Column("id_country")]
        public int IdCountry { get; set; }

        [Required]
        [Column("id_geographic_division1")]
        public int IdGeographicDivision1 { get; set; }

        [Required]
        [Column("id_geographic_division2")]
        public int IdGeographicDivision2 { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [ForeignKey("IdCountry")]
        public Country? Country { get; set; }

        [ForeignKey("IdGeographicDivision1")]
        public GeographicDivision1? Division1 { get; set; }

        [ForeignKey("IdGeographicDivision2")]
        public GeographicDivision2? Division2 { get; set; }
    }

    [Table("geographic_division4", Schema = "admin")]
    public class GeographicDivision4
    {
        [Key]
        [Column("id_geographic_division4")]
        public int IdGeographicDivision4 { get; set; }

        [Required]
        [Column("id_country")]
        public int IdCountry { get; set; }

        [Required]
        [Column("id_geographic_division1")]
        public int IdGeographicDivision1 { get; set; }

        [Required]
        [Column("id_geographic_division2")]
        public int IdGeographicDivision2 { get; set; }

        [Required]
        [Column("id_geographic_division3")]
        public int IdGeographicDivision3 { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Código postal generado por el sistema. Ejemplo: 188-10101-01</summary>
        [MaxLength(20)]
        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [ForeignKey("IdCountry")]
        public Country? Country { get; set; }

        [ForeignKey("IdGeographicDivision1")]
        public GeographicDivision1? Division1 { get; set; }

        [ForeignKey("IdGeographicDivision2")]
        public GeographicDivision2? Division2 { get; set; }

        [ForeignKey("IdGeographicDivision3")]
        public GeographicDivision3? Division3 { get; set; }
    }
}
