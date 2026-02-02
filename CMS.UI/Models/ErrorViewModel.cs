// ================================================================================
// ARCHIVO: CMS.UI/Models/ErrorViewModel.cs
// PROP�SITO: Modelo para la vista de error predeterminada de ASP.NET Core
// DESCRIPCI�N: Proporciona informaci�n contextual sobre errores HTTP y excepciones
//              no controladas. Se usa en conjunto con la vista Error.cshtml para
//              mostrar detalles t�cnicos en modo desarrollo y mensajes gen�ricos
//              en producci�n.
// PATR�N: Model-View pattern est�ndar de ASP.NET Core MVC
// AUTOR: System CMS - BITI Solutions S.A
// CREADO: 2024
// ================================================================================

namespace CMS.UI.Models
{
    /// <summary>
    /// Modelo de vista para la p�gina de error del sistema.
    /// Se utiliza en la vista Error.cshtml para mostrar informaci�n sobre errores
    /// HTTP (404, 500, etc.) y excepciones no controladas.
    /// 
    /// USO:
    /// - En modo Development: Muestra el RequestId para debugging
    /// - En modo Production: Muestra un mensaje gen�rico sin exponer detalles internos
    /// 
    /// FLUJO:
    /// 1. Ocurre una excepci�n o error HTTP
    /// 2. El middleware de excepciones captura el error
    /// 3. Redirecciona a /Home/Error con el RequestId
    /// 4. El HomeController devuelve la vista Error.cshtml con este modelo
    /// 5. La vista muestra el error seg�n el ambiente (Dev/Prod)
    /// 
    /// CONFIGURACI�N:
    /// En Program.cs debe estar configurado:
    /// - app.UseExceptionHandler("/Home/Error")
    /// - app.UseStatusCodePagesWithReExecute("/Home/Error/{0}")
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// ID �nico de la petici�n HTTP que gener� el error.
        /// Se obtiene autom�ticamente del contexto HTTP mediante Activity.Current?.Id
        /// o HttpContext.TraceIdentifier.
        /// 
        /// PROP�SITO:
        /// - Correlacionar errores en logs con peticiones espec�ficas
        /// - Facilitar el debugging en entornos distribuidos
        /// - Permitir al soporte t�cnico rastrear errores reportados por usuarios
        /// 
        /// FORMATO T�PICO:
        /// - Ejemplo: "0HMVD9K7L3V5N:00000001"
        /// - Es generado autom�ticamente por ASP.NET Core para cada request
        /// 
        /// SEGURIDAD:
        /// - Es seguro mostrar en producci�n (no expone datos sensibles)
        /// - Solo identifica la transacci�n, no el contenido
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica si se debe mostrar el RequestId en la vista de error.
        /// 
        /// L�GICA:
        /// - true: Muestra el RequestId en un bloque destacado
        /// - false: Oculta el RequestId (�til cuando es null o vac�o)
        /// 
        /// USO EN LA VISTA:
        /// @if (Model.ShowRequestId)
        /// {
        ///     <p><strong>Request ID:</strong> <code>@Model.RequestId</code></p>
        /// }
        /// 
        /// RECOMENDACI�N:
        /// - Development: Siempre mostrar (true)
        /// - Production: Mostrar solo si el RequestId es v�lido
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // ========================================================================
        // PROPIEDADES OPCIONALES (Puedes agregar seg�n necesidades futuras)
        // ========================================================================

        /// <summary>
        /// C�digo de estado HTTP del error (opcional).
        /// Ejemplos: 404 (Not Found), 500 (Internal Server Error), 403 (Forbidden)
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Mensaje de error amigable para el usuario (opcional).
        /// NO debe contener detalles t�cnicos en producci�n.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Ruta que se estaba intentando acceder cuando ocurri� el error (opcional).
        /// �til para debugging y an�lisis de errores frecuentes.
        /// </summary>
        public string? RequestPath { get; set; }
    }
}