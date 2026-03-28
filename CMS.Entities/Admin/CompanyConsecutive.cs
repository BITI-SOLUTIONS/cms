// ================================================================================
// ARCHIVO: CMS.Entities/Admin/CompanyConsecutive.cs
// PROPÓSITO: Entidad para almacenar consecutivos globales por compañía
// DESCRIPCIÓN: Esta tabla está en la BD central (cms, schema admin) y almacena
//              consecutivos como container_number que son globales por compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Admin;

/// <summary>
/// Entidad para almacenar consecutivos globales de cada compañía.
/// Cada compañía tiene un único registro con sus consecutivos actuales.
/// Se almacena en la BD central (cms) en el schema admin.
/// </summary>
[Table("company_consecutive", Schema = "admin")]
public class CompanyConsecutive
{
    [Key]
    [Column("id_company_consecutive")]
    public int Id { get; set; }

    /// <summary>
    /// ID de la compañía (FK a admin.company)
    /// </summary>
    [Column("id_company")]
    [Required]
    public int CompanyId { get; set; }

    // ===== CONSECUTIVOS ACTUALES =====

    /// <summary>
    /// Número de contenedor actual de la compañía (máx 50 caracteres)
    /// NOT NULL - usar string vacío como valor por defecto
    /// </summary>
    [Column("container_number")]
    [Required]
    [MaxLength(50)]
    public string ContainerNumber { get; set; } = string.Empty;

    // ===== ESPACIO PARA FUTUROS CONSECUTIVOS =====
    // Los siguientes campos se agregarán cuando se necesiten:
    // 
    // [Column("invoice_number")]
    // [MaxLength(50)]
    // public string? InvoiceNumber { get; set; }
    //
    // [Column("purchase_order_number")]
    // [MaxLength(50)]
    // public string? PurchaseOrderNumber { get; set; }
    //
    // [Column("receipt_number")]
    // [MaxLength(50)]
    // public string? ReceiptNumber { get; set; }

    // ===== AUDITORÍA =====

    [Column("createdate")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    [Column("record_date")]
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    [Required]
    [MaxLength(30)]
    public string CreatedBy { get; set; } = "SYSTEM";

    [Column("updated_by")]
    [Required]
    [MaxLength(30)]
    public string UpdatedBy { get; set; } = "SYSTEM";

    [Column("rowpointer")]
    public Guid RowPointer { get; set; } = Guid.NewGuid();

    // ===== NAVEGACIÓN =====

    /// <summary>
    /// Referencia a la compañía
    /// </summary>
    [ForeignKey("CompanyId")]
    public virtual Company? Company { get; set; }
}
