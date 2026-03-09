// ================================================================================
// ARCHIVO: CMS.Data/Services/AuditHelper.cs
// PROPÓSITO: Helpers para facilitar la auditoría de entidades EF Core
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-27
// ================================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace CMS.Data.Services;

/// <summary>
/// Clase helper estática para facilitar la extracción de datos de auditoría
/// </summary>
public static class AuditHelper
{
    /// <summary>
    /// Extrae los cambios de una entidad modificada
    /// </summary>
    /// <param name="entry">Entry de EF Core con los cambios</param>
    /// <returns>Diccionario con los cambios: ColumnName -> (OldValue, NewValue)</returns>
    public static Dictionary<string, (string? OldValue, string? NewValue)> GetChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, (string? OldValue, string? NewValue)>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
                continue;

            var columnName = GetColumnName(property.Metadata.PropertyInfo);
            var oldValue = property.OriginalValue?.ToString();
            var newValue = property.CurrentValue?.ToString();

            changes[columnName] = (oldValue, newValue);
        }

        return changes;
    }

    /// <summary>
    /// Extrae todos los valores de una entidad (para DELETE o INSERT)
    /// </summary>
    /// <param name="entity">La entidad</param>
    /// <returns>Diccionario con todos los valores: ColumnName -> Value</returns>
    public static Dictionary<string, string?> GetEntityValues(object entity)
    {
        var values = new Dictionary<string, string?>();

        var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Saltar propiedades de navegación
            if (property.PropertyType.IsClass && 
                property.PropertyType != typeof(string) &&
                !property.PropertyType.IsArray)
            {
                // Verificar si es una colección
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
                    continue;
            }

            // Saltar propiedades marcadas como NotMapped
            if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                continue;

            var columnName = GetColumnName(property);
            var value = property.GetValue(entity);

            values[columnName] = value?.ToString();
        }

        return values;
    }

    /// <summary>
    /// Obtiene el nombre de la columna de la base de datos para una propiedad
    /// </summary>
    public static string GetColumnName(PropertyInfo? propertyInfo)
    {
        if (propertyInfo == null)
            return "unknown";

        var columnAttr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
        return columnAttr?.Name ?? propertyInfo.Name;
    }

    /// <summary>
    /// Obtiene el nombre de la tabla de una entidad
    /// </summary>
    public static string GetTableName<T>() where T : class
    {
        var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableAttr?.Name ?? typeof(T).Name.ToLower();
    }

    /// <summary>
    /// Obtiene el nombre del schema de una entidad
    /// </summary>
    public static string GetSchemaName<T>() where T : class
    {
        var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableAttr?.Schema ?? "admin";
    }

    /// <summary>
    /// Obtiene el nombre y valor de la clave primaria de una entidad
    /// </summary>
    /// <param name="entity">La entidad</param>
    /// <param name="dbContext">El DbContext para obtener metadata</param>
    /// <returns>Tuple con (NombreColumna, Valor)</returns>
    public static (string ColumnName, string Value) GetPrimaryKey(object entity, DbContext dbContext)
    {
        var entityType = dbContext.Model.FindEntityType(entity.GetType());
        if (entityType == null)
        {
            return ("id", "unknown");
        }

        var key = entityType.FindPrimaryKey();
        if (key == null || !key.Properties.Any())
        {
            return ("id", "unknown");
        }

        var pkProperty = key.Properties.First();
        var columnName = pkProperty.GetColumnName() ?? pkProperty.Name;
        var value = pkProperty.PropertyInfo?.GetValue(entity)?.ToString() ?? "unknown";

        return (columnName, value);
    }

    /// <summary>
    /// Compara dos objetos del mismo tipo y devuelve las diferencias
    /// </summary>
    public static Dictionary<string, (string? OldValue, string? NewValue)> CompareEntities<T>(T original, T modified) where T : class
    {
        var changes = new Dictionary<string, (string? OldValue, string? NewValue)>();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Saltar propiedades de navegación y NotMapped
            if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                continue;

            if (property.PropertyType.IsClass && 
                property.PropertyType != typeof(string) &&
                !property.PropertyType.IsArray &&
                typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
                continue;

            var originalValue = property.GetValue(original);
            var modifiedValue = property.GetValue(modified);

            var originalStr = originalValue?.ToString();
            var modifiedStr = modifiedValue?.ToString();

            if (originalStr != modifiedStr)
            {
                var columnName = GetColumnName(property);
                changes[columnName] = (originalStr, modifiedStr);
            }
        }

        return changes;
    }

    /// <summary>
    /// Crea una copia superficial de una entidad para comparación posterior
    /// </summary>
    public static T CloneEntity<T>(T entity) where T : class, new()
    {
        var clone = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            // Saltar propiedades de navegación
            if (property.PropertyType.IsClass && 
                property.PropertyType != typeof(string) &&
                !property.PropertyType.IsArray)
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
                    continue;
            }

            var value = property.GetValue(entity);
            property.SetValue(clone, value);
        }

        return clone;
    }
}
