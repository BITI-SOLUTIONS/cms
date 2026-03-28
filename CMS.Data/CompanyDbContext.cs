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
        }
    }
}
