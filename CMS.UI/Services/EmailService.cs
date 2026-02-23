// ================================================================================
// ARCHIVO: CMS.UI/Services/EmailService.cs
// PROPÓSITO: Servicio para envío de correos electrónicos
// DESCRIPCIÓN: Maneja el envío de correos de recuperación de contraseña,
//              notificaciones y otros correos del sistema.
//              Lee configuración SMTP desde la base de datos (admin.system_config)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ACTUALIZADO: 2026-02-13 - Configuración desde BD
// ================================================================================

using CMS.Data.Services;
using CMS.Entities;
using System.Net;
using System.Net.Mail;

namespace CMS.UI.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly SystemConfigService _configService;

        public EmailService(ILogger<EmailService> logger, SystemConfigService configService)
        {
            _logger = logger;
            _configService = configService;
        }

        /// <summary>
        /// Envía un correo de recuperación de contraseña
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(
            string toEmail,
            string displayName,
            string resetToken,
            string companySchema,
            string companyName,
            string baseUrl)
        {
            try
            {
                var resetUrl = $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}&company={Uri.EscapeDataString(companySchema)}";

                var subject = $"[{companyName}] Restablecer contraseña";
                var body = GeneratePasswordResetEmailBody(displayName, resetUrl, companyName);

                var result = await SendEmailAsync(toEmail, subject, body, isHtml: true);

                if (result)
                {
                    _logger.LogInformation("✅ Email de reset enviado a: {Email}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error enviando email de reset a: {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Envía un correo de notificación de bloqueo de cuenta
        /// </summary>
        public async Task<bool> SendAccountLockedEmailAsync(
            string toEmail,
            string displayName,
            string companyName,
            DateTime lockoutEnd)
        {
            try
            {
                var subject = $"[{companyName}] Cuenta bloqueada temporalmente";
                var body = GenerateAccountLockedEmailBody(displayName, companyName, lockoutEnd);

                var result = await SendEmailAsync(toEmail, subject, body, isHtml: true);

                if (result)
                {
                    _logger.LogInformation("✅ Email de bloqueo enviado a: {Email}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error enviando email de bloqueo a: {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Envía un correo genérico usando configuración SMTP de la BD
        /// </summary>
        private async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Obtener configuración SMTP desde la BD
                var smtpSettings = await _configService.GetSmtpSettingsAsync();

                if (!smtpSettings.IsConfigured)
                {
                    _logger.LogError("❌ Configuración SMTP incompleta. Configure en admin.system_config");
                    return false;
                }

                _logger.LogInformation("📧 Enviando email a {To} via {Host}:{Port}", to, smtpSettings.Host, smtpSettings.Port);

                using var client = new SmtpClient(smtpSettings.Host, smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                    EnableSsl = smtpSettings.UseSsl,
                    Timeout = smtpSettings.TimeoutSeconds * 1000
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(smtpSettings.FromEmail!, smtpSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "❌ Error SMTP enviando email a {To}: {Message}", to, smtpEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error enviando email a {To}", to);
                return false;
            }
        }

        #region Email Templates

        private static string GeneratePasswordResetEmailBody(string displayName, string resetUrl, string companyName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .button:hover {{ background: #5a67d8; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Restablecer Contraseña</h1>
            <p>{companyName}</p>
        </div>
        <div class='content'>
            <h2>Hola {displayName},</h2>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>
            <center>
                <a href='{resetUrl}' class='button'>Restablecer Contraseña</a>
            </center>
            <div class='warning'>
                <strong>⚠️ Importante:</strong>
                <ul>
                    <li>Este enlace expira en 24 horas</li>
                    <li>Si no solicitaste este cambio, ignora este correo</li>
                    <li>Tu contraseña actual permanecerá sin cambios</li>
                </ul>
            </div>
            <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
            <p style='word-break: break-all; font-size: 12px; color: #666;'>{resetUrl}</p>
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
            <p>&copy; {DateTime.UtcNow.Year} {companyName} - Todos los derechos reservados</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateAccountLockedEmailBody(string displayName, string companyName, DateTime lockoutEnd)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(lockoutEnd, TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time"));
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .info-box {{ background: #e7f3ff; border: 1px solid #2196F3; padding: 15px; border-radius: 5px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 Cuenta Bloqueada</h1>
            <p>{companyName}</p>
        </div>
        <div class='content'>
            <h2>Hola {displayName},</h2>
            <p>Tu cuenta ha sido bloqueada temporalmente debido a múltiples intentos de inicio de sesión fallidos.</p>
            <div class='info-box'>
                <strong>📅 Tu cuenta será desbloqueada automáticamente:</strong>
                <p style='font-size: 18px; margin: 10px 0;'>{localTime:dddd, dd MMMM yyyy 'a las' HH:mm} (Hora Costa Rica)</p>
            </div>
            <p>Si necesitas acceso inmediato, puedes restablecer tu contraseña usando la opción <strong>""¿Olvidaste tu contraseña?""</strong> en la pantalla de login.</p>
            <p>Si no fuiste tú quien intentó acceder, te recomendamos cambiar tu contraseña inmediatamente.</p>
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
            <p>&copy; {DateTime.UtcNow.Year} {companyName} - Todos los derechos reservados</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
