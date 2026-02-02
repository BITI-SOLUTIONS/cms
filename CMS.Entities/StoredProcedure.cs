// ================================================================================
// ARCHIVO: CMS.Entities/StoredProcedure.cs
// PROPÓSITO: Entidad que representa los stored procedures del sistema
// DESCRIPCIÓN: Almacena nombres de SPs que se ejecutan con el schema de la compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2025-12-19
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities
{
    [Table("stored_procedure", Schema = "admin")]
    public class StoredProcedure : IAuditableEntity
    {
        [Key]
        [Column("id_stored_procedure")]
        public int ID_SP { get; set; }

        [Column("sp_code")]
        [Required]
        [MaxLength(50)]
        public string SP_CODE { get; set; } = default!;

        [Column("sp_name")]
        [Required]
        [MaxLength(200)]
        public string SP_NAME { get; set; } = default!;

        [Column("sp_description")]
        [MaxLength(500)]
        public string? SP_DESCRIPTION { get; set; }

        [Column("module")]
        [MaxLength(50)]
        public string? MODULE { get; set; }

        [Column("is_active")]
        public bool IS_ACTIVE { get; set; } = true;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; }

        [Column("record_date")]
        public DateTime RecordDate { get; set; }

        [Column("createdate")]
        public DateTime CreateDate { get; set; }

        [Column("created_by")]
        [Required]
        [MaxLength(30)]
        public string CreatedBy { get; set; } = default!;

        [Column("updated_by")]
        [Required]
        [MaxLength(30)]
        public string UpdatedBy { get; set; } = default!;
    }
}