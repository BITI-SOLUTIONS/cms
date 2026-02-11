// ================================================================================
// ARCHIVO: CMS.UI/Services/AuthenticatedApiMessageHandler.cs
// PROPÓSITO: Handler HTTP que agrega JWT (del API propio) a requests autenticadas
// ACTUALIZADO: 2026-02-11
// ================================================================================

namespace CMS.UI.Services
{
    /// <summary>
    /// HTTP Message Handler que intercepta requests al API y agrega el JWT.
    /// El JWT se obtiene del session storage después del login de Azure AD.
    /// </summary>
    public class AuthenticatedApiMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticatedApiMessageHandler> _logger;

        public AuthenticatedApiMessageHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticatedApiMessageHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogWarning("⚠️ HttpContext no disponible");
                return await base.SendAsync(request, cancellationToken);
            }

            var token = httpContext.Session.GetString("ApiToken");
            var tokenExpiry = httpContext.Session.GetString("ApiTokenExpiry");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("⚠️ No hay JWT en sesión - Usuario necesita login");
                return await base.SendAsync(request, cancellationToken);
            }

            // ⭐ Verificar si el token expiró. Parse en modo UTC/ISO
            if (!string.IsNullOrEmpty(tokenExpiry) &&
                DateTime.TryParse(tokenExpiry, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
            {
                if (DateTime.UtcNow >= expiry)
                {
                    _logger.LogWarning("⚠️ JWT expirado - Usuario necesita re-login");
                    httpContext.Session.Remove("ApiToken");
                    httpContext.Session.Remove("ApiTokenExpiry");
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _logger.LogDebug("✅ JWT agregado: {Method} {Uri}", request.Method, request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}