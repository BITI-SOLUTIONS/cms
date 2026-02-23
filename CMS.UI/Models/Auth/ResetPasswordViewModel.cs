// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/ResetPasswordViewModel.cs
// PROPÓSITO: ViewModel para la pantalla de restablecimiento de contraseña
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Auth
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener al menos: una mayúscula, una minúscula, un número y un carácter especial")]
        public string NewPassword { get; set; } = default!;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = default!;

        public string? CompanySchema { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool TokenValid { get; set; } = true;
    }
}
