// ================================================================================
// ARCHIVO: CMS.Data/Services/CompanyDbContextFactory.cs
// PROPÓSITO: Factory para crear DbContext dinámicos para cada compañía
// DESCRIPCIÓN: Permite crear conexiones a las bases de datos operacionales de cada compañía
//              usando los campos connection_string_development/production de admin.company
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-19
// ================================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CMS.Data.Services
{
    /// <summary>
    /// Factory para crear instancias de CompanyDbContext dinámicamente.
    /// IMPORTANTE: El connection string se obtiene de la tabla admin.company,
    /// NO del appsettings.json (el appsettings.json solo es para la BD central cms).
    /// </summary>
    public class CompanyDbContextFactory : ICompanyDbContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CompanyDbContextFactory> _logger;
        private readonly AppDbContext _centralDb;

        public CompanyDbContextFactory(
            IConfiguration configuration,
            ILogger<CompanyDbContextFactory> logger,
            AppDbContext centralDb)
        {
            _configuration = configuration;
            _logger = logger;
            _centralDb = centralDb;
        }

        /// <summary>
        /// Crea un DbContext para la base de datos de una compañía específica.
        /// El connection string se obtiene de admin.company.connection_string_development/production
        /// </summary>
        /// <param name="companyId">ID de la compañía</param>
        /// <returns>CompanyDbContext configurado para esa compañía</returns>
        public async Task<CompanyDbContext> CreateDbContextAsync(int companyId)
        {
            // Obtener información de la compañía desde la BD central
            var company = await _centralDb.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == companyId);

            if (company == null)
            {
                throw new InvalidOperationException($"Compañía con ID {companyId} no encontrada");
            }

            // Obtener el connection string correcto según el ambiente
            var connectionString = company.IS_PRODUCTION 
                ? company.CONNECTION_STRING_PRODUCTION 
                : company.CONNECTION_STRING_DEVELOPMENT;

            _logger.LogInformation(
                "🔗 Compañía {CompanyId} ({Schema}): IS_PRODUCTION={IsProduction}, ConnectionString={HasCS}",
                companyId, company.COMPANY_SCHEMA, company.IS_PRODUCTION, 
                !string.IsNullOrEmpty(connectionString) ? "Configurado" : "VACÍO");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning(
                    "⚠️ Compañía {CompanyId} ({Schema}) no tiene connection_string_{Env} configurado. Usando fallback basado en appsettings.",
                    companyId, company.COMPANY_SCHEMA, company.IS_PRODUCTION ? "production" : "development");

                // Fallback: construir connection string basado en el appsettings
                connectionString = BuildFallbackConnectionString(company.COMPANY_SCHEMA);
            }

            _logger.LogDebug(
                "Creando CompanyDbContext para compañía {CompanyId} ({Schema}), IsProduction: {IsProduction}",
                companyId, company.COMPANY_SCHEMA, company.IS_PRODUCTION);

            return CreateDbContextFromConnectionString(connectionString, company.COMPANY_SCHEMA);
        }

        /// <summary>
        /// Crea un DbContext para la base de datos de una compañía por su schema/código.
        /// NOTA: Este método usa fallback al appsettings. Preferir CreateDbContextAsync(int companyId)
        /// </summary>
        /// <param name="companySchema">Código/schema de la compañía (ej: "sinai", "eamr")</param>
        /// <returns>CompanyDbContext configurado para esa compañía</returns>
        public CompanyDbContext CreateDbContext(string companySchema)
        {
            // Construir connection string usando fallback (appsettings.json)
            var connectionString = BuildFallbackConnectionString(companySchema);

            return CreateDbContextFromConnectionString(connectionString, companySchema);
        }

        /// <summary>
        /// Crea un DbContext con un connection string específico.
        /// </summary>
        private CompanyDbContext CreateDbContextFromConnectionString(string connectionString, string companySchema)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            _logger.LogDebug("Creando CompanyDbContext para schema: {Schema}", companySchema);

            return new CompanyDbContext(optionsBuilder.Options, companySchema);
        }

        /// <summary>
        /// Construye el connection string fallback basado en appsettings.json.
        /// Se usa cuando la compañía no tiene connection_string configurado.
        /// </summary>
        private string BuildFallbackConnectionString(string companySchema)
        {
            // Determinar el ambiente
            var environment = _configuration["Environment"] ?? "Development";
            var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

            // Obtener el connection string base según el ambiente
            var connectionKey = isDevelopment ? "Development:DefaultConnection" : "Production:DefaultConnection";
            var baseConnectionString = _configuration[$"ConnectionStrings:{connectionKey}"];

            if (string.IsNullOrEmpty(baseConnectionString))
            {
                // Fallback al DefaultConnection simple
                baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrEmpty(baseConnectionString))
            {
                throw new InvalidOperationException("No se encontró connection string en appsettings.json");
            }

            // Parsear y modificar el connection string
            var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);

            // Cambiar el nombre de la base de datos al código de la compañía
            builder.Database = companySchema;

            _logger.LogDebug("Fallback connection string para {Schema}: Host={Host}, Database={Database}", 
                companySchema, builder.Host, builder.Database);

            return builder.ConnectionString;
        }

        /// <summary>
        /// Verifica si la base de datos de una compañía existe.
        /// </summary>
        public async Task<bool> DatabaseExistsAsync(string companySchema)
        {
            try
            {
                var connectionString = BuildFallbackConnectionString(companySchema);
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (NpgsqlException ex) when (ex.SqlState == "3D000") // Database does not exist
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando si existe la BD {Schema}", companySchema);
                return false;
            }
        }

        /// <summary>
        /// Crea la base de datos y schema para una nueva compañía.
        /// </summary>
        public async Task<bool> CreateCompanyDatabaseAsync(string companySchema)
        {
            try
            {
                // Obtener connection string base según ambiente
                var baseConnectionString = BuildFallbackConnectionString("postgres");
                var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
                builder.Database = "postgres"; // Conectar a la BD por defecto para crear la nueva

                await using var connection = new NpgsqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                // Crear la base de datos
                var createDbSql = $"CREATE DATABASE \"{companySchema}\" WITH OWNER = cmssystem ENCODING = 'UTF8'";
                await using (var cmd = new NpgsqlCommand(createDbSql, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("✅ Base de datos '{Schema}' creada exitosamente", companySchema);

                // Conectar a la nueva BD y crear el schema
                builder.Database = companySchema;
                await using var newDbConnection = new NpgsqlConnection(builder.ConnectionString);
                await newDbConnection.OpenAsync();

                var createSchemaSql = $"CREATE SCHEMA IF NOT EXISTS \"{companySchema}\"";
                await using (var cmd = new NpgsqlCommand(createSchemaSql, newDbConnection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("✅ Schema '{Schema}' creado exitosamente", companySchema);

                // Aplicar migraciones (crear tablas)
                using var dbContext = CreateDbContext(companySchema);
                await dbContext.Database.EnsureCreatedAsync();

                _logger.LogInformation("✅ Tablas creadas en BD '{Schema}'", companySchema);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creando BD para compañía {Schema}", companySchema);
                return false;
            }
        }
    }

    /// <summary>
    /// Interface para el factory de CompanyDbContext
    /// </summary>
    public interface ICompanyDbContextFactory
    {
        Task<CompanyDbContext> CreateDbContextAsync(int companyId);
        CompanyDbContext CreateDbContext(string companySchema);
        Task<bool> DatabaseExistsAsync(string companySchema);
        Task<bool> CreateCompanyDatabaseAsync(string companySchema);
    }
}
