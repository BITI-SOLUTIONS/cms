// ================================================================================
// ARCHIVO: CMS.Data/Services/IWarehouseService.cs
// PROPÓSITO: Interfaz del servicio de gestión de bodegas (WMS)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

using CMS.Entities.Operational;

namespace CMS.Data.Services
{
    public interface IWarehouseService
    {
        /// <summary>Obtiene bodegas con filtros y paginación.</summary>
        Task<(List<Warehouse> Items, int TotalCount)> GetWarehousesAsync(
            int companyId,
            string? search = null,
            string? warehouseType = null,
            int? warehouseLevel = null,
            int? parentId = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>Obtiene el árbol completo de bodegas (jerarquía).</summary>
        Task<List<Warehouse>> GetWarehouseTreeAsync(int companyId, bool activeOnly = true);

        /// <summary>Obtiene una bodega por ID.</summary>
        Task<Warehouse?> GetByIdAsync(int companyId, int warehouseId);

        /// <summary>Obtiene una bodega por código.</summary>
        Task<Warehouse?> GetByCodeAsync(int companyId, string code);

        /// <summary>Crea una nueva bodega.</summary>
        Task<Warehouse> CreateAsync(int companyId, Warehouse warehouse, string createdBy);

        /// <summary>Actualiza una bodega existente.</summary>
        Task<Warehouse> UpdateAsync(int companyId, Warehouse warehouse, string updatedBy);

        /// <summary>Desactiva (soft-delete) una bodega.</summary>
        Task<bool> DeactivateAsync(int companyId, int warehouseId, string updatedBy);

        /// <summary>Activa una bodega.</summary>
        Task<bool> ActivateAsync(int companyId, int warehouseId, string updatedBy);

        /// <summary>Verifica si un código ya existe en la compañía (excluyendo un ID).</summary>
        Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null);

        /// <summary>
        /// Valida que el usuario responsable exista en la BD central (cms.admin.user).
        /// Devuelve true si el usuario existe o si responsible_user_id es null.
        /// </summary>
        Task<bool> ValidateResponsibleUserAsync(int? responsibleUserId);

        /// <summary>Obtiene hijos directos de una bodega.</summary>
        Task<List<Warehouse>> GetChildrenAsync(int companyId, int parentId, bool activeOnly = true);

        /// <summary>Obtiene estadísticas de una bodega.</summary>
        Task<WarehouseStats> GetStatsAsync(int companyId, int warehouseId);
    }

    public class WarehouseStats
    {
        public int TotalChildren { get; set; }
        public int ActiveChildren { get; set; }
        public int TotalDescendants { get; set; }
    }
}
