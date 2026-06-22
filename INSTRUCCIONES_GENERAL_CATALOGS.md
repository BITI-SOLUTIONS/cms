# Instrucciones para activar el menú General Catalogs en Accounting

## Resumen de cambios realizados

Se completó exitosamente la implementación de la pantalla de **General Catalogs** para el mantenimiento de catálogos generales de contabilidad (específicamente razones de cancelación de asientos).

### Cambios en el código:

1. ✅ **Entidad actualizada**: `CMS.Entities/Operational/JournalEntryCancelReason.cs`
   - Campo renombrado de `DisplayOrder` a `SortOrder` para coincidir con la BD

2. ✅ **Servicio actualizado**: `CMS.Data/Services/JournalEntryCancelReasonService.cs`
   - Todas las referencias actualizadas a `SortOrder`

3. ✅ **DbContext actualizado**: `CMS.Data/CompanyDbContext.cs`
   - Índice actualizado a `SortOrder`

4. ✅ **API Controller actualizado**: `CMS.API/Controllers/JournalEntryCancelReasonController.cs`
   - DTO actualizado con campo `SortOrder`

5. ✅ **Vista actualizada**: `CMS.UI/Views/Accounting/GeneralCatalogs.cshtml`
   - Todas las referencias JavaScript actualizadas a `sortOrder`

6. ✅ **Compilación exitosa**: Todo el código compila correctamente

### Scripts SQL pendientes de ejecución:

Para hacer la pantalla visible en el menú de Accounting, necesitas ejecutar los siguientes scripts SQL en la base de datos `cms`:

#### 1. Crear permisos (ejecutar primero):

```bash
psql -h localhost -U cmssystem -d cms -f CMS.Data/Scripts/112_add_general_catalogs_permissions.sql
```

Este script crea los siguientes permisos:
- `Accounting.GeneralCatalogs.View` - Ver catálogos generales
- `Accounting.GeneralCatalogs.Create` - Crear registros
- `Accounting.GeneralCatalogs.Edit` - Editar registros
- `Accounting.GeneralCatalogs.Delete` - Eliminar registros

#### 2. Crear entrada de menú (ejecutar segundo):

```bash
psql -h localhost -U cmssystem -d cms -f CMS.Data/Scripts/111_add_general_catalogs_menu.sql
```

Este script:
- Crea la entrada de menú "General Catalogs" bajo Accounting
- La asocia con el permiso `Accounting.GeneralCatalogs.View`
- Le asigna el ícono `bi-card-list`
- La coloca al final de los submenús de Accounting

#### 3. Asignar permisos a tu usuario:

Después de ejecutar los scripts, necesitas asignar los permisos a tu usuario. Puedes hacerlo desde la UI:

1. Ve a **Administration > Company Management**
2. Selecciona la compañía donde quieres activar el módulo
3. En la pestaña de **Users**, selecciona tu usuario
4. Asigna los permisos de General Catalogs que necesites

O ejecuta este SQL (reemplaza `<TU_USER_ID>` y `<COMPANY_ID>`):

```sql
-- Asignar todos los permisos de General Catalogs a tu usuario
INSERT INTO admin.user_company_permission (user_id, company_id, permission_id, is_allowed)
SELECT 
	<TU_USER_ID>,
	<COMPANY_ID>,
	id_permission,
	true
FROM admin.permission
WHERE module = 'Accounting' AND category = 'GeneralCatalogs'
ON CONFLICT (user_id, company_id, permission_id) DO UPDATE
SET is_allowed = true;
```

### Verificación:

Después de ejecutar los scripts y asignar los permisos:

1. Cierra sesión y vuelve a iniciar sesión en el CMS
2. Navega al módulo **Accounting**
3. Deberías ver la nueva opción **General Catalogs** en el submenú
4. Al hacer clic, se abrirá la pantalla de mantenimiento de catálogos generales
5. Actualmente solo contiene el catálogo de **Razones de Cancelación de Asientos**

### Funcionalidad disponible:

La pantalla permite:
- ✅ Ver todas las razones de cancelación registradas
- ✅ Crear nuevas razones
- ✅ Editar razones existentes
- ✅ Eliminar razones (si no están en uso)
- ✅ Activar/desactivar razones
- ✅ Ordenar razones por `sort_order`
- ✅ Búsqueda y filtrado

### Notas técnicas:

- La tabla en la BD es: `{company_schema}.journal_entry_cancel_reason`
- La API está en: `/api/JournalEntryCancelReason`
- El controller UI está en: `AccountingController.GeneralCatalogs()`
- La vista está en: `Views/Accounting/GeneralCatalogs.cshtml`

---

**Estado actual**: ✅ Código listo y compilado. Solo faltan los scripts SQL de menú/permisos.
