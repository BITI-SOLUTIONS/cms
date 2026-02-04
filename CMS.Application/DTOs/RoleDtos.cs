// ================================================================================
// ARCHIVO: CMS.Application/DTOs/RoleDtos.cs
// PROPÓSITO: DTOs compartidos para transferencia de datos de Roles
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// DTO para listar roles en tablas.
    /// Incluye contadores de usuarios y permisos.
    /// </summary>
    public class RoleListDto
    {
        /// <summary>ID único del rol</summary>
        public int Id { get; set; }

        /// <summary>Nombre del rol (único)</summary>
        public string RoleName { get; set; } = default!;

        /// <summary>Descripción del rol</summary>
        public string? Description { get; set; }

        /// <summary>Indica si es un rol del sistema (no se puede eliminar)</summary>
        public bool IsSystem { get; set; }

        /// <summary>Cantidad de usuarios asignados a este rol</summary>
        public int UserCount { get; set; }

        /// <summary>Cantidad de permisos asignados a este rol</summary>
        public int PermissionCount { get; set; }
    }

    /// <summary>
    /// DTO para visualizar detalles completos de un rol.
    /// Incluye permisos y usuarios asignados.
    /// </summary>
    public class RoleDetailDto
    {
        /// <summary>ID del rol</summary>
        public int Id { get; set; }

        /// <summary>Nombre del rol</summary>
        public string RoleName { get; set; } = default!;

        /// <summary>Descripción</summary>
        public string? Description { get; set; }

        /// <summary>Rol del sistema (protegido)</summary>
        public bool IsSystem { get; set; }

        /// <summary>Fecha de creación</summary>
        public DateTime CreateDate { get; set; }

        /// <summary>Lista de permisos asignados</summary>
        public List<PermissionSimpleDto> Permissions { get; set; } = new();

        /// <summary>Lista de usuarios con este rol</summary>
        public List<UserSimpleDto> Users { get; set; } = new();
    }

    /// <summary>
    /// DTO para crear un nuevo rol.
    /// </summary>
    public class RoleCreateDto
    {
        /// <summary>Nombre del rol (obligatorio, único)</summary>
        public string RoleName { get; set; } = default!;

        /// <summary>Descripción del rol (opcional)</summary>
        public string? Description { get; set; }

        /// <summary>Marcar como rol del sistema (default: false)</summary>
        public bool IsSystem { get; set; } = false;
    }

    /// <summary>
    /// DTO para actualizar un rol existente.
    /// </summary>
    public class RoleUpdateDto
    {
        /// <summary>Nuevo nombre del rol</summary>
        public string RoleName { get; set; } = default!;

        /// <summary>Nueva descripción</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO simplificado de rol para listas relacionadas.
    /// </summary>
    public class RoleSimpleDto
    {
        /// <summary>ID del rol</summary>
        public int Id { get; set; }

        /// <summary>Nombre del rol</summary>
        public string RoleName { get; set; } = default!;
    }
}