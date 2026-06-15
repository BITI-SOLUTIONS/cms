// ================================================================================
// ARCHIVO: CMS.UI/Controllers/HRController.cs
// PROPÓSITO: Controlador de vistas para el módulo Human Resources
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-04
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class HRController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public HRController(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration       = configuration;
        }

        private string GetApiBaseUrl()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var baseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"];
            return baseUrl ?? (environment == "Production"
                ? "https://cms.biti-solutions.com"
                : "https://localhost:7001");
        }

        private void SetViewBag()
        {
            ViewBag.ApiBaseUrl = GetApiBaseUrl();
            ViewBag.ApiToken   = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken")
                              ?? _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                              ?? string.Empty;
        }

        // ============================================================
        // GET /HR/Employees
        // ============================================================
        [HttpGet]
        public IActionResult Employees()
        {
            SetViewBag();
            return View();
        }

        // ============================================================
        // GET /HR/Positions
        // ============================================================
        [HttpGet]
        public IActionResult Positions()
        {
            SetViewBag();
            return View();
        }
    }
}
