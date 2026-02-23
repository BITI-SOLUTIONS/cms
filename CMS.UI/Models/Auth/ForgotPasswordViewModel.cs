// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/ForgotPasswordViewModel.cs
// PROPÓSITO: ViewModel para la pantalla de recuperación de contraseña
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = default!;

        public string? CompanySchema { get; set; }
        public string? CompanyName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
