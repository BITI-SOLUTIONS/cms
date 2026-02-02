// ================================================================================
// ARCHIVO: CMS.UI/Services/SettingsApiService.cs
// PROPÓSITO: Servicio para consumir los endpoints de Settings de la API REST
// DESCRIPCIÓN: Proporciona métodos para gestionar usuarios, roles y permisos
//              desde la interfaz de usuario, consumiendo la API REST de CMS.
// ================================================================================

using System.Net.Http.Json;
using CMS.Application.DTOs; // ⭐ ÚNICO using necesario

namespace CMS.UI.Services
{
    /// <summary>
    /// Servicio encargado de comunicarse con la API REST para operaciones
    /// relacionadas con la gestión de usuarios, roles y permisos del sistema.
    /// Utiliza HttpClient configurado con el nombre "cmsapi" que debe estar
    /// registrado en Program.cs con la URL base de la API.
    /// </summary>
    public class SettingsApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SettingsApiService> _logger;

        public SettingsApiService(IHttpClientFactory factory, ILogger<SettingsApiService> logger)
        {
            _http = factory.CreateClient("cmsapi");
            _logger = logger;
        }

        // =====================================================
        // USUARIOS
        // =====================================================

        /// <summary>
        /// Obtiene la lista completa de usuarios del sistema.
        /// Endpoint: GET /api/user
        /// </summary>
        /// <returns>Lista de usuarios o null en caso de error</returns>
        public async Task<List<UserListDto>?> GetUsersAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<UserListDto>>("/api/user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de un usuario específico.
        /// Endpoint: GET /api/user/{id}
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Detalle del usuario o null si no existe</returns>
        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<UserDetailDto>($"/api/user/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// Endpoint: POST /api/user
        /// </summary>
        /// <param name="dto">Datos del usuario a crear</param>
        /// <returns>True si se creó exitosamente, False en caso contrario</returns>
        public async Task<bool> CreateUserAsync(UserCreateDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/user", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                return false;
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// Endpoint: PUT /api/user/{id}
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="dto">Nuevos datos del usuario</param>
        /// <returns>True si se actualizó exitosamente, False en caso contrario</returns>
        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"/api/user/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Desactiva un usuario (soft delete).
        /// Endpoint: DELETE /api/user/{id}
        /// </summary>
        /// <param name="id">ID del usuario a desactivar</param>
        /// <returns>True si se desactivó exitosamente, False en caso contrario</returns>
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"/api/user/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Asigna roles a un usuario (reemplaza los roles existentes).
        /// Endpoint: POST /api/user/{userId}/roles
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles a asignar</param>
        /// <returns>True si se asignaron exitosamente, False en caso contrario</returns>
        public async Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/user/{userId}/roles", roleIds);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando roles al usuario {UserId}", userId);
                return false;
            }
        }

        // =====================================================
        // ROLES
        // =====================================================

        /// <summary>
        /// Obtiene la lista completa de roles del sistema.
        /// Endpoint: GET /api/roles
        /// </summary>
        /// <returns>Lista de roles o null en caso de error</returns>
        public async Task<List<RoleListDto>?> GetRolesAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<RoleListDto>>("/api/roles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de un rol específico.
        /// Endpoint: GET /api/roles/{id}
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Detalle del rol o null si no existe</returns>
        public async Task<RoleDetailDto?> GetRoleByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<RoleDetailDto>($"/api/roles/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo rol {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo rol en el sistema.
        /// Endpoint: POST /api/roles
        /// </summary>
        /// <param name="dto">Datos del rol a crear</param>
        /// <returns>True si se creó exitosamente, False en caso contrario</returns>
        public async Task<bool> CreateRoleAsync(RoleCreateDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/roles", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                return false;
            }
        }

        /// <summary>
        /// Actualiza los datos de un rol existente.
        /// Endpoint: PUT /api/roles/{id}
        /// </summary>
        /// <param name="id">ID del rol a actualizar</param>
        /// <param name="dto">Nuevos datos del rol</param>
        /// <returns>True si se actualizó exitosamente, False en caso contrario</returns>
        public async Task<bool> UpdateRoleAsync(int id, RoleUpdateDto dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"/api/roles/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando rol {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Elimina un rol del sistema (solo si no tiene usuarios asignados).
        /// Endpoint: DELETE /api/roles/{id}
        /// </summary>
        /// <param name="id">ID del rol a eliminar</param>
        /// <returns>True si se eliminó exitosamente, False en caso contrario</returns>
        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"/api/roles/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando rol {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Asigna permisos a un rol (reemplaza los permisos existentes).
        /// Endpoint: POST /api/roles/{roleId}/permissions
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissions">Lista de permisos a asignar con su estado (permitido/denegado)</param>
        /// <returns>True si se asignaron exitosamente, False en caso contrario</returns>
        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<PermissionAssignment> permissions)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/roles/{roleId}/permissions", permissions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando permisos al rol {RoleId}", roleId);
                return false;
            }
        }

        // =====================================================
        // PERMISOS
        // =====================================================

        /// <summary>
        /// Obtiene la lista completa de permisos del sistema.
        /// Endpoint: GET /api/permissions
        /// </summary>
        /// <returns>Lista de permisos o null en caso de error</returns>
        public async Task<List<PermissionListDto>?> GetPermissionsAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PermissionListDto>>("/api/permissions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo permisos");
                return null;
            }
        }

        /// <summary>
        /// Obtiene la lista de módulos únicos de los permisos.
        /// Endpoint: GET /api/permissions/modules
        /// </summary>
        /// <returns>Lista de nombres de módulos o null en caso de error</returns>
        public async Task<List<string>?> GetModulesAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<string>>("/api/permissions/modules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo módulos");
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo permiso en el sistema.
        /// Endpoint: POST /api/permissions
        /// </summary>
        /// <param name="dto">Datos del permiso a crear</param>
        /// <returns>True si se creó exitosamente, False en caso contrario</returns>
        public async Task<bool> CreatePermissionAsync(PermissionCreateDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/permissions", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando permiso");
                return false;
            }
        }

        /// <summary>
        /// Actualiza los datos de un permiso existente.
        /// Endpoint: PUT /api/permissions/{id}
        /// </summary>
        /// <param name="id">ID del permiso a actualizar</param>
        /// <param name="dto">Nuevos datos del permiso</param>
        /// <returns>True si se actualizó exitosamente, False en caso contrario</returns>
        public async Task<bool> UpdatePermissionAsync(int id, PermissionCreateDto dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"/api/permissions/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando permiso {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Elimina un permiso del sistema (solo si no está en uso).
        /// Endpoint: DELETE /api/permissions/{id}
        /// </summary>
        /// <param name="id">ID del permiso a eliminar</param>
        /// <returns>True si se eliminó exitosamente, False en caso contrario</returns>
        public async Task<bool> DeletePermissionAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"/api/permissions/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando permiso {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Activa o desactiva un permiso (toggle del campo IsActive).
        /// Endpoint: PATCH /api/permissions/{id}/toggle
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <returns>True si se cambió el estado exitosamente, False en caso contrario</returns>
        public async Task<bool> TogglePermissionAsync(int id)
        {
            try
            {
                var response = await _http.PatchAsync($"/api/permissions/{id}/toggle", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado del permiso {Id}", id);
                return false;
            }
        }
    }
}