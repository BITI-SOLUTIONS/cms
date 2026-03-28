// ================================================================================
// ARCHIVO: CMS.Entities/Operational/FileComment.cs
// PROPÓSITO: Entidad para comentarios en archivos (BD Operacional)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Comentario en archivo (tabla {schema}.file_comment).
    /// Permite colaboración mediante comentarios en archivos.
    /// </summary>
    [Table("file_comment")]
    public class FileComment
    {
        [Key]
        [Column("id_file_comment")]
        public int IdFileComment { get; set; }

        /// <summary>
        /// ID del archivo
        /// </summary>
        [Column("id_file")]
        public int IdFile { get; set; }

        /// <summary>
        /// ID del comentario padre (para hilos de respuestas)
        /// </summary>
        [Column("parent_id")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Texto del comentario
        /// </summary>
        [Required]
        [Column("comment_text")]
        public string CommentText { get; set; } = string.Empty;

        /// <summary>
        /// IDs de usuarios mencionados (para notificaciones)
        /// </summary>
        [Column("mentioned_user_ids")]
        public int[]? MentionedUserIds { get; set; }

        // ===== Estado =====

        /// <summary>
        /// Si el comentario está resuelto
        /// </summary>
        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;

        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [Column("resolved_by")]
        public int? ResolvedBy { get; set; }

        /// <summary>
        /// Si el comentario fue editado
        /// </summary>
        [Column("is_edited")]
        public bool IsEdited { get; set; } = false;

        [Column("edited_at")]
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Si el comentario está eliminado (soft delete)
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ===== Auditoría estándar =====

        [Column("createdate")]
        public DateTime Createdate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "system";

        [MaxLength(30)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "system";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();

        // ===== Navegación =====

        [ForeignKey("IdFile")]
        public virtual FileDocument? File { get; set; }

        [ForeignKey("ParentId")]
        public virtual FileComment? Parent { get; set; }

        public virtual ICollection<FileComment> Replies { get; set; } = new List<FileComment>();

        // ===== Propiedades calculadas =====

        /// <summary>
        /// Número de respuestas
        /// </summary>
        [NotMapped]
        public int ReplyCount { get; set; }
    }
}
