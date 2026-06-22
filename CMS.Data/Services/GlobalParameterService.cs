// ================================================================================
// ARCHIVO: CMS.Data/Services/GlobalParameterService.cs
// PROPÓSITO: Servicio para gestionar parámetros globales por módulo (POR COMPAÑÍA)
// DESCRIPCIÓN: CRUD completo para parámetros globales con cache y validaciones
//              IMPORTANTE: Los parámetros están en la BD de cada compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-01-22
// MODIFICADO: 2026-01-22 - Cambiado de AppDbContext a CompanyDbContextFactory
//             2026-01-22 - Cambiado module_name→id_menu, parameter_key→code
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio para gestionar parámetros globales del sistema organizados por módulo (menú).
    /// Los parámetros están en la BD de cada compañía (ej: sinai.global_parameter).
    /// </summary>
    public class GlobalParameterService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly AppDbContext _centralDbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GlobalParameterService> _logger;
        private const string CACHE_KEY_PREFIX = "GlobalParameter_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

        public GlobalParameterService(
            ICompanyDbContextFactory dbContextFactory,
            AppDbContext centralDbContext,
            IMemoryCache cache,
            ILogger<GlobalParameterService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _centralDbContext = centralDbContext;
            _cache = cache;
            _logger = logger;
        }

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <summary>
        /// Obtiene todos los parámetros globales activos de una compañía
        /// </summary>
        public async Task<List<GlobalParameter>> GetAllParametersAsync(int companyId)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}All_C{companyId}";

            if (!_cache.TryGetValue(cacheKey, out List<GlobalParameter>? parameters))
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

                parameters = await dbContext.Set<GlobalParameter>()
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.MenuId)
                    .ThenBy(p => p.SortOrder)
                    .ThenBy(p => p.ParameterName)
                    .ToListAsync();

                _cache.Set(cacheKey, parameters, CACHE_DURATION);
                _logger.LogInformation("📋 Global parameters loaded for company {CompanyId} ({Count} items)", 
                    companyId, parameters.Count);
            }

            return parameters ?? new List<GlobalParameter>();
        }

        /// <summary>
        /// Obtiene todos los parámetros de un menú específico de una compañía
        /// </summary>
        public async Task<List<GlobalParameter>> GetParametersByMenuIdAsync(int companyId, int menuId)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}Menu_{menuId}_C{companyId}";

            if (!_cache.TryGetValue(cacheKey, out List<GlobalParameter>? parameters))
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

                parameters = await dbContext.Set<GlobalParameter>()
                    .AsNoTracking()
                    .Where(p => p.MenuId == menuId && p.IsActive)
                    .OrderBy(p => p.SortOrder)
                    .ThenBy(p => p.ParameterName)
                    .ToListAsync();

                _cache.Set(cacheKey, parameters, CACHE_DURATION);
                _logger.LogInformation("📋 Parameters for menu {MenuId} in company {CompanyId} loaded ({Count} items)", 
                    menuId, companyId, parameters.Count);
            }

            return parameters ?? new List<GlobalParameter>();
        }

        /// <summary>
        /// Obtiene un parámetro específico por menú y código de una compañía
        /// </summary>
        public async Task<GlobalParameter?> GetParameterAsync(int companyId, int menuId, string code)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}Menu{menuId}_{code}_C{companyId}";

            if (!_cache.TryGetValue(cacheKey, out GlobalParameter? parameter))
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

                parameter = await dbContext.Set<GlobalParameter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.MenuId == menuId 
                                           && p.Code == code 
                                           && p.IsActive);

                if (parameter != null)
                {
                    _cache.Set(cacheKey, parameter, CACHE_DURATION);
                }
            }

            return parameter;
        }

        /// <summary>
        /// Obtiene un parámetro específico por menú y código de una compañía SIN USAR CACHÉ
        /// (para consultas que necesitan datos en tiempo real)
        /// </summary>
        public async Task<GlobalParameter?> GetParameterNoCacheAsync(int companyId, int menuId, string code)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var parameter = await dbContext.Set<GlobalParameter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MenuId == menuId 
                                       && p.Code == code 
                                       && p.IsActive);

            return parameter;
        }

        /// <summary>
        /// Obtiene el valor de un parámetro Boolean
        /// </summary>
        public async Task<bool> GetBooleanValueAsync(int companyId, int menuId, string code, bool defaultValue = false)
        {
            var parameter = await GetParameterAsync(companyId, menuId, code);

            if (parameter == null || parameter.DataType != "boolean")
            {
                _logger.LogWarning("⚠️ Boolean parameter 'Menu{MenuId}.{Code}' not found in company {CompanyId} or wrong type, returning default: {Default}",
                    menuId, code, companyId, defaultValue);
                return defaultValue;
            }

            return parameter.ValueBoolean ?? defaultValue;
        }

        /// <summary>
        /// Obtiene el valor de un parámetro Integer
        /// </summary>
        public async Task<int> GetIntegerValueAsync(int companyId, int menuId, string code, int defaultValue = 0)
        {
            var parameter = await GetParameterAsync(companyId, menuId, code);

            if (parameter == null || parameter.DataType != "integer")
            {
                return defaultValue;
            }

            return parameter.ValueInteger ?? defaultValue;
        }

        /// <summary>
        /// Obtiene el valor de un parámetro String
        /// </summary>
        public async Task<string?> GetStringValueAsync(int companyId, int menuId, string code, string? defaultValue = null)
        {
            var parameter = await GetParameterAsync(companyId, menuId, code);

            if (parameter == null || parameter.DataType != "string")
            {
                return defaultValue;
            }

            return parameter.ValueString ?? defaultValue;
        }

        /// <summary>
        /// Obtiene la lista de menús únicos que tienen parámetros en una compañía
        /// </summary>
        public async Task<List<int>> GetMenuIdsAsync(int companyId)
        {
            var parameters = await GetAllParametersAsync(companyId);
            return parameters
                .Select(p => p.MenuId)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
        }

        // ============================================================
        // CRUD
        // ============================================================

        /// <summary>
        /// Obtiene un parámetro por ID de una compañía
        /// </summary>
        public async Task<GlobalParameter?> GetByIdAsync(int companyId, int id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            return await dbContext.Set<GlobalParameter>()
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        /// <summary>
        /// Crea un nuevo parámetro global en una compañía
        /// </summary>
        public async Task<GlobalParameter> CreateAsync(int companyId, GlobalParameter parameter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            // Validar unicidad de id_menu + code
            var exists = await dbContext.Set<GlobalParameter>()
                .AnyAsync(p => p.MenuId == parameter.MenuId 
                            && p.Code == parameter.Code);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Ya existe un parámetro con el código '{parameter.Code}' en el menú ID {parameter.MenuId}");
            }

            // Establecer valores de auditoría
            parameter.CreateDate = DateTime.UtcNow;
            parameter.RecordDate = DateTime.UtcNow;
            parameter.RowPointer = Guid.NewGuid();

            dbContext.Set<GlobalParameter>().Add(parameter);
            await dbContext.SaveChangesAsync();

            InvalidateCache(companyId, parameter.MenuId, parameter.Code);
            _logger.LogInformation("✅ Global parameter created in company {CompanyId}: Menu{MenuId}.{Code}", 
                companyId, parameter.MenuId, parameter.Code);

            return parameter;
        }

        /// <summary>
        /// Actualiza un parámetro existente de una compañía
        /// </summary>
        public async Task<GlobalParameter> UpdateAsync(int companyId, GlobalParameter parameter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await dbContext.Set<GlobalParameter>()
                .FirstOrDefaultAsync(p => p.ID == parameter.ID);

            if (existing == null)
            {
                throw new InvalidOperationException($"Parámetro con ID {parameter.ID} no encontrado");
            }

            // No permitir cambiar id_menu o code si es parámetro del sistema
            if (existing.IsSystem && 
                (existing.MenuId != parameter.MenuId || existing.Code != parameter.Code))
            {
                throw new InvalidOperationException("No se puede cambiar el menú o código de un parámetro del sistema");
            }

            // Validar unicidad si cambió la clave
            if (existing.Code != parameter.Code || existing.MenuId != parameter.MenuId)
            {
                var duplicateExists = await dbContext.Set<GlobalParameter>()
                    .AnyAsync(p => p.ID != parameter.ID 
                                && p.MenuId == parameter.MenuId 
                                && p.Code == parameter.Code);

                if (duplicateExists)
                {
                    throw new InvalidOperationException(
                        $"Ya existe otro parámetro con el código '{parameter.Code}' en el menú ID {parameter.MenuId}");
                }
            }

            // Actualizar campos
            existing.MenuId = parameter.MenuId;
            existing.Code = parameter.Code;
            existing.ParameterName = parameter.ParameterName;
            existing.Description = parameter.Description;
            existing.DataType = parameter.DataType;
            existing.ValueString = parameter.ValueString;
            existing.ValueBoolean = parameter.ValueBoolean;
            existing.ValueInteger = parameter.ValueInteger;
            existing.ValueDecimal = parameter.ValueDecimal;
            existing.ValueJson = parameter.ValueJson;
            existing.Category = parameter.Category;
            existing.SortOrder = parameter.SortOrder;
            existing.IsActive = parameter.IsActive;
            existing.RecordDate = DateTime.UtcNow;
            existing.UpdatedBy = parameter.UpdatedBy;

            await dbContext.SaveChangesAsync();

            InvalidateCache(companyId, existing.MenuId, existing.Code);
            _logger.LogInformation("✅ Global parameter updated in company {CompanyId}: Menu{MenuId}.{Code}", 
                companyId, existing.MenuId, existing.Code);

            return existing;
        }

        /// <summary>
        /// Elimina un parámetro de una compañía (solo si no es del sistema)
        /// </summary>
        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var parameter = await dbContext.Set<GlobalParameter>()
                .FirstOrDefaultAsync(p => p.ID == id);

            if (parameter == null)
            {
                return false;
            }

            if (parameter.IsSystem)
            {
                throw new InvalidOperationException("No se pueden eliminar parámetros del sistema");
            }

            dbContext.Set<GlobalParameter>().Remove(parameter);
            await dbContext.SaveChangesAsync();

            InvalidateCache(companyId, parameter.MenuId, parameter.Code);
            _logger.LogInformation("🗑️ Global parameter deleted from company {CompanyId}: Menu{MenuId}.{Code}", 
                companyId, parameter.MenuId, parameter.Code);

            return true;
        }

        // ============================================================
        // CACHE
        // ============================================================

        /// <summary>
        /// Invalida todo el cache de parámetros globales de una compañía
        /// </summary>
        public void InvalidateCache(int companyId, int? menuId = null, string? code = null)
        {
            // Invalidar cache general
            _cache.Remove($"{CACHE_KEY_PREFIX}All_C{companyId}");

            // Si se especifica un menuId, invalidar también ese caché específico
            if (menuId.HasValue)
            {
                _cache.Remove($"{CACHE_KEY_PREFIX}Menu_{menuId.Value}_C{companyId}");

                // Si además se especifica el code, invalidar la clave individual
                if (!string.IsNullOrEmpty(code))
                {
                    _cache.Remove($"{CACHE_KEY_PREFIX}Menu{menuId.Value}_{code}_C{companyId}");
                    _logger.LogInformation("🔄 Global parameter cache invalidated: Menu{MenuId}.{Code} for company {CompanyId}", 
                        menuId.Value, code, companyId);
                }
                else
                {
                    _logger.LogInformation("🔄 Global parameters cache invalidated for company {CompanyId}, menu {MenuId}", 
                        companyId, menuId.Value);
                }
            }
            else
            {
                _logger.LogInformation("🔄 Global parameters cache invalidated for company {CompanyId}", companyId);
            }
        }
    }
}
