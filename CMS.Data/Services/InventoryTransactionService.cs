// ================================================================================
// ARCHIVO: CMS.Data/Services/InventoryTransactionService.cs
// PROPÓSITO: Implementación del servicio de movimientos de inventario
// DESCRIPCIÓN: Gestiona TODOS los movimientos de inventario:
//              - Traslados simples (Transfer)
//              - Traslados vía bodega de tránsito (TransitTransfer)
//              - Ajustes, entradas, salidas
//              Actualiza el saldo en existence_warehouse al confirmar/completar.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-13
// ================================================================================

using CMS.Entities;
using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly AppDbContext _centralDbContext;
        private readonly ILogger<InventoryTransactionService> _logger;

        public InventoryTransactionService(
            ICompanyDbContextFactory dbContextFactory,
            AppDbContext centralDbContext,
            ILogger<InventoryTransactionService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _centralDbContext = centralDbContext;
            _logger = logger;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<(List<InventoryTransaction> Items, int TotalCount)> GetTransactionsAsync(
            int companyId,
            string? search = null,
            string? movementType = null,
            string? status = null,
            int? warehouseOriginId = null,
            int? warehouseDestId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            int page = 1,
            int pageSize = 20)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = db.InventoryTransactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(t =>
                    t.TransactionNumber.ToLower().Contains(s) ||
                    (t.Reference != null && t.Reference.ToLower().Contains(s)) ||
                    (t.Notes != null && t.Notes.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(movementType))
                query = query.Where(t => t.MovementType == movementType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (warehouseOriginId.HasValue)
                query = query.Where(t => t.IdWarehouseOrigin == warehouseOriginId.Value);

            if (warehouseDestId.HasValue)
                query = query.Where(t => t.IdWarehouseDest == warehouseDestId.Value);

            if (dateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.TransactionDate <= dateTo.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<InventoryTransaction?> GetByIdAsync(int companyId, int id)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.InventoryTransactions
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<InventoryTransactionLine>> GetLinesAsync(int companyId, int transactionId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.InventoryTransactionLines
                .Where(l => l.IdInventoryTransaction == transactionId)
                .OrderBy(l => l.LineNumber)
                .ToListAsync();
        }

        public async Task<bool> TransactionNumberExistsAsync(int companyId, string transactionNumber, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.InventoryTransactions.Where(t => t.TransactionNumber == transactionNumber);
            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> SecuritySealExistsAsync(int companyId, string securitySeal, int? excludeId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var query = db.InventoryTransactions
                .Where(t => t.SecuritySeal != null && t.SecuritySeal == securitySeal);
            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> AnySealExistsAsync(int companyId, string seal, int? excludeTransactionId = null)
        {
            if (string.IsNullOrWhiteSpace(seal)) return false;
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            seal = seal.Trim();

            // Check header seals
            var headerQuery = db.InventoryTransactions
                .Where(t => t.SecuritySeal != null && t.SecuritySeal == seal);
            if (excludeTransactionId.HasValue)
                headerQuery = headerQuery.Where(t => t.Id != excludeTransactionId.Value);
            if (await headerQuery.AnyAsync()) return true;

            // Check dest security seals on lines
            var lineQuery = db.InventoryTransactionLines
                .Where(l => l.DestSecuritySeal != null && l.DestSecuritySeal == seal);
            if (excludeTransactionId.HasValue)
                lineQuery = lineQuery.Where(l => l.IdInventoryTransaction != excludeTransactionId.Value);
            return await lineQuery.AnyAsync();
        }

        // ================================================================
        // GENERACIÓN DE NÚMERO
        // ================================================================

        public async Task<string> GenerateTransactionNumberAsync(int companyId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            var year = DateTime.Today.Year;
            var prefix = $"INV-{year}-";

            var lastNumber = await db.InventoryTransactions
                .Where(t => t.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(t => t.Id)
                .Select(t => t.TransactionNumber)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (lastNumber != null)
            {
                var parts = lastNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var n))
                    seq = n + 1;
            }

            return $"{prefix}{seq:D5}";
        }

        // ================================================================
        // CRUD
        // ================================================================

        public async Task<InventoryTransaction> CreateAsync(
            int companyId,
            InventoryTransaction transaction,
            List<InventoryTransactionLine> lines,
            string createdBy,
            int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            if (string.IsNullOrWhiteSpace(transaction.TransactionNumber))
                transaction.TransactionNumber = await GenerateTransactionNumberAsync(companyId);

            // Truncar a 30 chars para respetar el límite de la columna de auditoría
            var auditUser = (createdBy?.Length > 30 ? createdBy[..30] : createdBy) ?? "system";

            transaction.Status = InventoryTransactionStatus.Draft;
            transaction.CreatedBy = auditUser;
            transaction.UpdatedBy = auditUser;
            transaction.CreatedByUserId = userId;
            transaction.CreateDate = DateTime.UtcNow;
            transaction.RecordDate = DateTime.UtcNow;
            transaction.Rowpointer = Guid.NewGuid();
            transaction.AffectsStock = false;

            db.InventoryTransactions.Add(transaction);
            await db.SaveChangesAsync();

            // Guardar líneas
            int lineNum = 1;
            foreach (var line in lines)
            {
                line.IdInventoryTransaction = transaction.Id;
                line.LineNumber = lineNum++;
                line.LineStatus = "Pending";
                line.CreatedBy = auditUser;
                line.UpdatedBy = auditUser;
                line.CreateDate = DateTime.UtcNow;
                line.RecordDate = DateTime.UtcNow;
                line.Rowpointer = Guid.NewGuid();
                db.InventoryTransactionLines.Add(line);
            }
            await db.SaveChangesAsync();

            _logger.LogInformation("Movimiento {Num} creado para compañía {CompanyId}", transaction.TransactionNumber, companyId);
            return transaction;
        }

        public async Task<InventoryTransaction> UpdateAsync(int companyId, InventoryTransaction transaction, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id)
                ?? throw new InvalidOperationException($"Movimiento {transaction.Id} no encontrado");

            if (existing.Status != InventoryTransactionStatus.Draft)
                throw new InvalidOperationException("Solo se pueden editar movimientos en estado Draft");

            existing.MovementType = transaction.MovementType;
            existing.IdWarehouseOrigin = transaction.IdWarehouseOrigin;
            existing.IdWarehouseDest = transaction.IdWarehouseDest;
            existing.IdDistributionRoute = transaction.IdDistributionRoute;
            existing.Reference = transaction.Reference;
            existing.Notes = transaction.Notes;
            existing.TransactionDate = transaction.TransactionDate;
            existing.ExpectedArrivalDate = transaction.ExpectedArrivalDate;
            existing.IsTransitTransfer = transaction.IsTransitTransfer;
            existing.SecuritySeal = transaction.SecuritySeal;
            existing.DepartureTime = transaction.DepartureTime;
            existing.ArrivalTime = transaction.ArrivalTime;
            existing.OdometerOut = transaction.OdometerOut;
            existing.UpdatedBy = (updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy) ?? "system";
            existing.RecordDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return existing;
        }

        public async Task SaveLinesAsync(int companyId, int transactionId, List<InventoryTransactionLine> lines, string updatedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new InvalidOperationException("Movimiento no encontrado");

            if (existing.Status != InventoryTransactionStatus.Draft)
                throw new InvalidOperationException("Solo se pueden editar líneas en estado Draft");

            var oldLines = await db.InventoryTransactionLines
                .Where(l => l.IdInventoryTransaction == transactionId)
                .ToListAsync();

            db.InventoryTransactionLines.RemoveRange(oldLines);
            await db.SaveChangesAsync();

            int lineNum = 1;
            var auditUser = updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy ?? "SYSTEM";
            foreach (var line in lines)
            {
                line.Id = 0;
                line.IdInventoryTransaction = transactionId;
                line.LineNumber = lineNum++;
                line.LineStatus = "Pending";
                line.CreatedBy = auditUser;
                line.UpdatedBy = auditUser;
                line.CreateDate = DateTime.UtcNow;
                line.RecordDate = DateTime.UtcNow;
                line.Rowpointer = Guid.NewGuid();
                db.InventoryTransactionLines.Add(line);
            }
            await db.SaveChangesAsync();
        }

        // ================================================================
        // FLUJO DE ESTADOS
        // ================================================================

        public async Task<InventoryTransaction> ConfirmAsync(int companyId, int transactionId, string updatedBy, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var txn = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new InvalidOperationException("Movimiento no encontrado");

            if (txn.Status != InventoryTransactionStatus.Draft)
                throw new InvalidOperationException("Solo se pueden confirmar movimientos en Draft");

            var lines = await db.InventoryTransactionLines
                .Where(l => l.IdInventoryTransaction == transactionId)
                .ToListAsync();

            if (!lines.Any())
                throw new InvalidOperationException("El movimiento no tiene líneas");

            var now = DateTime.UtcNow;
            var auditUser = (updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy) ?? "SYSTEM";

            // Determinar estado destino
            if (txn.IsTransitTransfer)
            {
                txn.Status = InventoryTransactionStatus.InTransit;

                // Aumentar qty_in_transit para la bodega origen saliente
                foreach (var line in lines)
                {
                    line.LineStatus = "InTransit";
                    line.QtyDispatched = line.QtyRequested;
                    line.RecordDate = now;
                    line.UpdatedBy = auditUser;

                    // Reducir stock en bodega origen
                    await AdjustExistenceAsync(db, txn.IdWarehouseOrigin, line.IdItem, line.ItemCode,
                        -line.QtyDispatched, 0, 0, line.UnitCost ?? 0, transactionId, now, auditUser);

                    // Aumentar in_transit en bodega transit (destino del header)
                    if (txn.IdWarehouseDest.HasValue)
                        await AdjustExistenceAsync(db, txn.IdWarehouseDest.Value, line.IdItem, line.ItemCode,
                            0, 0, line.QtyDispatched, line.UnitCost ?? 0, transactionId, now, auditUser);
                }
            }
            else
            {
                // Movimiento simple: confirmar = ya despachado (espera recepción o completa directo)
                txn.Status = InventoryTransactionStatus.Confirmed;

                foreach (var line in lines)
                {
                    line.LineStatus = "InTransit";
                    line.QtyDispatched = line.QtyRequested;
                    line.RecordDate = now;
                    line.UpdatedBy = auditUser;

                    // Reducir en bodega origen
                    await AdjustExistenceAsync(db, txn.IdWarehouseOrigin, line.IdItem, line.ItemCode,
                        -line.QtyDispatched, 0, 0, line.UnitCost ?? 0, transactionId, now, auditUser);
                }
            }

            txn.ConfirmedDate = now;
            txn.ConfirmedByUserId = userId;
            txn.UpdatedBy = auditUser;
            txn.RecordDate = now;

            await db.SaveChangesAsync();
            return txn;
        }

        public async Task<InventoryTransaction> ReceiveLinesAsync(
            int companyId,
            int transactionId,
            List<int> lineIds,
            int receivedByUserId,
            string receivedBy,
            string? arrivalTime = null,
            string? departureTime = null,
            decimal? odometerOut = null,
            string? nextDestSeal = null,
            int? nextWarehouseId = null,
            Dictionary<int, decimal>? lineQtys = null,
            string? signature = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var txn = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new InvalidOperationException("Movimiento no encontrado");

            if (txn.Status != InventoryTransactionStatus.InTransit &&
                txn.Status != InventoryTransactionStatus.PartiallyReceived &&
                txn.Status != InventoryTransactionStatus.Confirmed)
                throw new InvalidOperationException("El movimiento no está en tránsito o confirmado");

            // Validate next dest seal uniqueness (server-side guard)
            if (!string.IsNullOrWhiteSpace(nextDestSeal))
            {
                var sealTrimmed = nextDestSeal.Trim();
                // Must not match header seals of ANY transaction (no exclusion: new seal must be globally unique)
                var headerMatch = await db.InventoryTransactions
                    .AnyAsync(t => t.SecuritySeal == sealTrimmed);
                // Must not match any existing line dest seal across all transactions
                var lineMatch = await db.InventoryTransactionLines
                    .AnyAsync(l => l.DestSecuritySeal == sealTrimmed);
                if (headerMatch || lineMatch)
                    throw new InvalidOperationException($"El sello '{sealTrimmed}' ya está en uso. Debe ingresar un sello único.");
            }

            var allLines = await db.InventoryTransactionLines
                .Where(l => l.IdInventoryTransaction == transactionId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var auditUser = (receivedBy?.Length > 30 ? receivedBy[..30] : receivedBy) ?? "SYSTEM";
            var parsedArrival   = TimeOnly.TryParse(arrivalTime,   out var at) ? at : (TimeOnly?)null;
            var parsedDeparture = TimeOnly.TryParse(departureTime, out var dt) ? dt : (TimeOnly?)null;

            // Hora Llegada must be strictly before Hora Salida (server-side guard)
            if (parsedArrival.HasValue && parsedDeparture.HasValue && parsedArrival.Value >= parsedDeparture.Value)
                throw new InvalidOperationException(
                    $"La Hora de Llegada ({parsedArrival.Value:HH:mm}) debe ser menor a la Hora de Salida ({parsedDeparture.Value:HH:mm}).");

            foreach (var lineId in lineIds)
            {
                var line = allLines.FirstOrDefault(l => l.Id == lineId);
                if (line == null || line.LineStatus == "Received") continue;

                line.LineStatus = "Received";
                // Use explicit qty if provided, otherwise fall back to dispatched → requested
                line.QtyReceived = (lineQtys != null && lineQtys.TryGetValue(lineId, out var overrideQty) && overrideQty > 0)
                    ? overrideQty
                    : (line.QtyDispatched > 0 ? line.QtyDispatched : line.QtyRequested);
                line.ReceivedDate = now;
                line.ReceivedByUserId = receivedByUserId;
                line.RecordDate = now;
                line.UpdatedBy = auditUser;

                // Persistir hora de llegada y km de salida en la línea recibida
                if (parsedArrival.HasValue) line.ArrivalTime = parsedArrival;
                if (odometerOut.HasValue)   line.OdometerOut = odometerOut;
                if (!string.IsNullOrWhiteSpace(signature)) line.Signature = signature;

                // Destino final de la línea
                var destWarehouseId = line.IdWarehouseDestLine ?? txn.IdWarehouseDest ?? txn.IdWarehouseOrigin;

                if (txn.IsTransitTransfer && txn.IdWarehouseDest.HasValue)
                {
                    // Quitar de in_transit de la bodega tránsito
                    await AdjustExistenceAsync(db, txn.IdWarehouseDest.Value, line.IdItem, line.ItemCode,
                        0, 0, -line.QtyReceived, line.UnitCost ?? 0, transactionId, now, auditUser);
                }

                // Agregar a qty_on_hand en bodega destino final
                await AdjustExistenceAsync(db, destWarehouseId, line.IdItem, line.ItemCode,
                    line.QtyReceived, 0, 0, line.UnitCost ?? 0, transactionId, now, auditUser);
            }

            // Si se especificó un sello para la siguiente bodega, aplicarlo a sus líneas pendientes
            if (!string.IsNullOrWhiteSpace(nextDestSeal) && nextWarehouseId.HasValue)
            {
                var nextLines = allLines
                    .Where(l => l.IdWarehouseDestLine == nextWarehouseId.Value && l.LineStatus != "Received");
                foreach (var nl in nextLines)
                {
                    nl.DestSecuritySeal = nextDestSeal.Trim().Length > 50 ? nextDestSeal.Trim()[..50] : nextDestSeal.Trim();
                    nl.RecordDate = now;
                    nl.UpdatedBy = auditUser;
                }
            }

            // Verificar si todos están recibidos
            var allReceived = allLines.All(l => l.LineStatus == "Received");
            txn.Status = allReceived
                ? InventoryTransactionStatus.Completed
                : InventoryTransactionStatus.PartiallyReceived;

            if (allReceived)
            {
                txn.CompletedDate = now;
                txn.AffectsStock = true;
            }

            txn.UpdatedBy = auditUser;
            txn.RecordDate = now;

            await db.SaveChangesAsync();
            return txn;
        }

        public async Task<InventoryTransaction> CompleteAsync(int companyId, int transactionId, string updatedBy, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var txn = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new InvalidOperationException("Movimiento no encontrado");

            if (txn.Status != InventoryTransactionStatus.Confirmed)
                throw new InvalidOperationException("El movimiento debe estar Confirmado para completar");

            var lines = await db.InventoryTransactionLines
                .Where(l => l.IdInventoryTransaction == transactionId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var auditUser = (updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy) ?? "SYSTEM";

            foreach (var line in lines)
            {
                line.QtyReceived = line.QtyDispatched > 0 ? line.QtyDispatched : line.QtyRequested;
                line.LineStatus = "Received";
                line.ReceivedDate = now;
                line.ReceivedByUserId = userId;
                line.RecordDate = now;
                line.UpdatedBy = auditUser;

                var destWarehouseId = line.IdWarehouseDestLine ?? txn.IdWarehouseDest ?? txn.IdWarehouseOrigin;

                await AdjustExistenceAsync(db, destWarehouseId, line.IdItem, line.ItemCode,
                    line.QtyReceived, 0, 0, line.UnitCost ?? 0, transactionId, now, auditUser);
            }

            txn.Status = InventoryTransactionStatus.Completed;
            txn.CompletedDate = now;
            txn.AffectsStock = true;
            txn.UpdatedBy = auditUser;
            txn.RecordDate = now;

            await db.SaveChangesAsync();
            return txn;
        }

        public async Task<InventoryTransaction> CancelAsync(int companyId, int transactionId, string reason, string updatedBy, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);

            var txn = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transactionId)
                ?? throw new InvalidOperationException("Movimiento no encontrado");

            if (txn.Status == InventoryTransactionStatus.Completed || txn.Status == InventoryTransactionStatus.Cancelled)
                throw new InvalidOperationException("No se puede cancelar un movimiento Completado o ya Cancelado");

            var now = DateTime.UtcNow;
            var auditUser = (updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy) ?? "SYSTEM";

            // Revertir movimientos de existencias si ya estaba confirmado / en tránsito
            if (txn.Status == InventoryTransactionStatus.Confirmed ||
                txn.Status == InventoryTransactionStatus.InTransit ||
                txn.Status == InventoryTransactionStatus.PartiallyReceived)
            {
                var lines = await db.InventoryTransactionLines
                    .Where(l => l.IdInventoryTransaction == transactionId)
                    .ToListAsync();

                foreach (var line in lines)
                {
                    // Devolver qty_on_hand a bodega origen (ya se había reducido al confirmar)
                    await AdjustExistenceAsync(db, txn.IdWarehouseOrigin, line.IdItem, line.ItemCode,
                        line.QtyDispatched, 0, 0, line.UnitCost ?? 0, transactionId, now, auditUser);

                    // Quitar qty_in_transit de bodega tránsito si aplica
                    if (txn.IsTransitTransfer && txn.IdWarehouseDest.HasValue)
                        await AdjustExistenceAsync(db, txn.IdWarehouseDest.Value, line.IdItem, line.ItemCode,
                            0, 0, -line.QtyDispatched, line.UnitCost ?? 0, transactionId, now, auditUser);

                    line.LineStatus = "Cancelled";
                    line.RecordDate = now;
                    line.UpdatedBy = auditUser;
                }
            }

            txn.Status = InventoryTransactionStatus.Cancelled;
            txn.CancelledDate = now;
            txn.CancelledByUserId = userId;
            txn.CancelReason = reason;
            txn.UpdatedBy = auditUser;
            txn.RecordDate = now;

            await db.SaveChangesAsync();
            return txn;
        }

        // ================================================================
        // EXISTENCIAS
        // ================================================================

        public async Task<List<ExistenceWarehouse>> GetExistencesByItemAsync(int companyId, int itemId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.ExistenceWarehouses
                .Where(e => e.IdItem == itemId)
                .OrderBy(e => e.IdWarehouse)
                .ToListAsync();
        }

        public async Task<List<ExistenceWarehouse>> GetExistencesByWarehouseAsync(int companyId, int warehouseId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.ExistenceWarehouses
                .Where(e => e.IdWarehouse == warehouseId && e.QtyOnHand > 0)
                .OrderBy(e => e.ItemCode)
                .ToListAsync();
        }

        public async Task<ExistenceWarehouse?> GetExistenceAsync(int companyId, int itemId, int warehouseId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await db.ExistenceWarehouses
                .FirstOrDefaultAsync(e => e.IdItem == itemId && e.IdWarehouse == warehouseId);
        }

        // ================================================================
        // HELPERS PRIVADOS
        // ================================================================

        private static async Task AdjustExistenceAsync(
            CompanyDbContext db,
            int warehouseId,
            int itemId,
            string itemCode,
            decimal deltaOnHand,
            decimal deltaReserved,
            decimal deltaInTransit,
            decimal unitCost,
            int transactionId,
            DateTime now,
            string updatedBy)
        {
            var auditUser = (updatedBy?.Length > 30 ? updatedBy[..30] : updatedBy) ?? "system";
            var existence = await db.ExistenceWarehouses
                .FirstOrDefaultAsync(e => e.IdItem == itemId && e.IdWarehouse == warehouseId);

            if (existence == null)
            {
                existence = new ExistenceWarehouse
                {
                    IdItem = itemId,
                    ItemCode = itemCode,
                    IdWarehouse = warehouseId,
                    QtyOnHand = 0,
                    QtyReserved = 0,
                    QtyInTransit = 0,
                    QtyAvailable = 0,
                    AverageCost = unitCost,
                    LastCost = unitCost,
                    CreatedBy = auditUser,
                    UpdatedBy = auditUser,
                    CreateDate = now,
                    RecordDate = now,
                    Rowpointer = Guid.NewGuid()
                };
                db.ExistenceWarehouses.Add(existence);
            }

            existence.QtyOnHand = Math.Max(0, existence.QtyOnHand + deltaOnHand);
            existence.QtyReserved = Math.Max(0, existence.QtyReserved + deltaReserved);
            existence.QtyInTransit = Math.Max(0, existence.QtyInTransit + deltaInTransit);
            existence.QtyAvailable = Math.Max(0, existence.QtyOnHand - existence.QtyReserved);

            if (unitCost > 0)
            {
                existence.LastCost = unitCost;
                if (deltaOnHand > 0 && existence.QtyOnHand > 0)
                    existence.AverageCost = ((existence.AverageCost * (existence.QtyOnHand - deltaOnHand)) + (unitCost * deltaOnHand)) / existence.QtyOnHand;
            }

            existence.LastTransactionId = transactionId;
            existence.LastMovementDate = now;
            existence.UpdatedBy = auditUser;
            existence.RecordDate = now;
        }
    }
}
