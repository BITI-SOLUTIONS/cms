using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace CMS.UI
{
    public class AuthenticatedApiMessageHandler : DelegatingHandler
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticatedApiMessageHandler(
            ITokenAcquisition tokenAcquisition,
            IHttpContextAccessor httpContextAccessor)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // ✅ Solo agregar token si el usuario está autenticado
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    string[] scopes = new[]
                    {
                        "api://b231a44d-7e9d-4d9b-8866-9a4b3c5ab5cd/access_as_user"
                    };

                    var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                catch (MsalUiRequiredException)
                {
                    // Usuario necesita re-autenticarse - dejar sin token
                }
                catch (Exception ex)
                {
                    // Log el error pero continuar sin token
                    Console.WriteLine($"⚠️ Error obteniendo token: {ex.Message}");
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}