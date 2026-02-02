// ================================================================================
// ARCHIVO: CMS.Application/DTOs/MenuDto.cs
// PROPÓSITO: DTO para transferencia de datos de Menús del sistema
// DESCRIPCIÓN: Define la estructura de menús que coincide EXACTAMENTE con la
//              entidad Menu de la base de datos para correcta serialización JSON.
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// DTO que representa un ítem del menú del sistema.
    /// Las propiedades coinciden exactamente con los nombres de columna en la BD
    /// para facilitar la serialización/deserialización JSON automática.
    /// Utilizado tanto por la API como por la UI para renderizar la navegación.
    /// </summary>
    public class MenuDto
    {
        /// <summary>ID único del menú (ID_MENU en BD)</summary>
        public int ID_MENU { get; set; }

        /// <summary>
        /// ID del menú padre (ID_PARENT en BD).
        /// Si es 0, es un menú de nivel raíz (sin padre).
        /// </summary>
        public int ID_PARENT { get; set; }

        /// <summary>Nombre del menú a mostrar en la UI (NAME en BD)</summary>
        public string NAME { get; set; } = default!;

        /// <summary>
        /// URL de destino del menú (URL en BD).
        /// Puede ser una ruta relativa (ej: "/Home") o "#" para menús contenedores.
        /// </summary>
        public string URL { get; set; } = default!;

        /// <summary>
        /// Clase CSS del ícono a mostrar (ICON en BD).
        /// Ejemplos: "bi-house-door", "bi-people", "bi-gear"
        /// Compatible con Bootstrap Icons.
        /// </summary>
        public string? ICON { get; set; }

        /// <summary>
        /// Orden de aparición del menú (ORDER en BD).
        /// Los menús se ordenan ascendentemente por este valor dentro de su nivel.
        /// </summary>
        public int ORDER { get; set; }

        /// <summary>
        /// Clave de permiso requerida para visualizar este menú (PERMISSION_KEY en BD).
        /// Si es null o vacío, el menú es visible para todos los usuarios autenticados.
        /// Ejemplo: "System.FullAccess", "Courses.View", "Students.Edit"
        /// </summary>
        public string? PERMISSION_KEY { get; set; }

        /// <summary>
        /// Indica si el menú está activo (IS_ACTIVE en BD).
        /// Los menús inactivos (false) no se devuelven en las consultas.
        /// Default: true
        /// </summary>
        public bool IS_ACTIVE { get; set; } = true;  // ✅ CORREGIDO: ACTIVE → IS_ACTIVE

        /// <summary>
        /// Menús hijos de este menú.
        /// Propiedad NO MAPEADA en BD (solo para construcción de jerarquía en cliente).
        /// </summary>
        public List<MenuDto> Children { get; set; } = new();
    }
}