using System.Security.Claims;

namespace CMS.API.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetCmsUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst("cms_user_id")?.Value;

            if (string.IsNullOrEmpty(value))
                throw new UnauthorizedAccessException("cms_user_id missing");

            return int.Parse(value);
        }
    }
}