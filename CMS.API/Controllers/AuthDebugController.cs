using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthDebugController : ControllerBase
    {
        [Authorize]
        [HttpGet("token")]
        public IActionResult Token()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            return Ok(new
            {
                token_received = authHeader
            });
        }

        [HttpGet("headers")]
        public IActionResult Headers()
        {
            return Ok(Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()));
        }
    }
}
