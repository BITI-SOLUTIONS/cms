// ================================================================================
// ARCHIVO: CMS.Application/DTOs/SetPasswordRequest.cs
// PROPÓSITO: DTO para establecer contraseña de usuario por administrador
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-23
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// Request para que un administrador establezca la contraseña de un usuario directamente
    /// </summary>
    public class SetPasswordRequest
    {
        /// <summary>
        /// Nueva contraseña (mínimo 8 caracteres)
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}
