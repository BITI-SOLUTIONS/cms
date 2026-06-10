// ================================================================================
// ARCHIVO: CMS.Data/CompanyDbContext.cs
// PROPÓSITO: DbContext para acceder a la base de datos operacional de una compañía
// DESCRIPCIÓN: Cada compañía tiene su propia BD con el mismo nombre que company_schema
//              Este contexto permite acceder a datos operacionales como items, inventory, etc.
//              NOTA: UnitOfMeasure está en la BD central (admin.unit_of_measure)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-19
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    /// <summary>
    /// DbContext para la base de datos operacional de una compañía específica.
    /// Cada compañía tiene su propia base de datos con el nombre igual a COMPANY_SCHEMA.
    /// </summary>
    public class CompanyDbContext : DbContext
    {
        private readonly string _schema;

        public CompanyDbContext(DbContextOptions<CompanyDbContext> options, string schema) 
            : base(options)
        {
            _schema = schema;
        }

        // ===== TABLAS OPERACIONALES =====

        /// <summary>
        /// Artículos/Productos del inventario
        /// </summary>
        public DbSet<Item> Items { get; set; } = null!;

        /// <summary>
        /// Historial de impresiones de etiquetas
        /// </summary>
        public DbSet<LabelPrintHistory> LabelPrintHistory { get; set; } = null!;

        /// <summary>
        /// Clasificaciones de artículos (una sola tabla con classification_group 1-6)
        /// </summary>
        public DbSet<Classification> Classifications { get; set; } = null!;

        // ===== FILE MANAGEMENT =====

        /// <summary>
        /// Carpetas de archivos
        /// </summary>
        public DbSet<FileFolder> FileFolders { get; set; } = null!;

        /// <summary>
        /// Archivos/Documentos
        /// </summary>
        public DbSet<FileDocument> Files { get; set; } = null!;

        /// <summary>
        /// Versiones de archivos
        /// </summary>
        public DbSet<FileVersion> FileVersions { get; set; } = null!;

        /// <summary>
        /// Comentarios en archivos
        /// </summary>
        public DbSet<FileComment> FileComments { get; set; } = null!;

        /// <summary>
        /// Etiquetas de archivos
        /// </summary>
        public DbSet<FileTag> FileTags { get; set; } = null!;

        // ===== WAREHOUSE MANAGEMENT =====

        /// <summary>
        /// Bodegas físicas y lógicas (WMS)
        /// </summary>
        public DbSet<Warehouse> Warehouses { get; set; } = null!;

        /// <summary>
        /// Traslados de stock entre bodegas
        /// </summary>
        public DbSet<StockTransfer> StockTransfers { get; set; } = null!;

        /// <summary>
        /// Líneas de artículos de un traslado de stock
        /// </summary>
        public DbSet<StockTransferLine> StockTransferLines { get; set; } = null!;

        // ===== LOCALIZATION =====

        /// <summary>
        /// Tipos de localización (Bodega, Empleado, Cliente, Proveedor, etc.)
        /// </summary>
        public DbSet<LocationType> LocationTypes { get; set; } = null!;

        /// <summary>
        /// Localizaciones físicas centralizadas (dirección, GPS, contacto)
        /// </summary>
        public DbSet<Location> Locations { get; set; } = null!;

        /// <summary>
        /// Rutas de distribución
        /// </summary>
        public DbSet<DistributionRoute> DistributionRoutes { get; set; } = null!;

        /// <summary>
        /// Paradas de rutas de distribución
        /// </summary>
        public DbSet<DistributionRouteStop> DistributionRouteStops { get; set; } = null!;

        // ===== INVENTORY MOVEMENTS =====

        /// <summary>
        /// Movimientos de inventario (traslados, ajustes, entradas, salidas)
        /// </summary>
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;

        /// <summary>
        /// Líneas de movimientos de inventario
        /// </summary>
        public DbSet<InventoryTransactionLine> InventoryTransactionLines { get; set; } = null!;

        /// <summary>
        /// Saldo de existencias por artículo y bodega
        /// </summary>
        public DbSet<ExistenceWarehouse> ExistenceWarehouses { get; set; } = null!;

        // ===== FLEET MANAGEMENT =====

        /// <summary>
        /// Unidades de transporte de la flota (camiones, carros, motos, etc.)
        /// </summary>
        public DbSet<TransportUnit> TransportUnits { get; set; } = null!;

        /// <summary>
        /// Historial de mantenimiento de unidades de transporte
        /// </summary>
        public DbSet<TransportUnitMaintenance> TransportUnitMaintenances { get; set; } = null!;

        /// <summary>
        /// Conductores de la flota de la compañía
        /// </summary>
        public DbSet<Driver> Drivers { get; set; } = null!;

        /// <summary>
        /// Aseguradoras registradas por la compañía
        /// </summary>
        public DbSet<Insurer> Insurers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar el esquema dinámico para todas las entidades
            modelBuilder.HasDefaultSchema(_schema);

            // Configurar la tabla Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("item", _schema);

                // Índices
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Barcode);
                entity.HasIndex(e => e.IdClassification1);
                entity.HasIndex(e => e.IdClassification2);
                entity.HasIndex(e => e.IdUnitOfMeasure);
                entity.HasIndex(e => e.IsActive);
            });

            // Configurar la tabla LabelPrintHistory
            modelBuilder.Entity<LabelPrintHistory>(entity =>
            {
                entity.ToTable("label_print_history", _schema);

                // Índices
                entity.HasIndex(e => e.IdItem);
                entity.HasIndex(e => e.ItemCode);
                entity.HasIndex(e => e.PrintDate);
                entity.HasIndex(e => e.PrintedBy);
            });

            // Configurar la tabla Classification (única tabla con classification_group)
            modelBuilder.Entity<Classification>(entity =>
            {
                entity.ToTable("classification", _schema);

                // Índice único compuesto: code + classification_group
                entity.HasIndex(e => new { e.Code, e.ClassificationGroup }).IsUnique();
                entity.HasIndex(e => e.ClassificationGroup);
                entity.HasIndex(e => e.IsActive);
            });

            // ===== FILE MANAGEMENT TABLES =====

            // Configurar la tabla FileFolder
            modelBuilder.Entity<FileFolder>(entity =>
            {
                entity.ToTable("file_folder", _schema);
                entity.HasKey(e => e.IdFileFolder);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.CategoryCode);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar la tabla File (FileDocument)
            modelBuilder.Entity<FileDocument>(entity =>
            {
                entity.ToTable("file", _schema);
                entity.HasKey(e => e.IdFile);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Uuid).IsUnique();
                entity.HasIndex(e => e.IdFileFolder);
                entity.HasIndex(e => e.CategoryCode);
                entity.HasIndex(e => e.FileExtension);
                entity.HasIndex(e => e.FileHash);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DeletedAt);
                entity.HasIndex(e => e.IsLocked);
                entity.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId });

                entity.HasOne(e => e.Folder)
                    .WithMany(e => e.Files)
                    .HasForeignKey(e => e.IdFileFolder)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar la tabla FileVersion
            modelBuilder.Entity<FileVersion>(entity =>
            {
                entity.ToTable("file_version", _schema);
                entity.HasKey(e => e.IdFileVersion);
                entity.HasIndex(e => e.IdFile);
                entity.HasIndex(e => new { e.IdFile, e.VersionNumber }).IsUnique();
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.File)
                    .WithMany(e => e.Versions)
                    .HasForeignKey(e => e.IdFile)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configurar la tabla FileComment
            modelBuilder.Entity<FileComment>(entity =>
            {
                entity.ToTable("file_comment", _schema);
                entity.HasKey(e => e.IdFileComment);
                entity.HasIndex(e => e.IdFile);
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.File)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.IdFile)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar la tabla FileTag
            modelBuilder.Entity<FileTag>(entity =>
            {
                entity.ToTable("file_tag", _schema);
                entity.HasKey(e => e.IdFileTag);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ===== WAREHOUSE =====

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("warehouse", _schema);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.WarehouseType);
                entity.HasIndex(e => e.WarehouseLevel);
                entity.HasIndex(e => e.IdParentWarehouse);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsDefault);
                entity.HasIndex(e => e.IdLocation);
            });

            modelBuilder.Entity<StockTransfer>(entity =>
            {
                entity.ToTable("stock_transfer", _schema);
                entity.HasIndex(e => e.TransferNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TransferDate);
                entity.HasIndex(e => e.IdWarehouseOrigin);
                entity.HasIndex(e => e.IdWarehouseDest);
                entity.HasIndex(e => e.RequestedBy);
            });

            modelBuilder.Entity<StockTransferLine>(entity =>
            {
                entity.ToTable("stock_transfer_line", _schema);
                entity.HasIndex(e => e.IdStockTransfer);
                entity.HasIndex(e => e.IdItem);
                entity.HasIndex(e => new { e.IdStockTransfer, e.LineNumber }).IsUnique();
            });

            // ===== LOCALIZATION =====

            modelBuilder.Entity<LocationType>(entity =>
            {
                entity.ToTable("location_type", _schema);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SortOrder);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("location", _schema);
                entity.HasIndex(e => e.IdLocationType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IdCountry);
                entity.HasIndex(e => e.IdProvince);
                entity.HasIndex(e => e.IdCanton);
                entity.HasIndex(e => e.IdDistrict);
                entity.HasIndex(e => e.IdNeighborhood);

                entity.HasOne(e => e.LocationType)
                    .WithMany()
                    .HasForeignKey(e => e.IdLocationType)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar la tabla FileFileTag (relación muchos a muchos)
            modelBuilder.Entity<FileFileTag>(entity =>
            {
                entity.ToTable("file_file_tag", _schema);
                entity.HasKey(e => new { e.IdFile, e.IdFileTag });

                entity.HasOne(e => e.File)
                    .WithMany()
                    .HasForeignKey(e => e.IdFile)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany()
                    .HasForeignKey(e => e.IdFileTag)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== DISTRIBUTION ROUTES =====

            modelBuilder.Entity<DistributionRoute>(entity =>
            {
                entity.ToTable("distribution_route", _schema);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Frequency);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IdOriginWarehouse);
            });

            modelBuilder.Entity<DistributionRouteStop>(entity =>
            {
                entity.ToTable("distribution_route_stop", _schema);
                entity.HasIndex(e => e.IdRoute);
                entity.HasIndex(e => e.StopOrder);
                entity.HasIndex(e => new { e.IdRoute, e.StopOrder }).IsUnique();
            });

            // ===== INVENTORY MOVEMENTS =====

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("inventory_transaction", _schema);
                entity.HasIndex(e => e.TransactionNumber).IsUnique();
                entity.HasIndex(e => e.MovementType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.IdWarehouseOrigin);
                entity.HasIndex(e => e.IdWarehouseDest);
                entity.HasIndex(e => e.IsTransitTransfer);
                entity.HasIndex(e => e.SecuritySeal).IsUnique().HasFilter("security_seal IS NOT NULL");
            });

            modelBuilder.Entity<InventoryTransactionLine>(entity =>
            {
                entity.ToTable("inventory_transaction_line", _schema);
                entity.HasIndex(e => e.IdInventoryTransaction);
                entity.HasIndex(e => e.IdItem);
                entity.HasIndex(e => e.LineStatus);
                entity.HasIndex(e => new { e.IdInventoryTransaction, e.LineNumber }).IsUnique();
            });

            modelBuilder.Entity<ExistenceWarehouse>(entity =>
            {
                entity.ToTable("existence_warehouse", _schema);
                entity.HasIndex(e => e.IdItem);
                entity.HasIndex(e => e.IdWarehouse);
                entity.HasIndex(e => new { e.IdItem, e.IdWarehouse }).IsUnique();
            });

            // ===== FLEET MANAGEMENT =====

            modelBuilder.Entity<TransportUnit>(entity =>
            {
                entity.ToTable("transport_unit", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_transport_unit");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.PlateNumber).IsUnique();
                entity.HasIndex(e => e.UnitType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IdWarehouse);
                // VIN y Número de motor únicos por compañía
                entity.HasIndex(e => e.VinNumber).IsUnique().HasFilter($"\"{_schema}\".transport_unit.vin_number IS NOT NULL");
                entity.HasIndex(e => e.EngineNumber).IsUnique().HasFilter($"\"{_schema}\".transport_unit.engine_number IS NOT NULL");
                entity.HasIndex(e => e.IdDriver);
                entity.HasIndex(e => e.IdInsurer);
            });

            modelBuilder.Entity<TransportUnitMaintenance>(entity =>
            {
                entity.ToTable("transport_unit_maintenance", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_transport_unit_maintenance");
                entity.HasIndex(e => e.IdTransportUnit);
                entity.HasIndex(e => e.MaintenanceType);
                entity.HasIndex(e => e.MaintenanceDate);

                entity.HasOne(e => e.TransportUnit)
                    .WithMany(e => e.MaintenanceRecords)
                    .HasForeignKey(e => e.IdTransportUnit)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Driver>(entity =>
            {
                entity.ToTable("driver", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_driver");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.IdNumber).IsUnique();
                entity.HasIndex(e => e.IdSystemUser);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LicenseNumber);
            });

            modelBuilder.Entity<Insurer>(entity =>
            {
                entity.ToTable("insurer", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_insurer");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });
        }
    }
}
