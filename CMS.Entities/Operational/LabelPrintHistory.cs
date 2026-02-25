// ================================================================================
// ARCHIVO: CMS.Entities/Operational/LabelPrintHistory.cs
// PROPÓSITO: Entidad para el historial de impresiones de etiquetas
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-24
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational;

/// <summary>
/// Representa un registro de historial de impresión de etiquetas
/// </summary>
[Table("label_print_history")]
public class LabelPrintHistory
{
    [Key]
    [Column("id_label_print_history")]
    public int Id { get; set; }

    // ===== Referencia al artículo =====

    [Column("id_item")]
    [Required]
    public int IdItem { get; set; }

    [Column("item_code")]
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Column("item_name")]
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    // ===== Datos de la etiqueta al momento de impresión =====

    [Column("label_item")]
    [MaxLength(200)]
    public string? LabelItem { get; set; }

    [Column("label_price")]
    public decimal LabelPrice { get; set; }

    [Column("label_item_barcode")]
    [MaxLength(100)]
    public string? LabelItemBarcode { get; set; }

    // ===== Configuración de qué imprimir =====
    
    [Column("print_label_name")]
    public bool PrintLabelName { get; set; } = true;

    [Column("print_label_price")]
    public bool PrintLabelPrice { get; set; } = true;

    [Column("print_label_barcode")]
    public bool PrintLabelBarcode { get; set; } = true;

    [Column("print_label_border")]
    public bool PrintLabelBorder { get; set; } = true;

    [Column("print_currency_symbol")]
    public bool PrintCurrencySymbol { get; set; } = true;

    // ===== Configuración de tamaño =====
    
    [Column("label_width_cm")]
    public decimal LabelWidthCm { get; set; } = 4.0m;

    [Column("label_height_cm")]
    public decimal LabelHeightCm { get; set; } = 2.0m;

    [Column("label_orientation")]
    [MaxLength(20)]
    public string LabelOrientation { get; set; } = "horizontal";

    // ===== Configuración de colores =====
    
    [Column("label_border_color")]
    [MaxLength(20)]
    public string LabelBorderColor { get; set; } = "#000000";

    [Column("label_name_color")]
    [MaxLength(20)]
    public string LabelNameColor { get; set; } = "#000000";

    [Column("label_price_color")]
    [MaxLength(20)]
    public string LabelPriceColor { get; set; } = "#16a34a";

    [Column("label_barcode_color")]
    [MaxLength(20)]
    public string LabelBarcodeColor { get; set; } = "#000000";

    // ===== Configuración de fuente y formato =====
    
    [Column("label_font_size")]
    public decimal LabelFontSize { get; set; } = 14.0m;

    [Column("label_font_family")]
    [MaxLength(50)]
    public string LabelFontFamily { get; set; } = "Arial";

    [Column("label_price_decimals")]
    public int LabelPriceDecimals { get; set; } = 2;

    [Column("label_thousand_separator")]
    [MaxLength(5)]
    public string LabelThousandSeparator { get; set; } = ",";

    [Column("label_currency_symbol")]
    [MaxLength(10)]
    public string LabelCurrencySymbol { get; set; } = "₡";

    // ===== Precio formateado =====
    
    [Column("formatted_price")]
    [MaxLength(50)]
    public string? FormattedPrice { get; set; }

    // ===== Información de impresión =====
    
    [Column("quantity_printed")]
    public int QuantityPrinted { get; set; } = 1;

    [Column("print_date")]
    public DateTime PrintDate { get; set; } = DateTime.UtcNow;

    [Column("printed_by")]
    [MaxLength(100)]
    public string? PrintedBy { get; set; }

    [Column("printer_name")]
    [MaxLength(100)]
    public string? PrinterName { get; set; }

    [Column("print_notes")]
    public string? PrintNotes { get; set; }

    // ===== Campos de auditoría =====

    [Column("createdate")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    [Column("record_date")]
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    [MaxLength(30)]
    public string CreatedBy { get; set; } = "SYSTEM";

    [Column("updated_by")]
    [MaxLength(30)]
    public string UpdatedBy { get; set; } = "SYSTEM";

    [Column("rowpointer")]
    public Guid RowPointer { get; set; } = Guid.NewGuid();
}
