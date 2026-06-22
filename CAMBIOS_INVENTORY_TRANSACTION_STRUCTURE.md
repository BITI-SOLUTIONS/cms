# CAMBIOS EN ESTRUCTURA DE inventory_transaction

## 📅 Fecha: 2025-01-20

## 🎯 Objetivo
Actualizar el código para reflejar los cambios en la estructura de la tabla `sinai.inventory_transaction`.

---

## 📊 Cambios en la Base de Datos

### Campos MODIFICADOS:
1. **`id_menu`**: Ahora es `NOT NULL` (antes era nullable)
   ```sql
   id_menu integer NOT NULL
   ```

2. **`created_by`**: Longitud aumentada a 150 caracteres
   ```sql
   created_by character varying(150) NOT NULL DEFAULT CURRENT_USER
   ```

3. **`updated_by`**: Longitud aumentada a 150 caracteres
   ```sql
   updated_by character varying(150) NOT NULL DEFAULT CURRENT_USER
   ```

### Campo NUEVO:
4. **`created_by_user_id`**: FK lógica a `cms.admin.user.id_user`
   ```sql
   created_by_user_id integer NOT NULL DEFAULT 0
   ```
   - Almacena el ID del usuario que creó el movimiento
   - FK lógica cross-DB hacia `cms.admin.user`
   - Default: 0

---

## 🔧 Cambios en el Código

### 1. **CMS.Entities/Operational/InventoryTransaction.cs**

#### ✅ Campo `IdMenu` ahora es `int` (NOT NULL):
```csharp
/// <summary>
/// FK lógica cross-db hacia admin.menu.id_menu
/// Indica desde qué pantalla/módulo se creó este movimiento de inventario.
/// No se puede declarar FK real porque admin.menu está en la BD central (cms)
/// y esta tabla está en la BD de la compañía.
/// </summary>
[Required]
[Column("id_menu")]
public int IdMenu { get; set; }
```

#### ✅ Campo `CreatedByUserId` ahora tiene default 0:
```csharp
/// <summary>FK lógica cross-DB hacia cms.admin.user.id_user — usuario que creó el movimiento</summary>
[Column("created_by_user_id")]
public int CreatedByUserId { get; set; } = 0;
```

#### ✅ Campos de auditoría con `MaxLength(150)`:
```csharp
[Required]
[MaxLength(150)]
[Column("created_by")]
public string CreatedBy { get; set; } = string.Empty;

[Required]
[MaxLength(150)]
[Column("updated_by")]
public string UpdatedBy { get; set; } = string.Empty;
```

---

### 2. **CMS.API/Controllers/InventoryTransactionController.cs**

#### ✅ DTOs actualizados:

**CreateInventoryTransactionDto**:
```csharp
public class CreateInventoryTransactionDto
{
	public string? TransactionNumber { get; set; }
	public int IdInventoryTransactionType { get; set; }
	public int IdMenu { get; set; }  // ⚠️ AHORA ES OBLIGATORIO (NOT NULL)
	public int IdWarehouseOrigin { get; set; }
	public int? IdWarehouseDest { get; set; }
	// ... resto de campos
}
```

**UpdateInventoryTransactionDto**:
```csharp
public class UpdateInventoryTransactionDto
{
	public int IdInventoryTransactionType { get; set; }
	public int IdMenu { get; set; }  // ⚠️ AHORA ES OBLIGATORIO (NOT NULL)
	public int IdWarehouseOrigin { get; set; }
	public int? IdWarehouseDest { get; set; }
	// ... resto de campos
}
```

---

### 3. **CMS.UI/wwwroot/js/inventoryMovements.js**

#### ✅ Fallback para `idMenu` en el payload:
```javascript
const payload = {
	transactionNumber: number || undefined,
	idInventoryTransactionType: typeId,
	idMenu: window.INV_MENU_ID || 8,  // ⚠️ Fallback a id_menu=8 si no está definido
	idWarehouseOrigin: originId,
	idWarehouseDest:   destId,
	// ... resto de campos
};
```

**⚠️ NOTA**: El fallback a `8` corresponde al menú "Warehouse & Distribution" (`/Warehouse`). Este es un valor de seguridad por si `window.INV_MENU_ID` no está definido.

---

## 🧪 Verificación

### Build Status: ✅ **EXITOSO**

### Pruebas recomendadas:
1. ✅ Crear un nuevo movimiento de inventario desde `Warehouse/InventoryMovements`
2. ✅ Editar un movimiento existente
3. ✅ Confirmar un movimiento
4. ✅ Recibir un movimiento en tránsito
5. ✅ Verificar que `id_menu` se está guardando correctamente en la BD
6. ✅ Verificar que `created_by_user_id` se inicializa con el ID del usuario actual

---

## 📝 Tipos de Movimiento Nuevos Agregados

Se crearon dos nuevos tipos de movimiento con `show_in_inventory_movements = FALSE`:

### 1. **Customer Invoice** (`CUSTINV`)
- Código: `CUSTINV`
- Nombre: `Customer Invoice`
- Descripción: Factura de cliente - Disminuye inventario por venta a clientes
- Icono: `bi-receipt` 📄
- CSS: `badge-sales`
- Sort Order: 110
- **NO visible en Inventory Movements** (solo desde módulos de facturación)

### 2. **Supplier Invoice** (`SUPPINV`)
- Código: `SUPPINV`
- Nombre: `Supplier Invoice`
- Descripción: Factura de proveedor - Aumenta inventario por compras a proveedores
- Icono: `bi-file-earmark-text` 📋
- CSS: `badge-purchase`
- Sort Order: 120
- **NO visible en Inventory Movements** (solo desde módulos de compras)

### Script de migración:
- Archivo: `CMS.Data/Scripts/101_add_customer_and_supplier_invoice_types.sql`
- Estado: ✅ **Ejecutado exitosamente**

---

## 🎯 Siguientes Pasos

1. Probar la creación de movimientos desde diferentes pantallas
2. Verificar que el rastreo de origen (`id_menu`) funciona correctamente
3. Validar que los nuevos tipos de movimiento NO aparecen en Inventory Movements
4. Los nuevos tipos estarán disponibles para usarlos desde los módulos de facturación cuando se implementen

---

## 📌 Notas Importantes

- **`id_menu`** es ahora **OBLIGATORIO** en todos los movimientos de inventario
- El fallback a `8` (Warehouse & Distribution) solo debe usarse en casos excepcionales
- **Siempre** asignar el `id_menu` correcto según el origen del movimiento
- Los tipos `CUSTINV` y `SUPPINV` están listos para usarse desde los módulos de Sales/Billing y Purchasing

---

## ✅ Resumen de Archivos Modificados

1. ✅ `CMS.Entities/Operational/InventoryTransaction.cs`
2. ✅ `CMS.API/Controllers/InventoryTransactionController.cs`
3. ✅ `CMS.UI/wwwroot/js/inventoryMovements.js`
4. ✅ `CMS.Data/Scripts/101_add_customer_and_supplier_invoice_types.sql` (nuevo)

---

**Autor**: BITI Solutions S.A.  
**Fecha**: 2025-01-20
