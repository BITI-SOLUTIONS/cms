// ================================================================================
// ARCHIVO: CMS.Data/Services/LocationService.cs
// PROPÓSITO: Implementación del servicio de localizaciones
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class LocationService : ILocationService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly ILogger<LocationService> _logger;

        public LocationService(ICompanyDbContextFactory dbContextFactory, ILogger<LocationService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<(IEnumerable<Location> Items, int Total)> GetPagedAsync(
            int companyId, int page, int pageSize,
            string? search = null, int? locationTypeId = null, bool? isActive = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.Locations.Include(x => x.LocationType).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Address != null && x.Address.ToLower().Contains(s)) ||
                    (x.PostalCode != null && x.PostalCode.ToLower().Contains(s)));
            }

            if (locationTypeId.HasValue)
                query = query.Where(x => x.IdLocationType == locationTypeId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.IdLocationType)
                .ThenBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<Location>> GetByTypeAsync(int companyId, int locationTypeId, bool? isActive = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.Locations.Where(x => x.IdLocationType == locationTypeId);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);
            return await query.OrderBy(x => x.Id).ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int companyId, int id)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.Locations.Include(x => x.LocationType).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Location> CreateAsync(int companyId, Location location, string createdBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            location.CreateDate = DateTime.UtcNow;
            location.RecordDate = DateTime.UtcNow;
            location.CreatedBy  = createdBy;
            location.UpdatedBy  = createdBy;
            db.Locations.Add(location);
            await db.SaveChangesAsync();
            _logger.LogInformation("Location created: {Id} by {User}", location.Id, createdBy);
            return location;
        }

        public async Task<Location> UpdateAsync(int companyId, Location location, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var existing = await db.Locations.FindAsync(location.Id)
                ?? throw new KeyNotFoundException($"Location {location.Id} not found");

            existing.IdLocationType = location.IdLocationType;
            existing.IdCountry      = location.IdCountry;
            existing.IdProvince     = location.IdProvince;
            existing.IdCanton       = location.IdCanton;
            existing.IdDistrict     = location.IdDistrict;
            existing.IdNeighborhood = location.IdNeighborhood;
            existing.Address        = location.Address?.Trim();
            existing.Address2       = location.Address2?.Trim();
            existing.PostalCode     = location.PostalCode?.Trim();
            existing.GpsLatitude    = location.GpsLatitude;
            existing.GpsLongitude   = location.GpsLongitude;
            existing.IsActive       = location.IsActive;
            existing.RecordDate     = DateTime.UtcNow;
            existing.UpdatedBy      = updatedBy;

            await db.SaveChangesAsync();
            _logger.LogInformation("Location updated: {Id} by {User}", existing.Id, updatedBy);
            return existing;
        }

        public async Task<bool> DeactivateAsync(int companyId, int id, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var entity = await db.Locations.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive  = false;
            entity.RecordDate = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int companyId, int id, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var entity = await db.Locations.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive  = true;
            entity.RecordDate = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var entity = await db.Locations.FindAsync(id);
            if (entity == null) return false;
            db.Locations.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
