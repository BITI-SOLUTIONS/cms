// ================================================================================
// ARCHIVO: CMS.Entities/Operational/GlobalParameter.cs
// PROPÓSITO: Entidad para parámetros globales por módulo (por compañía)
// DESCRIPCIÓN: Almacena configuraciones dinámicas organizadas por módulo en cada BD de compañía
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-01-22
// MODIFICADO: 2026-01-22 - Movido de Admin a Operational (ahora por compañía)
//             2026-01-22 - Cambiado module_name→id_menu, parameter_key→code
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    /// <summary>
    /// Parámetros de configuración global del sistema organizados por módulo.
    /// Soporta múltiples tipos de datos: string, boolean, integer, decimal, json.
    /// NOTA: Esta tabla está en la BD de cada compañía (ej: sinai.global_parameter)
    /// </summary>
    [Table("global_parameter")]
    public class GlobalParameter : IAuditableEntity
    {
        [Key]
        [Column("id_global_parameter")]
        public int ID { get; set; }

        /// <summary>
        /// ID del menú al que pertenece el parámetro.
        /// RELACIÓN LÓGICA CROSS-DB: referencia cms.admin.menu.id_menu
        /// No se puede declarar FK real porque esta tabla está en la BD de la compañía
        /// y admin.menu está en la BD central (cms).
        /// </summary>
        [Column("id_menu")]
        [Required]
        public int MenuId { get; set; }

        /// <summary>
        /// Código único del parámetro en formato snake_case
        /// Ejemplo: "allows_returns_received_in_transit"
        /// </summary>
        [Column("code")]
        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = default!;

        /// <summary>
        /// Nombre descriptivo del parámetro para mostrar en UI
        /// </summary>
        [Column("parameter_name")]
        [Required]
        [MaxLength(200)]
        public string ParameterName { get; set; } = default!;

        /// <summary>
        /// Descripción detallada del parámetro y su propósito
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Tipo de dato del parámetro: string, boolean, integer, decimal, json
        /// </summary>
        [Column("data_type")]
        [Required]
        [MaxLength(20)]
        public string DataType { get; set; } = "string";

        /// <summary>
        /// Valor del parámetro cuando DataType = 'string'
        /// </summary>
        [Column("value_string")]
        public string? ValueString { get; set; }

        /// <summary>
        /// Valor del parámetro cuando DataType = 'boolean'
        /// </summary>
        [Column("value_boolean")]
        public bool? ValueBoolean { get; set; }

        /// <summary>
        /// Valor del parámetro cuando DataType = 'integer'
        /// </summary>
        [Column("value_integer")]
        public int? ValueInteger { get; set; }

        /// <summary>
        /// Valor del parámetro cuando DataType = 'decimal'
        /// </summary>
        [Column("value_decimal", TypeName = "decimal(18,4)")]
        public decimal? ValueDecimal { get; set; }

        /// <summary>
        /// Valor del parámetro cuando DataType = 'json'
        /// </summary>
        [Column("value_json", TypeName = "jsonb")]
        public string? ValueJson { get; set; }

        /// <summary>
        /// Categoría opcional para agrupar parámetros dentro de un módulo
        /// </summary>
        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Orden de visualización dentro del módulo
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Indica si es un parámetro del sistema (no se puede eliminar)
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Indica si el parámetro está activo
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // IAuditableEntity
        // ============================================================

        [Column("createdate")]
        public DateTime CreateDate { get; set; }

        [Column("record_date")]
        public DateTime RecordDate { get; set; }

        [Column("created_by")]
        [MaxLength(150)]
        public string CreatedBy { get; set; } = default!;

        [Column("updated_by")]
        [MaxLength(150)]
        public string UpdatedBy { get; set; } = default!;

        [Column("rowpointer")]
        public Guid RowPointer { get; set; }

        // ============================================================
        // Métodos Helper
        // ============================================================

        /// <summary>
        /// Obtiene el valor del parámetro según su tipo de dato
        /// </summary>
        public object? GetValue()
        {
            return DataType?.ToLower() switch
            {
                "boolean" => ValueBoolean,
                "integer" => ValueInteger,
                "decimal" => ValueDecimal,
                "json" => ValueJson,
                _ => ValueString
            };
        }

        /// <summary>
        /// Establece el valor del parámetro según su tipo de dato
        /// </summary>
        public void SetValue(object? value)
        {
            // Si el valor es un JsonElement, extraer su valor real
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                value = jsonElement.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Number => jsonElement.TryGetInt32(out var intVal) ? intVal : 
                                                            jsonElement.TryGetDecimal(out var decVal) ? decVal : 
                                                            (object?)null,
                    System.Text.Json.JsonValueKind.String => jsonElement.GetString(),
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            switch (DataType?.ToLower())
            {
                case "boolean":
                    ValueBoolean = value != null ? Convert.ToBoolean(value) : null;
                    break;
                case "integer":
                    ValueInteger = value != null ? Convert.ToInt32(value) : null;
                    break;
                case "decimal":
                    ValueDecimal = value != null ? Convert.ToDecimal(value) : null;
                    break;
                case "json":
                    ValueJson = value?.ToString();
                    break;
                default:
                    ValueString = value?.ToString();
                    break;
            }
        }
    }
}
