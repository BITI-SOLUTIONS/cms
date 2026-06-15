// ================================================================================
// ARCHIVO: CMS.Data/Services/LocationTypeService.cs
// PROPÓSITO: Implementación del servicio de tipos de localización
// DESCRIPCIÓN: LocationType es un catálogo CENTRAL (admin.location_type, BD cms).
//              El parámetro companyId se mantiene en la firma para compatibilidad
//              con la interfaz, pero la consulta va siempre a la BD central.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// MODIFICADO: 2026-07-04 — Migrado a BD central (admin.location_type)
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class LocationTypeService : ILocationTypeService
    {
        private readonly AppDbContext _db;
        private readonly ICompanyDbContextFactory _companyDbContextFactory;
        private readonly ILogger<LocationTypeService> _logger;

        public LocationTypeService(
            AppDbContext db,
            ICompanyDbContextFactory companyDbContextFactory,
            ILogger<LocationTypeService> logger)
        {
            _db = db;
            _companyDbContextFactory = companyDbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<LocationType>> GetAllAsync(int companyId, bool? isActive = null)
        {
            var query = _db.LocationTypes.AsQueryable();
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        }

        public async Task<LocationType?> GetByIdAsync(int companyId, int id)
        {
            return await _db.LocationTypes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LocationType?> GetByCodeAsync(int companyId, string code)
        {
            return await _db.LocationTypes.FirstOrDefaultAsync(x => x.Code == code.ToUpper());
        }

        public async Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null)
        {
            var query = _db.LocationTypes.Where(x => x.Code == code.ToUpper());
            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> GetLocationCountAsync(int companyId, int locationTypeId)
        {
            using var companyDb = await _companyDbContextFactory.CreateDbContextAsync(companyId);
            return await companyDb.Locations.CountAsync(x => x.IdLocationType == locationTypeId);
        }

        public async Task<LocationType> CreateAsync(int companyId, LocationType locationType, string createdBy)
        {
            locationType.Code = locationType.Code.Trim().ToUpper();
            locationType.CreateDate = DateTime.UtcNow;
            locationType.CreatedBy = createdBy;
            _db.LocationTypes.Add(locationType);
            await _db.SaveChangesAsync();
            _logger.LogInformation("LocationType created: {Code} by {User}", locationType.Code, createdBy);
            return locationType;
        }

        public async Task<LocationType> UpdateAsync(int companyId, LocationType locationType, string updatedBy)
        {
            var existing = await _db.LocationTypes.FindAsync(locationType.Id)
                ?? throw new KeyNotFoundException($"LocationType {locationType.Id} not found");

            existing.Code        = locationType.Code.Trim().ToUpper();
            existing.Name        = locationType.Name.Trim();
            existing.Description = locationType.Description?.Trim();
            existing.Icon        = locationType.Icon?.Trim();
            existing.Color       = locationType.Color?.Trim();
            existing.SortOrder   = locationType.SortOrder;
            existing.IsActive    = locationType.IsActive;
            existing.RecordDate  = DateTime.UtcNow;
            existing.UpdatedBy   = updatedBy;

            await _db.SaveChangesAsync();
            _logger.LogInformation("LocationType updated: {Code} by {User}", existing.Code, updatedBy);
            return existing;
        }

        public async Task<bool> DeactivateAsync(int companyId, int id, string updatedBy)
        {
            var entity = await _db.LocationTypes.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive   = false;
            entity.RecordDate = DateTime.UtcNow;
            entity.UpdatedBy  = updatedBy;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int companyId, int id, string updatedBy)
        {
            var entity = await _db.LocationTypes.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive   = true;
            entity.RecordDate = DateTime.UtcNow;
            entity.UpdatedBy  = updatedBy;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            var entity = await _db.LocationTypes.FindAsync(id);
            if (entity == null) return false;
            // Nota: la verificación de localizaciones asociadas es cross-DB.
            // Se debe validar en el controlador usando GetLocationCountAsync antes de llamar Delete.
            _db.LocationTypes.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
