// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/EmailVerificationResultViewModel.cs
// PROPÓSITO: ViewModel para mostrar el resultado de la verificación de email
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

namespace CMS.UI.Models.Auth
{
    /// <summary>
    /// ViewModel para la página de resultado de verificación de email
    /// </summary>
    public class EmailVerificationResultViewModel
    {
        /// <summary>
        /// Indica si la verificación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje a mostrar al usuario
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Email que se intentó verificar
        /// </summary>
        public string? Email { get; set; }
    }
}
