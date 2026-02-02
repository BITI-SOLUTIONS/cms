using CMS.Data;
using CMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        public class AzureUserInfo
        {
            public string? ObjectId { get; set; }          // oid
            public string? UserPrincipalName { get; set; } // UPN
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

        [HttpPost("sync-user")]
        public async Task<ActionResult<CmsUserDto>> SyncUser([FromBody] AzureUserInfo model)
        {
            if (string.IsNullOrEmpty(model.ObjectId) && string.IsNullOrEmpty(model.UserPrincipalName))
            {
                return BadRequest("Azure user without ObjectId/UPN.");
            }

            Guid? oid = null;
            if (Guid.TryParse(model.ObjectId, out var parsed))
            {
                oid = parsed;
            }

            // Buscar por OID o UPN
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                (oid != null && u.AZURE_OID == oid) ||
                (model.UserPrincipalName != null && u.AZURE_UPN == model.UserPrincipalName));

            if (user == null)
            {
                // Crear usuario nuevo
                user = new User
                {
                    USER_NAME = model.UserPrincipalName ?? model.Email ?? model.ObjectId ?? "unknown",
                    AZURE_OID = oid,
                    AZURE_UPN = model.UserPrincipalName,
                    EMAIL = model.Email ?? model.UserPrincipalName ?? "",
                    DISPLAY_NAME = model.DisplayName ?? model.UserPrincipalName ?? "",
                    IS_ACTIVE = true,
                    FIRST_NAME = "System",
                    LAST_NAME = "User",
                    PHONE_NUMBER = string.Empty,
                    TIME_ZONE = "UTC",
                    DATE_OF_BIRTH = DateTime.UtcNow,
                    RecordDate = DateTime.UtcNow,
                    CreateDate = DateTime.UtcNow,
                    RowPointer = Guid.NewGuid(),
                    CreatedBy = "SYSTEM-AAD",
                    UpdatedBy = "SYSTEM-AAD"
                };

                _db.Users.Add(user);
            }
            else
            {
                // Actualizar datos básicos
                user.EMAIL = model.Email ?? user.EMAIL;
                user.DISPLAY_NAME = model.DisplayName ?? user.DISPLAY_NAME;
                user.AZURE_UPN = model.UserPrincipalName ?? user.AZURE_UPN;
                user.AZURE_OID = oid ?? user.AZURE_OID;
                user.UpdatedBy = "SYSTEM-AAD";
            }

            await _db.SaveChangesAsync();

            // Traer roles del usuario
            var roles = await (from ur in _db.UserRoles
                               join r in _db.Roles on ur.RoleId equals r.ID_ROLE
                               where ur.UserId == user.ID_USER
                               select r.ROLE_NAME).ToListAsync();

            var dto = new CmsUserDto
            {
                Id = user.ID_USER,
                Username = user.USER_NAME,
                DisplayName = user.DISPLAY_NAME,
                Email = user.EMAIL,
                Roles = roles
            };

            return Ok(dto);
        }
    }
}
