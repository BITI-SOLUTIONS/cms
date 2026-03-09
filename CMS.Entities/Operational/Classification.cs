// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Classification.cs
// PROPÓSITO: Entidad que representa las clasificaciones de artículos
// DESCRIPCIÓN: Esta entidad se almacena en la BD de cada compañía
//              Usa el campo classification_group para distinguir los 6 niveles:
//              1=Categoría, 2=Subcategoría, 3=Familia, 4=Grupo, 5=Línea, 6=Tipo
//              Tabla: {company_code}.classification (ej: sinai.classification)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-02
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Representa una clasificación de artículos.
    /// Se almacena en la base de datos de cada compañía.
    /// El campo ClassificationGroup indica el nivel (1-6):
    /// 1=Categoría, 2=Subcategoría, 3=Familia, 4=Grupo, 5=Línea, 6=Tipo
    /// </summary>
    [Table("classification")]
    public class Classification
    {
        [Key]
        [Column("id_classification")]
        public int Id { get; set; }

        /// <summary>
        /// Código único de la clasificación dentro de su grupo
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Grupo de clasificación (1-6):
        /// 1=Categoría, 2=Subcategoría, 3=Familia, 4=Grupo, 5=Línea, 6=Tipo
        /// </summary>
        [Required]
        [Column("classification_group")]
        public int ClassificationGroup { get; set; } = 1;

        /// <summary>
        /// Nombre de la clasificación
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la clasificación
        /// </summary>
        [MaxLength(200)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Indica si está activa
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Orden de visualización
        /// </summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; }

        // ===== AUDITORÍA =====

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required]
        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid RowPointer { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Obtiene el nombre descriptivo del grupo de clasificación
        /// </summary>
        [NotMapped]
        public string GroupName => ClassificationGroup switch
        {
            1 => "Categoría",
            2 => "Subcategoría",
            3 => "Familia",
            4 => "Grupo",
            5 => "Línea",
            6 => "Tipo",
            _ => "Desconocido"
        };
    }
}
