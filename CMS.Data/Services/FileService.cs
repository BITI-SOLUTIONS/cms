// ================================================================================
// ARCHIVO: CMS.Data/Services/FileService.cs
// PROPÓSITO: Servicio para gestión de archivos (CRUD, versionamiento, etc.)
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using CMS.Data.Services;
using CMS.Entities.Operational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio para gestión de archivos.
    /// Maneja CRUD, versionamiento, bloqueos y más.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ICompanyDbContextFactory _dbContextFactory;
        private readonly ILogger<FileService> _logger;

        public FileService(
            ICompanyDbContextFactory dbContextFactory,
            ILogger<FileService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        // ============================================================================
        // CARPETAS
        // ============================================================================

        public async Task<List<FileFolder>> GetFoldersAsync(int companyId, int? parentId = null, bool includeDeleted = false)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.FileFolders.AsQueryable();

            if (!includeDeleted)
                query = query.Where(f => f.IsActive && f.DeletedAt == null);

            if (parentId.HasValue)
                query = query.Where(f => f.ParentId == parentId.Value);
            else
                query = query.Where(f => f.ParentId == null);

            return await query
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<FileFolder?> GetFolderByIdAsync(int companyId, int folderId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await dbContext.FileFolders
                .Include(f => f.Children.Where(c => c.IsActive))
                .FirstOrDefaultAsync(f => f.IdFileFolder == folderId);
        }

        public async Task<FileFolder?> GetFolderByCodeAsync(int companyId, string code)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await dbContext.FileFolders
                .FirstOrDefaultAsync(f => f.Code == code && f.IsActive);
        }

        public async Task<FileFolder> CreateFolderAsync(int companyId, FileFolder folder, string? createdBy = null, int? createdById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            folder.Createdate = DateTime.UtcNow;
            folder.CreatedBy = createdBy ?? "system";
            folder.IsActive = true;

            dbContext.FileFolders.Add(folder);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Carpeta creada: {Code} en compañía {CompanyId}", folder.Code, companyId);
            return folder;
        }

        public async Task<FileFolder?> UpdateFolderAsync(int companyId, FileFolder folder, string? updatedBy = null, int? updatedById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var existing = await dbContext.FileFolders.FindAsync(folder.IdFileFolder);
            if (existing == null) return null;

            existing.Name = folder.Name;
            existing.Description = folder.Description;
            existing.ParentId = folder.ParentId;
            existing.CategoryCode = folder.CategoryCode;
            existing.Icon = folder.Icon;
            existing.Color = folder.Color;
            existing.IsPrivate = folder.IsPrivate;
            existing.AllowPublicAccess = folder.AllowPublicAccess;
            existing.SortOrder = folder.SortOrder;
            existing.RecordDate = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy ?? "system";

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteFolderAsync(int companyId, int folderId, string? deletedBy = null, int? deletedById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var folder = await dbContext.FileFolders.FindAsync(folderId);
            if (folder == null || folder.IsSystem) return false;

            folder.IsActive = false;
            folder.DeletedAt = DateTime.UtcNow;
            folder.DeletedBy = deletedById;

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Carpeta eliminada: {Code} en compañía {CompanyId}", folder.Code, companyId);
            return true;
        }

        // ============================================================================
        // ARCHIVOS
        // ============================================================================

        public async Task<(List<FileDocument> Files, int TotalCount)> GetFilesAsync(
            int companyId,
            string? search = null,
            int? folderId = null,
            string? categoryCode = null,
            string? extension = null,
            bool? isActive = true,
            bool includeDeleted = false,
            string? orderBy = "created_at",
            bool descending = true,
            int page = 1,
            int pageSize = 20)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.Files.AsQueryable();

            // Filtros
            if (!includeDeleted)
                query = query.Where(f => f.DeletedAt == null);

            if (isActive.HasValue)
                query = query.Where(f => f.IsActive == isActive.Value);

            if (folderId.HasValue)
                query = query.Where(f => f.IdFileFolder == folderId.Value);
            
            if (!string.IsNullOrWhiteSpace(categoryCode))
                query = query.Where(f => f.CategoryCode == categoryCode);

            if (!string.IsNullOrWhiteSpace(extension))
            {
                // Soportar múltiples extensiones separadas por coma
                var extensions = extension.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().ToLower().TrimStart('.'))
                    .ToList();

                if (extensions.Count == 1)
                    query = query.Where(f => f.FileExtension.ToLower().TrimStart('.') == extensions[0]);
                else
                    query = query.Where(f => extensions.Contains(f.FileExtension.ToLower().TrimStart('.')));
            }

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower().Trim();
                query = query.Where(f =>
                    f.Code.ToLower().Contains(search) ||
                    f.Name.ToLower().Contains(search) ||
                    (f.Title != null && f.Title.ToLower().Contains(search)) ||
                    (f.Description != null && f.Description.ToLower().Contains(search)) ||
                    (f.Tags != null && f.Tags.Any(t => t.ToLower().Contains(search))));
            }

            var totalCount = await query.CountAsync();

            // Ordenamiento
            IOrderedQueryable<FileDocument> orderedQuery = orderBy?.ToLower() switch
            {
                "name" => descending ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name),
                "code" => descending ? query.OrderByDescending(f => f.Code) : query.OrderBy(f => f.Code),
                "size" => descending ? query.OrderByDescending(f => f.FileSizeBytes) : query.OrderBy(f => f.FileSizeBytes),
                "extension" => descending ? query.OrderByDescending(f => f.FileExtension) : query.OrderBy(f => f.FileExtension),
                "updated_at" => descending ? query.OrderByDescending(f => f.RecordDate) : query.OrderBy(f => f.RecordDate),
                _ => descending ? query.OrderByDescending(f => f.Createdate) : query.OrderBy(f => f.Createdate)
            };

            var files = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (files, totalCount);
        }

        public async Task<FileDocument?> GetFileByIdAsync(int companyId, int fileId, bool includeContent = false)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var query = dbContext.Files.AsQueryable();

            if (!includeContent)
            {
                return await query
                    .Where(f => f.IdFile == fileId)
                    .Select(f => new FileDocument
                    {
                        IdFile = f.IdFile,
                        Code = f.Code,
                        Uuid = f.Uuid,
                        Name = f.Name,
                        Title = f.Title,
                        Description = f.Description,
                        IdFileFolder = f.IdFileFolder,
                        Path = f.Path,
                        CategoryCode = f.CategoryCode,
                        Tags = f.Tags,
                        FileExtension = f.FileExtension,
                        MimeType = f.MimeType,
                        FileSizeBytes = f.FileSizeBytes,
                        OriginalFilename = f.OriginalFilename,
                        FileHash = f.FileHash,
                        CurrentVersion = f.CurrentVersion,
                        VersionCount = f.VersionCount,
                        IsLocked = f.IsLocked,
                        LockedByUserId = f.LockedByUserId,
                        LockedByUserName = f.LockedByUserName,
                        LockedAt = f.LockedAt,
                        LockExpiresAt = f.LockExpiresAt,
                        IsFavorite = f.IsFavorite,
                        IsPinned = f.IsPinned,
                        IsPrivate = f.IsPrivate,
                        AllowDownload = f.AllowDownload,
                        AllowPrint = f.AllowPrint,
                        DocumentDate = f.DocumentDate,
                        ExpiryDate = f.ExpiryDate,
                        RelatedEntityType = f.RelatedEntityType,
                        RelatedEntityId = f.RelatedEntityId,
                        RelatedEntityCode = f.RelatedEntityCode,
                        IsActive = f.IsActive,
                        DeletedAt = f.DeletedAt,
                        ViewCount = f.ViewCount,
                        DownloadCount = f.DownloadCount,
                        LastViewedAt = f.LastViewedAt,
                        LastDownloadedAt = f.LastDownloadedAt,
                        Createdate = f.Createdate,
                        CreatedBy = f.CreatedBy,
                        RecordDate = f.RecordDate,
                        UpdatedBy = f.UpdatedBy
                    })
                    .FirstOrDefaultAsync();
            }

            return await query.FirstOrDefaultAsync(f => f.IdFile == fileId);
        }

        public async Task<FileDocument?> GetFileByCodeAsync(int companyId, string code, bool includeContent = false)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files
                .FirstOrDefaultAsync(f => f.Code == code && f.IsActive && f.DeletedAt == null);

            if (file == null) return null;

            return await GetFileByIdAsync(companyId, file.IdFile, includeContent);
        }

        public async Task<byte[]?> GetFileContentAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);
            return await dbContext.Files
                .Where(f => f.IdFile == fileId)
                .Select(f => f.FileContent)
                .FirstOrDefaultAsync();
        }

        public async Task<FileDocument> CreateFileAsync(
            int companyId,
            string originalFilename,
            byte[] content,
            string? title = null,
            string? description = null,
            int? folderId = null,
            string? categoryCode = null,
            string[]? tags = null,
            string? createdBy = null,
            int? createdById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var extension = Path.GetExtension(originalFilename).ToLower();
            var mimeType = GetMimeType(extension);
            var hash = ComputeHash(content);

            // Obtener carpeta por defecto si no se especifica
            if (!folderId.HasValue)
            {
                var rootFolder = await dbContext.FileFolders
                    .FirstOrDefaultAsync(f => f.Code == "ROOT" && f.IsActive);
                folderId = rootFolder?.IdFileFolder;
            }

            // Generar código único para el archivo
            var nextId = await dbContext.Files.AnyAsync() 
                ? await dbContext.Files.MaxAsync(f => f.IdFile) + 1 
                : 1;
            var fileCode = $"FILE-{nextId:D6}";

            var file = new FileDocument
            {
                Code = fileCode,
                Name = Path.GetFileNameWithoutExtension(originalFilename),
                Title = title,
                Description = description,
                IdFileFolder = folderId ?? 0,
                CategoryCode = categoryCode,
                Tags = tags,
                FileExtension = extension,
                MimeType = mimeType,
                FileSizeBytes = content.Length,
                OriginalFilename = originalFilename,
                FileContent = content,
                FileHash = hash,
                CurrentVersion = 1,
                VersionCount = 1,
                Createdate = DateTime.UtcNow,
                CreatedBy = createdBy ?? "system",
                IsActive = true
            };

            dbContext.Files.Add(file);
            await dbContext.SaveChangesAsync();

            // Crear versión inicial
            var version = new FileVersion
            {
                IdFile = file.IdFile,
                VersionNumber = 1,
                FileContent = content,
                FileSizeBytes = content.Length,
                FileHash = hash,
                Filename = originalFilename,
                MimeType = mimeType,
                ChangeType = FileChangeTypes.Create,
                ChangeSummary = "Versión inicial",
                CreatedAt = DateTime.UtcNow,
                Createdate = DateTime.UtcNow,
                CreatedBy = createdBy ?? "system"
            };

            dbContext.FileVersions.Add(version);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo creado: {Code} ({Filename}) en compañía {CompanyId}", 
                file.Code, originalFilename, companyId);

            return file;
        }

        public async Task<FileDocument?> UpdateFileMetadataAsync(
            int companyId,
            int fileId,
            string? name = null,
            string? title = null,
            string? description = null,
            int? folderId = null,
            string? categoryCode = null,
            string[]? tags = null,
            DateOnly? documentDate = null,
            DateOnly? expiryDate = null,
            bool? isFavorite = null,
            bool? isPinned = null,
            bool? isPrivate = null,
            bool? allowDownload = null,
            string? updatedBy = null,
            int? updatedById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null) return null;

            if (name != null) file.Name = name;
            if (title != null) file.Title = title;
            if (description != null) file.Description = description;
            if (folderId.HasValue) file.IdFileFolder = folderId.Value;
            if (categoryCode != null) file.CategoryCode = categoryCode;
            if (tags != null) file.Tags = tags;
            if (documentDate.HasValue) file.DocumentDate = documentDate;
            if (expiryDate.HasValue) file.ExpiryDate = expiryDate;
            if (isFavorite.HasValue) file.IsFavorite = isFavorite.Value;
            if (isPinned.HasValue) file.IsPinned = isPinned.Value;
            if (isPrivate.HasValue) file.IsPrivate = isPrivate.Value;
            if (allowDownload.HasValue) file.AllowDownload = allowDownload.Value;

            file.RecordDate = DateTime.UtcNow;
            file.UpdatedBy = updatedBy ?? "system";

            await dbContext.SaveChangesAsync();
            return file;
        }

        public async Task<FileDocument?> UpdateFileContentAsync(
            int companyId,
            int fileId,
            byte[] newContent,
            string? changeSummary = null,
            string? updatedBy = null,
            int? updatedById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null) return null;

            // Verificar si está bloqueado por otro usuario
            if (file.IsLocked && file.LockedByUserId != updatedById && !file.IsLockExpired)
            {
                throw new InvalidOperationException($"El archivo está bloqueado por {file.LockedByUserName}");
            }

            var newHash = ComputeHash(newContent);
            var newVersionNumber = file.CurrentVersion + 1;

            // Crear nueva versión
            var version = new FileVersion
            {
                IdFile = file.IdFile,
                VersionNumber = newVersionNumber,
                FileContent = newContent,
                FileSizeBytes = newContent.Length,
                FileHash = newHash,
                Filename = file.OriginalFilename,
                MimeType = file.MimeType,
                ChangeType = FileChangeTypes.Update,
                ChangeSummary = changeSummary ?? $"Versión {newVersionNumber}",
                CreatedAt = DateTime.UtcNow,
                Createdate = DateTime.UtcNow,
                CreatedBy = updatedBy ?? "system"
            };

            dbContext.FileVersions.Add(version);

            // Actualizar archivo principal
            file.FileContent = newContent;
            file.FileSizeBytes = newContent.Length;
            file.FileHash = newHash;
            file.CurrentVersion = newVersionNumber;
            file.VersionCount = newVersionNumber;
            file.RecordDate = DateTime.UtcNow;
            file.UpdatedBy = updatedBy ?? "system";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo actualizado: {Code} versión {Version} en compañía {CompanyId}", 
                file.Code, newVersionNumber, companyId);

            return file;
        }

        public async Task<bool> DeleteFileAsync(
            int companyId,
            int fileId,
            string? reason = null,
            string? deletedBy = null,
            int? deletedById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null) return false;

            file.IsActive = false;
            file.DeletedAt = DateTime.UtcNow;
            file.DeletedBy = deletedById;
            file.DeleteReason = reason;
            // Programar eliminación permanente en 30 días
            file.PermanentDeleteScheduledAt = DateTime.UtcNow.AddDays(30);

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo eliminado (papelera): {Code} en compañía {CompanyId}", 
                file.Code, companyId);

            return true;
        }

        public async Task<bool> RestoreFileAsync(int companyId, int fileId, string? restoredBy = null, int? restoredById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null || file.DeletedAt == null) return false;

            file.IsActive = true;
            file.DeletedAt = null;
            file.DeletedBy = null;
            file.DeleteReason = null;
            file.PermanentDeleteScheduledAt = null;
            file.RecordDate = DateTime.UtcNow;
            file.UpdatedBy = restoredBy ?? "system";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo restaurado: {Code} en compañía {CompanyId}", 
                file.Code, companyId);

            return true;
        }

        public async Task<bool> PermanentDeleteFileAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files
                .Include(f => f.Versions)
                .Include(f => f.Comments)
                .FirstOrDefaultAsync(f => f.IdFile == fileId);

            if (file == null) return false;

            dbContext.Files.Remove(file);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo eliminado permanentemente: {Code} en compañía {CompanyId}", 
                file.Code, companyId);

            return true;
        }

        // ============================================================================
        // BLOQUEO (CHECK-OUT / CHECK-IN)
        // ============================================================================

        public async Task<bool> LockFileAsync(
            int companyId,
            int fileId,
            int userId,
            string userName,
            string? reason = null,
            int lockDurationMinutes = 60)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null) return false;

            // Verificar si ya está bloqueado por otro usuario
            if (file.IsLocked && file.LockedByUserId != userId && !file.IsLockExpired)
            {
                return false;
            }

            file.IsLocked = true;
            file.LockedByUserId = userId;
            file.LockedByUserName = userName;
            file.LockedAt = DateTime.UtcNow;
            file.LockExpiresAt = DateTime.UtcNow.AddMinutes(lockDurationMinutes);
            file.LockReason = reason;

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo bloqueado: {Code} por usuario {User} en compañía {CompanyId}", 
                file.Code, userName, companyId);

            return true;
        }

        public async Task<bool> UnlockFileAsync(int companyId, int fileId, int userId, bool force = false)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null || !file.IsLocked) return false;

            // Solo el usuario que bloqueó puede desbloquear (o forzar)
            if (file.LockedByUserId != userId && !force)
            {
                return false;
            }

            file.IsLocked = false;
            file.LockedByUserId = null;
            file.LockedByUserName = null;
            file.LockedAt = null;
            file.LockExpiresAt = null;
            file.LockReason = null;

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Archivo desbloqueado: {Code} en compañía {CompanyId}", 
                file.Code, companyId);

            return true;
        }

        // ============================================================================
        // VERSIONES
        // ============================================================================

        public async Task<List<FileVersion>> GetFileVersionsAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            return await dbContext.FileVersions
                .Where(v => v.IdFile == fileId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new FileVersion
                {
                    IdFileVersion = v.IdFileVersion,
                    IdFile = v.IdFile,
                    VersionNumber = v.VersionNumber,
                    FileSizeBytes = v.FileSizeBytes,
                    FileHash = v.FileHash,
                    Filename = v.Filename,
                    MimeType = v.MimeType,
                    ChangeSummary = v.ChangeSummary,
                    ChangeDetails = v.ChangeDetails,
                    ChangeType = v.ChangeType,
                    CreatedAt = v.CreatedAt,
                    CreatedBy = v.CreatedBy
                })
                .ToListAsync();
        }

        public async Task<byte[]?> GetVersionContentAsync(int companyId, int fileId, int versionNumber)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            return await dbContext.FileVersions
                .Where(v => v.IdFile == fileId && v.VersionNumber == versionNumber)
                .Select(v => v.FileContent)
                .FirstOrDefaultAsync();
        }

        public async Task<FileDocument?> RestoreVersionAsync(
            int companyId,
            int fileId,
            int versionNumber,
            string? restoredBy = null,
            int? restoredById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var file = await dbContext.Files.FindAsync(fileId);
            if (file == null) return null;

            var version = await dbContext.FileVersions
                .FirstOrDefaultAsync(v => v.IdFile == fileId && v.VersionNumber == versionNumber);

            if (version == null) return null;

            var newVersionNumber = file.CurrentVersion + 1;

            // Crear nueva versión desde la restaurada
            var newVersion = new FileVersion
            {
                IdFile = file.IdFile,
                VersionNumber = newVersionNumber,
                FileContent = version.FileContent,
                FileSizeBytes = version.FileSizeBytes,
                FileHash = version.FileHash,
                Filename = version.Filename,
                MimeType = version.MimeType,
                ChangeType = FileChangeTypes.Restore,
                ChangeSummary = $"Restaurado desde versión {versionNumber}",
                CreatedAt = DateTime.UtcNow,
                Createdate = DateTime.UtcNow,
                CreatedBy = restoredBy ?? "system"
            };

            dbContext.FileVersions.Add(newVersion);

            // Actualizar archivo principal
            file.FileContent = version.FileContent;
            file.FileSizeBytes = version.FileSizeBytes;
            file.FileHash = version.FileHash;
            file.CurrentVersion = newVersionNumber;
            file.VersionCount = newVersionNumber;
            file.RecordDate = DateTime.UtcNow;
            file.UpdatedBy = restoredBy ?? "system";

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Versión {Version} restaurada para archivo {Code} en compañía {CompanyId}", 
                versionNumber, file.Code, companyId);

            return file;
        }

        // ============================================================================
        // COMENTARIOS
        // ============================================================================

        public async Task<List<FileComment>> GetFileCommentsAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            return await dbContext.FileComments
                .Where(c => c.IdFile == fileId && !c.IsDeleted)
                .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<FileComment> AddCommentAsync(
            int companyId,
            int fileId,
            string text,
            int? parentId = null,
            int[]? mentionedUserIds = null,
            string? createdBy = null,
            int? createdById = null)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            var comment = new FileComment
            {
                IdFile = fileId,
                ParentId = parentId,
                CommentText = text,
                MentionedUserIds = mentionedUserIds,
                CreatedAt = DateTime.UtcNow,
                Createdate = DateTime.UtcNow,
                CreatedBy = createdBy ?? "system"
            };

            dbContext.FileComments.Add(comment);
            await dbContext.SaveChangesAsync();

            return comment;
        }

        // ============================================================================
        // ESTADÍSTICAS
        // ============================================================================

        public async Task RecordViewAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            await dbContext.Files
                .Where(f => f.IdFile == fileId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.ViewCount, f => f.ViewCount + 1)
                    .SetProperty(f => f.LastViewedAt, DateTime.UtcNow));
        }

        public async Task RecordDownloadAsync(int companyId, int fileId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(companyId);

            await dbContext.Files
                .Where(f => f.IdFile == fileId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.DownloadCount, f => f.DownloadCount + 1)
                    .SetProperty(f => f.LastDownloadedAt, DateTime.UtcNow));
        }

        // ============================================================================
        // UTILIDADES
        // ============================================================================

        private static string ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToHexString(hashBytes);
        }

        private static string GetMimeType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".html" => "text/html",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }
    }

    /// <summary>
    /// Interface para el servicio de archivos.
    /// </summary>
    public interface IFileService
    {
        // Carpetas
        Task<List<FileFolder>> GetFoldersAsync(int companyId, int? parentId = null, bool includeDeleted = false);
        Task<FileFolder?> GetFolderByIdAsync(int companyId, int folderId);
        Task<FileFolder?> GetFolderByCodeAsync(int companyId, string code);
        Task<FileFolder> CreateFolderAsync(int companyId, FileFolder folder, string? createdBy = null, int? createdById = null);
        Task<FileFolder?> UpdateFolderAsync(int companyId, FileFolder folder, string? updatedBy = null, int? updatedById = null);
        Task<bool> DeleteFolderAsync(int companyId, int folderId, string? deletedBy = null, int? deletedById = null);

        // Archivos
        Task<(List<FileDocument> Files, int TotalCount)> GetFilesAsync(
            int companyId, string? search = null, int? folderId = null, string? categoryCode = null,
            string? extension = null, bool? isActive = true, bool includeDeleted = false,
            string? orderBy = "created_at", bool descending = true, int page = 1, int pageSize = 20);
        Task<FileDocument?> GetFileByIdAsync(int companyId, int fileId, bool includeContent = false);
        Task<FileDocument?> GetFileByCodeAsync(int companyId, string code, bool includeContent = false);
        Task<byte[]?> GetFileContentAsync(int companyId, int fileId);
        Task<FileDocument> CreateFileAsync(int companyId, string originalFilename, byte[] content,
            string? title = null, string? description = null, int? folderId = null, string? categoryCode = null,
            string[]? tags = null, string? createdBy = null, int? createdById = null);
        Task<FileDocument?> UpdateFileMetadataAsync(int companyId, int fileId, string? name = null,
            string? title = null, string? description = null, int? folderId = null, string? categoryCode = null,
            string[]? tags = null, DateOnly? documentDate = null, DateOnly? expiryDate = null,
            bool? isFavorite = null, bool? isPinned = null, bool? isPrivate = null, bool? allowDownload = null,
            string? updatedBy = null, int? updatedById = null);
        Task<FileDocument?> UpdateFileContentAsync(int companyId, int fileId, byte[] newContent,
            string? changeSummary = null, string? updatedBy = null, int? updatedById = null);
        Task<bool> DeleteFileAsync(int companyId, int fileId, string? reason = null, string? deletedBy = null, int? deletedById = null);
        Task<bool> RestoreFileAsync(int companyId, int fileId, string? restoredBy = null, int? restoredById = null);
        Task<bool> PermanentDeleteFileAsync(int companyId, int fileId);

        // Bloqueo
        Task<bool> LockFileAsync(int companyId, int fileId, int userId, string userName, string? reason = null, int lockDurationMinutes = 60);
        Task<bool> UnlockFileAsync(int companyId, int fileId, int userId, bool force = false);

        // Versiones
        Task<List<FileVersion>> GetFileVersionsAsync(int companyId, int fileId);
        Task<byte[]?> GetVersionContentAsync(int companyId, int fileId, int versionNumber);
        Task<FileDocument?> RestoreVersionAsync(int companyId, int fileId, int versionNumber, string? restoredBy = null, int? restoredById = null);

        // Comentarios
        Task<List<FileComment>> GetFileCommentsAsync(int companyId, int fileId);
        Task<FileComment> AddCommentAsync(int companyId, int fileId, string text, int? parentId = null,
            int[]? mentionedUserIds = null, string? createdBy = null, int? createdById = null);

        // Estadísticas
        Task RecordViewAsync(int companyId, int fileId);
        Task RecordDownloadAsync(int companyId, int fileId);
    }
}
