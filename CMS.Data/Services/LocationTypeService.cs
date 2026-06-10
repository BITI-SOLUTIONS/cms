// ================================================================================
// ARCHIVO: CMS.Data/Services/LocationTypeService.cs
// PROPÓSITO: Implementación del servicio de tipos de localización
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class LocationTypeService : ILocationTypeService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly ILogger<LocationTypeService> _logger;

        public LocationTypeService(ICompanyDbContextFactory dbContextFactory, ILogger<LocationTypeService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<LocationType>> GetAllAsync(int companyId, bool? isActive = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.LocationTypes.AsQueryable();
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        }

        public async Task<LocationType?> GetByIdAsync(int companyId, int id)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.LocationTypes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LocationType?> GetByCodeAsync(int companyId, string code)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.LocationTypes.FirstOrDefaultAsync(x => x.Code == code.ToUpper());
        }

        public async Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.LocationTypes.Where(x => x.Code == code.ToUpper());
            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> GetLocationCountAsync(int companyId, int locationTypeId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.Locations.CountAsync(x => x.IdLocationType == locationTypeId);
        }

        public async Task<LocationType> CreateAsync(int companyId, LocationType locationType, string createdBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            locationType.Code = locationType.Code.Trim().ToUpper();
            locationType.CreateDate = DateTime.UtcNow;
            locationType.CreatedBy = createdBy;
            db.LocationTypes.Add(locationType);
            await db.SaveChangesAsync();
            _logger.LogInformation("LocationType created: {Code} by {User}", locationType.Code, createdBy);
            return locationType;
        }

        public async Task<LocationType> UpdateAsync(int companyId, LocationType locationType, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var existing = await db.LocationTypes.FindAsync(locationType.Id)
                ?? throw new KeyNotFoundException($"LocationType {locationType.Id} not found");

            existing.Code        = locationType.Code.Trim().ToUpper();
            existing.Name        = locationType.Name.Trim();
            existing.Description = locationType.Description?.Trim();
            existing.Icon        = locationType.Icon?.Trim();
            existing.Color       = locationType.Color?.Trim();
            existing.SortOrder   = locationType.SortOrder;
            existing.IsActive    = locationType.IsActive;
            existing.RecordDate = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy;

            await db.SaveChangesAsync();
            _logger.LogInformation("LocationType updated: {Code} by {User}", existing.Code, updatedBy);
            return existing;
        }

        public async Task<bool> DeactivateAsync(int companyId, int id, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var entity = await db.LocationTypes.FindAsync(id);
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
            var entity = await db.LocationTypes.FindAsync(id);
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
            var entity = await db.LocationTypes.FindAsync(id);
            if (entity == null) return false;
            // Verificar si tiene localizaciones asociadas
            if (await db.Locations.AnyAsync(x => x.IdLocationType == id))
                throw new InvalidOperationException("No se puede eliminar un tipo de localización que tiene localizaciones asociadas.");
            db.LocationTypes.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
