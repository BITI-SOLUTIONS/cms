using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

[Route("auth-test")]
public class AuthTestController : Controller
{
    private readonly ITokenAcquisition _token;

    public AuthTestController(ITokenAcquisition token)
    {
        _token = token;
    }

    [Authorize]
    [HttpGet("token")]
    public async Task<IActionResult> GetToken()
    {
        string[] scopes = { "api://b231a44d-7e9d-4d9b-8866-9a4b3c5ab5cd/access_as_user" };

        try
        {
            var token = await _token.GetAccessTokenForUserAsync(scopes);
            return Content(token);
        }
        catch (Exception ex)
        {
            return Content("ERROR: " + ex.Message);
        }
    }
}