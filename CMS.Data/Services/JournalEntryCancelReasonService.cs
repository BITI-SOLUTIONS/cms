// ================================================================================
// ARCHIVO: CMS.Data/Services/JournalEntryCancelReasonService.cs
// PROPÓSITO: Servicio para gestión de razones de cancelación de asientos
// DESCRIPCIÓN: CRUD completo para el catálogo de razones de cancelación
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-XX
// ================================================================================

using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    public interface IJournalEntryCancelReasonService
    {
        Task<List<JournalEntryCancelReason>> GetAllReasonsAsync(int companyId, bool? isActive = null);
        Task<JournalEntryCancelReason?> GetReasonByIdAsync(int companyId, int idReason);
        Task<JournalEntryCancelReason?> GetReasonByCodeAsync(int companyId, string code);
        Task<JournalEntryCancelReason> CreateReasonAsync(int companyId, JournalEntryCancelReason reason, string currentUser);
        Task<JournalEntryCancelReason> UpdateReasonAsync(int companyId, JournalEntryCancelReason reason, string currentUser);
        Task<bool> DeleteReasonAsync(int companyId, int idReason);
        Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null);
    }

    public class JournalEntryCancelReasonService : IJournalEntryCancelReasonService
    {
        private readonly ICompanyDbContextFactory _contextFactory;
        private readonly ILogger<JournalEntryCancelReasonService> _logger;

        public JournalEntryCancelReasonService(
            ICompanyDbContextFactory contextFactory,
            ILogger<JournalEntryCancelReasonService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las razones de cancelación con filtro opcional por estado
        /// </summary>
        public async Task<List<JournalEntryCancelReason>> GetAllReasonsAsync(int companyId, bool? isActive = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);

            var query = context.JournalEntryCancelReasons.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una razón por su ID
        /// </summary>
        public async Task<JournalEntryCancelReason?> GetReasonByIdAsync(int companyId, int idReason)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);
            return await context.JournalEntryCancelReasons
                .FirstOrDefaultAsync(r => r.IdJournalEntryCancelReason == idReason);
        }

        /// <summary>
        /// Obtiene una razón por su código
        /// </summary>
        public async Task<JournalEntryCancelReason?> GetReasonByCodeAsync(int companyId, string code)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);
            return await context.JournalEntryCancelReasons
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        /// <summary>
        /// Crea una nueva razón de cancelación
        /// </summary>
        public async Task<JournalEntryCancelReason> CreateReasonAsync(
            int companyId, 
            JournalEntryCancelReason reason, 
            string currentUser)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);

            // Validar código único
            if (await CodeExistsAsync(companyId, reason.Code))
            {
                throw new InvalidOperationException($"El código '{reason.Code}' ya existe.");
            }

            reason.CreatedBy = currentUser;
            reason.UpdatedBy = currentUser;
            reason.CreateDate = DateTime.UtcNow;
            reason.RecordDate = DateTime.UtcNow;
            reason.RowPointer = Guid.NewGuid();

            context.JournalEntryCancelReasons.Add(reason);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Razón de cancelación creada: {Code} - {Name} por {User}",
                reason.Code, reason.Name, currentUser);

            return reason;
        }

        /// <summary>
        /// Actualiza una razón de cancelación existente
        /// </summary>
        public async Task<JournalEntryCancelReason> UpdateReasonAsync(
            int companyId, 
            JournalEntryCancelReason reason, 
            string currentUser)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);

            var existing = await context.JournalEntryCancelReasons
                .FirstOrDefaultAsync(r => r.IdJournalEntryCancelReason == reason.IdJournalEntryCancelReason)
                ?? throw new InvalidOperationException($"Razón con ID {reason.IdJournalEntryCancelReason} no encontrada.");

            // Validar código único (excluyendo el registro actual)
            if (await CodeExistsAsync(companyId, reason.Code, reason.IdJournalEntryCancelReason))
            {
                throw new InvalidOperationException($"El código '{reason.Code}' ya existe en otro registro.");
            }

            existing.Code = reason.Code;
            existing.Name = reason.Name;
            existing.Description = reason.Description;
            existing.IsActive = reason.IsActive;
            existing.SortOrder = reason.SortOrder;
            existing.UpdatedBy = currentUser;
            existing.RecordDate = DateTime.UtcNow;

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Razón de cancelación actualizada: {Code} - {Name} por {User}",
                reason.Code, reason.Name, currentUser);

            return existing;
        }

        /// <summary>
        /// Elimina una razón de cancelación
        /// </summary>
        public async Task<bool> DeleteReasonAsync(int companyId, int idReason)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);

            var reason = await context.JournalEntryCancelReasons
                .FirstOrDefaultAsync(r => r.IdJournalEntryCancelReason == idReason);

            if (reason == null)
            {
                return false;
            }

            // Verificar si está siendo usada por asientos cancelados
            var isInUse = await context.JournalEntries
                .AnyAsync(je => je.IdJournalEntryCancelReason == idReason);

            if (isInUse)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar la razón porque está siendo usada por asientos cancelados.");
            }

            context.JournalEntryCancelReasons.Remove(reason);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Razón de cancelación eliminada: {Code} - {Name}",
                reason.Code, reason.Name);

            return true;
        }

        /// <summary>
        /// Verifica si un código ya existe
        /// </summary>
        public async Task<bool> CodeExistsAsync(int companyId, string code, int? excludeId = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(companyId);

            var query = context.JournalEntryCancelReasons.Where(r => r.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.IdJournalEntryCancelReason != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
