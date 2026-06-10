// ================================================================================
// ARCHIVO: CMS.Data/Services/StockTransferService.cs
// PROPÓSITO: Servicio de gestión de traslados de stock entre bodegas
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-12
// ================================================================================

using CMS.Entities;
using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class StockTransferService : IStockTransferService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly AppDbContext _centralDbContext;
        private readonly ILogger<StockTransferService> _logger;

        public StockTransferService(
            ICompanyDbContextFactory dbContextFactory,
            AppDbContext centralDbContext,
            ILogger<StockTransferService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _centralDbContext = centralDbContext;
            _logger = logger;
        }

        // ================================================================
        // GET LIST
        // ================================================================
        public async Task<(List<StockTransfer> Items, int TotalCount)> GetTransfersAsync(
            int companyId,
            string? search = null,
            string? status = null,
            int? warehouseOriginId = null,
            int? warehouseDestId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            int page = 1,
            int pageSize = 20)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.StockTransfers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(t =>
                    t.TransferNumber.ToLower().Contains(s) ||
                    (t.Reference != null && t.Reference.ToLower().Contains(s)) ||
                    (t.Notes != null && t.Notes.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (warehouseOriginId.HasValue)
                query = query.Where(t => t.IdWarehouseOrigin == warehouseOriginId.Value);

            if (warehouseDestId.HasValue)
                query = query.Where(t => t.IdWarehouseDest == warehouseDestId.Value);

            if (dateFrom.HasValue)
                query = query.Where(t => t.TransferDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.TransferDate <= dateTo.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.TransferDate)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await ResolveWarehouseNamesAsync(db, items);
            await ResolveUserNamesAsync(items);

            return (items, total);
        }

        // ================================================================
        // GET BY ID (with lines)
        // ================================================================
        public async Task<StockTransfer?> GetByIdAsync(int companyId, int transferId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var transfer = await db.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null) return null;

            transfer.Lines = await db.StockTransferLines
                .Where(l => l.IdStockTransfer == transferId)
                .OrderBy(l => l.LineNumber)
                .ToListAsync();

            await ResolveWarehouseNamesAsync(db, [transfer]);
            await ResolveUserNamesAsync([transfer]);

            return transfer;
        }

        // ================================================================
        // CREATE
        // ================================================================
        public async Task<StockTransfer> CreateAsync(int companyId, StockTransfer transfer, string createdBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var byTrunc = createdBy?.Length > 30 ? createdBy[..30] : createdBy;

            transfer.Status = StockTransferStatus.Pending;
            transfer.CreatedBy = byTrunc;
            transfer.UpdatedBy = byTrunc;
            transfer.CreateDate = DateTime.UtcNow;
            transfer.RecordDate = DateTime.UtcNow;

            // Numerar líneas
            int lineNum = 1;
            foreach (var line in transfer.Lines)
            {
                line.LineNumber = lineNum++;
                line.CreatedBy = byTrunc;
                line.UpdatedBy = byTrunc;
                line.CreateDate = DateTime.UtcNow;
                line.RecordDate = DateTime.UtcNow;
            }

            db.StockTransfers.Add(transfer);
            await db.SaveChangesAsync();

            if (transfer.Lines.Count > 0)
            {
                foreach (var line in transfer.Lines)
                    line.IdStockTransfer = transfer.Id;

                db.StockTransferLines.AddRange(transfer.Lines);
                await db.SaveChangesAsync();
            }

            _logger.LogInformation("StockTransfer {Number} created for company {CompanyId}", transfer.TransferNumber, companyId);
            return transfer;
        }

        // ================================================================
        // UPDATE (solo en estado Pending)
        // ================================================================
        public async Task<StockTransfer> UpdateAsync(int companyId, StockTransfer transfer, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await db.StockTransfers.FindAsync(transfer.Id)
                ?? throw new InvalidOperationException($"Traslado {transfer.Id} no encontrado.");

            if (existing.Status != StockTransferStatus.Pending)
                throw new InvalidOperationException("Solo se pueden editar traslados en estado Pending.");

            existing.Reference = transfer.Reference;
            existing.Notes = transfer.Notes;
            existing.IdWarehouseOrigin = transfer.IdWarehouseOrigin;
            existing.IdWarehouseDest = transfer.IdWarehouseDest;
            existing.TransferDate = transfer.TransferDate;
            existing.ExpectedDate = transfer.ExpectedDate;
            var updTrunc = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;
            existing.UpdatedBy = updTrunc;
            existing.RecordDate = DateTime.UtcNow;

            // Reemplazar líneas
            var oldLines = await db.StockTransferLines
                .Where(l => l.IdStockTransfer == transfer.Id)
                .ToListAsync();
            db.StockTransferLines.RemoveRange(oldLines);

            int lineNum = 1;
            foreach (var line in transfer.Lines)
            {
                line.Id = 0;
                line.IdStockTransfer = transfer.Id;
                line.LineNumber = lineNum++;
                line.CreatedBy = updTrunc;
                line.UpdatedBy = updTrunc;
                line.CreateDate = DateTime.UtcNow;
                line.RecordDate = DateTime.UtcNow;
            }
            db.StockTransferLines.AddRange(transfer.Lines);

            await db.SaveChangesAsync();
            return existing;
        }

        // ================================================================
        // APPROVE → InProgress
        // ================================================================
        public async Task<StockTransfer> ApproveAsync(int companyId, int transferId, int approvedBy, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var transfer = await db.StockTransfers.FindAsync(transferId)
                ?? throw new InvalidOperationException($"Traslado {transferId} no encontrado.");

            if (transfer.Status != StockTransferStatus.Pending)
                throw new InvalidOperationException("Solo se pueden aprobar traslados en estado Pending.");

            transfer.Status = StockTransferStatus.InProgress;
            transfer.ApprovedBy = approvedBy;
            transfer.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;
            transfer.RecordDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return transfer;
        }

        // ================================================================
        // COMPLETE → Completed
        // ================================================================
        public async Task<StockTransfer> CompleteAsync(int companyId, int transferId, List<StockTransferLine> lines, int executedBy, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var transfer = await db.StockTransfers.FindAsync(transferId)
                ?? throw new InvalidOperationException($"Traslado {transferId} no encontrado.");

            if (transfer.Status != StockTransferStatus.InProgress)
                throw new InvalidOperationException("Solo se pueden completar traslados en estado InProgress.");

            // Actualizar cantidades reales transferidas
            foreach (var inputLine in lines)
            {
                var dbLine = await db.StockTransferLines
                    .FirstOrDefaultAsync(l => l.Id == inputLine.Id && l.IdStockTransfer == transferId);
                if (dbLine != null)
                    dbLine.QtyTransferred = inputLine.QtyTransferred;
            }

            transfer.Status = StockTransferStatus.Completed;
            transfer.ExecutedBy = executedBy;
            transfer.CompletedDate = DateTime.UtcNow;
            transfer.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;
            transfer.RecordDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return transfer;
        }

        // ================================================================
        // CANCEL
        // ================================================================
        public async Task<StockTransfer> CancelAsync(int companyId, int transferId, string cancelReason, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var transfer = await db.StockTransfers.FindAsync(transferId)
                ?? throw new InvalidOperationException($"Traslado {transferId} no encontrado.");

            if (transfer.Status == StockTransferStatus.Completed)
                throw new InvalidOperationException("No se puede cancelar un traslado ya completado.");

            transfer.Status = StockTransferStatus.Cancelled;
            transfer.CancelReason = cancelReason;
            transfer.CancelledDate = DateTime.UtcNow;
            transfer.UpdatedBy = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy;
            transfer.RecordDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return transfer;
        }

        // ================================================================
        // GENERATE NUMBER
        // ================================================================
        public async Task<string> GenerateTransferNumberAsync(int companyId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var year = DateTime.Today.Year;
            var prefix = $"TRF-{year}-";

            var last = await db.StockTransfers
                .Where(t => t.TransferNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNumber)
                .Select(t => t.TransferNumber)
                .FirstOrDefaultAsync();

            int next = 1;
            if (last != null && int.TryParse(last.Replace(prefix, ""), out var lastNum))
                next = lastNum + 1;

            return $"{prefix}{next:D5}";
        }

        // ================================================================
        // EXISTS
        // ================================================================
        public async Task<bool> TransferNumberExistsAsync(int companyId, string transferNumber, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.StockTransfers.Where(t => t.TransferNumber == transferNumber);
            if (excludeId.HasValue) query = query.Where(t => t.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private static async Task ResolveWarehouseNamesAsync(CompanyDbContext db, List<StockTransfer> items)
        {
            if (items.Count == 0) return;

            var warehouseIds = items
                .SelectMany(t => new[] { t.IdWarehouseOrigin, t.IdWarehouseDest })
                .Distinct()
                .ToList();

            var warehouses = await db.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .Select(w => new { w.Id, w.Code, w.Name })
                .ToListAsync();

            var dict = warehouses.ToDictionary(w => w.Id, w => $"{w.Code} – {w.Name}");

            foreach (var t in items)
            {
                t.OriginWarehouseName = dict.GetValueOrDefault(t.IdWarehouseOrigin);
                t.DestWarehouseName = dict.GetValueOrDefault(t.IdWarehouseDest);
            }
        }

        private async Task ResolveUserNamesAsync(List<StockTransfer> items)
        {
            if (items.Count == 0) return;

            var userIds = items
                .SelectMany(t => new[] { (int?)t.RequestedBy, t.ApprovedBy, t.ExecutedBy })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (userIds.Count == 0) return;

            var users = await _centralDbContext.Users
                .Where(u => userIds.Contains(u.ID_USER))
                .Select(u => new { u.ID_USER, u.DISPLAY_NAME, u.USER_NAME })
                .ToListAsync();

            var dict = users.ToDictionary(u => u.ID_USER, u => u.DISPLAY_NAME ?? u.USER_NAME);

            foreach (var t in items)
            {
                t.RequestedByName = dict.GetValueOrDefault(t.RequestedBy);
                if (t.ApprovedBy.HasValue) t.ApprovedByName = dict.GetValueOrDefault(t.ApprovedBy.Value);
                if (t.ExecutedBy.HasValue) t.ExecutedByName = dict.GetValueOrDefault(t.ExecutedBy.Value);
            }
        }
    }
}
