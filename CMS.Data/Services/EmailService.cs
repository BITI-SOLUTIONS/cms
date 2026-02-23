// ================================================================================
// ARCHIVO: CMS.Data/Services/EmailService.cs
// PROPÓSITO: Servicio para envío de correos electrónicos
// DESCRIPCIÓN: Maneja el envío de correos de verificación, bienvenida,
//              recuperación de contraseña y notificaciones.
//              Lee configuración SMTP desde la base de datos (admin.system_config)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-16
// ================================================================================

using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace CMS.Data.Services
{
    public interface IEmailService
    {
        Task<EmailResult> SendVerificationEmailAsync(string toEmail, string displayName, string verificationToken, string temporaryPassword, string baseUrl);
        Task<EmailResult> SendWelcomeEmailAsync(string toEmail, string displayName, string username, string baseUrl);
        Task<EmailResult> SendPasswordResetEmailAsync(string toEmail, string displayName, string resetToken, string temporaryPassword, string baseUrl);
        string GenerateTemporaryPassword(int length = 12);
        string GenerateSecureToken();
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly SystemConfigService _configService;

        public EmailService(ILogger<EmailService> logger, SystemConfigService configService)
        {
            _logger = logger;
            _configService = configService;
        }

        /// <summary>
        /// Envía correo de verificación de email con contraseña temporal
        /// </summary>
        public async Task<EmailResult> SendVerificationEmailAsync(
            string toEmail, 
            string displayName, 
            string verificationToken, 
            string temporaryPassword,
            string baseUrl)
        {
            try
            {
                var verificationUrl = $"{baseUrl}/Account/VerifyEmail?token={Uri.EscapeDataString(verificationToken)}&email={Uri.EscapeDataString(toEmail)}";
                
                var subject = "🔐 Verifica tu cuenta - CMS BITI Solutions";
                var body = GenerateVerificationEmailHtml(displayName, temporaryPassword, verificationUrl);

                var result = await SendEmailAsync(toEmail, subject, body);
                return result ? EmailResult.Success() : EmailResult.Failure("Error enviando email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de verificación a {Email}", toEmail);
                return EmailResult.Failure($"Error enviando correo: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía correo de bienvenida después de verificar email
        /// </summary>
        public async Task<EmailResult> SendWelcomeEmailAsync(string toEmail, string displayName, string username, string baseUrl)
        {
            try
            {
                var subject = "🎉 ¡Bienvenido a CMS BITI Solutions!";
                var body = GenerateWelcomeEmailHtml(displayName, username, baseUrl);

                var result = await SendEmailAsync(toEmail, subject, body);
                return result ? EmailResult.Success() : EmailResult.Failure("Error enviando email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de bienvenida a {Email}", toEmail);
                return EmailResult.Failure($"Error enviando correo: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía correo para restablecer contraseña
        /// </summary>
        public async Task<EmailResult> SendPasswordResetEmailAsync(
            string toEmail, 
            string displayName, 
            string resetToken, 
            string temporaryPassword,
            string baseUrl)
        {
            try
            {
                var resetUrl = $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";
                
                var subject = "🔑 Restablecer contraseña - CMS BITI Solutions";
                var body = GeneratePasswordResetEmailHtml(displayName, temporaryPassword, resetUrl);

                var result = await SendEmailAsync(toEmail, subject, body);
                return result ? EmailResult.Success() : EmailResult.Failure("Error enviando email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de reset a {Email}", toEmail);
                return EmailResult.Failure($"Error enviando correo: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera una contraseña temporal segura
        /// </summary>
        public string GenerateTemporaryPassword(int length = 12)
        {
            const string upperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowerCase = "abcdefghjkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%&*";
            
            var random = RandomNumberGenerator.Create();
            var password = new char[length];
            var allChars = upperCase + lowerCase + digits + special;
            
            // Asegurar al menos uno de cada tipo
            password[0] = GetRandomChar(random, upperCase);
            password[1] = GetRandomChar(random, lowerCase);
            password[2] = GetRandomChar(random, digits);
            password[3] = GetRandomChar(random, special);
            
            // Llenar el resto
            for (int i = 4; i < length; i++)
            {
                password[i] = GetRandomChar(random, allChars);
            }
            
            // Mezclar
            return new string(password.OrderBy(x => GetRandomInt(random)).ToArray());
        }

        /// <summary>
        /// Genera un token seguro para verificación/reset
        /// </summary>
        public string GenerateSecureToken()
        {
            var tokenBytes = new byte[32];
            RandomNumberGenerator.Fill(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        #region Private Methods

        private async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Obtener configuración SMTP desde la BD
                var smtpSettings = await _configService.GetSmtpSettingsAsync();

                _logger.LogInformation("📧 SMTP Config: Host={Host}, Port={Port}, Username={Username}, FromEmail={FromEmail}, IsConfigured={IsConfigured}",
                    smtpSettings.Host, 
                    smtpSettings.Port, 
                    string.IsNullOrEmpty(smtpSettings.Username) ? "(vacío)" : "***",
                    smtpSettings.FromEmail,
                    smtpSettings.IsConfigured);

                if (!smtpSettings.IsConfigured)
                {
                    _logger.LogWarning("⚠️ Configuración SMTP incompleta. Email simulado:");
                    _logger.LogWarning("   To: {To}", to);
                    _logger.LogWarning("   Subject: {Subject}", subject);

                    // En desarrollo, guardar el email en un archivo para debugging
                    var emailFile = Path.Combine(Path.GetTempPath(), $"cms_email_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.html");
                    await File.WriteAllTextAsync(emailFile, $"<h1>To: {to}</h1><h2>Subject: {subject}</h2><hr/>{body}");
                    _logger.LogInformation("📧 Email guardado en: {File}", emailFile);

                    return true; // Simulado exitoso
                }

                _logger.LogInformation("📧 Enviando email REAL a {To} via {Host}:{Port}", to, smtpSettings.Host, smtpSettings.Port);

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
                    IsBodyHtml = true
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("✅ Email enviado exitosamente a {Email}", to);
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

        private static char GetRandomChar(RandomNumberGenerator rng, string chars)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var index = BitConverter.ToUInt32(bytes, 0) % chars.Length;
            return chars[(int)index];
        }

        private static int GetRandomInt(RandomNumberGenerator rng)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        #endregion

        #region Email Templates

        private string GenerateVerificationEmailHtml(string displayName, string temporaryPassword, string verificationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #0f172a; color: #e2e8f0; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #1e293b; border-radius: 12px; padding: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 28px; font-weight: bold; color: #06b6d4; }}
        h1 {{ color: #f8fafc; font-size: 24px; margin-bottom: 20px; }}
        p {{ color: #94a3b8; line-height: 1.6; }}
        .password-box {{ background-color: #334155; border: 2px solid #06b6d4; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0; }}
        .password {{ font-family: 'Consolas', monospace; font-size: 24px; color: #22d3ee; letter-spacing: 2px; }}
        .warning {{ background-color: #7c2d12; border-left: 4px solid #ea580c; padding: 15px; border-radius: 0 8px 8px 0; margin: 20px 0; }}
        .warning strong {{ color: #fb923c; }}
        .btn {{ display: inline-block; background: linear-gradient(135deg, #06b6d4, #0891b2); color: white; text-decoration: none; padding: 15px 40px; border-radius: 8px; font-weight: bold; margin: 25px 0; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #334155; color: #64748b; font-size: 12px; }}
        .steps {{ background-color: #334155; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .step {{ display: flex; align-items: center; margin: 10px 0; }}
        .step-num {{ background-color: #06b6d4; color: #0f172a; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; margin-right: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🏢 CMS - BITI Solutions</div>
        </div>
        
        <h1>¡Hola {displayName}! 👋</h1>
        
        <p>Se ha creado una cuenta para ti en el sistema CMS de BITI Solutions. Para activar tu cuenta, necesitas verificar tu correo electrónico.</p>
        
        <div class='password-box'>
            <p style='margin: 0 0 10px 0; color: #94a3b8;'>Tu contraseña temporal:</p>
            <div class='password'>{temporaryPassword}</div>
        </div>
        
        <div class='warning'>
            <strong>⏰ ¡Importante!</strong>
            <p style='margin: 5px 0 0 0;'>Esta contraseña temporal expira en <strong>30 minutos</strong>. Debes cambiarla después de verificar tu correo.</p>
        </div>
        
        <div class='steps'>
            <p style='color: #f8fafc; font-weight: bold; margin-top: 0;'>📋 Pasos para activar tu cuenta:</p>
            <div class='step'><span class='step-num'>1</span> Haz clic en el botón de verificación</div>
            <div class='step'><span class='step-num'>2</span> Ingresa tu contraseña temporal</div>
            <div class='step'><span class='step-num'>3</span> Crea una nueva contraseña segura</div>
            <div class='step'><span class='step-num'>4</span> ¡Listo! Ya puedes usar el sistema</div>
        </div>
        
        <div style='text-align: center;'>
            <a href='{verificationUrl}' class='btn'>✅ Verificar mi correo</a>
        </div>
        
        <p style='font-size: 12px; color: #64748b;'>Si no puedes hacer clic en el botón, copia y pega este enlace en tu navegador:<br/>
        <span style='color: #06b6d4; word-break: break-all;'>{verificationUrl}</span></p>
        
        <div class='footer'>
            <p>Este correo fue enviado automáticamente por CMS - BITI Solutions S.A.</p>
            <p>Si no solicitaste esta cuenta, puedes ignorar este mensaje.</p>
            <p>© {DateTime.UtcNow.Year} BITI Solutions S.A. - Costa Rica</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailHtml(string displayName, string username, string baseUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #0f172a; color: #e2e8f0; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #1e293b; border-radius: 12px; padding: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 28px; font-weight: bold; color: #06b6d4; }}
        .welcome-icon {{ font-size: 64px; margin: 20px 0; }}
        h1 {{ color: #22c55e; font-size: 28px; margin-bottom: 20px; text-align: center; }}
        p {{ color: #94a3b8; line-height: 1.6; }}
        .info-box {{ background-color: #334155; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #475569; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #94a3b8; }}
        .info-value {{ color: #f8fafc; font-weight: bold; }}
        .btn {{ display: inline-block; background: linear-gradient(135deg, #22c55e, #16a34a); color: white; text-decoration: none; padding: 15px 40px; border-radius: 8px; font-weight: bold; margin: 25px 0; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #334155; color: #64748b; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🏢 CMS - BITI Solutions</div>
        </div>
        
        <div style='text-align: center;'>
            <div class='welcome-icon'>🎉</div>
            <h1>¡Bienvenido al equipo!</h1>
        </div>
        
        <p>Hola <strong>{displayName}</strong>,</p>
        
        <p>Tu cuenta ha sido verificada exitosamente. Ya puedes acceder a todas las funcionalidades del sistema CMS de BITI Solutions.</p>
        
        <div class='info-box'>
            <div class='info-row'>
                <span class='info-label'>Usuario:</span>
                <span class='info-value'>{username}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Estado:</span>
                <span class='info-value' style='color: #22c55e;'>✅ Verificado</span>
            </div>
        </div>
        
        <div style='text-align: center;'>
            <a href='{baseUrl}' class='btn'>🚀 Iniciar sesión</a>
        </div>
        
        <p style='color: #64748b; font-size: 14px;'>
            <strong>💡 Consejos de seguridad:</strong><br/>
            • Nunca compartas tu contraseña con nadie<br/>
            • Usa una contraseña única para este sistema<br/>
            • Cierra sesión cuando uses computadoras compartidas
        </p>
        
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} BITI Solutions S.A. - Costa Rica</p>
            <p>Soporte: soporte@biti-solutions.com</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetEmailHtml(string displayName, string temporaryPassword, string resetUrl)
        {
            // ⭐ Template optimizado para Gmail - evita que se corte el contenido
            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Restablecer Contraseña</title>
</head>
<body style='margin:0;padding:0;background-color:#0f172a;font-family:Arial,Helvetica,sans-serif;'>
<table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color:#0f172a;'>
<tr><td align='center' style='padding:20px 10px;'>
<table role='presentation' width='600' cellspacing='0' cellpadding='0' border='0' style='max-width:600px;background-color:#1e293b;border-radius:12px;'>
<tr><td align='center' style='padding:30px 20px 20px;'>
<span style='font-size:24px;font-weight:bold;color:#06b6d4;'>🏢 CMS - BITI Solutions</span>
</td></tr>
<tr><td style='padding:0 30px;'>
<h1 style='color:#f8fafc;font-size:22px;margin:0 0 15px;'>🔑 Restablecer Contraseña</h1>
<p style='color:#94a3b8;font-size:14px;line-height:1.5;margin:0 0 20px;'>Hola <strong style='color:#f8fafc;'>{displayName}</strong>, hemos recibido una solicitud para restablecer tu contraseña.</p>
<table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color:#334155;border:2px solid #eab308;border-radius:8px;margin:0 0 15px;'>
<tr><td align='center' style='padding:15px;'>
<span style='color:#94a3b8;font-size:13px;'>Tu contraseña temporal:</span><br/>
<span style='font-family:Consolas,monospace;font-size:22px;color:#fde047;letter-spacing:2px;font-weight:bold;'>{temporaryPassword}</span>
</td></tr>
</table>
<table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color:#7c2d12;border-left:4px solid #ea580c;border-radius:0 8px 8px 0;margin:0 0 20px;'>
<tr><td style='padding:12px 15px;'>
<span style='color:#fef2f2;font-size:13px;'><strong>⏰ ¡Importante!</strong> Esta contraseña expira en <strong>30 minutos</strong>.</span>
</td></tr>
</table>
<table role='presentation' cellspacing='0' cellpadding='0' border='0' align='center' style='margin:0 0 20px;'>
<tr><td align='center' style='background:linear-gradient(135deg,#eab308,#ca8a04);border-radius:8px;'>
<a href='{resetUrl}' target='_blank' style='display:inline-block;padding:14px 35px;color:#0f172a;text-decoration:none;font-weight:bold;font-size:14px;'>🔐 Cambiar mi contraseña</a>
</td></tr>
</table>
<p style='color:#64748b;font-size:12px;line-height:1.4;margin:0 0 20px;'>Si no solicitaste este cambio, ignora este correo.</p>
</td></tr>
<tr><td align='center' style='padding:20px;border-top:1px solid #334155;'>
<span style='color:#64748b;font-size:11px;'>© {DateTime.UtcNow.Year} BITI Solutions S.A. - Costa Rica</span>
</td></tr>
</table>
</td></tr>
</table>
</body>
</html>";
        }

        #endregion
    }

    /// <summary>
    /// Resultado del envío de email
    /// </summary>
    public class EmailResult
    {
        public bool IsSuccess { get; private set; }
        public string? Message { get; private set; }
        public string? ErrorMessage { get; private set; }

        private EmailResult() { }

        public static EmailResult Success(string? message = null) => new()
        {
            IsSuccess = true,
            Message = message ?? "Email enviado exitosamente"
        };

        public static EmailResult Failure(string error) => new()
        {
            IsSuccess = false,
            ErrorMessage = error
        };
    }
}
