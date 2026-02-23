// ================================================================================
// ARCHIVO: CMS.UI/Models/Auth/SelectCompanyViewModel.cs
// PROPÓSITO: ViewModel para la pantalla de selección de compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models.Auth
{
    public class SelectCompanyViewModel
    {
        [Required(ErrorMessage = "El código de compañía es requerido")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 10 caracteres")]
        [Display(Name = "Código de Compañía")]
        public string CompanySchema { get; set; } = default!;

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
