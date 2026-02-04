using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace CMS.UI
{
    public class AuthenticatedApiMessageHandler : DelegatingHandler
    {
        private readonly ITokenAcquisition _tokenAcquisition;

        public AuthenticatedApiMessageHandler(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string[] scopes = new[]
            {
                "api://b231a44d-7e9d-4d9b-8866-9a4b3c5ab5cd/access_as_user"
            };

            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}