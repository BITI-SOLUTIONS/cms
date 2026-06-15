// ================================================================================
// ARCHIVO: CMS.Entities/Operational/JobPosition.cs
// PROPÓSITO: Catálogo de puestos/cargos laborales (por compañía)
// DESCRIPCIÓN: Tabla OPERACIONAL en la BD de cada compañía ({schema}.job_position).
//              Define los puestos a los que pueden ser asignados los empleados del
//              módulo Human Resources.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-05
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    [Table("job_position")]
    public class JobPosition
    {
        [Key]
        [Column("id_job_position")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(150)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        /// <summary>Nivel jerárquico del puesto (Gerencia, Jefatura, Analista, Operativo, etc.)</summary>
        [MaxLength(60)][Column("level")]
        public string? Level { get; set; }

        /// <summary>Departamento al que pertenece este puesto (FK lógica a {schema}.department). NOT NULL.</summary>
        [Required]
        [Column("id_department")]
        public int IdDepartment { get; set; }

        /// <summary>Indica si este puesto corresponde a un conductor (chofer). Usado para filtrar conductores en unidades de transporte y rutas.</summary>
        [Column("is_driver")]
        public bool IsDriver { get; set; } = false;

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ── Auditoría ──
        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(150)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();
    }
}
