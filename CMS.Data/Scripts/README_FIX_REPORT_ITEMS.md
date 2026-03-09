# SOLUCIÓN DEFINITIVA: Reporte de Listado de Artículos

## ⚠️ Problema Principal

El error `column i.item_code does not exist` ocurre porque:
1. **La tabla `sinai.item` NO EXISTE** en la base de datos SINAI
2. Los scripts de creación de tabla no se han ejecutado

## 🔧 Solución (2 pasos)

### Paso 1: Ejecutar en BD `cms`

**Conexión**: `Host=10.0.0.1; Database=cms; Username=cmssystem`

Ejecutar la **PARTE A** del archivo `018_SOLUCION_DEFINITIVA_REPORTE_ITEMS.sql`:
- Actualiza la configuración del reporte
- Recrea los filtros correctamente (DROPDOWN estático, no DROPDOWN_SQL)
- Recrea las columnas

```bash
# Desde línea de comandos
psql -h 10.0.0.1 -U cmssystem -d cms -f CMS.Data/Scripts/018_SOLUCION_DEFINITIVA_REPORTE_ITEMS.sql
```

O copiar y pegar el contenido de **PARTE A** en pgAdmin/DBeaver conectado a `cms`.

### Paso 2: Ejecutar en BD `sinai`

**Conexión**: `Host=10.0.0.1; Database=sinai; Username=cmssystem`

Ejecutar la **PARTE B** del archivo `018_SOLUCION_DEFINITIVA_REPORTE_ITEMS.sql`:
- Crea el schema `sinai` si no existe
- Crea la tabla `sinai.item`
- Inserta 15 registros de prueba
- Otorga permisos

```bash
# Desde línea de comandos  
psql -h 10.0.0.1 -U cmssystem -d sinai -c "contenido de PARTE B"
```

**IMPORTANTE**: La PARTE B está comentada en el archivo SQL. Debes:
1. Copiar desde `CREATE SCHEMA IF NOT EXISTS sinai;` hasta `LIMIT 5;`
2. Pegar en pgAdmin/DBeaver conectado a la BD `sinai`

### Paso 3: Reiniciar aplicaciones

Reiniciar CMS.API y CMS.UI

## ✅ Verificación

1. Ir a **Reports & BI > General > Listado de Artículos**
2. El reporte debe mostrar los 15 artículos de prueba
3. Los filtros deben funcionar:
   - **Buscar**: Filtrar por código, nombre, marca, descripción
   - **Estado**: Activos, Inactivos, Todos
   - **Clasificación**: Solo tiene "Todas" (sin tabla classification aún)
   - **Fecha Desde/Hasta**: Filtrar por fecha de creación

## 📋 Cambios en código (ya aplicados)

1. `ReportController.cs` - `GenerateWhereCondition()`: 
   - Agregados filtros específicos para reporte de items (search, isActive, dateFrom, dateTo)
   - Manejo de errores PostgreSQL mejorado

2. `View.cshtml`: 
   - Mensaje de error mejorado con botón "Reintentar"

## 🗂️ Archivos SQL

- `016_fix_report_items_and_filters.sql` - Correcciones parciales (obsoleto)
- `017_create_item_table_sinai.sql` - Solo crea tabla (reemplazado)
- `018_SOLUCION_DEFINITIVA_REPORTE_ITEMS.sql` - **USAR ESTE** (completo)
