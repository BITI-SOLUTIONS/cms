// ================================================================================
// ARCHIVO: CMS.Application/DTOs/PermissionDtos.cs
// PROPÓSITO: DTOs compartidos para transferencia de datos de Permisos
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// DTO para listar permisos en tablas.
    /// Usado para mostrar el catálogo completo de permisos.
    /// </summary>
    public class PermissionListDto
    {
        /// <summary>ID único del permiso</summary>
        public int Id { get; set; }

        /// <summary>Clave única del permiso (ej: "System.FullAccess")</summary>
        public string PermissionKey { get; set; } = default!;

        /// <summary>Nombre legible del permiso (ej: "Acceso Total al Sistema")</summary>
        public string PermissionName { get; set; } = default!;

        /// <summary>Descripción detallada del permiso</summary>
        public string? Description { get; set; }

        /// <summary>Módulo al que pertenece (ej: "System", "Courses", "Students")</summary>
        public string? Module { get; set; }

        /// <summary>Indica si el permiso está activo</summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo permiso.
    /// </summary>
    public class PermissionCreateDto
    {
        /// <summary>Clave única del permiso (obligatorio)</summary>
        public string PermissionKey { get; set; } = default!;

        /// <summary>Nombre del permiso (obligatorio)</summary>
        public string PermissionName { get; set; } = default!;

        /// <summary>Descripción (opcional)</summary>
        public string? Description { get; set; }

        /// <summary>Módulo (opcional)</summary>
        public string? Module { get; set; }

        /// <summary>Estado inicial (default: true)</summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO simplificado de permiso para listas relacionadas.
    /// Incluye flag IsAllowed para indicar si está permitido o denegado.
    /// </summary>
    public class PermissionSimpleDto
    {
        /// <summary>ID del permiso</summary>
        public int Id { get; set; }

        /// <summary>Clave del permiso</summary>
        public string PermissionKey { get; set; } = default!;

        /// <summary>Nombre del permiso</summary>
        public string PermissionName { get; set; } = default!;

        /// <summary>Indica si está permitido (true) o denegado (false)</summary>
        public bool IsAllowed { get; set; } = true;
    }

    /// <summary>
    /// DTO para asignar permisos a roles o usuarios.
    /// Permite especificar si el permiso está permitido o denegado.
    /// </summary>
    public class PermissionAssignment
    {
        /// <summary>ID del permiso a asignar</summary>
        public int PermissionId { get; set; }

        /// <summary>true = permitido, false = denegado</summary>
        public bool IsAllowed { get; set; } = true;
    }
}