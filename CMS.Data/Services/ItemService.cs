// ================================================================================
// ARCHIVO: CMS.Data/Services/ItemService.cs
// PROPÓSITO: Servicio para gestionar artículos en la BD de cada compañía
// DESCRIPCIÓN: Operaciones CRUD para la tabla item en las BD operacionales
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-19
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio para gestionar artículos en la base de datos de cada compañía.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            ICompanyDbContextFactory dbContextFactory,
            ILogger<ItemService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los artículos de una compañía con paginación.
        /// </summary>
        public async Task<(List<Item> Items, int TotalCount)> GetItemsAsync(
            int companyId,
            string? search = null,
            string? category = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.Items.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(i => 
                    i.Code.ToLower().Contains(search) ||
                    i.Name.ToLower().Contains(search) ||
                    (i.Barcode != null && i.Barcode.ToLower().Contains(search)) ||
                    (i.Description != null && i.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Category == category);
            }

            if (isActive.HasValue)
            {
                query = query.Where(i => i.IsActive == isActive.Value);
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Paginación
            var items = await query
                .OrderBy(i => i.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un artículo por ID.
        /// </summary>
        public async Task<Item?> GetItemByIdAsync(int companyId, int itemId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await dbContext.Items.FindAsync(itemId);
        }

        /// <summary>
        /// Obtiene un artículo por código.
        /// </summary>
        public async Task<Item?> GetItemByCodeAsync(int companyId, string code)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await dbContext.Items
                .FirstOrDefaultAsync(i => i.Code.ToLower() == code.ToLower());
        }

        /// <summary>
        /// Crea un nuevo artículo.
        /// </summary>
        public async Task<Item> CreateItemAsync(int companyId, Item item, string? createdBy = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            item.CreateDate = DateTime.UtcNow;
            item.RecordDate = DateTime.UtcNow;
            item.CreatedBy = createdBy ?? "SYSTEM";
            item.UpdatedBy = createdBy ?? "SYSTEM";
            item.RowPointer = Guid.NewGuid();

            dbContext.Items.Add(item);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Artículo {Code} creado en compañía {CompanyId}", item.Code, companyId);

            return item;
        }

        /// <summary>
        /// Actualiza un artículo existente.
        /// </summary>
        public async Task<Item?> UpdateItemAsync(int companyId, Item item, string? updatedBy = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await dbContext.Items.FindAsync(item.Id);
            if (existing == null)
            {
                return null;
            }

            // Actualizar propiedades
            existing.Code = item.Code;
            existing.Name = item.Name;
            existing.Description = item.Description;

            // Label Item fields
            existing.LabelItem = item.LabelItem;
            existing.LabelPrice = item.LabelPrice;
            existing.LabelItemBarcode = item.LabelItemBarcode;
            existing.IsLabelItem = item.IsLabelItem;

            existing.Barcode = item.Barcode;
            existing.Category = item.Category;
            existing.Subcategory = item.Subcategory;
            existing.Brand = item.Brand;
            existing.UnitOfMeasure = item.UnitOfMeasure;
            existing.CostPrice = item.CostPrice;
            existing.SalePrice = item.SalePrice;
            existing.TaxRate = item.TaxRate;
            existing.MinStock = item.MinStock;
            existing.MaxStock = item.MaxStock;
            existing.CurrentStock = item.CurrentStock;
            existing.ImageUrl = item.ImageUrl;
            existing.IsActive = item.IsActive;
            existing.IsSellable = item.IsSellable;
            existing.IsPurchasable = item.IsPurchasable;
            existing.TrackLots = item.TrackLots;
            existing.TrackSerialNumbers = item.TrackSerialNumbers;

            // Auditoría - El trigger de la BD actualiza record_date y updated_by
            existing.RecordDate = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy ?? "SYSTEM";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Artículo {Code} actualizado en compañía {CompanyId}", item.Code, companyId);

            return existing;
        }

        /// <summary>
        /// Elimina (desactiva) un artículo.
        /// </summary>
        public async Task<bool> DeleteItemAsync(int companyId, int itemId, string? deletedBy = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var item = await dbContext.Items.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

            // Soft delete
            item.IsActive = false;
            item.RecordDate = DateTime.UtcNow;
            item.UpdatedBy = deletedBy ?? "SYSTEM";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Artículo {Code} desactivado en compañía {CompanyId}", item.Code, companyId);

            return true;
        }

        /// <summary>
        /// Obtiene las categorías distintas.
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync(int companyId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            return await dbContext.Items
                .Where(i => i.Category != null && i.Category != "")
                .Select(i => i.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene artículos de etiqueta (is_label_item = true) con paginación.
        /// </summary>
        public async Task<(List<Item> Items, int TotalCount)> GetLabelItemsAsync(
            int companyId,
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.Items.Where(i => i.IsLabelItem && i.IsActive);

            // Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(i =>
                    i.Code.ToLower().Contains(search) ||
                    i.Name.ToLower().Contains(search) ||
                    (i.LabelItem != null && i.LabelItem.ToLower().Contains(search)) ||
                    (i.Barcode != null && i.Barcode.ToLower().Contains(search)));
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Paginación
            var items = await query
                .OrderBy(i => i.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Actualiza solo los campos de etiqueta de un artículo.
        /// </summary>
        public async Task<Item?> UpdateLabelInfoAsync(
            int companyId,
            int itemId,
            string? labelItem,
            decimal labelPrice,
            string? labelItemBarcode,
            bool printLabelName,
            bool printLabelPrice,
            bool printLabelBarcode,
            decimal labelWidthCm,
            decimal labelHeightCm,
            string labelOrientation,
            bool printLabelBorder,
            string labelBorderColor,
            string labelNameColor,
            string labelPriceColor,
            string labelBarcodeColor,
            decimal labelFontSize,
            string labelFontFamily,
            int labelPriceDecimals,
            string labelThousandSeparator,
            string labelCurrencySymbol,
            bool printCurrencySymbol,
            string? updatedBy = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var item = await dbContext.Items.FindAsync(itemId);
            if (item == null)
            {
                return null;
            }

            // Actualizar campos de etiqueta
            item.LabelItem = labelItem;
            item.LabelPrice = labelPrice;
            item.LabelItemBarcode = labelItemBarcode;
            item.PrintLabelName = printLabelName;
            item.PrintLabelPrice = printLabelPrice;
            item.PrintLabelBarcode = printLabelBarcode;

            // Actualizar campos de tamaño y formato
            item.LabelWidthCm = labelWidthCm;
            item.LabelHeightCm = labelHeightCm;
            item.LabelOrientation = labelOrientation;
            item.PrintLabelBorder = printLabelBorder;
            item.LabelBorderColor = labelBorderColor;
            item.LabelNameColor = labelNameColor;
            item.LabelPriceColor = labelPriceColor;
            item.LabelBarcodeColor = labelBarcodeColor;

            // Actualizar campos de fuente y formato de precio
            item.LabelFontSize = labelFontSize;
            item.LabelFontFamily = labelFontFamily;
            item.LabelPriceDecimals = labelPriceDecimals;
            item.LabelThousandSeparator = labelThousandSeparator;
            item.LabelCurrencySymbol = labelCurrencySymbol;
            item.PrintCurrencySymbol = printCurrencySymbol;

            item.RecordDate = DateTime.UtcNow;
            item.UpdatedBy = updatedBy ?? "SYSTEM";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Etiqueta del artículo {Code} actualizada en compañía {CompanyId}", item.Code, companyId);

            return item;
        }

        /// <summary>
        /// Registra una impresión de etiqueta en el historial.
        /// </summary>
        public async Task<LabelPrintHistory> RecordLabelPrintAsync(
            int companyId,
            int itemId,
            string itemCode,
            string itemName,
            string? labelItem,
            decimal labelPrice,
            string? labelItemBarcode,
            bool printLabelName,
            bool printLabelPrice,
            bool printLabelBarcode,
            bool printLabelBorder,
            bool printCurrencySymbol,
            decimal labelWidthCm,
            decimal labelHeightCm,
            string labelOrientation,
            string labelBorderColor,
            string labelNameColor,
            string labelPriceColor,
            string labelBarcodeColor,
            decimal labelFontSize,
            string labelFontFamily,
            int labelPriceDecimals,
            string labelThousandSeparator,
            string labelCurrencySymbol,
            string? formattedPrice,
            int quantityPrinted,
            string? printedBy,
            string? printerName = null,
            string? printNotes = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var printHistory = new LabelPrintHistory
            {
                IdItem = itemId,
                ItemCode = itemCode,
                ItemName = itemName,
                LabelItem = labelItem,
                LabelPrice = labelPrice,
                LabelItemBarcode = labelItemBarcode,
                PrintLabelName = printLabelName,
                PrintLabelPrice = printLabelPrice,
                PrintLabelBarcode = printLabelBarcode,
                PrintLabelBorder = printLabelBorder,
                PrintCurrencySymbol = printCurrencySymbol,
                LabelWidthCm = labelWidthCm,
                LabelHeightCm = labelHeightCm,
                LabelOrientation = labelOrientation,
                LabelBorderColor = labelBorderColor,
                LabelNameColor = labelNameColor,
                LabelPriceColor = labelPriceColor,
                LabelBarcodeColor = labelBarcodeColor,
                LabelFontSize = labelFontSize,
                LabelFontFamily = labelFontFamily,
                LabelPriceDecimals = labelPriceDecimals,
                LabelThousandSeparator = labelThousandSeparator,
                LabelCurrencySymbol = labelCurrencySymbol,
                FormattedPrice = formattedPrice,
                QuantityPrinted = quantityPrinted,
                PrintDate = DateTime.UtcNow,
                PrintedBy = printedBy,
                PrinterName = printerName,
                PrintNotes = printNotes,
                CreateDate = DateTime.UtcNow,
                RecordDate = DateTime.UtcNow,
                CreatedBy = printedBy ?? "SYSTEM"
            };

            dbContext.LabelPrintHistory.Add(printHistory);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Impresión de etiqueta registrada: Artículo {ItemCode}, Cantidad {Quantity}, Usuario {User}", 
                itemCode, quantityPrinted, printedBy);

            return printHistory;
        }

        /// <summary>
        /// Obtiene el historial de impresiones de un artículo.
        /// </summary>
        public async Task<List<LabelPrintHistory>> GetLabelPrintHistoryAsync(
            int companyId,
            int? itemId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? printedBy = null,
            int page = 1,
            int pageSize = 50)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.LabelPrintHistory.AsQueryable();

            if (itemId.HasValue)
            {
                query = query.Where(h => h.IdItem == itemId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.PrintDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.PrintDate <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(printedBy))
            {
                query = query.Where(h => h.PrintedBy != null && h.PrintedBy.ToLower().Contains(printedBy.ToLower()));
            }

            return await query
                .OrderByDescending(h => h.PrintDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Interface para el servicio de Items
    /// </summary>
    public interface IItemService
    {
        Task<(List<Item> Items, int TotalCount)> GetItemsAsync(
            int companyId, 
            string? search = null, 
            string? category = null, 
            bool? isActive = null, 
            int page = 1, 
            int pageSize = 20);

        Task<(List<Item> Items, int TotalCount)> GetLabelItemsAsync(
            int companyId,
            string? search = null,
            int page = 1,
            int pageSize = 20);

        Task<Item?> GetItemByIdAsync(int companyId, int itemId);
        Task<Item?> GetItemByCodeAsync(int companyId, string code);
        Task<Item> CreateItemAsync(int companyId, Item item, string? createdBy = null);
        Task<Item?> UpdateItemAsync(int companyId, Item item, string? updatedBy = null);
        Task<Item?> UpdateLabelInfoAsync(int companyId, int itemId, string? labelItem, decimal labelPrice, string? labelItemBarcode, bool printLabelName, bool printLabelPrice, bool printLabelBarcode, decimal labelWidthCm, decimal labelHeightCm, string labelOrientation, bool printLabelBorder, string labelBorderColor, string labelNameColor, string labelPriceColor, string labelBarcodeColor, decimal labelFontSize, string labelFontFamily, int labelPriceDecimals, string labelThousandSeparator, string labelCurrencySymbol, bool printCurrencySymbol, string? updatedBy = null);
        Task<bool> DeleteItemAsync(int companyId, int itemId, string? deletedBy = null);
        Task<List<string>> GetCategoriesAsync(int companyId);

        // Historial de impresiones
        Task<LabelPrintHistory> RecordLabelPrintAsync(
            int companyId, int itemId, string itemCode, string itemName,
            string? labelItem, decimal labelPrice, string? labelItemBarcode,
            bool printLabelName, bool printLabelPrice, bool printLabelBarcode,
            bool printLabelBorder, bool printCurrencySymbol,
            decimal labelWidthCm, decimal labelHeightCm, string labelOrientation,
            string labelBorderColor, string labelNameColor, string labelPriceColor, string labelBarcodeColor,
            decimal labelFontSize, string labelFontFamily, int labelPriceDecimals,
            string labelThousandSeparator, string labelCurrencySymbol,
            string? formattedPrice, int quantityPrinted, string? printedBy,
            string? printerName = null, string? printNotes = null);

        Task<List<LabelPrintHistory>> GetLabelPrintHistoryAsync(
            int companyId, int? itemId = null, DateTime? fromDate = null, DateTime? toDate = null,
            string? printedBy = null, int page = 1, int pageSize = 50);
    }
}
