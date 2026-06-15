// ================================================================================
// ARCHIVO: CMS.Data/Services/ILocationTypeService.cs
// PROPÓSITO: Contrato del servicio de tipos de localización
// DESCRIPCIÓN: LocationType es un catálogo CENTRAL (admin.location_type, BD cms).
//              El parámetro companyId se mantiene por compatibilidad pero
//              la implementación opera sobre la BD central.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// MODIFICADO: 2026-07-04 — Migrado a BD central (admin.location_type)
// ================================================================================

using CMS.Entities.Operational;

namespace CMS.Data.Services
{
    public interface ILocationTypeService
    {
        Task<IEnumerable<LocationType>> GetAllAsync(int companyId, bool? isActive = null);
        Task<LocationType?> GetByIdAsync(int companyId, int id);
        Task<LocationType?> GetByCodeAsync(int companyId, string code);
        Task<LocationType> CreateAsync(int companyId, LocationType locationType, string createdBy);
        Task<LocationType> UpdateAsync(int companyId, LocationType locationType, string updatedBy);
        Task<bool> DeactivateAsync(int companyId, int id, string updatedBy);
        Task<bool> ActivateAsync(int companyId, int id, string updatedBy);
        Task<bool> DeleteAsync(int companyId, int id);
        Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null);
        Task<int> GetLocationCountAsync(int companyId, int locationTypeId);
    }
}
