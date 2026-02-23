// ================================================================================
// ARCHIVO: CMS.Data/Services/CompanyConfigService.cs
// PROPÓSITO: Servicio para cargar configuración de compañía desde BD
// ================================================================================

using CMS.Data;
using CMS.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio para obtener la configuración de una compañía desde [ADMIN].[COMPANY]
    /// Se utiliza en el bootstrap de Program.cs
    /// </summary>
    public class CompanyConfigService
    {
        private readonly AppDbContext _context;

        public CompanyConfigService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene la configuración de una compañía por su COMPANY_SCHEMA
        /// </summary>
        public async Task<Company> GetCompanyConfigAsync(string companySchema)
        {
            if (string.IsNullOrWhiteSpace(companySchema))
                throw new ArgumentException("companySchema no puede estar vacío", nameof(companySchema));

            try
            {
                var company = await _context.Companies
                    .AsNoTracking()
                    .Where(c => c.COMPANY_SCHEMA == companySchema && c.IS_ACTIVE)
                    .FirstOrDefaultAsync();

                if (company == null)
                    throw new InvalidOperationException(
                        $"No se encontró compañía activa con COMPANY_SCHEMA = '{companySchema}'. " +
                        $"Verifica que: 1) La tabla [ADMIN].[COMPANY] existe, " +
                        $"2) Existe un registro con COMPANY_SCHEMA = '{companySchema}', " +
                        $"3) IS_ACTIVE = 1 (true)");

                return company;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener configuración de compañía '{companySchema}' desde [ADMIN].[COMPANY]",
                    ex);
            }
        }

        /// <summary>
        /// Obtiene todas las compañías activas
        /// </summary>
        public async Task<List<Company>> GetAllActiveCompaniesAsync()
        {
            try
            {
                return await _context.Companies
                    .AsNoTracking()
                    .Where(c => c.IS_ACTIVE)
                    .OrderBy(c => c.COMPANY_NAME)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error al obtener todas las compañías activas",
                    ex);
            }
        }
    }
}