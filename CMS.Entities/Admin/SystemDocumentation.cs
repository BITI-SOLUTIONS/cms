// ================================================================================
// ARCHIVO: CMS.Entities/Admin/SystemDocumentation.cs
// PROPÓSITO: Entidad para documentación del sistema (PDFs)
// DESCRIPCIÓN: Almacena documentos PDF de ayuda del sistema CMS
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-08
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin
{
    /// <summary>
    /// Documentación del sistema CMS en formato PDF.
    /// Incluye manuales generales, documentación de módulos, tutoriales y FAQ.
    /// </summary>
    [Table("system_documentation", Schema = "admin")]
    public class SystemDocumentation : IAuditableEntity
    {
        [Key]
        [Column("id_system_documentation")]
        public int ID_SYSTEM_DOCUMENTATION { get; set; }

        /// <summary>
        /// Código único del documento (ej: DOC-GEN-001, MOD-INV-001)
        /// </summary>
        [Column("document_code")]
        [Required]
        [MaxLength(50)]
        public string DOCUMENT_CODE { get; set; } = default!;

        /// <summary>
        /// Título del documento
        /// </summary>
        [Column("document_title")]
        [Required]
        [MaxLength(200)]
        public string DOCUMENT_TITLE { get; set; } = default!;

        /// <summary>
        /// Descripción del contenido del documento
        /// </summary>
        [Column("document_description")]
        public string? DOCUMENT_DESCRIPTION { get; set; }

        /// <summary>
        /// Categoría: GENERAL, MODULE, TUTORIAL, FAQ
        /// </summary>
        [Column("document_category")]
        [Required]
        [MaxLength(50)]
        public string DOCUMENT_CATEGORY { get; set; } = "GENERAL";

        /// <summary>
        /// Nombre del módulo relacionado (si document_category = MODULE)
        /// </summary>
        [Column("module_name")]
        [MaxLength(100)]
        public string? MODULE_NAME { get; set; }

        /// <summary>
        /// Nombre original del archivo
        /// </summary>
        [Column("file_name")]
        [Required]
        [MaxLength(255)]
        public string FILE_NAME { get; set; } = default!;

        /// <summary>
        /// Contenido binario del archivo PDF
        /// </summary>
        [Column("file_data")]
        public byte[]? FILE_DATA { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        [Column("file_size_bytes")]
        public long? FILE_SIZE_BYTES { get; set; }

        /// <summary>
        /// Tipo MIME del archivo (application/pdf)
        /// </summary>
        [Column("content_type")]
        [MaxLength(100)]
        public string CONTENT_TYPE { get; set; } = "application/pdf";

        /// <summary>
        /// Versión del documento
        /// </summary>
        [Column("version")]
        [MaxLength(20)]
        public string VERSION { get; set; } = "1.0";

        /// <summary>
        /// Orden de visualización
        /// </summary>
        [Column("sort_order")]
        public int SORT_ORDER { get; set; }

        /// <summary>
        /// Indica si el documento está activo
        /// </summary>
        [Column("is_active")]
        public bool IS_ACTIVE { get; set; } = true;

        /// <summary>
        /// Indica si el documento es público (visible para todos)
        /// </summary>
        [Column("is_public")]
        public bool IS_PUBLIC { get; set; } = true;

        /// <summary>
        /// Permiso requerido para ver el documento (opcional)
        /// </summary>
        [Column("required_permission")]
        [MaxLength(100)]
        public string? REQUIRED_PERMISSION { get; set; }

        // ============================================================
        // IAuditableEntity Implementation
        // ============================================================

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid ROWPOINTER { get; set; } = Guid.NewGuid();
    }
}
