namespace CMS.API.Models
{
    /// <summary>
    /// DTO jerárquico para exponer menús con hijos.
    /// </summary>
    public class MenuNodeDto
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        // Marcamos como required para evitar warnings CS8618 y reflejar que
        // el contrato de salida siempre debe llevar estos campos.
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string Icon { get; set; }

        public int Order { get; set; }
        public string? PermissionKey { get; set; }

        public List<MenuNodeDto> Children { get; set; } = new();
    }
}
