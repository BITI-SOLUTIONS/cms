// ================================================================================
// ARCHIVO: CMS.Data/Services/PermissionService.cs
// PROPÓSITO: Servicio de infraestructura para cálculo de permisos efectivos
// DESCRIPCIÓN: Calcula todos los permisos de un usuario considerando:
//              - Permisos heredados por roles
//              - Permisos asignados directamente
//              - Permiso maestro "System.FullAccess"
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2025-12-19
// ================================================================================

using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// Calcula todos los permisos efectivos del usuario.
        /// Combina permisos de roles, permisos directos y el permiso maestro.
        /// </summary>
        /// <param name="userId">ID del usuario (campo ID_USER de tabla [ADMIN].[USER])</param>
        /// <returns>HashSet con todos los PermissionKey que el usuario tiene</returns>
        /// <remarks>
        /// Orden de evaluación:
        /// 1. Permisos heredados por roles (tabla [ADMIN].[ROLE_PERMISSION])
        /// 2. Permisos asignados directamente (tabla [ADMIN].[USER_PERMISSION])
        /// 3. Si tiene "System.FullAccess", devuelve TODOS los permisos del sistema
        /// </remarks>
        public async Task<HashSet<string>> GetUserPermissionsAsync(int userId)
        {
            var final = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // =====================================================================
            // 1️⃣ PERMISOS HEREDADOS POR ROLES
            // =====================================================================
            // Query: USER → USER_ROLE → ROLE_PERMISSION → PERMISSION
            var rolePermissions = await (
                from ur in _db.UserRoles
                join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
                join p in _db.Permissions on rp.PermissionId equals p.ID_PERMISSION
                where ur.UserId == userId && rp.IsAllowed
                select p.PERMISSION_KEY
            ).ToListAsync();

            foreach (var perm in rolePermissions)
            {
                final.Add(perm);
            }

            // =====================================================================
            // 2️⃣ PERMISOS ASIGNADOS DIRECTAMENTE AL USUARIO
            // =====================================================================
            // Query: USER → USER_PERMISSION → PERMISSION
            var userPermissions = await (
                from up in _db.UserPermissions
                join p in _db.Permissions on up.PermissionId equals p.ID_PERMISSION
                where up.UserId == userId && up.IsAllowed
                select p.PERMISSION_KEY
            ).ToListAsync();

            foreach (var perm in userPermissions)
            {
                final.Add(perm);
            }

            // =====================================================================
            // 3️⃣ PERMISO MAESTRO "System.FullAccess"
            // =====================================================================
            // Si el usuario tiene este permiso, automáticamente tiene TODOS los permisos
            if (final.Contains("System.FullAccess"))
            {
                var allPermKeys = await _db.Permissions
                    .Select(p => p.PERMISSION_KEY)
                    .ToListAsync();

                return new HashSet<string>(allPermKeys, StringComparer.OrdinalIgnoreCase);
            }

            return final;
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico.
        /// Método de conveniencia para validación rápida.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionKey">Clave del permiso (ej: "Users.Edit")</param>
        /// <returns>true si el usuario tiene el permiso, false si no</returns>
        public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionKey);
        }

        /// <summary>
        /// Verifica si un usuario tiene ALGUNO de los permisos especificados.
        /// Útil para validar acceso con permisos alternativos.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionKeys">Lista de permisos a verificar</param>
        /// <returns>true si tiene al menos uno de los permisos</returns>
        public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionKeys)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissionKeys.Any(pk => permissions.Contains(pk));
        }

        /// <summary>
        /// Verifica si un usuario tiene TODOS los permisos especificados.
        /// Útil para validar operaciones que requieren múltiples permisos.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionKeys">Lista de permisos requeridos</param>
        /// <returns>true si tiene todos los permisos</returns>
        public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionKeys)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissionKeys.All(pk => permissions.Contains(pk));
        }

        /// <summary>
        /// Verifica si un usuario es administrador completo del sistema.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>true si tiene el permiso "System.FullAccess"</returns>
        public async Task<bool> IsSystemAdminAsync(int userId)
        {
            return await HasPermissionAsync(userId, "System.FullAccess");
        }
    }
}