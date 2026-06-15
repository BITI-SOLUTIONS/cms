// ================================================================================
// ARCHIVO: CMS.Data/Services/ILocationService.cs
// PROPÓSITO: Contrato del servicio de localizaciones
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Entities.Operational;

namespace CMS.Data.Services
{
    public interface ILocationService
    {
        Task<(IEnumerable<Location> Items, int Total)> GetPagedAsync(
            int companyId,
            int page,
            int pageSize,
            string? search = null,
            int? locationTypeId = null,
            bool? isActive = null);

        Task<IEnumerable<Location>> GetByTypeAsync(int companyId, int locationTypeId, bool? isActive = null);
        Task<Location?> GetByIdAsync(int companyId, int id);
        Task<Location> CreateAsync(int companyId, Location location, string createdBy);
        Task<Location> UpdateAsync(int companyId, Location location, string updatedBy);
        Task<bool> DeactivateAsync(int companyId, int id, string updatedBy);
        Task<bool> ActivateAsync(int companyId, int id, string updatedBy);
        Task<bool> DeleteAsync(int companyId, int id);

        /// <summary>Asigna id_location_catalog en la localización indicada.</summary>
        Task SetLocationCatalogAsync(int companyId, int locationId, int catalogEntityId, string updatedBy);

        /// <summary>Limpia id_location_catalog (NULL) en la localización indicada.</summary>
        Task ClearLocationCatalogAsync(int companyId, int locationId, string updatedBy);

        /// <summary>Retorna ubicaciones disponibles (id_location_catalog IS NULL) más la actualmente asignada.</summary>
        Task<IEnumerable<Location>> GetAvailableByTypeAsync(int companyId, int locationTypeId, int? currentLocationId = null);
    }
}
