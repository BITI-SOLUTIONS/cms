// ================================================================================
// ARCHIVO: CMS.UI/Controllers/AccountController.cs
// PROPÓSITO: Maneja login, logout y callbacks de Azure AD
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-11
// ================================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.UI.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Inicia el flujo de login con Azure AD
        /// </summary>
        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var redirectUrl = Url.Action(nameof(SignInCallback), "Account", new { returnUrl });

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            _logger.LogInformation("🔐 Iniciando login con Azure AD. ReturnUrl: {ReturnUrl}", returnUrl);

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Callback después del login exitoso de Azure AD.
        /// El token JWT ya se obtuvo en Program.cs (OnTokenValidated).
        /// </summary>
        [HttpGet]
        public IActionResult SignInCallback(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Verificar si hay JWT en sesión
            var hasToken = !string.IsNullOrEmpty(HttpContext.Session.GetString("ApiToken"));

            if (hasToken)
            {
                _logger.LogInformation("✅ Login completado exitosamente. Redirigiendo a: {ReturnUrl}", returnUrl);
                return LocalRedirect(returnUrl);
            }
            else
            {
                _logger.LogWarning("⚠️ Login completado pero no se obtuvo JWT");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            _logger.LogInformation("🚪 Usuario cerrando sesión");

            // Limpiar sesión
            HttpContext.Session.Clear();

            // Cerrar sesión de Azure AD
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);

            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl
                });

            return new EmptyResult();
        }

        /// <summary>
        /// Página después de cerrar sesión
        /// </summary>
        [HttpGet]
        public IActionResult SignedOut()
        {
            _logger.LogInformation("✅ Sesión cerrada exitosamente");
            return View();
        }

        /// <summary>
        /// Maneja acceso denegado
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("⛔ Acceso denegado");
            return View();
        }
    }
}