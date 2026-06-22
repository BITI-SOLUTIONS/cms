// ================================================================================
// ARCHIVO: CMS.API/Authorization/PermissionAuthorizationHandler.cs
// PROPÓSITO: Handler de autorización personalizado basado en permisos del JWT
// DESCRIPCIÓN: Valida que el usuario tenga el permiso requerido en sus claims
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-19
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CMS.API.Authorization
{
    /// <summary>
    /// Requisito de autorización que especifica un permiso requerido
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionCode { get; }

        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }

    /// <summary>
    /// Handler que valida si el usuario tiene el permiso requerido en sus claims del JWT
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            PermissionRequirement requirement)
        {
            // Obtener el userId del JWT
            var userIdClaim = context.User.FindFirst("userId") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;

            // Obtener el companyId del JWT
            var companyIdClaim = context.User.FindFirst("companyId") ?? context.User.FindFirst("CompanyId");
            var companyId = companyIdClaim?.Value;

            _logger.LogInformation(
                "🔐 Validando permiso '{Permission}' para usuario {UserId} en compañía {CompanyId}", 
                requirement.PermissionCode, 
                userId ?? "UNKNOWN", 
                companyId ?? "UNKNOWN"
            );

            // Obtener todos los claims de tipo "permission" (singular, no "permissions")
            var permissionClaims = context.User.Claims
                .Where(c => c.Type == "permission" || c.Type == "permissions")
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation(
                "📋 Usuario tiene {Count} permisos en JWT: {Permissions}", 
                permissionClaims.Count,
                string.Join(", ", permissionClaims.Take(5)) + (permissionClaims.Count > 5 ? "..." : "")
            );

            // Verificar si el permiso requerido está en los claims
            if (permissionClaims.Contains(requirement.PermissionCode))
            {
                _logger.LogInformation("✅ Permiso '{Permission}' concedido", requirement.PermissionCode);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning(
                    "❌ Permiso '{Permission}' DENEGADO para usuario {UserId}. El permiso no está en el JWT.", 
                    requirement.PermissionCode, 
                    userId ?? "UNKNOWN"
                );
            }

            return Task.CompletedTask;
        }
    }
}
