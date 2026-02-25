// ================================================================================
// ARCHIVO: CMS.Data/CompanyDbContext.cs
// PROPÓSITO: DbContext para acceder a la base de datos operacional de una compañía
// DESCRIPCIÓN: Cada compañía tiene su propia BD con el mismo nombre que company_schema
//              Este contexto permite acceder a datos operacionales como items, inventory, etc.
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
                entity.HasIndex(e => e.Category);
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
        }
    }
}
