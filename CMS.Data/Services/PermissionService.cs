// ================================================================================
// ARCHIVO: CMS.Data/Services/PermissionService.cs
// PROPÓSITO: Servicio de infraestructura para cálculo de permisos efectivos
// DESCRIPCIÓN: Calcula todos los permisos de un usuario considerando:
//              - Permisos heredados por roles
//              - Permisos asignados directamente
//              - Permiso maestro "System.FullAccess"
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ACTUALIZADO: 2026-02-11
// ================================================================================

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMS.Data.Services
{
    /// <summary>
    /// Servicio de infraestructura para calcular permisos efectivos de usuarios.
    /// Combina permisos de roles y permisos directos.
    /// Compartido entre CMS.API y otros proyectos que necesiten validar permisos.
    /// </summary>
    public class PermissionService
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Constructor del servicio de permisos
        /// </summary>
        /// <param name="db">Contexto de base de datos</param>
        public PermissionService(AppDbContext db)
        {
            _db = db;
        }

        // =====================================================================
        // ⭐ MÉTODO PARA JWT: Extraer permisos del token sin consultar BD
        // =====================================================================
        /// <summary>
        /// Extrae permisos directamente del ClaimsPrincipal (JWT).
        /// Los permisos ya vienen en el token, no necesita consultar BD.
        /// Usar este método cuando el usuario ya tiene JWT válido.
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>HashSet con todos los permisos del token</returns>
        public HashSet<string> GetUserPermissionsFromToken(ClaimsPrincipal user)
        {
            return user.FindAll("permission")
                       .Select(c => c.Value)
                       .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        // =====================================================================
        // MÉTODO PARA AZURE AD: Buscar usuario y calcular permisos
        // =====================================================================
        /// <summary>
        /// Calcula permisos efectivos a partir de ClaimsPrincipal (usuario autenticado Azure AD).
        /// Busca el usuario por Azure OID y luego calcula sus permisos desde BD.
        /// Usar este método cuando el usuario viene de Azure AD (antes de tener JWT propio).
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado (de Azure AD)</param>
        /// <returns>HashSet con todos los PermissionKey que el usuario tiene</returns>
        public async Task<HashSet<string>> GetUserPermissionsAsync(ClaimsPrincipal user)
        {
            // ⭐ Buscar OID en los claims posibles de Azure AD
            var oidClaim = user.FindFirst("oid")?.Value
                        ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(oidClaim) || !Guid.TryParse(oidClaim, out var azureOid))
            {
                // Usuario no tiene OID válido, devolver sin permisos
                return new HashSet<string>();
            }

            // Buscar usuario en BD por Azure OID
            var dbUser = await _db.Users
                .Where(u => u.AZURE_OID == azureOid && u.IS_ACTIVE)
                .FirstOrDefaultAsync();

            if (dbUser == null)
            {
                // Usuario no existe o está inactivo
                return new HashSet<string>();
            }

            // Llamar al método existente con userId
            return await GetUserPermissionsAsync(dbUser.ID_USER);
        }

        /// <summary>
        /// Calcula todos los permisos efectivos del usuario desde BD.
        /// Combina permisos de roles, permisos directos y el permiso maestro.
        /// </summary>
        /// <param name="userId">ID del usuario (campo ID_USER de tabla [ADMIN].[USER])</param>
        /// <returns>HashSet con todos los PermissionKey que el usuario tiene</returns>
        /// <remarks>
        /// ⚠️ MÉTODO LEGACY: Redirige a GetUserPermissionsForCompanyAsync usando la primera compañía.
        /// Se recomienda usar GetUserPermissionsForCompanyAsync(userId, companyId) directamente.
        /// 
        /// Los permisos se obtienen de admin.user_company_permission (REGLA DE ORO).
        /// </remarks>
        public async Task<HashSet<string>> GetUserPermissionsAsync(int userId)
        {
            // ⚠️ MÉTODO LEGACY - Redirige a permisos de la primera compañía del usuario
            // Para permisos específicos, usar GetUserPermissionsForCompanyAsync(userId, companyId)

            // Obtener la primera compañía del usuario
            var firstCompanyId = await _db.UserCompanies
                .Where(uc => uc.ID_USER == userId && uc.IS_ACTIVE)
                .OrderByDescending(uc => uc.IS_DEFAULT)
                .Select(uc => uc.ID_COMPANY)
                .FirstOrDefaultAsync();

            if (firstCompanyId == 0)
            {
                // Usuario sin compañías asignadas
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // Usar el nuevo método por compañía
            return await GetUserPermissionsForCompanyAsync(userId, firstCompanyId);
        }

        // =====================================================================
        // ⭐ PERMISOS POR COMPAÑÍA - REGLA DE ORO
        // =====================================================================
        /// <summary>
        /// Calcula permisos efectivos de un usuario EN UNA COMPAÑÍA ESPECÍFICA.
        /// 
        /// ⚠️ REGLA DE ORO - PERMISOS:
        /// Los permisos se obtienen ÚNICAMENTE de admin.user_company_permission.
        /// La tabla admin.role_permission SOLO se usa para precargar permisos 
        /// cuando se asigna un rol a un usuario (copia inicial).
        /// 
        /// Lógica:
        /// 1. Obtener permisos de user_company_permission para (userId, companyId)
        /// 2. Separar en permitidos (is_allowed=true) y denegados (is_allowed=false)
        /// 3. Denegaciones siempre ganan sobre permisos permitidos
        /// </summary>
        public async Task<HashSet<string>> GetUserPermissionsForCompanyAsync(int userId, int companyId)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var denied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // =====================================================================
            // ⭐ ÚNICA FUENTE: user_company_permission
            // NO usar role_permission para verificación de permisos
            // =====================================================================
            var directPermissions = await (
                from ucp in _db.UserCompanyPermissions
                join p in _db.Permissions on ucp.ID_PERMISSION equals p.ID_PERMISSION
                where ucp.ID_USER == userId && ucp.ID_COMPANY == companyId && p.IS_ACTIVE
                select new { p.PERMISSION_KEY, ucp.IS_ALLOWED }
            ).ToListAsync();

            foreach (var dp in directPermissions)
            {
                if (dp.IS_ALLOWED)
                {
                    allowed.Add(dp.PERMISSION_KEY);
                }
                else
                {
                    denied.Add(dp.PERMISSION_KEY);
                }
            }

            // =====================================================================
            // APLICAR DENEGACIONES (siempre ganan)
            // =====================================================================
            var effective = new HashSet<string>(
                allowed.Except(denied),
                StringComparer.OrdinalIgnoreCase
            );

            // =====================================================================
            // PERMISO MAESTRO "System.FullAccess"
            // =====================================================================
            if (effective.Contains("System.FullAccess"))
            {
                var allPermKeys = await _db.Permissions
                    .Where(p => p.IS_ACTIVE)
                    .Select(p => p.PERMISSION_KEY)
                    .ToListAsync();

                // Incluso con FullAccess, aplicar denegaciones explícitas
                return new HashSet<string>(
                    allPermKeys.Except(denied),
                    StringComparer.OrdinalIgnoreCase
                );
            }

            return effective;
        }

        /// <summary>
        /// Obtiene los roles de un usuario en una compañía específica
        /// </summary>
        public async Task<List<string>> GetUserRolesForCompanyAsync(int userId, int companyId)
        {
            return await (
                from ucr in _db.UserCompanyRoles
                join r in _db.Roles on ucr.ID_ROLE equals r.ID_ROLE
                where ucr.ID_USER == userId && ucr.ID_COMPANY == companyId && ucr.IS_ACTIVE && r.IS_ACTIVE
                select r.ROLE_NAME
            ).ToListAsync();
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico.
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionKey);
        }

        /// <summary>
        /// Verifica si un usuario tiene ALGUNO de los permisos especificados.
        /// </summary>
        public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionKeys)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissionKeys.Any(pk => permissions.Contains(pk));
        }

        /// <summary>
        /// Verifica si un usuario tiene TODOS los permisos especificados.
        /// </summary>
        public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionKeys)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissionKeys.All(pk => permissions.Contains(pk));
        }

        /// <summary>
        /// Verifica si un usuario es administrador completo del sistema.
        /// </summary>
        public async Task<bool> IsSystemAdminAsync(int userId)
        {
            return await HasPermissionAsync(userId, "System.FullAccess");
        }
    }
}