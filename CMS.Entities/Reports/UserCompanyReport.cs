// ================================================================================
// ARCHIVO: CMS.Entities/Reports/UserCompanyReport.cs
// PROPÓSITO: Entidad para control de acceso a reportes por usuario y compañía
// DESCRIPCIÓN: Similar a UserCompanyPermission, pero para reportes.
//              Determina qué reportes puede ver cada usuario en cada compañía.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Control de acceso a reportes por usuario y compañía.
    /// Un usuario solo puede ver los reportes para los que tiene un registro 
    /// con is_allowed = true en esta tabla.
    /// </summary>
    [Table("user_company_reports", Schema = "admin")]
    public class UserCompanyReport : IAuditableEntity
    {
        [Column("id_user")]
        [Required]
        public int UserId { get; set; }

        [Column("id_company")]
        [Required]
        public int CompanyId { get; set; }

        [Column("id_report_definition")]
        [Required]
        public int ReportDefinitionId { get; set; }

        /// <summary>
        /// true = usuario tiene acceso al reporte
        /// false = usuario tiene acceso denegado explícitamente
        /// </summary>
        [Column("is_allowed")]
        public bool IsAllowed { get; set; } = true;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Auditoría
        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        [Column("created_by")]
        [MaxLength(30)]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Column("updated_by")]
        [MaxLength(30)]
        public string UpdatedBy { get; set; } = "SYSTEM";

        // Navegación
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }

        [ForeignKey(nameof(ReportDefinitionId))]
        public virtual ReportDefinition? ReportDefinition { get; set; }
    }
}
