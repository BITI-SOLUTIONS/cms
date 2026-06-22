# ✅ IMPLEMENTACIÓN COMPLETADA: Columna id_menu en inventory_transaction

## 📋 Resumen de Cambios

Se agregó la columna `id_menu` a la tabla `sinai.inventory_transaction` para rastrear desde qué pantalla/módulo se originó cada movimiento de inventario. Esta es una **relación lógica cross-database** hacia `admin.menu` en la BD central (cms), por lo que NO se puede declarar como FK real.

---

## 🗂️ Archivos Modificados

### 1. **Base de Datos**
- ✅ `CMS.Data/Scripts/026_add_id_menu_to_inventory_transaction.sql` (NUEVO)
  - Script de migración que agrega la columna `id_menu`
  - Incluye índice, comentarios y verificación
  - **PENDIENTE EJECUCIÓN** en la base de datos

### 2. **Entidad**
- ✅ `CMS.Entities/Operational/InventoryTransaction.cs`
  - Agregada propiedad `public int? IdMenu { get; set; }`
  - Mapeada a columna `id_menu`
  - Incluye documentación de la relación cross-db

### 3. **API Controller**
- ✅ `CMS.API/Controllers/InventoryTransactionController.cs`
  - `CreateInventoryTransactionDto`: agregado campo `IdMenu`
  - `UpdateInventoryTransactionDto`: agregado campo `IdMenu`
  - `MapFromDto()`: mapea `IdMenu = dto.IdMenu`
  - Acción `Update()`: ahora incluye `IdMenu` en la construcción de la entidad

### 4. **Servicio**
- ✅ `CMS.Data/Services/InventoryTransactionService.cs`
  - Método `UpdateAsync()`: actualiza `existing.IdMenu = transaction.IdMenu`
  - Método `CreateAsync()`: ya recibe la entidad completa, por lo que `IdMenu` se persiste automáticamente

### 5. **UI Controller**
- ✅ `CMS.UI/Controllers/WarehouseController.cs`
  - Acción `InventoryMovements()`: pasa `ViewBag.IdMenu = 8` (Warehouse & Distribution)

### 6. **Vista Razor**
- ✅ `CMS.UI/Views/Warehouse/InventoryMovements.cshtml`
  - Captura `var idMenu = ViewBag.IdMenu as int? ?? 0;`
  - Expone `window.INV_MENU_ID = @idMenu;` en JavaScript

### 7. **JavaScript Frontend**
- ✅ `CMS.UI/wwwroot/js/inventoryMovements.js`
  - Función `saveMovement()`: incluye `idMenu: window.INV_MENU_ID || null` en el payload

---

## 🚀 Pasos para Completar la Implementación

### 1. Ejecutar el Script de Migración

✅ **COMPLETADO** - La migración se ejecutó exitosamente en la base de datos `sinai`.

**Comando ejecutado:**
```bash
$env:PGPASSWORD='POStgres2026'; psql -h 10.0.0.1 -p 5432 -U postgres -d sinai
```

**Resultados:**
- ✅ Columna `id_menu` creada exitosamente
- ✅ Comentario agregado a la columna
- ✅ Índice `ix_sinai_inventory_transaction_id_menu` creado
- 📊 **11 registros existentes** con `id_menu = NULL` (registros históricos)

### 2. Verificar la Migración

✅ **VERIFICADO** - La migración se verificó exitosamente.

**Resultados de verificación:**

```sql
-- Columna creada correctamente
column_name | data_type | is_nullable | column_default 
-------------+-----------+-------------+----------------
id_menu     | integer   | YES         | 

-- Índice creado correctamente
indexname: ix_sinai_inventory_transaction_id_menu
indexdef: CREATE INDEX ix_sinai_inventory_transaction_id_menu ON sinai.inventory_transaction USING btree (id_menu)

-- Estado de registros
total_registros: 11
con_id_menu: 0
sin_id_menu: 11 (registros históricos)
```

### 3. (Opcional) Backfill de Datos Históricos

Si deseas asignar un `id_menu` por defecto a los registros existentes:

```sql
-- Opción A: Asignar el menú "Warehouse & Distribution" (id_menu = 8)
UPDATE sinai.inventory_transaction 
SET id_menu = 8 
WHERE id_menu IS NULL;

-- Opción B: Asignar según un submenú específico si existe
-- (consultar admin.menu para obtener el id_menu correcto)
```

### 4. Probar la Funcionalidad

1. **Iniciar la aplicación** (CMS.UI y CMS.API)
2. **Navegar a** `/Warehouse/InventoryMovements`
3. **Crear un nuevo movimiento** de inventario
4. **Verificar en la BD** que `id_menu = 8`:

```sql
SELECT 
	id_inventory_transaction,
	transaction_number,
	id_menu,
	id_inventory_transaction_type,
	transaction_date
FROM sinai.inventory_transaction
ORDER BY id_inventory_transaction DESC
LIMIT 5;
```

5. **Editar un movimiento existente** y verificar que `id_menu` se mantiene

---

## 🔍 Verificación de Build

```bash
✅ Build successful
```

Todos los cambios compilan correctamente.

---

## 📝 Notas Importantes

### Relación Cross-Database
- La columna `id_menu` es una **relación lógica**, NO una FK real
- `admin.menu` está en la BD central (`cms`)
- `sinai.inventory_transaction` está en la BD de la compañía (`sinai`)
- La integridad referencial se mantiene a nivel de aplicación

### ID del Menú Actual
Según el archivo `.github/copilot-instructions.md`, el menú principal "Warehouse & Distribution" tiene `id_menu = 8`. Si existe un submenú específico para "Inventory Movements", actualizar el valor en:
- `CMS.UI/Controllers/WarehouseController.cs` → línea 134: `ViewBag.IdMenu = 8;`

### Extensibilidad
Si en el futuro se crean movimientos de inventario desde otras pantallas (ej: desde `Sales`, `Purchasing`, etc.), cada controlador debe:
1. Pasar el `id_menu` correspondiente via `ViewBag`
2. Exponer la variable en JavaScript
3. Incluirla en el payload de creación/actualización

---

## 🎯 Ejemplo de Consulta de Auditoría

Una vez implementado, podrás rastrear desde qué menú se crearon los movimientos:

```sql
SELECT 
	it.transaction_number,
	it.transaction_date,
	m.name AS menu_origen,
	m.url AS menu_url,
	itt.code AS tipo_movimiento,
	its.code AS estado
FROM sinai.inventory_transaction it
LEFT JOIN cms.admin.menu m ON m.id_menu = it.id_menu
LEFT JOIN cms.admin.inventory_transaction_type itt ON itt.id_inventory_transaction_type = it.id_inventory_transaction_type
LEFT JOIN cms.admin.inventory_transaction_status its ON its.id_inventory_transaction_status = it.id_inventory_transaction_status
ORDER BY it.transaction_date DESC
LIMIT 20;
```

---

## ✅ Lista de Verificación Final

- [x] Script de migración creado
- [x] Entidad `InventoryTransaction` actualizada
- [x] DTOs del controller extendidos
- [x] Mapeos de DTOs actualizados
- [x] Servicio `UpdateAsync()` actualizado
- [x] Controller UI pasa `IdMenu` a la vista
- [x] Vista Razor expone `IdMenu` a JavaScript
- [x] JavaScript incluye `IdMenu` en payload
- [x] Build exitoso
- [x] **Script de migración ejecutado en BD**
- [x] **Migración verificada exitosamente**
- [ ] **PENDIENTE:** Probar creación de movimiento en UI
- [ ] **PENDIENTE:** Verificar que `id_menu` se guarda correctamente en nuevos registros

---

## 🔗 Referencias

- **Copilot Instructions**: `.github/copilot-instructions.md`
- **Estructura de menús**: Admin → Menu Management (ID 153)
- **BD Central**: `cms` (schema: `admin`)
- **BD Compañía**: `sinai` (schema: `sinai`)

---

**Fecha de Implementación:** 2026-01-23  
**Autor:** EAMR, BITI SOLUTIONS S.A
