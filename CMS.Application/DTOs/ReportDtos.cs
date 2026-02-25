// ================================================================================
// ARCHIVO: CMS.Application/DTOs/ReportDtos.cs
// PROPÓSITO: DTOs para el sistema de reportes dinámicos
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

namespace CMS.Application.DTOs
{
    // ===== CATEGORÍAS =====

    public class ReportCategoryDto
    {
        public int Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Icon { get; set; } = "bi-folder";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ReportCount { get; set; }
    }

    // ===== REPORTES =====

    public class ReportListDto
    {
        public int Id { get; set; }
        public string ReportCode { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFavorite { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReportDetailDto
    {
        public int Id { get; set; }
        public string ReportCode { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DataSourceType { get; set; } = "SQL";
        public string? DataSource { get; set; }
        public string ConnectionType { get; set; } = "ADMIN";
        public string Icon { get; set; } = string.Empty;
        public int DefaultPageSize { get; set; }
        public bool AllowExportExcel { get; set; }
        public bool AllowExportPdf { get; set; }
        public bool AllowExportCsv { get; set; }
        public string? RequiredPermission { get; set; }
        public bool IsActive { get; set; }
        public List<ReportFilterDto> Filters { get; set; } = new();
        public List<ReportColumnDto> Columns { get; set; } = new();
    }

    public class ReportCreateDto
    {
        public string ReportCode { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string DataSourceType { get; set; } = "SQL";
        public string? DataSource { get; set; }
        public string ConnectionType { get; set; } = "ADMIN";
        public string Icon { get; set; } = "bi-file-earmark-bar-graph";
        public int DefaultPageSize { get; set; } = 25;
        public bool AllowExportExcel { get; set; } = true;
        public bool AllowExportPdf { get; set; } = true;
        public bool AllowExportCsv { get; set; } = true;
        public string? RequiredPermission { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ReportUpdateDto : ReportCreateDto
    {
        public int Id { get; set; }
    }

    // ===== FILTROS =====

    public class ReportFilterDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string FilterKey { get; set; } = string.Empty;
        public string FilterName { get; set; } = string.Empty;
        public string? FilterDescription { get; set; }
        public string FilterType { get; set; } = "TEXT";
        public string? DataSource { get; set; }
        public string? DefaultValue { get; set; }
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public string? MinValue { get; set; }
        public string? MaxValue { get; set; }
        public int ColSpan { get; set; } = 3;
        public int SortOrder { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        
        // Para SELECT/MULTISELECT - opciones parseadas
        public List<SelectOption>? Options { get; set; }
    }

    public class SelectOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class ReportFilterCreateDto
    {
        public string FilterKey { get; set; } = string.Empty;
        public string FilterName { get; set; } = string.Empty;
        public string? FilterDescription { get; set; }
        public string FilterType { get; set; } = "TEXT";
        public string? DataSource { get; set; }
        public string? DefaultValue { get; set; }
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public string? MinValue { get; set; }
        public string? MaxValue { get; set; }
        public int ColSpan { get; set; } = 3;
        public int SortOrder { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
    }

    // ===== COLUMNAS =====

    public class ReportColumnDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string ColumnKey { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string? ColumnDescription { get; set; }
        public string DataType { get; set; } = "STRING";
        public string? FormatPattern { get; set; }
        public string? Width { get; set; }
        public string? MinWidth { get; set; }
        public string TextAlign { get; set; } = "LEFT";
        public string? CssClass { get; set; }
        public string? BadgeConfig { get; set; }
        public string? LinkTemplate { get; set; }
        public string LinkTarget { get; set; } = "_self";
        public bool IsSortable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsVisible { get; set; }
        public bool IsExportable { get; set; }
        public bool ShowTotal { get; set; }
        public string? AggregationType { get; set; }
        public int SortOrder { get; set; }
        public string? DefaultSortDirection { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReportColumnCreateDto
    {
        public string ColumnKey { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string? ColumnDescription { get; set; }
        public string DataType { get; set; } = "STRING";
        public string? FormatPattern { get; set; }
        public string? Width { get; set; }
        public string TextAlign { get; set; } = "LEFT";
        public string? BadgeConfig { get; set; }
        public string? LinkTemplate { get; set; }
        public bool IsSortable { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool IsExportable { get; set; } = true;
        public bool ShowTotal { get; set; }
        public string? AggregationType { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ===== EJECUCIÓN =====

    public class ReportExecuteRequest
    {
        public int ReportId { get; set; }
        public Dictionary<string, object?> Filters { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string? SortColumn { get; set; }
        public string SortDirection { get; set; } = "ASC";
        public string? ExportType { get; set; } // null = visualización, EXCEL, PDF, CSV
    }

    public class ReportExecuteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalRows { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Data { get; set; } = new();
        public Dictionary<string, object?>? Totals { get; set; } // Totales por columna
        public int ExecutionTimeMs { get; set; }
    }

    // ===== FAVORITOS =====

    public class ReportFavoriteDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string? FavoriteName { get; set; }
        public string? SavedFilters { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class AddFavoriteRequest
    {
        public int ReportId { get; set; }
        public string? FavoriteName { get; set; }
        public Dictionary<string, object?>? SavedFilters { get; set; }
    }
}
