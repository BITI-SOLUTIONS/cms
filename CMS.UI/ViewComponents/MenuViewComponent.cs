// ================================================================================
// ARCHIVO: CMS.UI/ViewComponents/MenuViewComponent.cs
// PROPÓSITO: ViewComponent para renderizar el menú de navegación del sistema
// DESCRIPCIÓN: Obtiene los menús PLANOS desde la API y los ordena para su 
//              visualización. La construcción de la jerarquía se hace en la vista.
//              Los menús ya vienen filtrados por permisos desde el backend.
// ================================================================================

using CMS.Application.DTOs; // ⭐ CAMBIO: Usar DTOs de Application
using CMS.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.ViewComponents
{
    /// <summary>
    /// ViewComponent que renderiza el menú de navegación principal del sistema.
    /// Se invoca desde _Layout.cshtml con: @await Component.InvokeAsync("Menu")
    /// Los menús se obtienen en formato PLANO desde la API REST.
    /// La vista (Default.cshtml) construye la jerarquía padre-hijo.
    /// </summary>
    public class MenuViewComponent : ViewComponent
    {
        private readonly MenuApiService _menuApi;
        private readonly ILogger<MenuViewComponent> _logger;

        public MenuViewComponent(MenuApiService menuApi, ILogger<MenuViewComponent> logger)
        {
            _menuApi = menuApi;
            _logger = logger;
        }

        /// <summary>
        /// Método principal invocado al renderizar el ViewComponent.
        /// Obtiene los menús PLANOS desde la API y los ordena antes de pasarlos a la vista.
        /// La vista Default.cshtml se encarga de construir la estructura jerárquica.
        /// </summary>
        /// <returns>
        /// ViewComponentResult con la lista de menús ordenada por ID_PARENT y ORDER.
        /// La vista Default.cshtml filtra los padres (ID_PARENT == 0) y construye hijos.
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Obtener menús PLANOS desde la API (ya filtrados por permisos)
                var menus = await _menuApi.GetMenusAsync();

                if (menus == null || !menus.Any())
                {
                    _logger.LogWarning("⚠️ No se obtuvieron menús o la lista está vacía");
                    return View(new List<MenuDto>());
                }

                // Ordenar por padre y luego por orden
                // IMPORTANTE: NO construir jerarquía aquí, solo ordenar
                // La vista Default.cshtml filtra por ID_PARENT == 0 y construye hijos
                var orderedMenus = menus
                    .OrderBy(m => m.IdParent)
                    .ThenBy(m => m.Order)
                    .ToList();

                _logger.LogInformation("✅ Menús ordenados: {Count} ítems", orderedMenus.Count);

                return View(orderedMenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al procesar menús en MenuViewComponent");
                return View(new List<MenuDto>());
            }
        }
    }
}