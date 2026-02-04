using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CMS.UI.Services
{
    public class UserSyncApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<UserSyncApiService> _logger;

        public UserSyncApiService(IHttpClientFactory factory, ILogger<UserSyncApiService> logger)
        {
            _http = factory.CreateClient("cmsapi");
            _logger = logger;
        }

        public class AzureUserInfo
        {
            public string? ObjectId { get; set; }
            public string? UserPrincipalName { get; set; }
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
        }

        public class CmsUserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = default!;
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
            public List<string> Roles { get; set; } = new();
        }

        public async Task<CmsUserDto?> SyncAzureUserAsync(AzureUserInfo azureUser)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/sync-user", azureUser);
                response.EnsureSuccessStatusCode();

                var dto = await response.Content.ReadFromJsonAsync<CmsUserDto>();
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando usuario de Azure AD con API.");
                return null;
            }
        }
    }
}