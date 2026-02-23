// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/LoginViewModel.cs
// PROPÓSITO: ViewModel para la pantalla de login con email/contraseña
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = default!;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? CompanySchema { get; set; }
        public string? CompanyName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
        public int? RemainingAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
