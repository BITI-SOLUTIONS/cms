// ================================================================================
// ARCHIVO: CMS.UI/Controllers/UsersController.cs
// PROPÓSITO: Controlador para gestión completa de usuarios
// DESCRIPCIÓN: CRUD completo de usuarios con búsqueda, filtros y paginación
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-14
// ================================================================================

using CMS.UI.Models.Users;
using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UsersApiService _usersApi;
        private readonly UserAuthApiService _userAuthApi;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UsersApiService usersApi, 
            UserAuthApiService userAuthApi,
            ILogger<UsersController> logger)
        {
            _usersApi = usersApi;
            _userAuthApi = userAuthApi;
            _logger = logger;
        }

        #region Lista de Usuarios

        /// <summary>
        /// Vista principal - Lista de usuarios con búsqueda y filtros
        /// GET: /Users
        /// </summary>
        public async Task<IActionResult> Index(string? search = null, string? status = null, int page = 1)
        {
            try
            {
                const int pageSize = 10; // 10 usuarios por página
                var result = await _usersApi.GetUsersAsync(search, status, page, pageSize);

                var viewModel = new UserListViewModel
                {
                    Users = result.Users,
                    TotalCount = result.TotalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    StatusFilter = status
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de usuarios");
                TempData["Error"] = "Error al cargar la lista de usuarios";
                return View(new UserListViewModel());
            }
        }

        #endregion

        #region Detalle de Usuario

        /// <summary>
        /// Vista de detalle de un usuario
        /// GET: /Users/Details/{id}
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _usersApi.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle del usuario {UserId}", id);
                TempData["Error"] = "Error al cargar el usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Crear Usuario

        /// <summary>
        /// Formulario para crear usuario
        /// GET: /Users/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var viewModel = new UserCreateViewModel
            {
                AvailableRoles = await _usersApi.GetAvailableRolesAsync(),
                AvailableCountries = await _usersApi.GetCountriesAsync(),
                AvailableGenders = await _usersApi.GetGendersAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la creación de usuario
        /// POST: /Users/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                return View(model);
            }

            try
            {
                var result = await _usersApi.CreateUserAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = $"Usuario '{model.Username}' creado exitosamente";

                    // Si el ID es válido, redirigir a Details; si no, a Index
                    if (result.UserId.HasValue && result.UserId.Value > 0)
                    {
                        return RedirectToAction(nameof(Details), new { id = result.UserId.Value });
                    }
                    else
                    {
                        // Usuario creado pero no pudimos obtener el ID - redirigir a lista
                        _logger.LogWarning("Usuario creado pero UserId retornado es 0 o null");
                        return RedirectToAction(nameof(Index));
                    }
                }

                ModelState.AddModelError("", result.ErrorMessage ?? "Error al crear el usuario");
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                ModelState.AddModelError("", "Error interno al crear el usuario");
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                return View(model);
            }
        }

        #endregion

        #region Editar Usuario

        /// <summary>
        /// Formulario para editar usuario
        /// GET: /Users/Edit/{id}
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _usersApi.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener compañías asignadas al usuario
                var userCompanies = await _usersApi.GetUserCompaniesAsync(id);

                var viewModel = new UserEditViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DisplayName = user.DisplayName,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    IdCountry = user.IdCountry,
                    IdGender = user.IdGender,
                    TimeZone = user.TimeZone,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    SelectedRoleIds = user.Roles?.Select(r => r.Id).ToList() ?? new List<int>(),
                    SelectedCompanyIds = userCompanies.Select(c => c.CompanyId).ToList(),
                    AvailableRoles = await _usersApi.GetAvailableRolesAsync(),
                    AvailableCountries = await _usersApi.GetCountriesAsync(),
                    AvailableGenders = await _usersApi.GetGendersAsync(),
                    AvailableCompanies = await _usersApi.GetAvailableCompaniesAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando usuario para editar {UserId}", id);
                TempData["Error"] = "Error al cargar el usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la edición de usuario
        /// POST: /Users/Edit/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                model.AvailableCompanies = await _usersApi.GetAvailableCompaniesAsync();
                return View(model);
            }

            try
            {
                var result = await _usersApi.UpdateUserAsync(model);

                if (result.Success)
                {
                    // ⭐ Guardar las compañías asignadas
                    if (model.SelectedCompanyIds != null && model.SelectedCompanyIds.Any())
                    {
                        var companiesResult = await _usersApi.AssignCompaniesAsync(id, model.SelectedCompanyIds);
                        if (!companiesResult.Success)
                        {
                            _logger.LogWarning("⚠️ Error asignando compañías: {Error}", companiesResult.ErrorMessage);
                            TempData["Warning"] = $"Usuario actualizado, pero hubo un error asignando compañías: {companiesResult.ErrorMessage}";
                        }
                    }
                    else
                    {
                        // Si no hay compañías seleccionadas, limpiar las existentes
                        await _usersApi.AssignCompaniesAsync(id, new List<int>());
                    }

                    TempData["Success"] = "Usuario actualizado exitosamente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                ModelState.AddModelError("", result.ErrorMessage ?? "Error al actualizar el usuario");
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                model.AvailableCompanies = await _usersApi.GetAvailableCompaniesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {UserId}", id);
                ModelState.AddModelError("", "Error interno al actualizar el usuario");
                model.AvailableRoles = await _usersApi.GetAvailableRolesAsync();
                model.AvailableCountries = await _usersApi.GetCountriesAsync();
                model.AvailableGenders = await _usersApi.GetGendersAsync();
                model.AvailableCompanies = await _usersApi.GetAvailableCompaniesAsync();
                return View(model);
            }
        }

        #endregion

        #region Eliminar / Desactivar Usuario

        /// <summary>
        /// Desactiva un usuario (soft delete)
        /// POST: /Users/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _usersApi.DeleteUserAsync(id);
                
                if (result.Success)
                {
                    TempData["Success"] = "Usuario desactivado exitosamente";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al desactivar el usuario";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error desactivando usuario {UserId}", id);
                TempData["Error"] = "Error interno al desactivar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Reactiva un usuario
        /// POST: /Users/Activate/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var result = await _usersApi.ActivateUserAsync(id);
                
                if (result.Success)
                {
                    TempData["Success"] = "Usuario activado exitosamente";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al activar el usuario";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activando usuario {UserId}", id);
                TempData["Error"] = "Error interno al activar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Reset de Contraseña

        /// <summary>
        /// Envía email de reset de contraseña
        /// POST: /Users/ResetPassword/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var result = await _usersApi.SendPasswordResetAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Se ha enviado un correo para restablecer la contraseña";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al enviar el correo";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando reset de contraseña para usuario {UserId}", id);
                TempData["Error"] = "Error interno al enviar el correo";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Establece contraseña directamente (Admin)
        /// POST: /Users/SetPassword
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken] // AJAX call
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordModel model)
        {
            try
            {
                if (model == null || model.UserId <= 0)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return Json(new { success = false, message = "La contraseña es requerida" });
                }

                if (model.NewPassword.Length < 8)
                {
                    return Json(new { success = false, message = "La contraseña debe tener al menos 8 caracteres" });
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Las contraseñas no coinciden" });
                }

                var result = await _usersApi.SetPasswordAsync(model.UserId, model.NewPassword);

                return Json(new { success = result.Success, message = result.Success ? "Contraseña establecida correctamente" : result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo contraseña para usuario {UserId}", model?.UserId);
                return Json(new { success = false, message = "Error interno al establecer la contraseña" });
            }
        }

        #endregion

        #region API para AJAX

        /// <summary>
        /// Verifica si un username está disponible
        /// GET: /Users/CheckUsername?username=xxx
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username, int? excludeId = null)
        {
            var isAvailable = await _usersApi.CheckUsernameAvailableAsync(username, excludeId);
            return Json(new { available = isAvailable });
        }

        /// <summary>
        /// Verifica si un email está disponible
        /// GET: /Users/CheckEmail?email=xxx
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email, int? excludeId = null)
        {
            var isAvailable = await _usersApi.CheckEmailAvailableAsync(email, excludeId);
            return Json(new { available = isAvailable });
        }

        /// <summary>
        /// Asigna compañías a un usuario vía AJAX
        /// POST: /Users/AssignCompanies
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AssignCompanies([FromBody] AssignCompaniesRequest request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                    return Json(new { success = false, message = "Datos inválidos" });

                var result = await _usersApi.AssignCompaniesAsync(request.UserId, request.CompanyIds ?? new List<int>());

                if (result.Success)
                {
                    _logger.LogInformation("✅ Compañías asignadas a usuario {UserId}: {Companies}", 
                        request.UserId, string.Join(", ", request.CompanyIds ?? new List<int>()));
                    return Json(new { success = true, message = "Compañías asignadas exitosamente" });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Error al asignar compañías" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando compañías al usuario {UserId}", request?.UserId);
                return Json(new { success = false, message = "Error interno al asignar compañías" });
            }
        }

        /// <summary>
        /// Asigna roles a un usuario vía AJAX
        /// POST: /Users/AssignRoles
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequest request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                    return Json(new { success = false, message = "Datos inválidos" });

                var result = await _usersApi.AssignRolesAsync(request.UserId, request.RoleIds ?? new List<int>());

                if (result.Success)
                {
                    _logger.LogInformation("✅ Roles asignados a usuario {UserId}: {Roles}", 
                        request.UserId, string.Join(", ", request.RoleIds ?? new List<int>()));
                    return Json(new { success = true, message = "Roles asignados exitosamente" });
                }

                return Json(new { success = false, message = result.ErrorMessage ?? "Error al asignar roles" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando roles al usuario {UserId}", request?.UserId);
                return Json(new { success = false, message = "Error interno al asignar roles" });
            }
        }

        // DTOs para requests AJAX
        public class AssignCompaniesRequest
        {
            public int UserId { get; set; }
            public List<int>? CompanyIds { get; set; }
        }

        public class AssignRolesRequest
        {
            public int UserId { get; set; }
            public List<int>? RoleIds { get; set; }
        }

        public class SetPasswordModel
        {
            public int UserId { get; set; }
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        #endregion

        #region Verificación de Email

        /// <summary>
        /// Envía email de verificación
        /// POST: /Users/SendVerificationEmail/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(int id)
        {
            try
            {
                var result = await _usersApi.SendVerificationEmailAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Se ha enviado el correo de verificación con una nueva contraseña temporal";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al enviar el correo de verificación";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de verificación para usuario {UserId}", id);
                TempData["Error"] = "Error interno al enviar el correo de verificación";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #endregion

        #region Eliminación Permanente

        /// <summary>
        /// Elimina permanentemente un usuario
        /// POST: /Users/DeletePermanent/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            try
            {
                var result = await _usersApi.DeletePermanentAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Usuario eliminado permanentemente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Si tiene referencias, mostrar mensaje pero redirigir a Details
                    if (result.ErrorMessage?.Contains("referencias") == true)
                    {
                        TempData["Warning"] = result.ErrorMessage;
                        TempData["CanForceDelete"] = true;
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    TempData["Error"] = result.ErrorMessage ?? "Error al eliminar el usuario";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando permanentemente usuario {UserId}", id);
                TempData["Error"] = "Error interno al eliminar el usuario";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Elimina forzadamente un usuario (elimina referencias primero)
        /// POST: /Users/DeleteForce/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForce(int id)
        {
            try
            {
                var result = await _usersApi.DeleteForceAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Usuario y sus referencias eliminados permanentemente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al eliminar el usuario";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando forzadamente usuario {UserId}", id);
                TempData["Error"] = "Error interno al eliminar el usuario";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #endregion

        #region Autorización por Compañía

        /// <summary>
        /// Vista de gestión de roles y permisos de un usuario en una compañía
        /// GET: /Users/CompanyAuth/{userId}/{companyId}
        /// </summary>
        public async Task<IActionResult> CompanyAuth(int userId, int companyId)
        {
            try
            {
                var authSummary = await _userAuthApi.GetAuthSummaryAsync(userId, companyId);

                if (authSummary == null)
                {
                    TempData["Error"] = "No se pudo obtener la información de autorización";
                    return RedirectToAction(nameof(Details), new { id = userId });
                }

                var viewModel = new UserCompanyAuthViewModel
                {
                    UserId = authSummary.UserId,
                    UserName = authSummary.UserName,
                    CompanyId = authSummary.CompanyId,
                    CompanyName = authSummary.CompanyName,
                    TotalEffectivePermissions = authSummary.TotalEffectivePermissions,
                    Roles = authSummary.Roles.Select(r => new UserCompanyRoleItem
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        IsActive = r.IsActive,
                        IsAssigned = r.IsAssigned
                    }).ToList(),
                    Permissions = authSummary.Permissions.Select(p => new UserCompanyPermissionItem
                    {
                        PermissionId = p.PermissionId,
                        PermissionKey = p.PermissionKey,
                        PermissionName = p.PermissionName,
                        Module = p.Module,
                        Source = p.Source,
                        IsAllowed = p.IsAllowed,
                        IsDenied = p.IsDenied
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo autorización para usuario {UserId} en compañía {CompanyId}", 
                    userId, companyId);
                TempData["Error"] = "Error al cargar la información de autorización";
                return RedirectToAction(nameof(Details), new { id = userId });
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario en una compañía
        /// POST: /Users/AssignRole
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int userId, int companyId, int roleId)
        {
            try
            {
                var success = await _userAuthApi.AssignRoleAsync(userId, companyId, roleId);

                if (success)
                {
                    TempData["Success"] = "Rol asignado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo asignar el rol";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando rol");
                TempData["Error"] = "Error interno al asignar rol";
            }

            return RedirectToAction(nameof(CompanyAuth), new { userId, companyId });
        }

        /// <summary>
        /// Remueve un rol de un usuario en una compañía
        /// POST: /Users/RemoveRole
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(int userId, int companyId, int roleId)
        {
            try
            {
                var success = await _userAuthApi.RemoveRoleAsync(userId, companyId, roleId);

                if (success)
                {
                    TempData["Success"] = "Rol removido exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo remover el rol";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removiendo rol");
                TempData["Error"] = "Error interno al remover rol";
            }

            return RedirectToAction(nameof(CompanyAuth), new { userId, companyId });
        }

        /// <summary>
        /// Otorga un permiso directo a un usuario en una compañía
        /// POST: /Users/GrantPermission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermission(int userId, int companyId, int permissionId)
        {
            try
            {
                var success = await _userAuthApi.GrantPermissionAsync(userId, companyId, permissionId);

                if (success)
                {
                    TempData["Success"] = "Permiso otorgado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo otorgar el permiso";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error otorgando permiso");
                TempData["Error"] = "Error interno al otorgar permiso";
            }

            return RedirectToAction(nameof(CompanyAuth), new { userId, companyId });
        }

        /// <summary>
        /// Deniega un permiso a un usuario en una compañía
        /// POST: /Users/DenyPermission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyPermission(int userId, int companyId, int permissionId)
        {
            try
            {
                var success = await _userAuthApi.DenyPermissionAsync(userId, companyId, permissionId);

                if (success)
                {
                    TempData["Success"] = "Permiso denegado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo denegar el permiso";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error denegando permiso");
                TempData["Error"] = "Error interno al denegar permiso";
            }

            return RedirectToAction(nameof(CompanyAuth), new { userId, companyId });
        }

        /// <summary>
        /// Remueve un permiso directo de un usuario (vuelve a depender de roles)
        /// POST: /Users/RemoveDirectPermission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDirectPermission(int userId, int companyId, int permissionId)
        {
            try
            {
                var success = await _userAuthApi.RemoveDirectPermissionAsync(userId, companyId, permissionId);

                if (success)
                {
                    TempData["Success"] = "Override de permiso removido";
                }
                else
                {
                    TempData["Error"] = "No se pudo remover el override de permiso";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removiendo permiso directo");
                TempData["Error"] = "Error interno";
            }

            return RedirectToAction(nameof(CompanyAuth), new { userId, companyId });
        }

        #endregion
    }
}
