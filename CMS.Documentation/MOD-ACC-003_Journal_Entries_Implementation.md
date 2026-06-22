# MOD-ACC-003: Journal Entries (Asientos de Diario) - Implementación Completa

**Estado:** ✅ COMPLETADO 100%  
**Fecha:** 2025-01-XX  
**Autor:** BITI SOLUTIONS S.A

---

## 📋 RESUMEN EJECUTIVO

Se ha implementado completamente el módulo de **Asientos de Diario (Journal Entries)** siguiendo las mejores prácticas de SAP FI, Oracle Financials y otros sistemas ERP reconocidos. La implementación incluye:

- ✅ Modelo de datos completo (encabezado + líneas)
- ✅ Servicio de negocio con validaciones
- ✅ API REST completa
- ✅ Interfaz de usuario maestro-detalle
- ✅ Validación de cuadre en tiempo real
- ✅ Flujo de estados (Draft → Posted → Reversed)
- ✅ Dimensiones contables (centros de costo, proyectos)
- ✅ Multi-moneda con tipo de cambio
- ✅ Trazabilidad y auditoría completa

---

## 🗂️ ESTRUCTURA DE DATOS

### Tabla Principal: `{company_schema}.journal_entry`

**Propósito:** Encabezado de asientos contables

**Campos clave:**
- `id_journal_entry` (PK, SERIAL)
- `entry_number` (VARCHAR 50, UNIQUE) - Número de asiento
- `entry_type` (VARCHAR 30) - Manual, Automatic, Reversal, Adjustment, Closing, Opening
- `description` (VARCHAR 500) - Descripción del asiento
- `posting_date` (DATE) - Fecha contable
- `period` (VARCHAR 7) - Período contable YYYY-MM
- `fiscal_year` (INTEGER) - Año fiscal
- `status` (VARCHAR 20) - Draft, Posted, Reversed, Cancelled
- `debit_total` (NUMERIC) - Total débito
- `credit_total` (NUMERIC) - Total crédito
- `currency_code` (VARCHAR 3) - Moneda
- `exchange_rate` (NUMERIC) - Tipo de cambio
- Campos de aprobación, reversión y auditoría

### Tabla Detalle: `{company_schema}.journal_entry_line`

**Propósito:** Líneas de asiento (débitos y créditos)

**PK Compuesta:** `(id_journal_entry, id_journal_entry_line)`

**Campos clave:**
- `id_journal_entry` (PK1, FK → journal_entry)
- `id_journal_entry_line` (PK2) - Número de línea (1, 2, 3...)
- `id_chart_of_accounts` (FK → chart_of_accounts) - Cuenta contable
- `line_description` (VARCHAR 500) - Descripción de la línea
- `debit_amount` (NUMERIC) - Monto débito
- `credit_amount` (NUMERIC) - Monto crédito
- `cost_center_code` (VARCHAR 30) - Centro de costo
- `project_code` (VARCHAR 30) - Proyecto
- `department_code` (VARCHAR 30) - Departamento
- `business_partner_*` - Datos del socio de negocio
- `due_date` (DATE) - Fecha de vencimiento
- Campos de impuestos y reconciliación

---

## 🏗️ ARQUITECTURA DE CAPAS

### 1️⃣ **CAPA DE DATOS**

#### `CMS.Data/Services/JournalEntryService.cs`

**Interface:** `IJournalEntryService`

**Métodos principales:**

```csharp
// Consultas
GetJournalEntriesAsync(companyId, status, type, dateFrom, dateTo, search)
GetJournalEntryByIdAsync(companyId, idJournalEntry)
GetJournalEntryByNumberAsync(companyId, entryNumber)
GetNextEntryNumberAsync(companyId, period)

// CRUD
CreateJournalEntryAsync(companyId, entry, currentUser)
UpdateJournalEntryAsync(companyId, entry, currentUser)
DeleteJournalEntryAsync(companyId, idJournalEntry)

// Operaciones contables
PostJournalEntryAsync(companyId, idJournalEntry, userId, currentUser)
ReverseJournalEntryAsync(companyId, idJournalEntry, reversalDate, reason, userId, currentUser)
CancelJournalEntryAsync(companyId, idJournalEntry, reason, userId, currentUser)
ApproveJournalEntryAsync(companyId, idJournalEntry, userId, notes, currentUser)

// Validaciones
ValidateBalanceAsync(entry) → (isBalanced, difference)
ValidateJournalEntryAsync(companyId, entry) → List<string> errors
```

**Lógica de negocio:**

1. **Validación de cuadre:** Débito = Crédito (tolerancia 0.01)
2. **Numeración automática:** Formato `JE-{period}-{seq}`
3. **Líneas secuenciales:** `id_journal_entry_line` comienza en 1 por asiento
4. **Estados protegidos:** Solo Draft puede editarse/eliminarse
5. **Reversión automática:** Invierte débito ↔ crédito y contabiliza automáticamente
6. **Aprobación opcional:** Flag `requires_approval`

**Validaciones implementadas:**

```csharp
✅ Descripción requerida
✅ Mínimo 2 líneas
✅ Cuadre débito = crédito
✅ Período formato YYYY-MM
✅ Cuenta contable requerida por línea
✅ No débito y crédito simultáneos en misma línea
✅ Montos no negativos
✅ Estado válido para operación
```

#### `CMS.Data/CompanyDbContext.cs`

**Configuración EF Core:**

```csharp
public DbSet<JournalEntry> JournalEntries { get; set; }
public DbSet<JournalEntryLine> JournalEntryLines { get; set; }

// Mappings
entity.ToTable("journal_entry", _schema);
entity.HasKey(e => e.IdJournalEntry);
entity.HasIndex(e => e.EntryNumber).IsUnique();
entity.Ignore(e => e.Lines); // Navigation property

entity.ToTable("journal_entry_line", _schema);
entity.HasKey(e => new { e.IdJournalEntry, e.IdJournalEntryLine });
```

### 2️⃣ **CAPA API**

#### `CMS.API/Controllers/JournalEntryController.cs`

**Endpoints REST:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/JournalEntry` | Lista con filtros |
| GET | `/api/JournalEntry/{id}` | Obtener por ID (incluye líneas) |
| GET | `/api/JournalEntry/number/{entryNumber}` | Obtener por número |
| GET | `/api/JournalEntry/next-number?period=` | Siguiente número |
| GET | `/api/JournalEntry/exists?entryNumber=` | Verificar existencia |
| POST | `/api/JournalEntry` | Crear asiento |
| PUT | `/api/JournalEntry/{id}` | Actualizar asiento |
| DELETE | `/api/JournalEntry/{id}` | Eliminar asiento |
| POST | `/api/JournalEntry/{id}/post` | Contabilizar |
| POST | `/api/JournalEntry/{id}/reverse` | Revertir |
| POST | `/api/JournalEntry/{id}/cancel` | Cancelar |
| POST | `/api/JournalEntry/{id}/approve` | Aprobar |
| POST | `/api/JournalEntry/validate-balance` | Validar cuadre |
| POST | `/api/JournalEntry/validate` | Validación completa |

**DTOs:**

```csharp
JournalEntryDto          // Encabezado + lista de líneas
JournalEntryLineDto      // Línea individual
ReversalRequest          // Petición de reversión
CancelRequest            // Petición de cancelación
ApprovalRequest          // Petición de aprobación
```

**Seguridad:**

- ✅ Autorización JWT requerida (`[Authorize]`)
- ✅ `CompanyId` obtenido del token
- ✅ `UserId` obtenido del token
- ✅ Validación de permisos por compañía

### 3️⃣ **CAPA UI**

#### `CMS.UI/Views/Accounting/JournalEntries.cshtml`

**Características de la interfaz:**

1. **Tema oscuro** consistente con el resto del CMS
2. **Filtros avanzados:** Búsqueda, estado, tipo, rango de fechas
3. **Tabla de lista:** Con paginación y acciones por estado
4. **Modal maestro-detalle:**
   - Tab Encabezado: Datos generales del asiento
   - Tab Líneas: Grid editable de débitos/créditos
   - Tab Opciones: Trazabilidad de origen
5. **Indicadores visuales:**
   - Badges de estado con colores
   - Totales de débito/crédito en tiempo real
   - Indicador de cuadre (verde/rojo)
6. **Acciones por estado:**
   - Draft: Editar, Contabilizar, Eliminar
   - Posted: Revertir
   - Reversed/Cancelled: Solo ver

**Estilos implementados:**

```css
✅ Texto claro sobre fondo oscuro
✅ Badges de estado con colores (Draft/Posted/Reversed/Cancelled)
✅ Débitos en verde, créditos en azul
✅ Indicador de cuadre con color dinámico
✅ Grid de líneas con scroll y hover
✅ Modales apilados sin conflictos de z-index
```

#### `CMS.UI/wwwroot/js/journalEntries.js`

**Patrón IIFE (Immediately Invoked Function Expression)**

**Funciones públicas:**

```javascript
JE.init()               // Inicialización
JE.load()               // Cargar lista
JE.openNew()            // Nuevo asiento
JE.openEdit(id)         // Editar asiento
JE.save()               // Guardar borrador
JE.saveAndPost()        // Guardar y contabilizar
JE.confirmDelete(id)    // Eliminar con confirmación
JE.confirmPost(id)      // Contabilizar con confirmación
JE.openReverse(id)      // Revertir asiento
JE.addLine()            // Agregar línea
JE.editLine(index)      // Editar línea
JE.saveLine()           // Guardar línea
JE.deleteLine(index)    // Eliminar línea
```

**Validaciones en cliente:**

```javascript
✅ Descripción requerida
✅ Mínimo 2 líneas
✅ Cuenta requerida por línea
✅ Débito o crédito (no ambos)
✅ Cuadre en tiempo real
✅ Montos no negativos
```

**Carga de catálogos:**

```javascript
loadAccounts()          // Desde ChartOfAccountsController
loadCostCenters()       // Desde CostCenterController
```

**Renderizado dinámico:**

```javascript
renderTable(entries)    // Lista de asientos
renderLines()           // Grid de líneas
updateTotals()          // Recalcula débito/crédito/diferencia
```

---

## 🔄 FLUJO DE ESTADOS

```
┌─────────┐
│  Draft  │ (Borrador - Editable)
└────┬────┘
	 │
	 │ save()
	 │
	 ▼
┌─────────┐
│  Draft  │ ◄──── Puede editarse múltiples veces
└────┬────┘
	 │
	 │ saveAndPost() o confirmPost()
	 │
	 ▼
┌──────────┐
│  Posted  │ (Contabilizado - No editable)
└────┬─────┘
	 │
	 │ openReverse()
	 │
	 ▼
┌───────────┐
│ Reversed  │ (Revertido - No editable)
└───────────┘

┌─────────┐
│  Draft  │
└────┬────┘
	 │
	 │ confirmCancel()
	 │
	 ▼
┌───────────┐
│ Cancelled │ (Cancelado - No editable)
└───────────┘
```

**Reglas de transición:**

| Estado Actual | Editar | Eliminar | Contabilizar | Revertir | Cancelar |
|---------------|--------|----------|--------------|----------|----------|
| **Draft** | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Posted** | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Reversed** | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Cancelled** | ❌ | ❌ | ❌ | ❌ | ❌ |

---

## 🧮 VALIDACIÓN DE CUADRE (PARTIDA DOBLE)

### Regla Fundamental de Contabilidad

```
Σ Débitos = Σ Créditos
```

**Implementación:**

```csharp
public Task<(bool IsBalanced, decimal Difference)> ValidateBalanceAsync(JournalEntry entry)
{
	var totalDebit = entry.Lines.Sum(l => l.DebitAmount);
	var totalCredit = entry.Lines.Sum(l => l.CreditAmount);
	var difference = Math.Abs(totalDebit - totalCredit);

	// Tolerancia de 0.01 para errores de redondeo
	var isBalanced = difference < 0.01m;

	return Task.FromResult((isBalanced, difference));
}
```

**Visualización en UI:**

```
┌──────────────────────────────────────────────────────┐
│  Total Débito       Total Crédito       Diferencia   │
│     10,500.00           10,500.00           0.00     │
│                     Cuadrado ✓                       │
└──────────────────────────────────────────────────────┘
```

**Colores:**

- Verde: Cuadrado (diferencia < 0.01)
- Rojo: Descuadrado (diferencia ≥ 0.01)

---

## 🔐 SEGURIDAD Y PERMISOS

### Permisos requeridos (futuro):

```
Accounting.JournalEntries.View
Accounting.JournalEntries.Create
Accounting.JournalEntries.Edit
Accounting.JournalEntries.Delete
Accounting.JournalEntries.Post
Accounting.JournalEntries.Reverse
Accounting.JournalEntries.Approve
```

### Auditoría automática:

```csharp
createdate      TIMESTAMP    // Fecha de creación
record_date     TIMESTAMP    // Última modificación
created_by      VARCHAR(30)  // Usuario creador
updated_by      VARCHAR(30)  // Usuario modificador
posted_by_user_id           // Usuario que contabilizó
approved_by_user_id         // Usuario que aprobó
cancelled_by_user_id        // Usuario que canceló
```

---

## 📊 DIMENSIONES ANALÍTICAS

### Por línea:

```
✅ Centro de Costo (cost_center_code, cost_center_name)
✅ Proyecto (project_code, project_name)
✅ Departamento (department_code, department_name)
✅ Socio de Negocio (business_partner_type, code, name)
```

**Uso futuro:**

- Reportes por centro de costo
- Análisis de rentabilidad por proyecto
- Control presupuestario por departamento
- Conciliación de cuentas por cobrar/pagar

---

## 💱 MULTI-MONEDA

### Configuración:

```csharp
currency_code      VARCHAR(3)     // CRC, USD, EUR
exchange_rate      NUMERIC        // Tipo de cambio
debit_amount       NUMERIC        // Monto en moneda transacción
credit_amount      NUMERIC
debit_amount_base  NUMERIC        // Monto en moneda base (futuro)
credit_amount_base NUMERIC
```

**Cálculo automático:**

```csharp
debit_amount_base = debit_amount * exchange_rate
credit_amount_base = credit_amount * exchange_rate
```

---

## 🔄 REVERSIÓN AUTOMÁTICA

### Lógica de reversión:

1. Crear nuevo asiento tipo "Reversal"
2. Copiar todas las líneas del original
3. **Invertir débito ↔ crédito:**
   ```
   Nueva Línea:
	 debit_amount  = original.credit_amount
	 credit_amount = original.debit_amount
   ```
4. Contabilizar automáticamente el asiento de reversión
5. Cambiar estado del original a "Reversed"

### Campos de trazabilidad:

```csharp
is_reversing           // true si es asiento de reversión
id_reversed_entry      // FK al asiento original
reversal_date          // Fecha de la reversión
```

---

## 🎯 CASOS DE USO PRINCIPALES

### 1. Crear Asiento Manual

```
Usuario → Click "Nuevo Asiento"
		→ Ingresar descripción, fecha, período
		→ Agregar líneas (cuenta, débito/crédito)
		→ Validar cuadre
		→ Guardar borrador
		→ Contabilizar
```

### 2. Revertir Asiento Contabilizado

```
Usuario → Buscar asiento Posted
		→ Click "Revertir"
		→ Ingresar fecha de reversión
		→ Ingresar motivo
		→ Sistema crea asiento inverso automáticamente
		→ Sistema contabiliza el reverso
		→ Original queda como "Reversed"
```

### 3. Editar Asiento en Borrador

```
Usuario → Buscar asiento Draft
		→ Click "Editar"
		→ Modificar encabezado o líneas
		→ Validar cuadre
		→ Guardar cambios
```

---

## 📦 ARCHIVOS IMPLEMENTADOS

### Backend (CMS.Data / CMS.API)

```
✅ CMS.Entities/Operational/JournalEntry.cs              (Entidades)
✅ CMS.Data/Services/JournalEntryService.cs              (Servicio)
✅ CMS.Data/CompanyDbContext.cs                          (EF Mappings)
✅ CMS.API/Controllers/JournalEntryController.cs         (API REST)
✅ CMS.API/Program.cs                                    (DI Registration)
```

### Frontend (CMS.UI)

```
✅ CMS.UI/Controllers/AccountingController.cs            (UI Controller)
✅ CMS.UI/Views/Accounting/JournalEntries.cshtml         (Vista Razor)
✅ CMS.UI/wwwroot/js/journalEntries.js                   (Lógica cliente)
```

### Base de Datos

```
✅ CMS.Data/Scripts/103_create_journal_entry_tables.sql
✅ CMS.Data/Scripts/106_alter_journal_entry_line_composite_key.sql
```

---

## ✅ ESTADO DE COMPILACIÓN

```
✅ Build successful
✅ Sin errores de compilación
✅ Sin warnings críticos
✅ Todas las capas integradas
```

---

## 🚀 PRÓXIMOS PASOS RECOMENDADOS

### Fase 2 (Futuro):

1. **Integración con otros módulos:**
   - Generación automática desde Ventas (facturas)
   - Generación automática desde Compras
   - Generación automática desde Nómina
   - Generación automática desde Inventario

2. **Reportes:**
   - Libro Diario
   - Mayor General
   - Balance de Comprobación
   - Estado de Resultados
   - Balance General

3. **Mejoras UI:**
   - Plantillas de asientos recurrentes
   - Búsqueda avanzada multi-criterio
   - Exportar a Excel
   - Adjuntar documentos soporte

4. **Seguridad:**
   - Implementar permisos granulares
   - Workflow de aprobación multi-nivel
   - Auditoría detallada de cambios

5. **Integraciones:**
   - E-Invoicing (contabilización automática)
   - Banking (conciliación bancaria)
   - Reportes fiscales (declaraciones)

---

## 📞 SOPORTE

**Desarrollador:** BITI SOLUTIONS S.A  
**Fecha de implementación:** 2025-01-XX  
**Versión:** 1.0.0

---

## 📝 NOTAS TÉCNICAS

### Composite Key en journal_entry_line

El diseño usa PK compuesta `(id_journal_entry, id_journal_entry_line)` donde `id_journal_entry_line` es el número de línea secuencial (1, 2, 3...) dentro del asiento, **no** un ID autoincremental global.

**Ventajas:**

- ✅ Facilita ordenamiento natural de líneas
- ✅ Permite reordenar líneas fácilmente
- ✅ Mantiene integridad referencial fuerte

### Eliminación de campos redundantes

Se eliminaron `code` y `name` de `journal_entry_line` porque:

- La cuenta contable ya tiene `id_chart_of_accounts` (FK)
- Los datos `code` y `name` se pueden obtener por JOIN
- Evita inconsistencias por duplicación
- Sigue principios de normalización

### Validación de cuadre con tolerancia

Se usa tolerancia de 0.01 para errores de redondeo:

```csharp
difference < 0.01m  // Considerado "cuadrado"
```

Esto evita falsos negativos por redondeo de decimales.

---

**FIN DEL DOCUMENTO** 🎉
