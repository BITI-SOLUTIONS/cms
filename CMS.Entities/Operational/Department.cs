// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Department.cs
// PROPÓSITO: Catálogo de departamentos organizacionales (por compañía)
// DESCRIPCIÓN: Tabla OPERACIONAL en la BD de cada compañía ({schema}.department).
//              Define los departamentos a los que pertenecen los empleados del
//              módulo Human Resources.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-04
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    [Table("department")]
    public class Department
    {
        [Key]
        [Column("id_department")]
        public int Id { get; set; }

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(150)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)][Column("description")]
        public string? Description { get; set; }

        /// <summary>Ícono Bootstrap Icons (ej: bi-briefcase, bi-people)</summary>
        [MaxLength(60)][Column("icon")]
        public string? Icon { get; set; }

        /// <summary>Color hexadecimal para representación visual</summary>
        [MaxLength(20)][Column("color")]
        public string? Color { get; set; }

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
