// ================================================================================
// ARCHIVO: CMS.Data/Services/CompanyConsecutiveService.cs
// PROPÓSITO: Servicio para gestionar los consecutivos globales por compañía
// DESCRIPCIÓN: Este servicio maneja operaciones CRUD sobre la tabla 
//              admin.company_consecutive en la BD central (cms)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using CMS.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services;

/// <summary>
/// Servicio para gestionar consecutivos globales de compañías
/// </summary>
public interface ICompanyConsecutiveService
{
    /// <summary>
    /// Obtiene el registro de consecutivos de una compañía
    /// </summary>
    Task<CompanyConsecutive?> GetByCompanyIdAsync(int companyId);

    /// <summary>
    /// Obtiene el container_number actual de una compañía (retorna string vacío si no existe)
    /// </summary>
    Task<string> GetContainerNumberAsync(int companyId);

    /// <summary>
    /// Actualiza el container_number de una compañía
    /// </summary>
    Task<bool> UpdateContainerNumberAsync(int companyId, string containerNumber, string updatedBy);

    /// <summary>
    /// Crea un registro de consecutivos para una nueva compañía
    /// </summary>
    Task<CompanyConsecutive> CreateForCompanyAsync(int companyId, string createdBy);

    /// <summary>
    /// Obtiene o crea el registro de consecutivos para una compañía
    /// </summary>
    Task<CompanyConsecutive> GetOrCreateAsync(int companyId, string createdBy);
}

/// <summary>
/// Implementación del servicio de consecutivos de compañía
/// </summary>
public class CompanyConsecutiveService : ICompanyConsecutiveService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CompanyConsecutiveService> _logger;

    public CompanyConsecutiveService(
        AppDbContext dbContext,
        ILogger<CompanyConsecutiveService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CompanyConsecutive?> GetByCompanyIdAsync(int companyId)
    {
        return await _dbContext.CompanyConsecutives
            .FirstOrDefaultAsync(cc => cc.CompanyId == companyId);
    }

    /// <inheritdoc />
    public async Task<string> GetContainerNumberAsync(int companyId)
    {
        var containerNumber = await _dbContext.CompanyConsecutives
            .Where(cc => cc.CompanyId == companyId)
            .Select(cc => cc.ContainerNumber)
            .FirstOrDefaultAsync();

        // Retornar string vacío si no existe registro
        var result = containerNumber ?? string.Empty;

        _logger.LogDebug("Obtenido container_number '{ContainerNumber}' para compañía {CompanyId}",
            result, companyId);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateContainerNumberAsync(int companyId, string containerNumber, string updatedBy)
    {
        try
        {
            // Asegurar que containerNumber nunca sea null (la BD requiere NOT NULL)
            var safeContainerNumber = containerNumber ?? string.Empty;

            // Truncar updatedBy a 30 caracteres (límite de la BD)
            var safeUpdatedBy = updatedBy.Length > 30 ? updatedBy[..30] : updatedBy;

            var consecutive = await _dbContext.CompanyConsecutives
                .FirstOrDefaultAsync(cc => cc.CompanyId == companyId);

            if (consecutive == null)
            {
                // Crear registro si no existe
                consecutive = new CompanyConsecutive
                {
                    CompanyId = companyId,
                    ContainerNumber = safeContainerNumber,
                    CreatedBy = safeUpdatedBy,
                    UpdatedBy = safeUpdatedBy,
                    CreateDate = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow
                };
                _dbContext.CompanyConsecutives.Add(consecutive);
                _logger.LogInformation("Creado registro de consecutivos para compañía {CompanyId} con container_number '{ContainerNumber}'",
                    companyId, safeContainerNumber);
            }
            else
            {
                // Actualizar registro existente (record_date y updated_by se actualizan por trigger)
                consecutive.ContainerNumber = safeContainerNumber;
                // Nota: updated_by y record_date se actualizan automáticamente por el trigger de la BD
                _logger.LogInformation("Actualizado container_number de compañía {CompanyId} a '{ContainerNumber}'",
                    companyId, safeContainerNumber);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando container_number para compañía {CompanyId}", companyId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<CompanyConsecutive> CreateForCompanyAsync(int companyId, string createdBy)
    {
        // Truncar createdBy a 30 caracteres (límite de la BD)
        var safeCreatedBy = createdBy.Length > 30 ? createdBy[..30] : createdBy;

        var consecutive = new CompanyConsecutive
        {
            CompanyId = companyId,
            ContainerNumber = string.Empty, // NOT NULL requiere valor
            CreatedBy = safeCreatedBy,
            UpdatedBy = safeCreatedBy,
            CreateDate = DateTime.UtcNow,
            RecordDate = DateTime.UtcNow
        };

        _dbContext.CompanyConsecutives.Add(consecutive);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Creado registro de consecutivos para nueva compañía {CompanyId}", companyId);
        return consecutive;
    }

    /// <inheritdoc />
    public async Task<CompanyConsecutive> GetOrCreateAsync(int companyId, string createdBy)
    {
        var consecutive = await GetByCompanyIdAsync(companyId);
        
        if (consecutive == null)
        {
            consecutive = await CreateForCompanyAsync(companyId, createdBy);
        }

        return consecutive;
    }
}
