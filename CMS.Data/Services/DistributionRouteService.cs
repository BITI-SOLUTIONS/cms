// ================================================================================
// ARCHIVO: CMS.Data/Services/DistributionRouteService.cs
// PROPÓSITO: Servicio de rutas de distribución
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-10
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class DistributionRouteService : IDistributionRouteService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly AppDbContext _centralDb;
        private readonly ILogger<DistributionRouteService> _logger;

        public DistributionRouteService(
            ICompanyDbContextFactory dbContextFactory,
            AppDbContext centralDb,
            ILogger<DistributionRouteService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _centralDb = centralDb;
            _logger = logger;
        }

        // ================================================================
        // GET LIST
        // ================================================================
        public async Task<(List<DistributionRoute> Items, int TotalCount)> GetRoutesAsync(
            int companyId,
            string? search = null,
            string? status = null,
            string? frequency = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.DistributionRoutes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    r.Code.ToLower().Contains(s) ||
                    r.Name.ToLower().Contains(s) ||
                    (r.Description != null && r.Description.ToLower().Contains(s)) ||
                    (r.VehiclePlate != null && r.VehiclePlate.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (!string.IsNullOrWhiteSpace(frequency))
                query = query.Where(r => r.Frequency == frequency);

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Resolver nombres: bodega origen y conductor
            await ResolveNamesAsync(companyId, items, db);

            return (items, total);
        }

        // ================================================================
        // GET BY ID
        // ================================================================
        public async Task<DistributionRoute?> GetByIdAsync(int companyId, int routeId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var route = await db.DistributionRoutes
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null) return null;

            route.Stops = await db.DistributionRouteStops
                .Where(s => s.IdRoute == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .ToListAsync();

            await ResolveNamesAsync(companyId, new List<DistributionRoute> { route }, db);

            return route;
        }

        // ================================================================
        // CREATE
        // ================================================================
        public async Task<DistributionRoute> CreateAsync(int companyId, DistributionRoute route, string createdBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var by = (createdBy ?? "system")[..Math.Min(createdBy?.Length ?? 6, 30)];
            route.CreatedBy   = by;
            route.UpdatedBy   = by;
            route.CreatedAt   = DateTime.UtcNow;
            route.UpdatedAt   = DateTime.UtcNow;
            route.RowPointer  = Guid.NewGuid();

            db.DistributionRoutes.Add(route);
            await db.SaveChangesAsync();

            _logger.LogInformation("Route created: {Code} (Company {CompanyId}) by {User}", route.Code, companyId, by);
            return route;
        }

        // ================================================================
        // UPDATE
        // ================================================================
        public async Task<DistributionRoute> UpdateAsync(int companyId, DistributionRoute route, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await db.DistributionRoutes.FindAsync(route.Id)
                ?? throw new KeyNotFoundException($"Route {route.Id} not found");

            var by = (updatedBy ?? "system")[..Math.Min(updatedBy?.Length ?? 6, 30)];

            existing.Code                    = route.Code;
            existing.Name                    = route.Name;
            existing.Description             = route.Description;
            existing.Status                  = route.Status;
            existing.Frequency               = route.Frequency;
            existing.OperationDays           = route.OperationDays;
            existing.DepartureTime           = route.DepartureTime;
            existing.EstimatedDurationMinutes = route.EstimatedDurationMinutes;
            existing.EstimatedDistanceKm     = route.EstimatedDistanceKm;
            existing.VehiclePlate            = route.VehiclePlate;
            existing.VehicleDescription      = route.VehicleDescription;
            existing.DriverUserId            = route.DriverUserId;
            existing.IdOriginWarehouse       = route.IdOriginWarehouse;
            existing.MaxWeightKg             = route.MaxWeightKg;
            existing.MaxVolumeM3             = route.MaxVolumeM3;
            existing.RequiresSignature       = route.RequiresSignature;
            existing.RequiresPhoto           = route.RequiresPhoto;
            existing.AllowsPartialDelivery   = route.AllowsPartialDelivery;
            existing.IsActive                = route.IsActive;
            existing.Notes                   = route.Notes;
            existing.UpdatedBy               = by;
            existing.UpdatedAt               = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return existing;
        }

        // ================================================================
        // DEACTIVATE / ACTIVATE
        // ================================================================
        public async Task<bool> DeactivateAsync(int companyId, int routeId, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var route = await db.DistributionRoutes.FindAsync(routeId);
            if (route == null) return false;
            var by = (updatedBy ?? "system")[..Math.Min(updatedBy?.Length ?? 6, 30)];
            route.IsActive  = false;
            route.Status    = RouteStatus.Inactive;
            route.UpdatedBy = by;
            route.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int companyId, int routeId, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var route = await db.DistributionRoutes.FindAsync(routeId);
            if (route == null) return false;
            var by = (updatedBy ?? "system")[..Math.Min(updatedBy?.Length ?? 6, 30)];
            route.IsActive  = true;
            route.Status    = RouteStatus.Active;
            route.UpdatedBy = by;
            route.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // CODE EXISTS
        // ================================================================
        public async Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var q = db.DistributionRoutes.Where(r => r.Code.ToUpper() == code.ToUpper());
            if (excludeId.HasValue) q = q.Where(r => r.Id != excludeId.Value);
            return await q.AnyAsync();
        }

        // ================================================================
        // STOPS
        // ================================================================
        public async Task<List<DistributionRouteStop>> GetStopsAsync(int companyId, int routeId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.DistributionRouteStops
                .Where(s => s.IdRoute == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .ToListAsync();
        }

        public async Task SaveStopsAsync(int companyId, int routeId, List<DistributionRouteStop> stops, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var by = (updatedBy ?? "system")[..Math.Min(updatedBy?.Length ?? 6, 30)];

            // Eliminar paradas existentes (soft-delete)
            var existing = await db.DistributionRouteStops
                .Where(s => s.IdRoute == routeId)
                .ToListAsync();
            db.DistributionRouteStops.RemoveRange(existing);

            // Insertar nuevas
            int order = 1;
            foreach (var stop in stops)
            {
                stop.IdRoute    = routeId;
                stop.StopOrder  = order++;
                stop.CreatedBy  = by;
                stop.UpdatedBy  = by;
                stop.CreatedAt  = DateTime.UtcNow;
                stop.UpdatedAt  = DateTime.UtcNow;
                stop.RowPointer = Guid.NewGuid();
                stop.IsActive   = true;
                db.DistributionRouteStops.Add(stop);
            }

            await db.SaveChangesAsync();
        }

        public async Task<bool> DeleteStopAsync(int companyId, int stopId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var stop = await db.DistributionRouteStops.FindAsync(stopId);
            if (stop == null) return false;
            db.DistributionRouteStops.Remove(stop);
            await db.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private async Task ResolveNamesAsync(
            int companyId,
            List<DistributionRoute> routes,
            CompanyDbContext db)
        {
            // Resolver nombres de bodegas origen
            var warehouseIds = routes
                .Where(r => r.IdOriginWarehouse.HasValue)
                .Select(r => r.IdOriginWarehouse!.Value)
                .Distinct()
                .ToList();

            if (warehouseIds.Any())
            {
                var warehouses = await db.Warehouses
                    .Where(w => warehouseIds.Contains(w.Id))
                    .Select(w => new { w.Id, w.Name })
                    .ToListAsync();

                foreach (var r in routes)
                    r.OriginWarehouseName = warehouses
                        .FirstOrDefault(w => w.Id == r.IdOriginWarehouse)?.Name;
            }

            // Resolver nombres de conductores (central DB)
            var driverIds = routes
                .Where(r => r.DriverUserId.HasValue)
                .Select(r => r.DriverUserId!.Value)
                .Distinct()
                .ToList();

            if (driverIds.Any())
            {
                var drivers = await _centralDb.Users
                    .Where(u => driverIds.Contains(u.ID_USER))
                    .Select(u => new { u.ID_USER, u.FIRST_NAME, u.LAST_NAME })
                    .ToListAsync();

                foreach (var r in routes)
                {
                    var d = drivers.FirstOrDefault(u => u.ID_USER == r.DriverUserId);
                    if (d != null)
                        r.DriverName = $"{d.FIRST_NAME} {d.LAST_NAME}".Trim();
                }
            }
        }
    }
}
