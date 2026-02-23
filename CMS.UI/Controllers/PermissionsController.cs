// ================================================================================
// ARCHIVO: CMS.UI/Controllers/PermissionsController.cs
// PROPÓSITO: Controlador para gestión de Permisos en la UI
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

using CMS.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class PermissionsController : Controller
    {
        private readonly PermissionsApiService _permissionsApi;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            PermissionsApiService permissionsApi,
            ILogger<PermissionsController> logger)
        {
            _permissionsApi = permissionsApi;
            _logger = logger;
        }

        #region Lista

        /// <summary>
        /// Lista de permisos
        /// GET: /Permissions
        /// </summary>
        public async Task<IActionResult> Index(string? module = null, string? search = null)
        {
            try
            {
                var permissions = await _permissionsApi.GetAllAsync();
                var modules = await _permissionsApi.GetModulesAsync();

                // Filtrar por módulo
                if (!string.IsNullOrEmpty(module))
                {
                    permissions = permissions.Where(p => p.Module == module).ToList();
                }

                // Filtrar por búsqueda
                if (!string.IsNullOrEmpty(search))
                {
                    permissions = permissions.Where(p => 
                        p.PermissionKey.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.PermissionName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                ViewBag.Modules = new SelectList(modules, module);
                ViewBag.CurrentModule = module;
                ViewBag.SearchTerm = search;

                return View(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo permisos");
                TempData["Error"] = "Error al cargar permisos";
                return View(new List<PermissionsApiService.PermissionDto>());
            }
        }

        #endregion

        #region Crear

        /// <summary>
        /// Vista para crear permiso
        /// GET: /Permissions/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var modules = await _permissionsApi.GetModulesAsync();
            ViewBag.Modules = modules;
            return View(new PermissionsApiService.PermissionCreateDto());
        }

        /// <summary>
        /// Crear permiso
        /// POST: /Permissions/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermissionsApiService.PermissionCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modules = await _permissionsApi.GetModulesAsync();
                    ViewBag.Modules = modules;
                    return View(model);
                }

                var result = await _permissionsApi.CreateAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = $"Permiso '{model.PermissionKey}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = result.ErrorMessage ?? "Error al crear permiso";
                var modulesList = await _permissionsApi.GetModulesAsync();
                ViewBag.Modules = modulesList;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando permiso");
                TempData["Error"] = "Error interno al crear permiso";
                return View(model);
            }
        }

        #endregion

        #region Editar

        /// <summary>
        /// Vista para editar permiso
        /// GET: /Permissions/Edit/{id}
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var permission = await _permissionsApi.GetByIdAsync(id);
                if (permission == null)
                {
                    TempData["Error"] = "Permiso no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var modules = await _permissionsApi.GetModulesAsync();
                ViewBag.Modules = modules;
                ViewBag.PermissionKey = permission.PermissionKey;

                var model = new PermissionsApiService.PermissionUpdateDto
                {
                    PermissionName = permission.PermissionName,
                    Description = permission.Description,
                    Module = permission.Module,
                    IsActive = permission.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo permiso {Id}", id);
                TempData["Error"] = "Error al cargar permiso";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Editar permiso
        /// POST: /Permissions/Edit/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PermissionsApiService.PermissionUpdateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modules = await _permissionsApi.GetModulesAsync();
                    ViewBag.Modules = modules;
                    return View(model);
                }

                var result = await _permissionsApi.UpdateAsync(id, model);

                if (result.Success)
                {
                    TempData["Success"] = "Permiso actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = result.ErrorMessage ?? "Error al actualizar permiso";
                var modulesList = await _permissionsApi.GetModulesAsync();
                ViewBag.Modules = modulesList;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando permiso {Id}", id);
                TempData["Error"] = "Error interno al actualizar permiso";
                return View(model);
            }
        }

        #endregion

        #region Eliminar

        /// <summary>
        /// Eliminar permiso
        /// POST: /Permissions/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _permissionsApi.DeleteAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Permiso eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al eliminar permiso";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando permiso {Id}", id);
                TempData["Error"] = "Error interno al eliminar permiso";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
