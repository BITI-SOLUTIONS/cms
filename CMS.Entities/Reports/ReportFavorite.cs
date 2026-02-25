// ================================================================================
// ARCHIVO: CMS.Entities/Reports/ReportFavorite.cs
// PROPÓSITO: Entidad para reportes favoritos de usuario
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Reports
{
    /// <summary>
    /// Reportes marcados como favoritos por cada usuario
    /// </summary>
    [Table("report_favorite", Schema = "admin")]
    public class ReportFavorite
    {
        [Key]
        [Column("id_favorite")]
        public int Id { get; set; }

        [Column("id_user")]
        public int UserId { get; set; }

        [Column("id_report")]
        public int ReportId { get; set; }

        [Column("saved_filters", TypeName = "jsonb")]
        public string? SavedFilters { get; set; }

        [Column("favorite_name")]
        [MaxLength(100)]
        public string? FavoriteName { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ReportId")]
        public virtual ReportDefinition? Report { get; set; }
    }
}
