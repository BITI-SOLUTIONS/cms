// ================================================================================
// ARCHIVO: CMS.Data/Services/WarehouseService.cs
// PROPÓSITO: Servicio de gestión de bodegas (WMS)
// DESCRIPCIÓN: CRUD + jerarquía + validaciones para la tabla warehouse en la BD de la compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Entities;
using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly AppDbContext _centralDbContext;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(
            ICompanyDbContextFactory dbContextFactory,
            AppDbContext centralDbContext,
            ILogger<WarehouseService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _centralDbContext = centralDbContext;
            _logger = logger;
        }

        public async Task<(List<Warehouse> Items, int TotalCount)> GetWarehousesAsync(
            int companyId,
            string? search = null,
            string? warehouseType = null,
            int? warehouseLevel = null,
            int? parentId = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.Warehouses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(w =>
                    w.Code.ToLower().Contains(s) ||
                    w.Name.ToLower().Contains(s) ||
                    (w.Description != null && w.Description.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(warehouseType))
                query = query.Where(w => w.WarehouseType == warehouseType);

            if (warehouseLevel.HasValue)
                query = query.Where(w => w.WarehouseLevel == warehouseLevel.Value);

            if (parentId.HasValue)
                query = query.Where(w => w.IdParentWarehouse == parentId.Value);

            if (isActive.HasValue)
                query = query.Where(w => w.IsActive == isActive.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(w => w.WarehouseLevel)
                .ThenBy(w => w.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await ResolveResponsibleAsync(items);
            await ResolveLocationAsync(companyId, items);
            await ResolveTransportUnitAsync(companyId, items);

            return (items, total);
        }

        public async Task<List<Warehouse>> GetWarehouseTreeAsync(int companyId, bool activeOnly = true)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.Warehouses.AsQueryable();
            if (activeOnly) query = query.Where(w => w.IsActive);

            var all = await query.OrderBy(w => w.WarehouseLevel).ThenBy(w => w.Name).ToListAsync();

            // Construir árbol en memoria
            var dict = all.ToDictionary(w => w.Id);
            var roots = new List<Warehouse>();

            foreach (var w in all)
            {
                if (w.IdParentWarehouse.HasValue && dict.TryGetValue(w.IdParentWarehouse.Value, out var parent))
                {
                    parent.Children.Add(w);
                    w.Parent = parent;
                }
                else
                {
                    roots.Add(w);
                }
            }

            return roots;
        }

        public async Task<Warehouse?> GetByIdAsync(int companyId, int warehouseId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse != null)
            {
                await ResolveResponsibleAsync(warehouse);
                await ResolveLocationAsync(companyId, new[] { warehouse });
                await ResolveTransportUnitAsync(companyId, new[] { warehouse });
            }
            return warehouse;
        }

        public async Task<Warehouse?> GetByCodeAsync(int companyId, string code)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.Warehouses.FirstOrDefaultAsync(w => w.Code == code);
        }

        public async Task<Warehouse> CreateAsync(int companyId, Warehouse warehouse, string createdBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var byTrunc = createdBy?.Length > 30 ? createdBy[..30] : createdBy;

            warehouse.CreatedAt = DateTime.UtcNow;
            warehouse.CreatedBy = byTrunc;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedBy = byTrunc;

            db.Warehouses.Add(warehouse);
            await db.SaveChangesAsync();

            _logger.LogInformation("Warehouse created: {Code} by {User} for company {CompanyId}",
                warehouse.Code, createdBy, companyId);

            return warehouse;
        }

        public async Task<Warehouse> UpdateAsync(int companyId, Warehouse warehouse, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouse.Id)
                ?? throw new KeyNotFoundException($"Warehouse {warehouse.Id} not found");

            existing.Code = warehouse.Code;
            existing.Name = warehouse.Name;
            existing.Description = warehouse.Description;
            existing.WarehouseType = warehouse.WarehouseType;
            existing.WarehouseLevel = warehouse.WarehouseLevel;
            existing.IdParentWarehouse = warehouse.IdParentWarehouse;
            existing.IsDefault = warehouse.IsDefault;
            existing.AllowsNegativeStock = warehouse.AllowsNegativeStock;
            existing.RequiresLocation = warehouse.RequiresLocation;
            existing.RequiresLotTracking = warehouse.RequiresLotTracking;
            existing.RequiresExpiryDate = warehouse.RequiresExpiryDate;
            existing.IsManaged = warehouse.IsManaged;
            existing.MaxCapacity = warehouse.MaxCapacity;
            existing.CapacityUnit = warehouse.CapacityUnit;
            existing.IdLocation = warehouse.IdLocation;
            existing.ResponsibleUserId = warehouse.ResponsibleUserId;
            existing.Notes = warehouse.Notes;
            existing.IdTransportUnit = warehouse.IdTransportUnit;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;

            await db.SaveChangesAsync();

            _logger.LogInformation("Warehouse updated: {Code} by {User} for company {CompanyId}",
                existing.Code, updatedBy, companyId);

            return existing;
        }

        public async Task<bool> DeactivateAsync(int companyId, int warehouseId, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null) return false;

            warehouse.IsActive = false;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int companyId, int warehouseId, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null) return false;

            warehouse.IsActive = true;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.Warehouses.Where(w => w.Code == code);
            if (excludeId.HasValue) query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<Warehouse>> GetChildrenAsync(int companyId, int parentId, bool activeOnly = true)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.Warehouses.Where(w => w.IdParentWarehouse == parentId);
            if (activeOnly) query = query.Where(w => w.IsActive);

            return await query.OrderBy(w => w.Name).ToListAsync();
        }

        public async Task<WarehouseStats> GetStatsAsync(int companyId, int warehouseId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var directChildren = await db.Warehouses
                .Where(w => w.IdParentWarehouse == warehouseId)
                .ToListAsync();

            // Contar todos los descendientes de forma recursiva (hasta 5 niveles)
            var allDescendants = await GetAllDescendantsCountAsync(db, warehouseId);

            return new WarehouseStats
            {
                TotalChildren = directChildren.Count,
                ActiveChildren = directChildren.Count(c => c.IsActive),
                TotalDescendants = allDescendants
            };
        }

        private async Task<int> GetAllDescendantsCountAsync(CompanyDbContext db, int parentId, int depth = 0)
        {
            if (depth > 5) return 0;

            var children = await db.Warehouses
                .Where(w => w.IdParentWarehouse == parentId)
                .Select(w => w.Id)
                .ToListAsync();

            var count = children.Count;
            foreach (var childId in children)
                count += await GetAllDescendantsCountAsync(db, childId, depth + 1);

            return count;
        }

        // ===== RESOLUCIÓN CROSS-DB: id_user → cms.admin.user =====

        /// <summary>
        /// Valida que el usuario exista en la BD central (cms.admin.user).
        /// </summary>
        public async Task<bool> ValidateResponsibleUserAsync(int? responsibleUserId)
        {
            if (!responsibleUserId.HasValue) return true;
            return await _centralDbContext.Set<User>()
                .AnyAsync(u => u.ID_USER == responsibleUserId.Value);
        }

        /// <summary>
        /// Resuelve el nombre, email y teléfono del responsable desde la BD central (cms)
        /// consultando admin.user por responsible_user_id.
        /// Se llama después de obtener el/los warehouses para no mezclar contextos.
        /// </summary>
        private async Task ResolveResponsibleAsync(Warehouse warehouse)
        {
            if (!warehouse.ResponsibleUserId.HasValue) return;

            var user = await _centralDbContext.Set<User>()
                .Where(u => u.ID_USER == warehouse.ResponsibleUserId.Value)
                .Select(u => new { u.DISPLAY_NAME, u.EMAIL, u.PHONE_NUMBER })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                warehouse.ResponsibleName  = user.DISPLAY_NAME;
                warehouse.ResponsibleEmail = user.EMAIL;
                warehouse.ResponsiblePhone = user.PHONE_NUMBER;
            }
        }

        private async Task ResolveResponsibleAsync(IEnumerable<Warehouse> warehouses)
        {
            var ids = warehouses
                .Where(w => w.ResponsibleUserId.HasValue)
                .Select(w => w.ResponsibleUserId!.Value)
                .Distinct()
                .ToList();

            if (!ids.Any()) return;

            var users = await _centralDbContext.Set<User>()
                .Where(u => ids.Contains(u.ID_USER))
                .Select(u => new { u.ID_USER, u.DISPLAY_NAME, u.EMAIL, u.PHONE_NUMBER })
                .ToListAsync();

            var dict = users.ToDictionary(u => u.ID_USER);

            foreach (var w in warehouses)
            {
                if (w.ResponsibleUserId.HasValue && dict.TryGetValue(w.ResponsibleUserId.Value, out var u))
                {
                    w.ResponsibleName  = u.DISPLAY_NAME;
                    w.ResponsibleEmail = u.EMAIL;
                    w.ResponsiblePhone = u.PHONE_NUMBER;
                }
            }
        }
            private async Task ResolveTransportUnitAsync(int companyId, IEnumerable<Warehouse> warehouses)
            {
                var ids = warehouses
                    .Where(w => w.IdTransportUnit.HasValue)
                    .Select(w => w.IdTransportUnit!.Value)
                    .Distinct()
                    .ToList();

                if (!ids.Any()) return;

                using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
                var units = await db.TransportUnits
                    .Where(u => ids.Contains(u.Id))
                    .Select(u => new { u.Id, Display = u.PlateNumber + " — " + u.Name })
                    .ToListAsync();

                var dict = units.ToDictionary(u => u.Id);

                foreach (var w in warehouses)
                {
                    if (w.IdTransportUnit.HasValue && dict.TryGetValue(w.IdTransportUnit.Value, out var u))
                        w.TransportUnitName = u.Display;
                }
            }

                private async Task ResolveLocationAsync(int companyId, IEnumerable<Warehouse> warehouses)
            {
                var ids = warehouses
                    .Where(w => w.IdLocation.HasValue)
                    .Select(w => w.IdLocation!.Value)
                    .Distinct()
                    .ToList();

                if (!ids.Any()) return;

                using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
                var locations = await db.Locations
                    .Where(l => ids.Contains(l.Id))
                    .Select(l => new { l.Id, l.Address, l.IdCountry, l.GpsLatitude, l.GpsLongitude })
                    .ToListAsync();

                var dict = locations.ToDictionary(l => l.Id);

                foreach (var w in warehouses)
                {
                    if (w.IdLocation.HasValue && dict.TryGetValue(w.IdLocation.Value, out var loc))
                    {
                        w.LocationAddress     = loc.Address;
                        w.LocationCity        = null;
                        w.LocationCountryCode = loc.IdCountry?.ToString();
                        w.LocationGpsLatitude  = loc.GpsLatitude.HasValue  ? (decimal?)Convert.ToDecimal(loc.GpsLatitude.Value)  : null;
                        w.LocationGpsLongitude = loc.GpsLongitude.HasValue ? (decimal?)Convert.ToDecimal(loc.GpsLongitude.Value) : null;
                    }
                }
            }
        }
    }
