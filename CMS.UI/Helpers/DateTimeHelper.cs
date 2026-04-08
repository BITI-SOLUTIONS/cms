// ================================================================================
// ARCHIVO: CMS.UI/Helpers/DateTimeHelper.cs
// PROPÓSITO: Helper para convertir fechas UTC al timezone local de la compañía
// DESCRIPCIÓN: Las fechas se almacenan en UTC en la BD. Este helper convierte
//              usando el offset almacenado en admin.country.time_zone (ej: "UTC-06:00")
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

using System.Text.RegularExpressions;

namespace CMS.UI.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly Regex OffsetRegex = new(@"UTC([+-])(\d{2}):(\d{2})", RegexOptions.Compiled);

        /// <summary>
        /// Parsea un string de offset UTC (ej: "UTC-06:00", "UTC+05:30") a minutos totales.
        /// Retorna 0 si el formato no es reconocido.
        /// </summary>
        public static int ParseOffsetMinutes(string? utcOffsetString)
        {
            if (string.IsNullOrEmpty(utcOffsetString)) return 0;
            var match = OffsetRegex.Match(utcOffsetString);
            if (!match.Success) return 0;
            var sign = match.Groups[1].Value == "+" ? 1 : -1;
            var hours = int.Parse(match.Groups[2].Value);
            var minutes = int.Parse(match.Groups[3].Value);
            return sign * (hours * 60 + minutes);
        }

        /// <summary>
        /// Convierte una fecha UTC al horario local de la compañía.
        /// </summary>
        /// <param name="utcDate">Fecha en UTC (DateTimeKind.Utc o Unspecified almacenado como UTC)</param>
        /// <param name="utcOffsetString">Offset del timezone, ej: "UTC-06:00"</param>
        public static DateTime ToCompanyTime(DateTime utcDate, string? utcOffsetString)
        {
            var offsetMinutes = ParseOffsetMinutes(utcOffsetString);
            if (offsetMinutes == 0) return utcDate;
            return DateTime.SpecifyKind(utcDate, DateTimeKind.Utc)
                           .Add(TimeSpan.FromMinutes(offsetMinutes));
        }

        /// <summary>
        /// Formatea una fecha UTC como fecha local de la compañía.
        /// </summary>
        public static string FormatDate(DateTime utcDate, string? utcOffsetString, string format = "dd/MM/yyyy")
            => ToCompanyTime(utcDate, utcOffsetString).ToString(format);

        /// <summary>
        /// Formatea una fecha UTC como fecha+hora local de la compañía.
        /// </summary>
        public static string FormatDateTime(DateTime utcDate, string? utcOffsetString, string format = "dd/MM/yyyy HH:mm")
            => ToCompanyTime(utcDate, utcOffsetString).ToString(format);
    }
}
