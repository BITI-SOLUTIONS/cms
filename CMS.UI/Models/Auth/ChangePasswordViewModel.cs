// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/ChangePasswordViewModel.cs
// PROPÓSITO: ViewModel para cambio de contraseña de usuario autenticado
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Auth
{
    /// <summary>
    /// ViewModel para el formulario de cambio de contraseña
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje de éxito después de cambiar la contraseña
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Mensaje de error si falla el cambio
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
