// ================================================================================
// ARCHIVO: CMS.Application/DTOs/MenuDto.cs
// PROPÓSITO: DTO para transferencia de datos de Menús del sistema
// DESCRIPCIÓN: Define la estructura de menús para serialización JSON en camelCase.
//              Los nombres de propiedades siguen convención C# (PascalCase) y se
//              convierten automáticamente a camelCase en JSON.
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// DTO que representa un ítem del menú del sistema.
    /// Las propiedades se serializan a JSON en camelCase para el frontend.
    /// </summary>
    public class MenuDto
    {
        /// <summary>ID único del menú</summary>
        public int IdMenu { get; set; }

        /// <summary>
        /// ID del menú padre.
        /// Si es 0, es un menú de nivel raíz (sin padre).
        /// </summary>
        public int IdParent { get; set; }

        /// <summary>Nombre del menú a mostrar en la UI</summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// URL de destino del menú.
        /// Puede ser una ruta relativa (ej: "/Home") o "#" para menús contenedores.
        /// </summary>
        public string Url { get; set; } = default!;

        /// <summary>
        /// Clase CSS del ícono a mostrar.
        /// Ejemplos: "bi-house-door", "bi-people", "bi-gear"
        /// Compatible con Bootstrap Icons.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Orden de aparición del menú.
        /// Los menús se ordenan ascendentemente por este valor dentro de su nivel.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Clave de permiso requerida para visualizar este menú.
        /// Si es null o vacío, el menú es visible para todos los usuarios autenticados.
        /// Ejemplo: "System.FullAccess", "Courses.View", "Students.Edit"
        /// </summary>
        public string? PermissionKey { get; set; }

        /// <summary>
        /// Indica si el menú está activo.
        /// Los menús inactivos (false) no se devuelven en las consultas.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Menús hijos de este menú.
        /// Propiedad para construcción de jerarquía en el cliente.
        /// </summary>
        public List<MenuDto> Children { get; set; } = new();
    }
}