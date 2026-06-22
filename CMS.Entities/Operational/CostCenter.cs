using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational;

/// <summary>
/// Centro de costo (Cost Center).
/// Unidad organizacional que incurre en costos pero no genera ingresos directamente.
/// Se usa para asignación y control de gastos, análisis de rentabilidad, presupuestación y reportes gerenciales.
/// </summary>
[Table("cost_center")]
public class CostCenter
{
    // ===== PK + CAMPOS BASE =====

    [Key]
    [Column("id_cost_center")]
    public int IdCostCenter { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    // ===== JERARQUÍA =====

    [Column("id_parent_cost_center")]
    public int? IdParentCostCenter { get; set; }

    [Column("hierarchy_level")]
    public int HierarchyLevel { get; set; } = 1;

    [MaxLength(500)]
    [Column("full_path")]
    public string? FullPath { get; set; }

    // Navegación para jerarquía
    [ForeignKey(nameof(IdParentCostCenter))]
    public CostCenter? ParentCostCenter { get; set; }

    public ICollection<CostCenter> ChildCostCenters { get; set; } = new List<CostCenter>();

    // ===== CLASIFICACIÓN Y TIPO =====

    [Required]
    [MaxLength(30)]
    [Column("cost_center_type")]
    public string CostCenterType { get; set; } = CostCenterTypes.Operational;

    [MaxLength(50)]
    [Column("category")]
    public string? Category { get; set; }

    // ===== RESPONSABLE =====

    /// <summary>
    /// RELACIÓN LÓGICA CROSS-DB: Referencia a cms.admin.user.id
    /// No es FK real porque están en diferentes bases de datos.
    /// </summary>
    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [MaxLength(200)]
    [Column("responsible_name")]
    public string? ResponsibleName { get; set; }

    // ===== LOCALIZACIÓN FÍSICA =====

    [MaxLength(200)]
    [Column("location")]
    public string? Location { get; set; }

    [MaxLength(100)]
    [Column("department")]
    public string? Department { get; set; }

    [MaxLength(100)]
    [Column("division")]
    public string? Division { get; set; }

    // ===== CONTROL DE VIGENCIA TEMPORAL =====

    [Required]
    [Column("valid_from")]
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;

    [Column("valid_to")]
    public DateTime? ValidTo { get; set; }

    // ===== PRESUPUESTO Y CONTROL FINANCIERO =====

    [Column("annual_budget")]
    public decimal? AnnualBudget { get; set; }

    [MaxLength(3)]
    [Column("budget_currency")]
    public string BudgetCurrency { get; set; } = "CRC";

    [Column("allow_over_budget")]
    public bool AllowOverBudget { get; set; } = false;

    // ===== FLAGS OPERACIONALES =====

    [Column("is_posting_allowed")]
    public bool IsPostingAllowed { get; set; } = true;

    [Column("is_blocked")]
    public bool IsBlocked { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // ===== DIMENSIONES DE ANÁLISIS ADICIONALES =====

    [MaxLength(30)]
    [Column("profit_center_code")]
    public string? ProfitCenterCode { get; set; }

    [MaxLength(30)]
    [Column("business_area_code")]
    public string? BusinessAreaCode { get; set; }

    [MaxLength(10)]
    [Column("company_code")]
    public string? CompanyCode { get; set; }

    // ===== NOTAS =====

    [MaxLength(2000)]
    [Column("notes")]
    public string? Notes { get; set; }

    // ===== AUDITORÍA =====

    [Column("createdate")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    [Column("record_date")]
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(30)]
    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [Column("updated_by")]
    public string UpdatedBy { get; set; } = string.Empty;

    [Column("rowpointer")]
    public Guid RowPointer { get; set; } = Guid.NewGuid();

    // ===== CONSTANTES =====

    /// <summary>
    /// Tipos de centro de costo
    /// </summary>
    public static class CostCenterTypes
    {
        public const string Operational = "Operational";        // Centros operativos (producción, ventas, logística)
        public const string Administrative = "Administrative";  // Centros administrativos (finanzas, RRHH, TI)
        public const string ServiceCenter = "ServiceCenter";    // Centros de servicio interno (mantenimiento, calidad)
        public const string Auxiliary = "Auxiliary";            // Centros auxiliares que redistribuyen costos
    }

    /// <summary>
    /// Categorías comunes de centros de costo
    /// </summary>
    public static class CostCenterCategories
    {
        public const string Production = "Producción";
        public const string Sales = "Ventas";
        public const string Marketing = "Marketing";
        public const string Logistics = "Logística";
        public const string Administration = "Administración";
        public const string Finance = "Finanzas";
        public const string HumanResources = "Recursos Humanos";
        public const string Technology = "Tecnología";
        public const string Quality = "Calidad";
        public const string Maintenance = "Mantenimiento";
        public const string Research = "Investigación y Desarrollo";
    }
}
