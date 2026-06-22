# ✅ IMPLEMENTACIÓN COMPLETADA: Campo show_in_inventory_movements

## 🎉 Resumen Ejecutivo

Se agregó exitosamente el campo `show_in_inventory_movements` a la tabla `admin.inventory_transaction_type` y se configuró el filtrado en la pantalla Warehouse/InventoryMovements para mostrar solo los tipos de movimiento con este flag en TRUE.

---

## 📊 Estado de la Implementación

### Base de Datos ✅
- **Tabla**: `admin.inventory_transaction_type` (BD central: cms)
- **Columna agregada**: `show_in_inventory_movements BOOLEAN NOT NULL DEFAULT TRUE`
- **Estado inicial**: Los 10 tipos existentes tienen el valor TRUE por defecto
- **Índice**: No requerido (columna booleana de filtrado)

### Código ✅
- **Entidad actualizada**: `CMS.Entities/Admin/InventoryTransactionType.cs`
- **API Controller**: `CMS.API/Controllers/InventoryTransactionTypeController.cs`
- **Frontend**: `CMS.UI/wwwroot/js/inventoryMovements.js`
- **Build**: ✅ Successful

---

## 📁 Archivos Modificados

### 1. Base de Datos
**`CMS.Data/Scripts/100_add_show_in_inventory_movements_to_transaction_type.sql`** (NUEVO)
```sql
ALTER TABLE admin.inventory_transaction_type 
ADD COLUMN show_in_inventory_movements BOOLEAN NOT NULL DEFAULT TRUE;
```
- ✅ Ejecutado en BD central (cms)
- ✅ Todos los tipos existentes con valor TRUE

### 2. Entidad
**`CMS.Entities/Admin/InventoryTransactionType.cs`**
```csharp
[Column("show_in_inventory_movements")]
public bool ShowInInventoryMovements { get; set; } = true;
```

### 3. API Controller
**`CMS.API/Controllers/InventoryTransactionTypeController.cs`**

**Cambios realizados:**
- Método `GetAll()` acepta parámetro `showInInventoryMovements` para filtrar
- Respuesta incluye el campo `ShowInInventoryMovements`
- Método `Update()` actualiza el campo

**Endpoint actualizado:**
```
GET /api/inventory-transaction-type?isActive=true&showInInventoryMovements=true
```

### 4. Frontend
**`CMS.UI/wwwroot/js/inventoryMovements.js`**

**Función actualizada:**
```javascript
async function loadMovementTypes() {
	const types = await invFetch(
		'/api/inventory-transaction-type?isActive=true&showInInventoryMovements=true'
	);
	INV.movementTypes = types || [];
	// ... poblar selects
}
```

---

## 🎯 Comportamiento Actual

### Tipos de Movimiento (10 en total)
Todos con `show_in_inventory_movements = TRUE` por defecto:

1. Transfer - Traslado entre Bodegas ✅
2. TransitTransfer - Traslado vía Tránsito ✅
3. PurchaseReceipt - Entrada por Compra ✅
4. SaleIssue - Salida por Venta ✅
5. AdjustmentIn - Ajuste de Inventario (+) ✅
6. AdjustmentOut - Ajuste de Inventario (-) ✅
7. CustomerReturn - Devolución de Cliente ✅
8. SupplierReturn - Devolución a Proveedor ✅
9. WriteOff - Merma / Baja ✅
10. PhysicalCount - Conteo Físico ✅

### Pantalla Warehouse/InventoryMovements
- **Dropdown "Tipo de Movimiento"**: Muestra solo tipos con `show_in_inventory_movements = TRUE`
- **Filtro de búsqueda**: También usa solo tipos visibles
- **Comportamiento actual**: Todos los 10 tipos se muestran (valor por defecto TRUE)

---

## 🔧 Cómo Ocultar un Tipo de Movimiento

Si deseas que un tipo NO aparezca en Inventory Movements, actualiza el campo en la BD:

```sql
-- Ejemplo: Ocultar "PhysicalCount" en InventoryMovements
UPDATE admin.inventory_transaction_type 
SET show_in_inventory_movements = FALSE 
WHERE code = 'PhysicalCount';
```

O usa el endpoint del API (si tienes una pantalla de administración):

```http
PUT /api/inventory-transaction-type/{id}
Content-Type: application/json

{
  "code": "PhysicalCount",
  "name": "Conteo Físico",
  "showInInventoryMovements": false,
  ...otros campos
}
```

**Resultado:** El tipo "PhysicalCount" ya no aparecerá en los dropdowns de Warehouse/InventoryMovements.

---

## 🧪 Cómo Probar

### 1. Verificar en Base de Datos
```sql
-- Ver estado actual de todos los tipos
SELECT code, name, show_in_inventory_movements, is_active 
FROM admin.inventory_transaction_type 
ORDER BY sort_order;
```

### 2. Probar en Frontend
1. Iniciar aplicación (API + UI)
2. Navegar a `/Warehouse/InventoryMovements`
3. Clic en "Nuevo Movimiento"
4. Verificar que el dropdown "Tipo de Movimiento" muestre los tipos correctos

### 3. Probar Filtrado
```sql
-- Ocultar un tipo temporalmente
UPDATE admin.inventory_transaction_type 
SET show_in_inventory_movements = FALSE 
WHERE code = 'WriteOff';
```

**Resultado esperado:** El tipo "WriteOff" (Merma/Baja) ya no debe aparecer en el dropdown.

```sql
-- Restaurar
UPDATE admin.inventory_transaction_type 
SET show_in_inventory_movements = TRUE 
WHERE code = 'WriteOff';
```

### 4. Verificar API
```bash
# PowerShell
$token = "tu_token_jwt"
Invoke-RestMethod -Uri "https://localhost:7001/api/inventory-transaction-type?showInInventoryMovements=true" `
  -Headers @{ Authorization = "Bearer $token" } | ConvertTo-Json
```

---

## 📊 Consultas Útiles

### Ver tipos visibles y ocultos
```sql
SELECT 
	CASE 
		WHEN show_in_inventory_movements THEN 'Visible' 
		ELSE 'Oculto' 
	END AS estado,
	COUNT(*) AS cantidad
FROM admin.inventory_transaction_type
GROUP BY show_in_inventory_movements;
```

### Listar tipos con su estado de visibilidad
```sql
SELECT 
	code,
	name,
	CASE 
		WHEN show_in_inventory_movements THEN '✅ Visible' 
		ELSE '❌ Oculto' 
	END AS visibilidad,
	is_active,
	sort_order
FROM admin.inventory_transaction_type
ORDER BY sort_order;
```

---

## ✅ Lista de Verificación

- [x] Script SQL creado
- [x] Script ejecutado en BD central (cms)
- [x] Columna agregada con valor por defecto TRUE
- [x] Entidad actualizada
- [x] API Controller actualizado (GetAll con filtro)
- [x] API Controller actualizado (Update incluye campo)
- [x] Frontend actualizado (loadMovementTypes con filtro)
- [x] Build exitoso
- [ ] **PENDIENTE:** Probar ocultar un tipo y verificar en UI
- [ ] **PENDIENTE:** (Opcional) Crear pantalla de administración para gestionar visibilidad

---

## 🎯 Resultado Final

```
┌─────────────────────────────────────────────────────────┐
│  ✅ CAMPO show_in_inventory_movements IMPLEMENTADO      │
├─────────────────────────────────────────────────────────┤
│  • Base de datos: admin.inventory_transaction_type      │
│  • Columna: BOOLEAN NOT NULL DEFAULT TRUE               │
│  • API: Filtrado por showInInventoryMovements           │
│  • Frontend: Solo muestra tipos con TRUE                │
│  • Estado inicial: 10 tipos con TRUE (todos visibles)   │
└─────────────────────────────────────────────────────────┘
```

---

## 🔮 Próximos Pasos (Opcional)

### Crear Pantalla de Administración
Si deseas gestionar la visibilidad desde la UI en lugar de SQL:

1. Crear vista en `/Admin/InventoryTransactionTypes`
2. Agregar checkbox "Mostrar en Inventory Movements"
3. El CRUD ya está listo en el API Controller

### Aplicar Filtrado a Otras Pantallas
Si necesitas que otros módulos también filtren tipos:
- Actualizar sus respectivos `loadMovementTypes()` con el parámetro `showInInventoryMovements`
- O crear campos adicionales: `show_in_purchases`, `show_in_sales`, etc.

---

**Implementado**: 2026-01-23  
**Build status**: ✅ Successful  
**Migración BD**: ✅ Ejecutada y verificada  
**Sistema listo**: ✅ Para uso inmediato  

🚀 **¡Todo listo! Ahora puedes controlar qué tipos de movimiento se muestran en Inventory Movements.**
