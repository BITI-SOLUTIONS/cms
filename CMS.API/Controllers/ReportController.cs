// ================================================================================
// ARCHIVO: CMS.API/Controllers/ReportController.cs
// PROPÓSITO: API para gestión y ejecución de reportes dinámicos
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using CMS.Application.DTOs;
using CMS.Data;
using CMS.Entities.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Npgsql;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ReportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ReportController(
            AppDbContext db,
            ILogger<ReportController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _db = db;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        #region Categorías

        // GET: api/report/categories
        [HttpGet("categories")]
        public async Task<ActionResult<List<ReportCategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _db.ReportCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new ReportCategoryDto
                    {
                        Id = c.Id,
                        CategoryCode = c.CategoryCode,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        Icon = c.Icon,
                        SortOrder = c.SortOrder,
                        IsActive = c.IsActive,
                        ReportCount = _db.ReportDefinitions.Count(r => r.CategoryId == c.Id && r.IsActive)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo categorías de reportes");
                return StatusCode(500, new { message = "Error obteniendo categorías" });
            }
        }

        #endregion

        #region Listado de Reportes

        // GET: api/report - Lista de reportes
        [HttpGet]
        public async Task<ActionResult<List<ReportListDto>>> GetReports(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? onlyFavorites = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = _db.ReportDefinitions
                    .Include(r => r.Category)
                    .Where(r => r.IsActive);

                if (categoryId.HasValue)
                    query = query.Where(r => r.CategoryId == categoryId.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(r => 
                        r.ReportName.ToLower().Contains(searchLower) ||
                        r.ReportCode.ToLower().Contains(searchLower) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchLower)));
                }

                var reports = await query
                    .OrderBy(r => r.Category!.SortOrder)
                    .ThenBy(r => r.SortOrder)
                    .ThenBy(r => r.ReportName)
                    .Select(r => new ReportListDto
                    {
                        Id = r.Id,
                        ReportCode = r.ReportCode,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        CategoryName = r.Category!.CategoryName,
                        Icon = r.Icon,
                        IsActive = r.IsActive,
                        SortOrder = r.SortOrder,
                        IsFavorite = userId.HasValue && _db.ReportFavorites
                            .Any(f => f.ReportId == r.Id && f.UserId == userId.Value)
                    })
                    .ToListAsync();

                if (onlyFavorites == true)
                {
                    reports = reports.Where(r => r.IsFavorite).ToList();
                }

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reportes");
                return StatusCode(500, new { message = "Error obteniendo reportes" });
            }
        }

        // GET: api/report/{id} - Detalle de reporte con filtros y columnas
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDetailDto>> GetReport(int id)
        {
            try
            {
                var report = await _db.ReportDefinitions
                    .Include(r => r.Category)
                    .Include(r => r.Filters.Where(f => f.IsActive).OrderBy(f => f.SortOrder))
                    .Include(r => r.Columns.Where(c => c.IsActive).OrderBy(c => c.SortOrder))
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                    return NotFound(new { message = "Reporte no encontrado" });

                var dto = new ReportDetailDto
                {
                    Id = report.Id,
                    ReportCode = report.ReportCode,
                    ReportName = report.ReportName,
                    Description = report.Description,
                    CategoryId = report.CategoryId,
                    CategoryName = report.Category?.CategoryName ?? "",
                    DataSourceType = report.DataSourceType,
                    DataSource = report.DataSource,
                    ConnectionType = report.ConnectionType,
                    Icon = report.Icon,
                    DefaultPageSize = report.DefaultPageSize,
                    AllowExportExcel = report.AllowExportExcel,
                    AllowExportPdf = report.AllowExportPdf,
                    AllowExportCsv = report.AllowExportCsv,
                    RequiredPermission = report.RequiredPermission,
                    IsActive = report.IsActive,
                    Filters = report.Filters.Select(f => MapFilterToDto(f)).ToList(),
                    Columns = report.Columns.Select(c => new ReportColumnDto
                    {
                        Id = c.Id,
                        ReportId = c.ReportId,
                        ColumnKey = c.ColumnKey,
                        ColumnName = c.ColumnName,
                        ColumnDescription = c.ColumnDescription,
                        DataType = c.DataType,
                        FormatPattern = c.FormatPattern,
                        Width = c.Width,
                        MinWidth = c.MinWidth,
                        TextAlign = c.TextAlign,
                        CssClass = c.CssClass,
                        BadgeConfig = c.BadgeConfig,
                        LinkTemplate = c.LinkTemplate,
                        LinkTarget = c.LinkTarget,
                        IsSortable = c.IsSortable,
                        IsFilterable = c.IsFilterable,
                        IsVisible = c.IsVisible,
                        IsExportable = c.IsExportable,
                        ShowTotal = c.ShowTotal,
                        AggregationType = c.AggregationType,
                        SortOrder = c.SortOrder,
                        DefaultSortDirection = c.DefaultSortDirection,
                        IsActive = c.IsActive
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte {Id}", id);
                return StatusCode(500, new { message = "Error obteniendo reporte" });
            }
        }

        private ReportFilterDto MapFilterToDto(ReportFilter filter)
        {
            var dto = new ReportFilterDto
            {
                Id = filter.Id,
                ReportId = filter.ReportId,
                FilterKey = filter.FilterKey,
                FilterName = filter.FilterName,
                FilterDescription = filter.FilterDescription,
                FilterType = filter.FilterType,
                DataSource = filter.DataSource,
                DefaultValue = filter.DefaultValue,
                Placeholder = filter.Placeholder,
                IsRequired = filter.IsRequired,
                MinValue = filter.MinValue,
                MaxValue = filter.MaxValue,
                ColSpan = filter.ColSpan,
                SortOrder = filter.SortOrder,
                GroupName = filter.GroupName,
                IsActive = filter.IsActive,
                IsVisible = filter.IsVisible
            };

            // Parsear opciones si es SELECT/MULTISELECT
            if ((filter.FilterType == "SELECT" || filter.FilterType == "MULTISELECT") 
                && !string.IsNullOrEmpty(filter.DataSource))
            {
                try
                {
                    // Si empieza con [ es JSON directo
                    if (filter.DataSource.TrimStart().StartsWith("["))
                    {
                        dto.Options = JsonSerializer.Deserialize<List<SelectOption>>(filter.DataSource);
                    }
                    // Si no, es una referencia a tabla (TABLE:nombre:id:text)
                }
                catch
                {
                    // Ignorar errores de parseo
                }
            }

            return dto;
        }

        #endregion

        #region Ejecución de Reportes

        // POST: api/report/execute
        [HttpPost("execute")]
        public async Task<ActionResult<ReportExecuteResult>> ExecuteReport([FromBody] ReportExecuteRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            int? userId = GetCurrentUserId();
            int? companyId = GetCurrentCompanyId();

            try
            {
                var report = await _db.ReportDefinitions
                    .Include(r => r.Filters)
                    .Include(r => r.Columns)
                    .FirstOrDefaultAsync(r => r.Id == request.ReportId && r.IsActive);

                if (report == null)
                    return NotFound(new { message = "Reporte no encontrado" });

                // Verificar permiso si es requerido
                if (!string.IsNullOrEmpty(report.RequiredPermission))
                {
                    // TODO: Verificar permiso del usuario
                }

                // Construir y ejecutar query
                var result = await ExecuteReportQueryAsync(report, request);

                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                // Registrar ejecución
                await LogReportExecutionAsync(report.Id, userId, companyId, request.Filters, 
                    result.TotalRows, result.ExecutionTimeMs, request.ExportType, 
                    result.Success ? "SUCCESS" : "ERROR", result.ErrorMessage);

                return Ok(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error ejecutando reporte {ReportId}", request.ReportId);

                await LogReportExecutionAsync(request.ReportId, userId, companyId, request.Filters,
                    0, (int)stopwatch.ElapsedMilliseconds, request.ExportType, "ERROR", ex.Message);

                return StatusCode(500, new ReportExecuteResult
                {
                    Success = false,
                    ErrorMessage = "Error ejecutando reporte: " + ex.Message
                });
            }
        }

        private async Task<ReportExecuteResult> ExecuteReportQueryAsync(ReportDefinition report, ReportExecuteRequest request)
        {
            var result = new ReportExecuteResult { Success = true };

            if (string.IsNullOrEmpty(report.DataSource))
            {
                result.Success = false;
                result.ErrorMessage = "El reporte no tiene una fuente de datos configurada";
                return result;
            }

            // Obtener connection string
            string? connectionString;
            if (report.ConnectionType == "COMPANY")
            {
                connectionString = await GetCompanyConnectionStringAsync();
            }
            else
            {
                // Usar connection string central (cms) según el ambiente real (ASPNETCORE_ENVIRONMENT)
                var env = _environment.IsDevelopment() ? "Development" : "Production";
                connectionString = _configuration[$"ConnectionStrings:{env}:DefaultConnection"];
                _logger.LogDebug("Usando connection string {Env}: {ConnStr}", env, connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0)));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                result.Success = false;
                result.ErrorMessage = "No se pudo obtener la conexión a la base de datos";
                return result;
            }

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Preparar el query
            var baseQuery = report.DataSource;

            // Construir query con paginación
            var offset = (request.Page - 1) * request.PageSize;
            var sortColumn = request.SortColumn ?? report.Columns.FirstOrDefault()?.ColumnKey ?? "1";
            var sortDirection = request.SortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

            // Query para contar total
            var countQuery = $"SELECT COUNT(*) FROM ({baseQuery}) AS count_query";

            // Query con paginación
            var pagedQuery = $@"
                SELECT * FROM ({baseQuery}) AS base_query
                ORDER BY {sortColumn} {sortDirection}
                OFFSET {offset} LIMIT {request.PageSize}";

            await using var countCmd = new NpgsqlCommand(countQuery, connection);
            await using var dataCmd = new NpgsqlCommand(pagedQuery, connection);

            // Agregar parámetros
            foreach (var filter in report.Filters.Where(f => f.IsActive))
            {
                var paramName = filter.FilterKey.TrimStart('@');
                object? value = null;

                if (request.Filters.TryGetValue(paramName, out var filterValue) && filterValue != null)
                {
                    value = ConvertFilterValue(filterValue, filter.FilterType);
                }

                countCmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                dataCmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
            }

            // Ejecutar count
            var totalRows = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            result.TotalRows = totalRows;
            result.Page = request.Page;
            result.PageSize = request.PageSize;
            result.TotalPages = (int)Math.Ceiling(totalRows / (double)request.PageSize);

            // Ejecutar query de datos
            await using var reader = await dataCmd.ExecuteReaderAsync();
            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(i => reader.GetName(i))
                .ToList();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                foreach (var column in columns)
                {
                    var value = reader[column];
                    row[column] = value == DBNull.Value ? null : value;
                }
                result.Data.Add(row);
            }

            return result;
        }

        private object? ConvertFilterValue(object value, string filterType)
        {
            if (value == null) return null;

            var strValue = value.ToString();
            if (string.IsNullOrEmpty(strValue)) return null;

            return filterType switch
            {
                "NUMBER" or "INTEGER" => int.TryParse(strValue, out var intVal) ? intVal : null,
                "DECIMAL" => decimal.TryParse(strValue, out var decVal) ? decVal : null,
                "DATE" or "DATETIME" => DateTime.TryParse(strValue, out var dateVal) ? dateVal : null,
                "CHECKBOX" => bool.TryParse(strValue, out var boolVal) && boolVal,
                _ => strValue
            };
        }

        private async Task<string?> GetCompanyConnectionStringAsync()
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue) return null;

            var company = await _db.Companies.FindAsync(companyId.Value);
            if (company == null) return null;

            return company.IS_PRODUCTION
                ? company.CONNECTION_STRING_PRODUCTION
                : company.CONNECTION_STRING_DEVELOPMENT;
        }

        private async Task LogReportExecutionAsync(int reportId, int? userId, int? companyId,
            Dictionary<string, object?> filters, int rowsReturned, int executionTimeMs,
            string? exportType, string status, string? errorMessage)
        {
            try
            {
                // No registrar si no hay userId o companyId (requeridos en la tabla)
                if (!userId.HasValue || !companyId.HasValue)
                {
                    _logger.LogWarning("No se puede registrar ejecución de reporte: userId={UserId}, companyId={CompanyId}", userId, companyId);
                    return;
                }

                var log = new ReportExecutionLog
                {
                    ReportId = reportId,
                    UserId = userId.Value,
                    CompanyId = companyId.Value,
                    FiltersUsed = JsonSerializer.Serialize(filters),
                    RowsReturned = rowsReturned,
                    ExecutionTimeMs = executionTimeMs,
                    ExportType = exportType,
                    Status = status,
                    ErrorMessage = errorMessage,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    CreatedBy = User.Identity?.Name ?? "SYSTEM",
                    UpdatedBy = User.Identity?.Name ?? "SYSTEM"
                };

                _db.ReportExecutionLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error registrando ejecución de reporte");
            }
        }

        #endregion

        #region Favoritos

        // GET: api/report/favorites
        [HttpGet("favorites")]
        public async Task<ActionResult<List<ReportFavoriteDto>>> GetFavorites()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var favorites = await _db.ReportFavorites
                    .Include(f => f.Report)
                    .Where(f => f.UserId == userId.Value)
                    .OrderBy(f => f.Report!.ReportName)
                    .Select(f => new ReportFavoriteDto
                    {
                        Id = f.Id,
                        ReportId = f.ReportId,
                        ReportName = f.Report!.ReportName,
                        FavoriteName = f.FavoriteName,
                        SavedFilters = f.SavedFilters,
                        CreateDate = f.CreateDate
                    })
                    .ToListAsync();

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo favoritos");
                return StatusCode(500, new { message = "Error obteniendo favoritos" });
            }
        }

        // POST: api/report/favorites
        [HttpPost("favorites")]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                // Verificar si ya existe
                var exists = await _db.ReportFavorites
                    .AnyAsync(f => f.UserId == userId.Value && f.ReportId == request.ReportId);

                if (exists)
                    return BadRequest(new { message = "El reporte ya está en favoritos" });

                var favorite = new ReportFavorite
                {
                    UserId = userId.Value,
                    ReportId = request.ReportId,
                    FavoriteName = request.FavoriteName,
                    SavedFilters = request.SavedFilters != null 
                        ? JsonSerializer.Serialize(request.SavedFilters) 
                        : null
                };

                _db.ReportFavorites.Add(favorite);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Reporte agregado a favoritos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando favorito");
                return StatusCode(500, new { message = "Error agregando favorito" });
            }
        }

        // DELETE: api/report/favorites/{reportId}
        [HttpDelete("favorites/{reportId}")]
        public async Task<IActionResult> RemoveFavorite(int reportId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var favorite = await _db.ReportFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.ReportId == reportId);

                if (favorite == null)
                    return NotFound(new { message = "Favorito no encontrado" });

                _db.ReportFavorites.Remove(favorite);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Reporte removido de favoritos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removiendo favorito");
                return StatusCode(500, new { message = "Error removiendo favorito" });
            }
        }

        #endregion

        #region Administración (CRUD)

        // POST: api/report - Crear reporte
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ReportDetailDto>> CreateReport([FromBody] ReportCreateDto dto)
        {
            try
            {
                // Validar código único
                if (await _db.ReportDefinitions.AnyAsync(r => r.ReportCode == dto.ReportCode))
                    return BadRequest(new { message = "El código de reporte ya existe" });

                var report = new ReportDefinition
                {
                    ReportCode = dto.ReportCode,
                    ReportName = dto.ReportName,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    DataSourceType = dto.DataSourceType,
                    DataSource = dto.DataSource,
                    ConnectionType = dto.ConnectionType,
                    Icon = dto.Icon,
                    DefaultPageSize = dto.DefaultPageSize,
                    AllowExportExcel = dto.AllowExportExcel,
                    AllowExportPdf = dto.AllowExportPdf,
                    AllowExportCsv = dto.AllowExportCsv,
                    RequiredPermission = dto.RequiredPermission,
                    IsActive = dto.IsActive,
                    CreatedBy = User.Identity?.Name ?? "SYSTEM",
                    UpdatedBy = User.Identity?.Name ?? "SYSTEM"
                };

                _db.ReportDefinitions.Add(report);
                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ Reporte creado: {ReportCode} - {ReportName}", report.ReportCode, report.ReportName);

                return CreatedAtAction(nameof(GetReport), new { id = report.Id }, await GetReport(report.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando reporte");
                return StatusCode(500, new { message = "Error creando reporte" });
            }
        }

        // PUT: api/report/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] ReportUpdateDto dto)
        {
            try
            {
                var report = await _db.ReportDefinitions.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = "Reporte no encontrado" });

                // Validar código único si cambió
                if (dto.ReportCode != report.ReportCode)
                {
                    if (await _db.ReportDefinitions.AnyAsync(r => r.ReportCode == dto.ReportCode && r.Id != id))
                        return BadRequest(new { message = "El código de reporte ya existe" });
                }

                report.ReportCode = dto.ReportCode;
                report.ReportName = dto.ReportName;
                report.Description = dto.Description;
                report.CategoryId = dto.CategoryId;
                report.DataSourceType = dto.DataSourceType;
                report.DataSource = dto.DataSource;
                report.ConnectionType = dto.ConnectionType;
                report.Icon = dto.Icon;
                report.DefaultPageSize = dto.DefaultPageSize;
                report.AllowExportExcel = dto.AllowExportExcel;
                report.AllowExportPdf = dto.AllowExportPdf;
                report.AllowExportCsv = dto.AllowExportCsv;
                report.RequiredPermission = dto.RequiredPermission;
                report.IsActive = dto.IsActive;
                report.UpdatedBy = User.Identity?.Name ?? "SYSTEM";

                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ Reporte actualizado: {ReportCode}", report.ReportCode);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando reporte {Id}", id);
                return StatusCode(500, new { message = "Error actualizando reporte" });
            }
        }

        // DELETE: api/report/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            try
            {
                var report = await _db.ReportDefinitions.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = "Reporte no encontrado" });

                // Soft delete
                report.IsActive = false;
                report.UpdatedBy = User.Identity?.Name ?? "SYSTEM";
                await _db.SaveChangesAsync();

                _logger.LogInformation("🗑️ Reporte desactivado: {ReportCode}", report.ReportCode);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando reporte {Id}", id);
                return StatusCode(500, new { message = "Error eliminando reporte" });
            }
        }

        #endregion

        #region Helpers

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private int? GetCurrentCompanyId()
        {
            // El claim es "companyId" (minúscula) según JwtTokenService
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }

        #endregion
    }
}
