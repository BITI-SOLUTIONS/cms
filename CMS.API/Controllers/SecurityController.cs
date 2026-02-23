// ================================================================================
// ARCHIVO: CMS.API/Controllers/SecurityController.cs
// PROPÓSITO: Endpoints para operaciones de seguridad (hash, encriptación, etc.)
// DESCRIPCIÓN: Proporciona utilidades para generar hashes de contraseñas
//              y encriptar/desencriptar valores de configuración
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using CMS.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;
        private readonly EncryptionService _encryptionService;

        public SecurityController(
            ILogger<SecurityController> logger,
            EncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
        }

        // =====================================================================
        // DTOs
        // =====================================================================

        public class HashPasswordRequest
        {
            public string Password { get; set; } = default!;
        }

        public class HashPasswordResponse
        {
            public bool Success { get; set; }
            public string? Hash { get; set; }
            public string? Message { get; set; }
        }

        public class VerifyPasswordRequest
        {
            public string Password { get; set; } = default!;
            public string Hash { get; set; } = default!;
        }

        public class VerifyPasswordResponse
        {
            public bool Success { get; set; }
            public bool IsValid { get; set; }
            public string? Message { get; set; }
        }

        public class EncryptRequest
        {
            public string PlainText { get; set; } = default!;
        }

        public class EncryptResponse
        {
            public bool Success { get; set; }
            public string? EncryptedValue { get; set; }
            public string? Message { get; set; }
        }

        public class DecryptRequest
        {
            public string EncryptedText { get; set; } = default!;
        }

        public class DecryptResponse
        {
            public bool Success { get; set; }
            public string? DecryptedValue { get; set; }
            public string? Message { get; set; }
        }

        // =====================================================================
        // ENDPOINTS: Hash de Contraseñas
        // =====================================================================

        /// <summary>
        /// Genera un hash PBKDF2 de una contraseña.
        /// Este hash se puede almacenar en la columna password_hash de admin.user
        /// </summary>
        [HttpPost("hash-password")]
        [AllowAnonymous] // ⚠️ En producción, considera proteger este endpoint
        public ActionResult<HashPasswordResponse> HashPassword([FromBody] HashPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new HashPasswordResponse
                    {
                        Success = false,
                        Message = "La contraseña es requerida"
                    });
                }

                if (request.Password.Length < 8)
                {
                    return BadRequest(new HashPasswordResponse
                    {
                        Success = false,
                        Message = "La contraseña debe tener al menos 8 caracteres"
                    });
                }

                var hash = PasswordHasher.HashPassword(request.Password);

                _logger.LogInformation("🔐 Hash generado exitosamente");

                return Ok(new HashPasswordResponse
                {
                    Success = true,
                    Hash = hash,
                    Message = "Hash generado exitosamente. Puede usar este valor en admin.user.password_hash"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generando hash");
                return StatusCode(500, new HashPasswordResponse
                {
                    Success = false,
                    Message = "Error interno al generar el hash"
                });
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash
        /// </summary>
        [HttpPost("verify-password")]
        [AllowAnonymous]
        public ActionResult<VerifyPasswordResponse> VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Hash))
                {
                    return BadRequest(new VerifyPasswordResponse
                    {
                        Success = false,
                        Message = "La contraseña y el hash son requeridos"
                    });
                }

                var isValid = PasswordHasher.VerifyPassword(request.Password, request.Hash);

                return Ok(new VerifyPasswordResponse
                {
                    Success = true,
                    IsValid = isValid,
                    Message = isValid ? "Contraseña válida" : "Contraseña incorrecta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verificando contraseña");
                return StatusCode(500, new VerifyPasswordResponse
                {
                    Success = false,
                    Message = "Error interno al verificar la contraseña"
                });
            }
        }

        // =====================================================================
        // ENDPOINTS: Encriptación de Configuraciones
        // =====================================================================

        /// <summary>
        /// Encripta un valor para almacenarlo de forma segura en admin.system_config
        /// Usar para campos con is_encrypted = true (ej: contraseñas SMTP)
        /// </summary>
        [HttpPost("encrypt")]
        [AllowAnonymous] // ⚠️ TEMPORAL para configuración inicial - cambiar a [Authorize] en producción
        public ActionResult<EncryptResponse> Encrypt([FromBody] EncryptRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.PlainText))
                {
                    return BadRequest(new EncryptResponse
                    {
                        Success = false,
                        Message = "El texto a encriptar es requerido"
                    });
                }

                var encrypted = _encryptionService.Encrypt(request.PlainText);

                _logger.LogInformation("🔒 Valor encriptado exitosamente");

                return Ok(new EncryptResponse
                {
                    Success = true,
                    EncryptedValue = encrypted,
                    Message = "Valor encriptado exitosamente. Use este valor en admin.system_config.config_value"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error encriptando valor");
                return StatusCode(500, new EncryptResponse
                {
                    Success = false,
                    Message = "Error interno al encriptar"
                });
            }
        }

        /// <summary>
        /// Desencripta un valor almacenado en admin.system_config
        /// </summary>
        [HttpPost("decrypt")]
        [Authorize] // Requiere autenticación
        public ActionResult<DecryptResponse> Decrypt([FromBody] DecryptRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.EncryptedText))
                {
                    return BadRequest(new DecryptResponse
                    {
                        Success = false,
                        Message = "El texto encriptado es requerido"
                    });
                }

                var decrypted = _encryptionService.Decrypt(request.EncryptedText);

                _logger.LogInformation("🔓 Valor desencriptado exitosamente");

                return Ok(new DecryptResponse
                {
                    Success = true,
                    DecryptedValue = decrypted,
                    Message = "Valor desencriptado exitosamente"
                });
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return BadRequest(new DecryptResponse
                {
                    Success = false,
                    Message = "No se pudo desencriptar: el valor es inválido o la clave es incorrecta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error desencriptando valor");
                return StatusCode(500, new DecryptResponse
                {
                    Success = false,
                    Message = "Error interno al desencriptar"
                });
            }
        }

        /// <summary>
        /// Genera un token seguro aleatorio (útil para reset de contraseña, API keys, etc.)
        /// </summary>
        [HttpGet("generate-token")]
        [AllowAnonymous]
        public ActionResult<object> GenerateToken([FromQuery] int length = 32)
        {
            try
            {
                if (length < 16 || length > 128)
                {
                    return BadRequest(new { success = false, message = "La longitud debe estar entre 16 y 128" });
                }

                var token = PasswordHasher.GenerateSecureToken(length);

                return Ok(new
                {
                    success = true,
                    token = token,
                    length = length,
                    message = "Token generado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generando token");
                return StatusCode(500, new { success = false, message = "Error interno al generar token" });
            }
        }
    }
}
