// ================================================================================
// ARCHIVO: CMS.API/Controllers/SupportController.cs
// PROPÓSITO: API para solicitudes de soporte técnico
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-04
// ACTUALIZADO: 2026-03-04 - Corregido uso de SystemConfigService para SMTP
// ================================================================================

using CMS.Data;
using CMS.Data.Services;
using CMS.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;

namespace CMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SystemConfigService _configService;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            AppDbContext context, 
            SystemConfigService configService,
            ILogger<SupportController> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Envía una solicitud de soporte técnico
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult> SendSupportRequest([FromBody] SupportRequestDto request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
                return Unauthorized();

            try
            {
                // Obtener configuración SMTP usando SystemConfigService
                var smtpSettings = await _configService.GetSmtpSettingsAsync();

                if (!smtpSettings.IsConfigured)
                {
                    _logger.LogError("❌ Configuración SMTP no está completa. Host={Host}, FromEmail={FromEmail}", 
                        smtpSettings.Host, smtpSettings.FromEmail);
                    return StatusCode(500, new { message = "Configuración de correo no disponible" });
                }

                _logger.LogInformation("📧 Usando SMTP: {Host}:{Port} desde {FromEmail}", 
                    smtpSettings.Host, smtpSettings.Port, smtpSettings.FromEmail);

                // El correo se envía AL correo de soporte (FromEmail)
                var supportEmail = smtpSettings.FromEmail;

                // Construir el mensaje
                var subjectText = GetSubjectText(request.Subject);
                var emailSubject = $"[CMS Soporte] {subjectText} - {request.CompanyName}";

                var emailBody = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .info-box {{ background: #f5f5f5; border-left: 4px solid #667eea; padding: 15px; margin: 15px 0; }}
        .footer {{ background: #333; color: #aaa; padding: 15px; text-align: center; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>🎫 Nueva Solicitud de Soporte</h2>
    </div>
    <div class='content'>
        <div class='info-box'>
            <p><strong>👤 Usuario:</strong> {request.UserName}</p>
            <p><strong>📧 Email:</strong> {request.UserEmail}</p>
            <p><strong>🏢 Compañía:</strong> {request.CompanyName}</p>
            <p><strong>📋 Asunto:</strong> {subjectText}</p>
            <p><strong>📅 Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm} (UTC-6)</p>
        </div>
        
        <h3>📝 Descripción del Problema:</h3>
        <p style='background: #fff; border: 1px solid #ddd; padding: 15px; border-radius: 5px;'>
            {request.Message.Replace("\n", "<br/>")}
        </p>
    </div>
    <div class='footer'>
        <p>Este mensaje fue enviado desde CMS - BITI Solutions S.A</p>
        <p>Sistema de Gestión Empresarial</p>
    </div>
</body>
</html>";

                // Enviar correo
                using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                    EnableSsl = smtpSettings.UseSsl,
                    Timeout = smtpSettings.TimeoutSeconds * 1000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings.FromEmail!, smtpSettings.FromName),
                    Subject = emailSubject,
                    Body = emailBody,
                    IsBodyHtml = true
                };

                // Enviar a soporte
                mailMessage.To.Add(supportEmail!);

                // CC al usuario para confirmar que se envió
                if (!string.IsNullOrEmpty(request.UserEmail))
                {
                    mailMessage.CC.Add(request.UserEmail);
                }

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("✅ Solicitud de soporte enviada: {Subject} de {User}", 
                    subjectText, request.UserEmail);

                // Registrar actividad
                await LogActivityAsync(userId, ActivityTypes.SUPPORT_REQUEST, 
                    $"Solicitud de soporte enviada: {subjectText}");

                return Ok(new { message = "Solicitud enviada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error enviando solicitud de soporte");
                return StatusCode(500, new { message = "Error al enviar la solicitud. Intenta de nuevo." });
            }
        }

        private static string GetSubjectText(string subjectCode)
        {
            return subjectCode switch
            {
                "problema_cuenta" => "Problema con mi cuenta",
                "problema_acceso" => "No puedo acceder al sistema",
                "error_sistema" => "Error en el sistema",
                "solicitud_permiso" => "Solicitud de permisos",
                "otro" => "Consulta general",
                _ => "Solicitud de soporte"
            };
        }

        private async Task LogActivityAsync(int userId, string activityType, string description)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            int? companyId = int.TryParse(companyIdClaim, out var cid) ? cid : null;

            var log = new UserActivityLog
            {
                ID_USER = userId,
                ID_COMPANY = companyId,
                ACTIVITY_TYPE = activityType,
                ACTIVITY_DESCRIPTION = description,
                IP_ADDRESS = HttpContext.Connection.RemoteIpAddress?.ToString(),
                USER_AGENT = Request.Headers.UserAgent.ToString(),
                DEVICE_INFO = GetDeviceInfo(Request.Headers.UserAgent.ToString()),
                IS_SUCCESS = true,
                ACTIVITY_DATE = DateTime.UtcNow
            };

            _context.UserActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private string GetDeviceInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Desconocido";
            if (userAgent.Contains("Windows")) return "Windows PC";
            if (userAgent.Contains("Mac")) return "Mac";
            if (userAgent.Contains("iPhone")) return "iPhone";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("Linux")) return "Linux";
            return "Navegador Web";
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("userId")?.Value;

            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }
    }

    // =====================================================
    // DTOs
    // =====================================================

    public class SupportRequestDto
    {
        public string Subject { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? CompanyName { get; set; }
    }
}
