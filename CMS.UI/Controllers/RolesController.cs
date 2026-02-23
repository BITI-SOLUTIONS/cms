// ================================================================================
// ARCHIVO: CMS.UI/Controllers/RolesController.cs
// PROPÓSITO: Controlador para gestión de Roles en la UI
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-17
// ================================================================================

using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RolesController> _logger;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RolesController(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RolesController> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("cmsapi");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        #region DTOs

        public class RoleDto
        {
            public int Id { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsSystem { get; set; }
            public bool IsActive { get; set; }
            public int UserCount { get; set; }
            public int PermissionCount { get; set; }
        }

        public class RoleDetailDto
        {
            public int Id { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsSystem { get; set; }
            public bool IsActive { get; set; }
            public List<PermissionDto> Permissions { get; set; } = new();
            public List<UserSimpleDto> Users { get; set; } = new();
        }

        public class PermissionDto
        {
            public int Id { get; set; }
            public string PermissionKey { get; set; } = string.Empty;
            public string PermissionName { get; set; } = string.Empty;
            public bool IsAllowed { get; set; }
        }

        public class UserSimpleDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string? Email { get; set; }
        }

        // DTO para respuesta de autorización usuario-compañía
        public class UserCompanyAuthSummary
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public List<UserCompanyPermissionDto> Permissions { get; set; } = new();
            public int TotalEffectivePermissions { get; set; }
        }

        public class UserCompanyPermissionDto
        {
            public int PermissionId { get; set; }
            public string PermissionKey { get; set; } = string.Empty;
            public string PermissionName { get; set; } = string.Empty;
            public string Module { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public bool IsAllowed { get; set; }
            public bool IsDenied { get; set; }
        }

        #endregion

        private void ConfigureAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("⚠️ No hay ApiToken en sesión para autenticar la petición");
            }
        }

        private string GetApiBaseUrl()
        {
            // Usar ASPNETCORE_ENVIRONMENT para determinar el ambiente correcto
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var baseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"];

            _logger.LogDebug("🔧 RolesController - Environment: {Env}, BaseUrl: {Url}", environment, baseUrl);

            return baseUrl ?? "https://localhost:7001";
        }

        /// <summary>
        /// Lista de roles
        /// GET: /Roles
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roles = JsonSerializer.Deserialize<List<RoleDto>>(json, JsonOptions) ?? new();
                    return View(roles);
                }

                _logger.LogWarning("Error obteniendo roles: {Status}", response.StatusCode);
                TempData["Error"] = "Error al cargar roles";
                return View(new List<RoleDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                TempData["Error"] = "Error al cargar roles";
                return View(new List<RoleDto>());
            }
        }

        /// <summary>
        /// Detalle de un rol con sus permisos
        /// GET: /Roles/Details/{id}
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                ConfigureAuthHeader();
                // Usar el nuevo endpoint que devuelve detalles completos
                var url = $"{GetApiBaseUrl()}/api/role/{id}/details";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var role = JsonSerializer.Deserialize<RoleDetailDto>(json, JsonOptions);
                    _logger.LogInformation("📋 Rol obtenido: {RoleName} - Permisos: {PermCount}, Usuarios: {UserCount}",
                        role?.RoleName, role?.Permissions?.Count ?? 0, role?.Users?.Count ?? 0);

                    // Cargar compañías para el modal de asignar usuario
                    var companiesUrl = $"{GetApiBaseUrl()}/api/catalog/companies";
                    var companiesResponse = await _httpClient.GetAsync(companiesUrl);
                    if (companiesResponse.IsSuccessStatusCode)
                    {
                        var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                        var companies = JsonSerializer.Deserialize<List<CompanySimpleDto>>(companiesJson, JsonOptions);
                        ViewBag.Companies = companies ?? new List<CompanySimpleDto>();
                    }
                    else
                    {
                        ViewBag.Companies = new List<CompanySimpleDto>();
                    }

                    return View(role);
                }

                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo rol {Id}", id);
                TempData["Error"] = "Error al cargar rol";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Vista para crear rol
        /// GET: /Roles/Create
        /// </summary>
        public IActionResult Create()
        {
            return View(new RoleCreateModel());
        }

        /// <summary>
        /// Crear rol
        /// POST: /Roles/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role";
                var content = new StringContent(
                    JsonSerializer.Serialize(new { roleName = model.RoleName, description = model.Description }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Rol '{model.RoleName}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                var errorJson = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al crear rol: {errorJson}";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                TempData["Error"] = "Error interno al crear rol";
                return View(model);
            }
        }

        /// <summary>
        /// Vista para editar rol
        /// GET: /Roles/Edit/{id}
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role/{id}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var role = JsonSerializer.Deserialize<RoleDetailDto>(json, JsonOptions);

                    if (role == null)
                    {
                        TempData["Error"] = "Rol no encontrado";
                        return RedirectToAction(nameof(Index));
                    }

                    var model = new RoleEditModel
                    {
                        Id = role.Id,
                        RoleName = role.RoleName,
                        Description = role.Description,
                        IsSystem = role.IsSystem
                    };

                    return View(model);
                }

                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo rol {Id}", id);
                TempData["Error"] = "Error al cargar rol";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Editar rol
        /// POST: /Roles/Edit/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleEditModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role/{id}";
                var content = new StringContent(
                    JsonSerializer.Serialize(new { roleName = model.RoleName, description = model.Description }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Rol actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                var errorJson = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al actualizar rol: {errorJson}";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando rol {Id}", id);
                TempData["Error"] = "Error interno al actualizar rol";
                return View(model);
            }
        }

        /// <summary>
        /// Eliminar rol
        /// POST: /Roles/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role/{id}";
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Rol eliminado exitosamente";
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al eliminar rol: {errorJson}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando rol {Id}", id);
                TempData["Error"] = "Error interno al eliminar rol";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Vista para gestionar permisos del rol
        /// GET: /Roles/ManagePermissions/{id}?userId={userId}&companyId={companyId}
        /// </summary>
        public async Task<IActionResult> ManagePermissions(int id, int? userId = null, int? companyId = null)
        {
            try
            {
                ConfigureAuthHeader();

                // Obtener el rol
                var roleUrl = $"{GetApiBaseUrl()}/api/role/{id}";
                var roleResponse = await _httpClient.GetAsync(roleUrl);

                if (!roleResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Rol no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var roleJson = await roleResponse.Content.ReadAsStringAsync();
                var role = JsonSerializer.Deserialize<RoleDto>(roleJson, JsonOptions);

                if (role == null || role.IsSystem)
                {
                    TempData["Error"] = role == null ? "Rol no encontrado" : "No se pueden modificar los permisos de un rol del sistema";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener todos los permisos disponibles
                var allPermissionsUrl = $"{GetApiBaseUrl()}/api/permission";
                var allPermissionsResponse = await _httpClient.GetAsync(allPermissionsUrl);
                var allPermissions = new List<PermissionDto>();

                if (allPermissionsResponse.IsSuccessStatusCode)
                {
                    var allPermissionsJson = await allPermissionsResponse.Content.ReadAsStringAsync();
                    allPermissions = JsonSerializer.Deserialize<List<PermissionDto>>(allPermissionsJson, JsonOptions) ?? new List<PermissionDto>();
                }

                // Obtener compañías disponibles
                var companiesUrl = $"{GetApiBaseUrl()}/api/catalog/companies";
                var companiesResponse = await _httpClient.GetAsync(companiesUrl);
                var companies = new List<CompanySimpleDto>();
                if (companiesResponse.IsSuccessStatusCode)
                {
                    var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
                    companies = JsonSerializer.Deserialize<List<CompanySimpleDto>>(companiesJson, JsonOptions) ?? new List<CompanySimpleDto>();
                }

                // Obtener usuarios con este rol
                var usersUrl = $"{GetApiBaseUrl()}/api/role/{id}/details";
                var usersResponse = await _httpClient.GetAsync(usersUrl);
                var users = new List<UserSimpleDto>();
                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersJson = await usersResponse.Content.ReadAsStringAsync();
                    var roleDetails = JsonSerializer.Deserialize<RoleDetailDto>(usersJson, JsonOptions);
                    users = roleDetails?.Users ?? new List<UserSimpleDto>();
                }

                var model = new ManagePermissionsViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.RoleName,
                    AllPermissions = allPermissions,
                    AvailableCompanies = companies,
                    AvailableUsers = users,
                    UserId = userId,
                    CompanyId = companyId
                };

                // Determinar qué permisos cargar según el modo
                if (userId.HasValue && companyId.HasValue)
                {
                    // Modo usuario-compañía: cargar permisos directos del usuario
                    var userPermissionsUrl = $"{GetApiBaseUrl()}/api/users/{userId}/companies/{companyId}/auth";
                    var userPermissionsResponse = await _httpClient.GetAsync(userPermissionsUrl);

                    if (userPermissionsResponse.IsSuccessStatusCode)
                    {
                        var userAuthJson = await userPermissionsResponse.Content.ReadAsStringAsync();
                        var userAuth = JsonSerializer.Deserialize<UserCompanyAuthSummary>(userAuthJson, JsonOptions);

                        if (userAuth != null)
                        {
                            model.UserName = userAuth.UserName;
                            model.CompanyName = userAuth.CompanyName;

                            // Obtener permisos efectivos (permitidos directamente o por rol)
                            model.AssignedPermissionIds = userAuth.Permissions
                                .Where(p => p.IsAllowed)
                                .Select(p => p.PermissionId)
                                .ToList();
                        }
                    }
                }
                else
                {
                    // Modo rol: cargar permisos del rol
                    var permissionsUrl = $"{GetApiBaseUrl()}/api/role/{id}/permissions";
                    var permissionsResponse = await _httpClient.GetAsync(permissionsUrl);
                    var currentPermissions = new List<PermissionDto>();

                    if (permissionsResponse.IsSuccessStatusCode)
                    {
                        var permissionsJson = await permissionsResponse.Content.ReadAsStringAsync();
                        currentPermissions = JsonSerializer.Deserialize<List<PermissionDto>>(permissionsJson, JsonOptions) ?? new List<PermissionDto>();
                    }

                    model.AssignedPermissionIds = currentPermissions.Select(p => p.Id).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando permisos del rol {Id}", id);
                TempData["Error"] = "Error cargando permisos del rol";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Guardar permisos del rol o usuario-compañía
        /// POST: /Roles/ManagePermissions/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(int id, ManagePermissionsViewModel model)
        {
            try
            {
                ConfigureAuthHeader();

                // Determinar si es modo usuario-compañía o modo rol
                if (model.UserId.HasValue && model.CompanyId.HasValue)
                {
                    // MODO USUARIO-COMPAÑÍA: Guardar permisos directos del usuario
                    var selectedIds = model.SelectedPermissionIds ?? new List<int>();

                    // Obtener permisos actuales del usuario en la compañía
                    var currentUrl = $"{GetApiBaseUrl()}/api/users/{model.UserId}/companies/{model.CompanyId}/auth";
                    var currentResponse = await _httpClient.GetAsync(currentUrl);

                    if (currentResponse.IsSuccessStatusCode)
                    {
                        var currentJson = await currentResponse.Content.ReadAsStringAsync();
                        var currentAuth = JsonSerializer.Deserialize<UserCompanyAuthSummary>(currentJson, JsonOptions);

                        if (currentAuth != null)
                        {
                            // Determinar qué permisos agregar y cuáles remover
                            var currentPermissionIds = currentAuth.Permissions
                                .Where(p => p.IsAllowed && p.Source == "direct")
                                .Select(p => p.PermissionId)
                                .ToList();

                            // Permisos a agregar (seleccionados pero no tienen override directo)
                            var toAdd = selectedIds.Except(currentPermissionIds).ToList();

                            // Permisos a remover (tenían override pero ya no están seleccionados)
                            var toRemove = currentPermissionIds.Except(selectedIds).ToList();

                            // Agregar permisos directos
                            foreach (var permId in toAdd)
                            {
                                var addUrl = $"{GetApiBaseUrl()}/api/users/{model.UserId}/companies/{model.CompanyId}/permissions";
                                var addPayload = new { PermissionId = permId, IsAllowed = true };
                                var addContent = new StringContent(JsonSerializer.Serialize(addPayload), System.Text.Encoding.UTF8, "application/json");
                                await _httpClient.PostAsync(addUrl, addContent);
                            }

                            // Remover permisos directos
                            foreach (var permId in toRemove)
                            {
                                var removeUrl = $"{GetApiBaseUrl()}/api/users/{model.UserId}/companies/{model.CompanyId}/permissions/{permId}";
                                await _httpClient.DeleteAsync(removeUrl);
                            }
                        }
                    }

                    TempData["Success"] = "Permisos del usuario actualizados correctamente";
                    return RedirectToAction(nameof(ManagePermissions), new { id, userId = model.UserId, companyId = model.CompanyId });
                }
                else
                {
                    // MODO ROL: Guardar permisos del rol
                    var url = $"{GetApiBaseUrl()}/api/role/{id}/permissions";
                    var payload = new { PermissionIds = model.SelectedPermissionIds ?? new List<int>() };
                    var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Permisos del rol actualizados correctamente";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    var errorJson = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar permisos: {errorJson}";
                    return RedirectToAction(nameof(ManagePermissions), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando permisos del rol {Id}", id);
                TempData["Error"] = "Error interno al guardar permisos";
                return RedirectToAction(nameof(ManagePermissions), new { id });
            }
        }

        #region Asignar Usuario a Rol

        /// <summary>
        /// API endpoint para obtener compañías (para el modal de asignar usuario)
        /// GET: /api/Roles/GetCompanies
        /// </summary>
        [HttpGet]
        [Route("/api/Roles/GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/catalog/companies";
                _logger.LogInformation("📋 Obteniendo compañías de: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📋 Respuesta de compañías: {Json}", json);
                    var companies = JsonSerializer.Deserialize<List<CompanySimpleDto>>(json, JsonOptions);
                    return new JsonResult(companies, JsonOptions);
                }

                _logger.LogWarning("⚠️ Error obteniendo compañías: {Status}", response.StatusCode);
                return new JsonResult(new List<CompanySimpleDto>(), JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañías");
                return new JsonResult(new List<CompanySimpleDto>(), JsonOptions);
            }
        }

        /// <summary>
        /// API endpoint para obtener usuarios de una compañía que NO tienen el rol especificado
        /// GET: /api/Roles/GetUsersForCompany?companyId={id}&roleId={id}
        /// </summary>
        [HttpGet]
        [Route("/api/Roles/GetUsersForCompany")]
        public async Task<IActionResult> GetUsersForCompany(int companyId, int roleId)
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role/{roleId}/available-users/{companyId}";
                _logger.LogInformation("📋 Obteniendo usuarios de: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📋 Respuesta de usuarios: {Json}", json);
                    var users = JsonSerializer.Deserialize<List<UserForRoleDto>>(json, JsonOptions);
                    return new JsonResult(users, JsonOptions);
                }

                _logger.LogWarning("⚠️ Error obteniendo usuarios: {Status}", response.StatusCode);
                return new JsonResult(new List<UserForRoleDto>(), JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios para compañía {CompanyId}", companyId);
                return new JsonResult(new List<UserForRoleDto>(), JsonOptions);
            }
        }

        /// <summary>
        /// Asignar un usuario a un rol en una compañía específica
        /// POST: /Roles/AssignUserToRole
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUserToRole(int roleId, int companyId, int userId)
        {
            try
            {
                ConfigureAuthHeader();
                var url = $"{GetApiBaseUrl()}/api/role/{roleId}/assign-user";
                var payload = new { userId, companyId };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario asignado al rol correctamente";
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error asignando usuario: {Error}", errorJson);
                    TempData["Error"] = "Error al asignar usuario al rol";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando usuario {UserId} al rol {RoleId}", userId, roleId);
                TempData["Error"] = "Error interno al asignar usuario";
            }

            return RedirectToAction(nameof(Details), new { id = roleId });
        }

        #endregion

        #region Models

        public class CompanySimpleDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Code { get; set; }
        }

        public class UserForRoleDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? DisplayName { get; set; }
        }

        public class ManagePermissionsViewModel
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public List<PermissionDto> AllPermissions { get; set; } = new();
            public List<int> AssignedPermissionIds { get; set; } = new();
            public List<int>? SelectedPermissionIds { get; set; }

            // Filtros opcionales para permisos por usuario-compañía
            public int? UserId { get; set; }
            public int? CompanyId { get; set; }
            public string? UserName { get; set; }
            public string? CompanyName { get; set; }

            // Listas para los dropdowns
            public List<UserSimpleDto> AvailableUsers { get; set; } = new();
            public List<CompanySimpleDto> AvailableCompanies { get; set; } = new();

            // Indica si se está editando permisos de usuario-compañía
            public bool IsUserCompanyMode => UserId.HasValue && CompanyId.HasValue;
        }

        public class RoleCreateModel
        {
            public string RoleName { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        public class RoleEditModel
        {
            public int Id { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsSystem { get; set; }
        }

        #endregion
    }
}
