// ================================================================================
// ARCHIVO: CMS.Data/Services/IStockTransferService.cs
// PROPÓSITO: Interfaz del servicio de traslados de stock entre bodegas
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-12
// ================================================================================

using CMS.Entities.Operational;

namespace CMS.Data.Services
{
    public interface IStockTransferService
    {
        /// <summary>Obtiene traslados con filtros y paginación.</summary>
        Task<(List<StockTransfer> Items, int TotalCount)> GetTransfersAsync(
            int companyId,
            string? search = null,
            string? status = null,
            int? warehouseOriginId = null,
            int? warehouseDestId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>Obtiene un traslado por ID incluyendo sus líneas.</summary>
        Task<StockTransfer?> GetByIdAsync(int companyId, int transferId);

        /// <summary>Crea un nuevo traslado (estado Pending).</summary>
        Task<StockTransfer> CreateAsync(int companyId, StockTransfer transfer, string createdBy);

        /// <summary>Actualiza un traslado en estado Pending.</summary>
        Task<StockTransfer> UpdateAsync(int companyId, StockTransfer transfer, string updatedBy);

        /// <summary>Cambia estado a InProgress (aprobado).</summary>
        Task<StockTransfer> ApproveAsync(int companyId, int transferId, int approvedBy, string updatedBy);

        /// <summary>Cambia estado a Completed y registra cantidades trasladadas.</summary>
        Task<StockTransfer> CompleteAsync(int companyId, int transferId, List<StockTransferLine> lines, int executedBy, string updatedBy);

        /// <summary>Cancela un traslado.</summary>
        Task<StockTransfer> CancelAsync(int companyId, int transferId, string cancelReason, string updatedBy);

        /// <summary>Genera el número de consecutivo siguiente para el traslado.</summary>
        Task<string> GenerateTransferNumberAsync(int companyId);

        /// <summary>Verifica si el número de traslado ya existe.</summary>
        Task<bool> TransferNumberExistsAsync(int companyId, string transferNumber, int? excludeId = null);
    }
}
