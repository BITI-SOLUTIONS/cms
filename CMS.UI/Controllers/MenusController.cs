// ================================================================================
// ARCHIVO: CMS.UI/Controllers/MenusController.cs
// PROPÓSITO: Controlador para gestión de Menús en la UI
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
    public class MenusController : Controller
    {
        private readonly MenusAdminApiService _menusApi;
        private readonly PermissionsApiService _permissionsApi;
        private readonly ILogger<MenusController> _logger;

        public MenusController(
            MenusAdminApiService menusApi,
            PermissionsApiService permissionsApi,
            ILogger<MenusController> logger)
        {
            _menusApi = menusApi;
            _permissionsApi = permissionsApi;
            _logger = logger;
        }

        #region Lista

        /// <summary>
        /// Lista de menús en estructura jerárquica
        /// GET: /Menus
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var menus = await _menusApi.GetAllAsync();

                // Construir jerarquía
                var parentMenus = menus.Where(m => m.IdParent == 0).OrderBy(m => m.Order).ToList();
                foreach (var parent in parentMenus)
                {
                    parent.ParentName = "(Raíz)";
                }

                var childMenus = menus.Where(m => m.IdParent != 0).ToList();
                foreach (var child in childMenus)
                {
                    var parent = menus.FirstOrDefault(m => m.IdMenu == child.IdParent);
                    child.ParentName = parent?.Name ?? "Desconocido";
                }

                // Ordenar: primero padres, luego hijos agrupados por padre
                var orderedMenus = new List<MenusAdminApiService.MenuDto>();
                foreach (var parent in parentMenus)
                {
                    orderedMenus.Add(parent);
                    orderedMenus.AddRange(childMenus.Where(c => c.IdParent == parent.IdMenu).OrderBy(c => c.Order));
                }

                return View(orderedMenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo menús");
                TempData["Error"] = "Error al cargar menús";
                return View(new List<MenusAdminApiService.MenuDto>());
            }
        }

        #endregion

        #region Crear

        /// <summary>
        /// Vista para crear menú
        /// GET: /Menus/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            await LoadViewBagData();
            return View(new MenusAdminApiService.MenuCreateDto { IsActive = true });
        }

        /// <summary>
        /// Crear menú
        /// POST: /Menus/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenusAdminApiService.MenuCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadViewBagData();
                    return View(model);
                }

                var result = await _menusApi.CreateAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = $"Menú '{model.Name}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = result.ErrorMessage ?? "Error al crear menú";
                await LoadViewBagData();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando menú");
                TempData["Error"] = "Error interno al crear menú";
                await LoadViewBagData();
                return View(model);
            }
        }

        #endregion

        #region Editar

        /// <summary>
        /// Vista para editar menú
        /// GET: /Menus/Edit/{id}
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var menu = await _menusApi.GetByIdAsync(id);
                if (menu == null)
                {
                    TempData["Error"] = "Menú no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await LoadViewBagData();

                var model = new MenusAdminApiService.MenuUpdateDto
                {
                    IdParent = menu.IdParent,
                    Name = menu.Name,
                    Url = menu.Url,
                    Icon = menu.Icon,
                    Order = menu.Order,
                    PermissionKey = menu.PermissionKey,
                    IsActive = menu.IsActive
                };

                ViewBag.MenuId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo menú {Id}", id);
                TempData["Error"] = "Error al cargar menú";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Editar menú
        /// POST: /Menus/Edit/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenusAdminApiService.MenuUpdateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadViewBagData();
                    ViewBag.MenuId = id;
                    return View(model);
                }

                var result = await _menusApi.UpdateAsync(id, model);

                if (result.Success)
                {
                    TempData["Success"] = "Menú actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = result.ErrorMessage ?? "Error al actualizar menú";
                await LoadViewBagData();
                ViewBag.MenuId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando menú {Id}", id);
                TempData["Error"] = "Error interno al actualizar menú";
                await LoadViewBagData();
                return View(model);
            }
        }

        #endregion

        #region Eliminar

        /// <summary>
        /// Eliminar menú
        /// POST: /Menus/Delete/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _menusApi.DeleteAsync(id);

                if (result.Success)
                {
                    TempData["Success"] = "Menú eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage ?? "Error al eliminar menú";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando menú {Id}", id);
                TempData["Error"] = "Error interno al eliminar menú";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Helpers

        private async Task LoadViewBagData()
        {
            // Cargar menús padre
            var menus = await _menusApi.GetAllAsync();
            var parentMenus = menus.Where(m => m.IdParent == 0).OrderBy(m => m.Order).ToList();
            
            var parentList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "(Raíz - Sin padre)" }
            };
            parentList.AddRange(parentMenus.Select(m => new SelectListItem
            {
                Value = m.IdMenu.ToString(),
                Text = m.Name
            }));
            ViewBag.ParentMenus = parentList;

            // Cargar permisos
            var permissions = await _permissionsApi.GetAllAsync();
            var permissionList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "(Sin permiso - visible para todos)" }
            };
            permissionList.AddRange(permissions.OrderBy(p => p.Module).ThenBy(p => p.PermissionKey).Select(p => new SelectListItem
            {
                Value = p.PermissionKey,
                Text = $"[{p.Module}] {p.PermissionName}"
            }));
            ViewBag.Permissions = permissionList;

            // Iconos comunes de Bootstrap Icons
            ViewBag.CommonIcons = new List<string>
            {
                "bi-house", "bi-gear", "bi-people", "bi-person", "bi-key",
                "bi-shield-check", "bi-building", "bi-file-text", "bi-graph-up",
                "bi-calendar", "bi-bell", "bi-envelope", "bi-cart", "bi-credit-card",
                "bi-box", "bi-folder", "bi-database", "bi-server", "bi-code-square",
                "bi-diagram-3", "bi-list-ul", "bi-grid", "bi-table", "bi-bar-chart"
            };
        }

        #endregion
    }
}
