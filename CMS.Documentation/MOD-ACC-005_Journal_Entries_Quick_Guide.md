# 📒 Journal Entries (Asientos de Diario) - Guía Rápida

> **Módulo de Contabilidad - Sistema CMS**  
> Estado: ✅ **COMPLETADO 100%**

---

## 🎯 ¿Qué es?

El módulo de **Journal Entries** permite registrar, editar, contabilizar y revertir asientos contables siguiendo el principio de **partida doble** (débito = crédito).

---

## 🚀 Acceso Rápido

**URL:** `/Accounting/JournalEntries`

**Permisos:** (futuro)
- `Accounting.JournalEntries.View`
- `Accounting.JournalEntries.Create`
- `Accounting.JournalEntries.Edit`
- `Accounting.JournalEntries.Post`
- `Accounting.JournalEntries.Reverse`

---

## ⚡ Funciones Principales

### 1️⃣ Crear Asiento

1. Click en **"Nuevo Asiento"**
2. Llenar datos del encabezado (descripción, fecha, etc.)
3. Ir a tab **"Líneas"**
4. Click en **"Agregar Línea"**
5. Seleccionar cuenta contable
6. Ingresar débito o crédito (no ambos)
7. Repetir hasta tener al menos 2 líneas
8. Verificar que **Débito = Crédito** (indicador verde)
9. Click en **"Guardar Borrador"**

### 2️⃣ Contabilizar Asiento

1. Buscar asiento en estado **Draft**
2. Click en botón **verde (Contabilizar)**
3. Confirmar acción
4. El asiento pasa a estado **Posted** (no editable)

### 3️⃣ Revertir Asiento

1. Buscar asiento en estado **Posted**
2. Click en botón **rojo (Revertir)**
3. Ingresar fecha de reversión
4. Ingresar motivo
5. El sistema crea automáticamente un asiento inverso
6. El asiento original pasa a estado **Reversed**

---

## 🎨 Interfaz

### Filtros

- 🔍 **Búsqueda:** Número, descripción, referencia
- 📊 **Estado:** Draft, Posted, Reversed, Cancelled
- 🏷️ **Tipo:** Manual, Automatic, Reversal, etc.
- 📅 **Fechas:** Rango de fechas contables

### Tabla Principal

| Columna | Descripción |
|---------|-------------|
| **Número** | ID único del asiento |
| **Fecha Contable** | Fecha de registro |
| **Período** | YYYY-MM |
| **Descripción** | Concepto del asiento |
| **Tipo** | Manual, Automatic, etc. |
| **Débito** | Total débito (verde) |
| **Crédito** | Total crédito (azul) |
| **Estado** | Badge con color |
| **Acciones** | Botones según estado |

### Modal de Edición

**Tab 1: Encabezado**
- Número de asiento (auto)
- Tipo, descripción, referencia
- Fechas, período, año fiscal
- Moneda y tipo de cambio

**Tab 2: Líneas** ⭐
- Indicador de cuadre en tiempo real
- Grid editable
- Botón "Agregar Línea"
- Doble-click para editar

**Tab 3: Opciones**
- Trazabilidad de origen
- Módulo fuente
- Documento fuente

---

## 🔄 Estados del Asiento

```
┌─────────┐
│  Draft  │ ← Puede editarse
└────┬────┘
	 │ Contabilizar
	 ▼
┌──────────┐
│  Posted  │ ← No puede editarse
└────┬─────┘
	 │ Revertir
	 ▼
┌───────────┐
│ Reversed  │ ← Estado final
└───────────┘
```

---

## 🧮 Validaciones Automáticas

✅ **Cuadre contable:** Σ Débitos = Σ Créditos  
✅ **Mínimo 2 líneas**  
✅ **Descripción requerida**  
✅ **Cuenta requerida por línea**  
✅ **No débito y crédito simultáneos**  
✅ **Montos no negativos**  
✅ **Período formato YYYY-MM**  

---

## 💡 Consejos

### ✔️ Hacer

- Usar descripciones claras y concisas
- Verificar el cuadre antes de guardar (indicador verde)
- Usar referencias de documentos originales
- Seleccionar el centro de costo apropiado
- Contabilizar solo cuando esté seguro

### ❌ Evitar

- Contabilizar asientos descuadrados
- Editar asientos ya contabilizados (use reversión)
- Usar cuentas de encabezado (solo cuentas de detalle)
- Dejar descripciones vacías o genéricas
- Olvidar el centro de costo si es requerido

---

## 🔧 Solución de Problemas

### ❓ "El asiento no está cuadrado"

**Causa:** Total de débitos ≠ Total de créditos

**Solución:**
1. Revisar cada línea
2. Verificar que los montos sean correctos
3. Usar el indicador visual (rojo = descuadrado)
4. Ajustar hasta que el indicador esté verde

### ❓ "No puedo editar el asiento"

**Causa:** El asiento está en estado Posted, Reversed o Cancelled

**Solución:**
- Si está Posted: usar la función "Revertir"
- Si está Reversed/Cancelled: no se puede editar (crear uno nuevo)

### ❓ "No puedo contabilizar"

**Posibles causas:**
1. El asiento no está cuadrado
2. Faltan datos requeridos
3. Tiene menos de 2 líneas

**Solución:** Revisar validaciones y completar datos faltantes

### ❓ "No aparece la cuenta en el dropdown"

**Causa:** La cuenta no está marcada como "Permite Imputación"

**Solución:** Ir a Chart of Accounts → Editar cuenta → Activar "Permite Imputación"

---

## 📊 Ejemplos de Uso

### Ejemplo 1: Venta al Contado

```
Descripción: Venta de mercadería según factura #1234
Fecha: 2025-01-15
Período: 2025-01

LÍNEAS:
1. Cuenta: 1-10-01-00-00 (Caja)           | Débito: ₡100,000 | Crédito: -
2. Cuenta: 4-10-01-00-00 (Ventas)         | Débito: -        | Crédito: ₡100,000

Total Débito:  ₡100,000
Total Crédito: ₡100,000
Diferencia:    ₡0.00 ✓
```

### Ejemplo 2: Compra al Crédito

```
Descripción: Compra de inventario a proveedor XYZ
Fecha: 2025-01-20
Período: 2025-01

LÍNEAS:
1. Cuenta: 1-30-01-00-00 (Inventario)     | Débito: ₡50,000 | Crédito: -
2. Cuenta: 2-10-01-00-00 (Cuentas x Pagar) | Débito: -      | Crédito: ₡50,000

Total Débito:  ₡50,000
Total Crédito: ₡50,000
Diferencia:    ₡0.00 ✓
```

### Ejemplo 3: Pago de Salarios

```
Descripción: Pago de planilla quincenal enero 2025
Fecha: 2025-01-15
Período: 2025-01

LÍNEAS:
1. Cuenta: 5-20-01-00-00 (Gastos Salarios) | Débito: ₡200,000 | Crédito: -
2. Cuenta: 1-10-01-00-00 (Caja)            | Débito: -        | Crédito: ₡200,000

Total Débito:  ₡200,000
Total Crédito: ₡200,000
Diferencia:    ₡0.00 ✓
```

---

## 🎨 Atajos de Teclado (Futuro)

| Atajo | Acción |
|-------|--------|
| `Ctrl+N` | Nuevo asiento |
| `Ctrl+S` | Guardar borrador |
| `Ctrl+P` | Contabilizar |
| `Esc` | Cerrar modal |
| `Enter` | Guardar línea |

---

## 📞 Soporte

**Documentación completa:** `MOD-ACC-003_Journal_Entries_Implementation.md`

**Desarrollador:** BITI SOLUTIONS S.A

---

## 🔗 Enlaces Relacionados

- [Chart of Accounts](/Accounting/ChartOfAccounts) - Plan de cuentas
- [Cost Centers](/Accounting/CostCenters) - Centros de costo
- [Documentación técnica](../CMS.Documentation/MOD-ACC-003_Journal_Entries_Implementation.md)

---

**Última actualización:** 2025-01-XX  
**Versión:** 1.0.0
