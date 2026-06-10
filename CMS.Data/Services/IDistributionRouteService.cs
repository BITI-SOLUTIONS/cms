// ================================================================================
// ARCHIVO: CMS.Data/Services/IDistributionRouteService.cs
// PROPÓSITO: Interfaz del servicio de rutas de distribución
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-10
// ================================================================================

using CMS.Entities.Operational;

namespace CMS.Data.Services
{
    public interface IDistributionRouteService
    {
        /// <summary>Obtiene rutas con filtros y paginación.</summary>
        Task<(List<DistributionRoute> Items, int TotalCount)> GetRoutesAsync(
            int companyId,
            string? search = null,
            string? status = null,
            string? frequency = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>Obtiene una ruta por ID (incluye paradas).</summary>
        Task<DistributionRoute?> GetByIdAsync(int companyId, int routeId);

        /// <summary>Crea una nueva ruta (con sus paradas iniciales).</summary>
        Task<DistributionRoute> CreateAsync(int companyId, DistributionRoute route, string createdBy);

        /// <summary>Actualiza una ruta existente.</summary>
        Task<DistributionRoute> UpdateAsync(int companyId, DistributionRoute route, string updatedBy);

        /// <summary>Desactiva (soft-delete) una ruta.</summary>
        Task<bool> DeactivateAsync(int companyId, int routeId, string updatedBy);

        /// <summary>Activa una ruta.</summary>
        Task<bool> ActivateAsync(int companyId, int routeId, string updatedBy);

        /// <summary>Verifica si un código ya existe en la compañía.</summary>
        Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null);

        // ── Paradas ──────────────────────────────────────────────────────────

        /// <summary>Obtiene las paradas de una ruta, ordenadas.</summary>
        Task<List<DistributionRouteStop>> GetStopsAsync(int companyId, int routeId);

        /// <summary>Reemplaza todas las paradas de una ruta (upsert masivo).</summary>
        Task SaveStopsAsync(int companyId, int routeId, List<DistributionRouteStop> stops, string updatedBy);

        /// <summary>Elimina una parada específica.</summary>
        Task<bool> DeleteStopAsync(int companyId, int stopId);
    }
}
