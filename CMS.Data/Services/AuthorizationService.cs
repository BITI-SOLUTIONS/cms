// ================================================================================
// ARCHIVO: CMS.Data/Services/AuthorizationService.cs
// PROPÓSITO: Servicio centralizado de autorización basado en compañía
// DESCRIPCIÓN: Calcula permisos efectivos de un usuario en una compañía específica
//              considerando roles y permisos directos/denegaciones
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

using CMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio de autorización que calcula los permisos efectivos de un usuario
    /// en una compañía específica.
    /// 
    /// Jerarquía de evaluación:
    /// 1. Obtener permisos de TODOS los roles del usuario en ESA compañía
    /// 2. Aplicar permisos directos (is_allowed = true añade permisos)
    /// 3. Aplicar denegaciones (is_allowed = false SIEMPRE tiene prioridad)
    /// </summary>
    public class AuthorizationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(AppDbContext context, ILogger<AuthorizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region DTOs

        /// <summary>
        /// Resultado del cálculo de permisos efectivos
        /// </summary>
        public class EffectivePermissionsResult
        {
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public List<string> Roles { get; set; } = new();
            public HashSet<string> AllowedPermissions { get; set; } = new();
            public HashSet<string> DeniedPermissions { get; set; } = new();
            
            /// <summary>
            /// Permisos finales después de aplicar denegaciones
            /// </summary>
            public HashSet<string> EffectivePermissions { get; set; } = new();
        }

        /// <summary>
        /// Información de roles de un usuario en una compañía
        /// </summary>
        public class UserCompanyRolesInfo
        {
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public List<RoleInfo> Roles { get; set; } = new();
        }

        public class RoleInfo
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Obtiene los permisos efectivos de un usuario en una compañía específica
        /// </summary>
        public async Task<EffectivePermissionsResult> GetEffectivePermissionsAsync(int userId, int companyId)
        {
            _logger.LogDebug("🔐 Calculando permisos para Usuario {UserId} en Compañía {CompanyId}", userId, companyId);

            var result = new EffectivePermissionsResult
            {
                UserId = userId,
                CompanyId = companyId
            };

            // 1. Obtener roles del usuario en esta compañía
            var userRoles = await _context.UserCompanyRoles
                .AsNoTracking()
                .Where(ucr => ucr.ID_USER == userId && ucr.ID_COMPANY == companyId && ucr.IS_ACTIVE)
                .Join(_context.Roles.Where(r => r.IS_ACTIVE),
                    ucr => ucr.ID_ROLE,
                    r => r.ID_ROLE,
                    (ucr, r) => new { ucr.ID_ROLE, r.ROLE_NAME })
                .ToListAsync();

            result.Roles = userRoles.Select(r => r.ROLE_NAME).ToList();

            _logger.LogDebug("📋 Roles encontrados: {Roles}", string.Join(", ", result.Roles));

            // 2. Obtener permisos de todos los roles
            var roleIds = userRoles.Select(r => r.ID_ROLE).ToList();
            
            var rolePermissions = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsAllowed)
                .Join(_context.Permissions.Where(p => p.IS_ACTIVE),
                    rp => rp.PermissionId,
                    p => p.ID_PERMISSION,
                    (rp, p) => p.PERMISSION_KEY)
                .Distinct()
                .ToListAsync();

            foreach (var perm in rolePermissions)
            {
                result.AllowedPermissions.Add(perm);
            }

            _logger.LogDebug("📋 Permisos de roles: {Count}", result.AllowedPermissions.Count);

            // 3. Obtener permisos directos del usuario en esta compañía
            var directPermissions = await _context.UserCompanyPermissions
                .AsNoTracking()
                .Where(ucp => ucp.ID_USER == userId && ucp.ID_COMPANY == companyId)
                .Join(_context.Permissions.Where(p => p.IS_ACTIVE),
                    ucp => ucp.ID_PERMISSION,
                    p => p.ID_PERMISSION,
                    (ucp, p) => new { p.PERMISSION_KEY, ucp.IS_ALLOWED })
                .ToListAsync();

            // Separar permitidos y denegados
            foreach (var dp in directPermissions)
            {
                if (dp.IS_ALLOWED)
                {
                    result.AllowedPermissions.Add(dp.PERMISSION_KEY);
                    _logger.LogDebug("➕ Permiso directo añadido: {Permission}", dp.PERMISSION_KEY);
                }
                else
                {
                    result.DeniedPermissions.Add(dp.PERMISSION_KEY);
                    _logger.LogDebug("➖ Permiso denegado: {Permission}", dp.PERMISSION_KEY);
                }
            }

            // 4. Calcular permisos efectivos (permitidos - denegados)
            result.EffectivePermissions = new HashSet<string>(
                result.AllowedPermissions.Except(result.DeniedPermissions)
            );

            _logger.LogInformation("✅ Permisos efectivos para Usuario {UserId} en Compañía {CompanyId}: {Count} permisos, {Denied} denegados",
                userId, companyId, result.EffectivePermissions.Count, result.DeniedPermissions.Count);

            return result;
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico en una compañía
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, int companyId, string permissionKey)
        {
            var permissions = await GetEffectivePermissionsAsync(userId, companyId);
            return permissions.EffectivePermissions.Contains(permissionKey);
        }

        /// <summary>
        /// Verifica si un usuario tiene ALGUNO de los permisos especificados
        /// </summary>
        public async Task<bool> HasAnyPermissionAsync(int userId, int companyId, params string[] permissionKeys)
        {
            var permissions = await GetEffectivePermissionsAsync(userId, companyId);
            return permissionKeys.Any(pk => permissions.EffectivePermissions.Contains(pk));
        }

        /// <summary>
        /// Verifica si un usuario tiene TODOS los permisos especificados
        /// </summary>
        public async Task<bool> HasAllPermissionsAsync(int userId, int companyId, params string[] permissionKeys)
        {
            var permissions = await GetEffectivePermissionsAsync(userId, companyId);
            return permissionKeys.All(pk => permissions.EffectivePermissions.Contains(pk));
        }

        /// <summary>
        /// Obtiene los roles de un usuario en una compañía específica
        /// </summary>
        public async Task<UserCompanyRolesInfo> GetUserRolesInCompanyAsync(int userId, int companyId)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == companyId);

            var roles = await _context.UserCompanyRoles
                .AsNoTracking()
                .Where(ucr => ucr.ID_USER == userId && ucr.ID_COMPANY == companyId)
                .Join(_context.Roles,
                    ucr => ucr.ID_ROLE,
                    r => r.ID_ROLE,
                    (ucr, r) => new RoleInfo
                    {
                        RoleId = r.ID_ROLE,
                        RoleName = r.ROLE_NAME,
                        IsActive = ucr.IS_ACTIVE && r.IS_ACTIVE
                    })
                .ToListAsync();

            return new UserCompanyRolesInfo
            {
                UserId = userId,
                CompanyId = companyId,
                CompanyName = company?.COMPANY_NAME ?? "Desconocida",
                Roles = roles
            };
        }

        /// <summary>
        /// Obtiene todos los roles de un usuario en TODAS sus compañías
        /// </summary>
        public async Task<List<UserCompanyRolesInfo>> GetAllUserRolesAsync(int userId)
        {
            var userCompanies = await _context.UserCompanies
                .AsNoTracking()
                .Where(uc => uc.ID_USER == userId && uc.IS_ACTIVE)
                .Select(uc => uc.ID_COMPANY)
                .ToListAsync();

            var result = new List<UserCompanyRolesInfo>();

            foreach (var companyId in userCompanies)
            {
                var rolesInfo = await GetUserRolesInCompanyAsync(userId, companyId);
                result.Add(rolesInfo);
            }

            return result;
        }

        #endregion

        #region Métodos de Gestión

        /// <summary>
        /// Asigna un rol a un usuario en una compañía
        /// </summary>
        public async Task<bool> AssignRoleToUserInCompanyAsync(int userId, int companyId, int roleId, string createdBy)
        {
            try
            {
                // Verificar que no exista ya
                var exists = await _context.UserCompanyRoles
                    .AnyAsync(ucr => ucr.ID_USER == userId && ucr.ID_COMPANY == companyId && ucr.ID_ROLE == roleId);

                if (exists)
                {
                    _logger.LogWarning("⚠️ El rol {RoleId} ya está asignado al usuario {UserId} en compañía {CompanyId}",
                        roleId, userId, companyId);
                    return false;
                }

                var newRole = new UserCompanyRole
                {
                    ID_USER = userId,
                    ID_COMPANY = companyId,
                    ID_ROLE = roleId,
                    IS_ACTIVE = true,
                    RowPointer = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy
                };

                _context.UserCompanyRoles.Add(newRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Rol {RoleId} asignado a usuario {UserId} en compañía {CompanyId}",
                    roleId, userId, companyId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error asignando rol {RoleId} a usuario {UserId} en compañía {CompanyId}",
                    roleId, userId, companyId);
                return false;
            }
        }

        /// <summary>
        /// Remueve un rol de un usuario en una compañía
        /// </summary>
        public async Task<bool> RemoveRoleFromUserInCompanyAsync(int userId, int companyId, int roleId)
        {
            try
            {
                var userRole = await _context.UserCompanyRoles
                    .FirstOrDefaultAsync(ucr => ucr.ID_USER == userId && ucr.ID_COMPANY == companyId && ucr.ID_ROLE == roleId);

                if (userRole == null)
                {
                    _logger.LogWarning("⚠️ No se encontró rol {RoleId} para usuario {UserId} en compañía {CompanyId}",
                        roleId, userId, companyId);
                    return false;
                }

                _context.UserCompanyRoles.Remove(userRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Rol {RoleId} removido de usuario {UserId} en compañía {CompanyId}",
                    roleId, userId, companyId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removiendo rol {RoleId} de usuario {UserId} en compañía {CompanyId}",
                    roleId, userId, companyId);
                return false;
            }
        }

        /// <summary>
        /// Otorga un permiso directo a un usuario en una compañía
        /// </summary>
        public async Task<bool> GrantPermissionAsync(int userId, int companyId, int permissionId, string createdBy)
        {
            return await SetDirectPermissionAsync(userId, companyId, permissionId, true, createdBy);
        }

        /// <summary>
        /// Deniega un permiso a un usuario en una compañía (anula permisos de roles)
        /// </summary>
        public async Task<bool> DenyPermissionAsync(int userId, int companyId, int permissionId, string createdBy)
        {
            return await SetDirectPermissionAsync(userId, companyId, permissionId, false, createdBy);
        }

        /// <summary>
        /// Remueve un permiso directo (el usuario vuelve a depender solo de sus roles)
        /// </summary>
        public async Task<bool> RemoveDirectPermissionAsync(int userId, int companyId, int permissionId)
        {
            try
            {
                var permission = await _context.UserCompanyPermissions
                    .FirstOrDefaultAsync(ucp => ucp.ID_USER == userId && ucp.ID_COMPANY == companyId && ucp.ID_PERMISSION == permissionId);

                if (permission == null)
                {
                    return false;
                }

                _context.UserCompanyPermissions.Remove(permission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Permiso directo {PermissionId} removido de usuario {UserId} en compañía {CompanyId}",
                    permissionId, userId, companyId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removiendo permiso directo");
                return false;
            }
        }

        private async Task<bool> SetDirectPermissionAsync(int userId, int companyId, int permissionId, bool isAllowed, string createdBy)
        {
            try
            {
                var existing = await _context.UserCompanyPermissions
                    .FirstOrDefaultAsync(ucp => ucp.ID_USER == userId && ucp.ID_COMPANY == companyId && ucp.ID_PERMISSION == permissionId);

                if (existing != null)
                {
                    existing.IS_ALLOWED = isAllowed;
                    existing.UpdatedBy = createdBy;
                    existing.RecordDate = DateTime.UtcNow;
                }
                else
                {
                    var newPermission = new UserCompanyPermission
                    {
                        ID_USER = userId,
                        ID_COMPANY = companyId,
                        ID_PERMISSION = permissionId,
                        IS_ALLOWED = isAllowed,
                        RowPointer = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        RecordDate = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy
                    };

                    _context.UserCompanyPermissions.Add(newPermission);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Permiso {PermissionId} {Action} para usuario {UserId} en compañía {CompanyId}",
                    permissionId, isAllowed ? "otorgado" : "denegado", userId, companyId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error configurando permiso directo");
                return false;
            }
        }

        #endregion
    }
}
