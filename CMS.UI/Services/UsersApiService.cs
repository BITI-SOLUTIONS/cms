// ================================================================================
// ARCHIVO: CMS.UI/Services/UsersApiService.cs
// PROPÓSITO: Servicio para comunicación con la API de usuarios
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

using CMS.UI.Models.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using System.Text.Json;

namespace CMS.UI.Services
{
    public class UsersApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UsersApiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public UsersApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsersApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("cmsapi");
            
            // Agregar token JWT si existe
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            
            return client;
        }

        #region Usuarios CRUD

        /// <summary>
        /// Obtiene lista de usuarios con filtros y paginación
        /// </summary>
        public async Task<UserListResult> GetUsersAsync(string? search = null, string? status = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var client = CreateClient();
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={status}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var url = $"api/user?{string.Join("&", queryParams)}";
                _logger.LogInformation("🔍 Llamando API: {Url}", url);

                var response = await client.GetAsync(url);

                _logger.LogInformation("📥 Respuesta API: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📄 Contenido: {Content}", content);

                    var users = JsonSerializer.Deserialize<List<UserApiDto>>(content, JsonOptions);
                    _logger.LogInformation("✅ Usuarios deserializados: {Count}", users?.Count ?? 0);

                    return new UserListResult
                    {
                        Users = users?.Select(MapToItemViewModel).ToList() ?? new List<UserItemViewModel>(),
                        TotalCount = users?.Count ?? 0
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error obteniendo usuarios: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new UserListResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Excepción obteniendo usuarios");
                return new UserListResult();
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<UserDetailViewModel?> GetUserByIdAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"api/user/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDetailApiDto>(JsonOptions);
                    return user != null ? MapToDetailViewModel(user) : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario {UserId}", id);
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public async Task<UserOperationResult> CreateUserAsync(UserCreateViewModel model)
        {
            try
            {
                var client = CreateClient();
                
                var dto = new
                {
                    username = model.Username,
                    email = model.Email,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    displayName = model.DisplayName ?? $"{model.FirstName} {model.LastName}",
                    phoneNumber = model.PhoneNumber,
                    dateOfBirth = model.DateOfBirth,
                    idCountry = model.IdCountry,
                    idGender = model.IdGender,
                    timeZone = model.TimeZone,
                    password = model.Password,
                    isActive = model.IsActive,
                    isEmailVerified = model.IsEmailVerified,
                    roleIds = model.SelectedRoleIds,
                    sendWelcomeEmail = model.SendWelcomeEmail
                };

                var response = await client.PostAsJsonAsync("api/user", dto);

                if (response.IsSuccessStatusCode)
                {
                    // La API devuelve un UserDetailDto completo
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("✅ Usuario creado exitosamente. Respuesta: {Content}", 
                        content.Length > 200 ? content.Substring(0, 200) + "..." : content);

                    try
                    {
                        // Intentar extraer el ID de la respuesta JSON
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;

                        // Puede ser directamente el UserDetailDto o estar envuelto en value (ActionResult)
                        if (root.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out int userId))
                        {
                            _logger.LogInformation("✅ Usuario ID extraído: {UserId}", userId);
                            return UserOperationResult.Ok(userId);
                        }
                        else if (root.TryGetProperty("value", out var valueElement) && 
                                 valueElement.TryGetProperty("id", out var valueIdElement) &&
                                 valueIdElement.TryGetInt32(out int valueUserId))
                        {
                            _logger.LogInformation("✅ Usuario ID extraído de value: {UserId}", valueUserId);
                            return UserOperationResult.Ok(valueUserId);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No se pudo extraer el ID de la respuesta. Estructura: {Props}", 
                                string.Join(", ", root.EnumerateObject().Select(p => p.Name)));
                            return UserOperationResult.Ok(0);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning(parseEx, "⚠️ Error parseando respuesta de creación");
                        return UserOperationResult.Ok(0);
                    }
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error creando usuario: StatusCode={StatusCode}, Response={Error}", 
                    response.StatusCode, error);

                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(error, JsonOptions);
                    var errorMessage = errorObj?.Message ?? "Error al crear el usuario";

                    // Agregar detalles si están disponibles
                    if (!string.IsNullOrEmpty(errorObj?.Details))
                    {
                        _logger.LogWarning("📋 Detalles del error: Code={Code}, Details={Details}", 
                            errorObj.Code, errorObj.Details);
                    }

                    return UserOperationResult.Error(errorMessage);
                }
                catch
                {
                    return UserOperationResult.Error($"Error al crear el usuario (HTTP {(int)response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                return UserOperationResult.Error("Error interno al crear el usuario");
            }
        }

        /// <summary>
        /// Actualiza un usuario
        /// </summary>
        public async Task<UserOperationResult> UpdateUserAsync(UserEditViewModel model)
        {
            try
            {
                var client = CreateClient();
                
                var dto = new
                {
                    email = model.Email,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    displayName = model.DisplayName ?? $"{model.FirstName} {model.LastName}",
                    phoneNumber = model.PhoneNumber,
                    dateOfBirth = model.DateOfBirth,
                    idCountry = model.IdCountry,
                    idGender = model.IdGender,
                    timeZone = model.TimeZone,
                    isActive = model.IsActive,
                    isEmailVerified = model.IsEmailVerified,
                    roleIds = model.SelectedRoleIds
                };

                var response = await client.PutAsJsonAsync($"api/user/{model.Id}", dto);

                if (response.IsSuccessStatusCode)
                {
                    return UserOperationResult.Ok(model.Id);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error actualizando usuario: {Error}", error);
                
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(error, JsonOptions);
                    return UserOperationResult.Error(errorObj?.Message ?? "Error al actualizar el usuario");
                }
                catch
                {
                    return UserOperationResult.Error("Error al actualizar el usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {UserId}", model.Id);
                return UserOperationResult.Error("Error interno al actualizar el usuario");
            }
        }

        /// <summary>
        /// Desactiva un usuario (soft delete)
        /// </summary>
        public async Task<UserOperationResult> DeleteUserAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.DeleteAsync($"api/user/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return UserOperationResult.Ok();
                }

                return UserOperationResult.Error("Error al desactivar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error desactivando usuario {UserId}", id);
                return UserOperationResult.Error("Error interno");
            }
        }

        /// <summary>
        /// Activa un usuario
        /// </summary>
        public async Task<UserOperationResult> ActivateUserAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/user/{id}/activate", null);

                if (response.IsSuccessStatusCode)
                {
                    return UserOperationResult.Ok();
                }

                return UserOperationResult.Error("Error al activar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activando usuario {UserId}", id);
                return UserOperationResult.Error("Error interno");
            }
        }

        /// <summary>
        /// Envía email de reset de contraseña
        /// </summary>
        public async Task<UserOperationResult> SendPasswordResetAsync(int id)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/user/{id}/reset-password", null);

                if (response.IsSuccessStatusCode)
                {
                    return UserOperationResult.Ok();
                }

                return UserOperationResult.Error("Error al enviar el correo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando reset de contraseña para usuario {UserId}", id);
                return UserOperationResult.Error("Error interno");
            }
        }

        /// <summary>
        /// Establece la contraseña de un usuario directamente (sin enviar correo)
        /// </summary>
        public async Task<UserOperationResult> SetPasswordAsync(int id, string newPassword)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync($"api/user/{id}/set-password", new { newPassword });

                if (response.IsSuccessStatusCode)
                {
                    return UserOperationResult.Ok();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error estableciendo contraseña: {Error}", errorContent);

                try
                {
                    var errorResult = JsonSerializer.Deserialize<ApiResponse>(errorContent, JsonOptions);
                    return UserOperationResult.Error(errorResult?.Message ?? "Error al establecer la contraseña");
                }
                catch
                {
                    return UserOperationResult.Error("Error al establecer la contraseña");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo contraseña para usuario {UserId}", id);
                return UserOperationResult.Error("Error interno");
            }
        }

        #endregion

        #region Validaciones

        public async Task<bool> CheckUsernameAvailableAsync(string username, int? excludeId = null)
        {
            try
            {
                var client = CreateClient();
                var url = $"api/user/check-username?username={Uri.EscapeDataString(username)}";
                if (excludeId.HasValue)
                    url += $"&excludeId={excludeId}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AvailabilityResponse>(JsonOptions);
                    return result?.Available ?? false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckEmailAvailableAsync(string email, int? excludeId = null)
        {
            try
            {
                var client = CreateClient();
                var url = $"api/user/check-email?email={Uri.EscapeDataString(email)}";
                if (excludeId.HasValue)
                    url += $"&excludeId={excludeId}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AvailabilityResponse>(JsonOptions);
                    return result?.Available ?? false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Datos para Dropdowns

        public async Task<List<SelectListItem>> GetAvailableRolesAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/role");

                if (response.IsSuccessStatusCode)
                {
                    var roles = await response.Content.ReadFromJsonAsync<List<RoleApiDto>>(JsonOptions);
                    return roles?.Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.RoleName
                    }).ToList() ?? new List<SelectListItem>();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                return new List<SelectListItem>();
            }
        }

        public async Task<List<SelectListItem>> GetCountriesAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/catalog/countries");

                if (response.IsSuccessStatusCode)
                {
                    var countries = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>(JsonOptions);
                    return countries?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList() ?? new List<SelectListItem>();
                }

                // Fallback con países básicos
                return new List<SelectListItem>
                {
                    new() { Value = "1", Text = "Costa Rica" },
                    new() { Value = "2", Text = "Estados Unidos" },
                    new() { Value = "3", Text = "México" }
                };
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new() { Value = "1", Text = "Costa Rica" }
                };
            }
        }

        public async Task<List<SelectListItem>> GetGendersAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/catalog/genders");

                if (response.IsSuccessStatusCode)
                {
                    var genders = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>(JsonOptions);
                    return genders?.Select(g => new SelectListItem
                    {
                        Value = g.Id.ToString(),
                        Text = g.Name
                    }).ToList() ?? new List<SelectListItem>();
                }

                // Fallback
                return new List<SelectListItem>
                {
                    new() { Value = "1", Text = "Masculino" },
                    new() { Value = "2", Text = "Femenino" },
                    new() { Value = "3", Text = "Otro" }
                };
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new() { Value = "1", Text = "Masculino" },
                    new() { Value = "2", Text = "Femenino" }
                };
            }
        }

        #endregion

        #region Mappers

        private static UserItemViewModel MapToItemViewModel(UserApiDto dto)
        {
            return new UserItemViewModel
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                IsActive = dto.IsActive,
                IsEmailVerified = dto.IsEmailVerified,
                CreateDate = dto.CreateDate,
                LastLogin = dto.LastLogin,
                Roles = dto.Roles ?? new List<string>()
            };
        }

        private static UserDetailViewModel MapToDetailViewModel(UserDetailApiDto dto)
        {
            return new UserDetailViewModel
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                TimeZone = dto.TimeZone,
                IdCountry = dto.IdCountry,
                IdGender = dto.IdGender,
                IsActive = dto.IsActive,
                IsEmailVerified = dto.IsEmailVerified,
                IsPhoneVerified = dto.IsPhoneVerified,
                LastLogin = dto.LastLogin,
                LastLoginIp = dto.LastLoginIp,
                LoginCount = dto.LoginCount,
                FailedLoginAttempts = dto.FailedLoginAttempts,
                LockoutEnd = dto.LockoutEnd,
                LastPasswordChange = dto.LastPasswordChange,
                AzureOid = dto.AzureOid,
                AzureUpn = dto.AzureUpn,
                CreateDate = dto.CreateDate,
                CreatedBy = dto.CreatedBy,
                Roles = dto.Roles?.Select(r => new RoleItemViewModel
                {
                    Id = r.Id,
                    Name = r.RoleName
                }).ToList() ?? new List<RoleItemViewModel>(),
                DirectPermissions = dto.DirectPermissions?.Select(p => new PermissionItemViewModel
                {
                    Id = p.Id,
                    Key = p.PermissionKey,
                    Name = p.PermissionName,
                    IsAllowed = p.IsAllowed
                }).ToList() ?? new List<PermissionItemViewModel>(),
                Companies = dto.Companies?.Select(c => new UserCompanyViewModel
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    IsDefault = c.IsDefault,
                    IsActive = c.IsActive
                }).ToList() ?? new List<UserCompanyViewModel>()
            };
        }

        #endregion

        #region DTOs Internos para deserialización

        private class UserApiDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? DisplayName { get; set; }
            public bool IsActive { get; set; }
            public bool IsEmailVerified { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? LastLogin { get; set; }
            public List<string>? Roles { get; set; }
        }

        private class UserDetailApiDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? DisplayName { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PhoneNumber { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? TimeZone { get; set; }
            public int IdCountry { get; set; }
            public int IdGender { get; set; }
            public bool IsActive { get; set; }
            public bool IsEmailVerified { get; set; }
            public bool IsPhoneVerified { get; set; }
            public DateTime? LastLogin { get; set; }
            public string? LastLoginIp { get; set; }
            public int LoginCount { get; set; }
            public int FailedLoginAttempts { get; set; }
            public DateTime? LockoutEnd { get; set; }
            public DateTime? LastPasswordChange { get; set; }
            public Guid? AzureOid { get; set; }
            public string? AzureUpn { get; set; }
            public DateTime CreateDate { get; set; }
            public string? CreatedBy { get; set; }
            public List<RoleApiDto>? Roles { get; set; }
            public List<PermissionApiDto>? DirectPermissions { get; set; }
            public List<CompanyApiDto>? Companies { get; set; }
        }

        private class RoleApiDto
        {
            public int Id { get; set; }
            public string RoleName { get; set; } = string.Empty;
        }

        private class PermissionApiDto
        {
            public int Id { get; set; }
            public string PermissionKey { get; set; } = string.Empty;
            public string PermissionName { get; set; } = string.Empty;
            public bool IsAllowed { get; set; }
        }

        private class CompanyApiDto
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public bool IsDefault { get; set; }
            public bool IsActive { get; set; }
        }

        private class CatalogItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class CreateUserResponse
        {
            public int Id { get; set; }
        }

        private class ApiErrorResponse
        {
            public string? Message { get; set; }
            public string? Code { get; set; }
            public string? Details { get; set; }
            public DateTime? Timestamp { get; set; }
        }

        private class ApiResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        private class AvailabilityResponse
        {
            public bool Available { get; set; }
        }

        #endregion

        #region Acciones de Email y Eliminación

        /// <summary>
        /// Envía email de verificación
        /// </summary>
        public async Task<UserOperationResult> SendVerificationEmailAsync(int userId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"api/user/{userId}/send-verification", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("✅ Email de verificación enviado para usuario {UserId}", userId);
                    return UserOperationResult.Ok();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error enviando email de verificación: {Error}", error);

                try
                {
                    using var doc = JsonDocument.Parse(error);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() 
                        : "Error al enviar email de verificación";
                    return UserOperationResult.Error(message ?? "Error desconocido");
                }
                catch
                {
                    return UserOperationResult.Error("Error al enviar email de verificación");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de verificación para usuario {UserId}", userId);
                return UserOperationResult.Error($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina permanentemente un usuario (verifica referencias primero)
        /// </summary>
        public async Task<UserOperationResult> DeletePermanentAsync(int userId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.DeleteAsync($"api/user/{userId}/permanent");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Usuario {UserId} eliminado permanentemente", userId);
                    return UserOperationResult.Ok();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error eliminando usuario: {Error}", error);

                try
                {
                    using var doc = JsonDocument.Parse(error);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() 
                        : "Error al eliminar usuario";
                    return UserOperationResult.Error(message ?? "Error desconocido");
                }
                catch
                {
                    return UserOperationResult.Error("Error al eliminar usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario {UserId}", userId);
                return UserOperationResult.Error($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina forzadamente un usuario (elimina referencias primero)
        /// </summary>
        public async Task<UserOperationResult> DeleteForceAsync(int userId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.DeleteAsync($"api/user/{userId}/force");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Usuario {UserId} eliminado forzadamente", userId);
                    return UserOperationResult.Ok();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error eliminando usuario (forzado): {Error}", error);

                try
                {
                    using var doc = JsonDocument.Parse(error);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() 
                        : "Error al eliminar usuario";
                    return UserOperationResult.Error(message ?? "Error desconocido");
                }
                catch
                {
                    return UserOperationResult.Error("Error al eliminar usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario (forzado) {UserId}", userId);
                return UserOperationResult.Error($"Error interno: {ex.Message}");
            }
        }

        #endregion

        #region User Companies

        /// <summary>
        /// Obtiene las compañías disponibles para asignar
        /// </summary>
        public async Task<List<SelectListItem>> GetAvailableCompaniesAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("api/catalog/companies");

                if (response.IsSuccessStatusCode)
                {
                    var companies = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>(JsonOptions);
                    return companies?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList() ?? new List<SelectListItem>();
                }

                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañías");
                return new List<SelectListItem>();
            }
        }

        /// <summary>
        /// Obtiene las compañías asignadas a un usuario
        /// </summary>
        public async Task<List<UserCompanyViewModel>> GetUserCompaniesAsync(int userId)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"api/user/{userId}/companies");

                if (response.IsSuccessStatusCode)
                {
                    var companies = await response.Content.ReadFromJsonAsync<List<UserCompanyApiDto>>(JsonOptions);
                    return companies?.Select(c => new UserCompanyViewModel
                    {
                        CompanyId = c.CompanyId,
                        CompanyName = c.CompanyName,
                        IsDefault = c.IsDefault,
                        IsActive = c.IsActive
                    }).ToList() ?? new List<UserCompanyViewModel>();
                }

                return new List<UserCompanyViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compañías del usuario {UserId}", userId);
                return new List<UserCompanyViewModel>();
            }
        }

        /// <summary>
        /// Asigna compañías a un usuario
        /// </summary>
        public async Task<UserOperationResult> AssignCompaniesAsync(int userId, List<int> companyIds)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync($"api/user/{userId}/companies", new { CompanyIds = companyIds }, JsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Compañías asignadas a usuario {UserId}: {Companies}", 
                        userId, string.Join(", ", companyIds));
                    return UserOperationResult.Ok();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error asignando compañías: {Error}", error);

                try
                {
                    using var doc = JsonDocument.Parse(error);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() 
                        : "Error al asignar compañías";
                    return UserOperationResult.Error(message ?? "Error desconocido");
                }
                catch
                {
                    return UserOperationResult.Error("Error al asignar compañías");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando compañías al usuario {UserId}", userId);
                return UserOperationResult.Error($"Error interno: {ex.Message}");
            }
        }

        private class UserCompanyApiDto
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public bool IsDefault { get; set; }
            public bool IsActive { get; set; }
        }

        #endregion

        #region User Roles

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        public async Task<UserOperationResult> AssignRolesAsync(int userId, List<int> roleIds)
        {
            try
            {
                var client = CreateClient();
                var response = await client.PostAsJsonAsync($"api/user/{userId}/roles", roleIds, JsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Roles asignados a usuario {UserId}: {Roles}", 
                        userId, string.Join(", ", roleIds));
                    return UserOperationResult.Ok();
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("❌ Error asignando roles: {Error}", error);

                try
                {
                    using var doc = JsonDocument.Parse(error);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() 
                        : "Error al asignar roles";
                    return UserOperationResult.Error(message ?? "Error desconocido");
                }
                catch
                {
                    return UserOperationResult.Error("Error al asignar roles");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando roles al usuario {UserId}", userId);
                return UserOperationResult.Error($"Error interno: {ex.Message}");
            }
        }

        #endregion
    }
}
