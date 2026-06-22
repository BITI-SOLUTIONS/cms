# 📊 Pantallas Implementadas en el Módulo de Contabilidad (Accounting)

## 🎯 Resumen General

En el módulo de **Accounting** del sistema CMS se han implementado hasta ahora **3 pantallas completas y funcionales**:

1. ✅ **Chart of Accounts** (Plan de Cuentas)
2. ✅ **Cost Centers** (Centros de Costo)
3. ✅ **Journal Entries** (Asientos de Diario) 🆕

---

## ✅ 1. Chart of Accounts (Plan de Cuentas)

### 📍 Ubicación
- **Ruta UI**: `/Accounting/ChartOfAccounts`
- **Vista**: `CMS.UI/Views/Accounting/ChartOfAccounts.cshtml`
- **JavaScript**: `CMS.UI/wwwroot/js/chartOfAccounts.js`
- **API Controller**: `CMS.API/Controllers/ChartOfAccountsController.cs`
- **Service**: `CMS.Data/Services/ChartOfAccountsService.cs`
- **Entity**: `CMS.Entities/Operational/ChartOfAccounts.cs`
- **Tabla BD**: `{company_schema}.chart_of_accounts` (ej: `sinai.chart_of_accounts`)
- **Script SQL**: `CMS.Data/Scripts/104_create_chart_of_accounts_table.sql`

### 🎨 Características

#### Interfaz de Usuario
- ✅ **Header** con título y botón "Nueva Cuenta"
- ✅ **Filtros completos**:
  - Búsqueda por código/nombre/descripción
  - Tipo de cuenta (Asset, Liability, Equity, Income, Expense)
  - Naturaleza (Debit, Credit)
  - Estado (Activo/Inactivo)
  - Permite imputación (Sí/No)
- ✅ **Vista dual con tabs**:
  - **Lista**: Tabla con paginación
  - **Árbol**: Jerarquía visual expandible
- ✅ **Tabla con columnas**:
  - Código (formato jerárquico `X-XX-XX-XX-XX`)
  - Nombre
  - Tipo de cuenta (badge con color)
  - Naturaleza (badge Débito/Crédito)
  - Estado de imputación
  - Acciones (Editar/Eliminar)
- ✅ **Modal CRUD con 4 tabs**:
  - **General**: Código, nombre, descripción, cuenta padre, nivel
  - **Tipo y Clasificación**: Tipo, categoría, grupo contable, naturaleza
  - **Impuestos**: IVA aplicable, retención, exención
  - **Controles**: Permite imputación, requiere dimensiones, estado

#### Funcionalidades
- ✅ Crear nueva cuenta con validación de código jerárquico
- ✅ Editar cuenta existente
- ✅ Eliminar cuenta (valida que no tenga transacciones)
- ✅ Auto-cálculo de nivel jerárquico según padre
- ✅ Validación de formato de código (1-00-00-00-00)
- ✅ Validación de unicidad de código
- ✅ Prevención de ciclos en jerarquía
- ✅ Dropdown dinámico de cuentas padre
- ✅ Paginación (20 ítems por página)
- ✅ Vista de árbol jerárquico con expansión/colapso

#### Características del Catálogo
- **Estructura jerárquica** hasta 6 niveles
- **Código tipo SAP**: `1-00-00-00-00` (nivel raíz), `1-10-00-00-00` (sublevel), etc.
- **5 tipos de cuentas**:
  - 🟢 Asset (Activo)
  - 🔴 Liability (Pasivo)
  - 🔵 Equity (Patrimonio)
  - 🟡 Income (Ingreso)
  - 🟠 Expense (Gasto)
- **Naturaleza**: Debit/Credit (Débito/Crédito)
- **Control de imputación**: Bandera para permitir movimientos directos
- **Catálogo precargado**: 50+ cuentas estándar Costa Rica

#### Paleta de Colores
| Tipo      | Color Badge | Icono       |
|-----------|-------------|-------------|
| Asset     | `#10b981`   | 💰 Verde    |
| Liability | `#ef4444`   | 📊 Rojo     |
| Equity    | `#3b82f6`   | 🏛️ Azul    |
| Income    | `#f59e0b`   | 💵 Naranja  |
| Expense   | `#8b5cf6`   | 💸 Violeta  |

---

## ✅ 2. Cost Centers (Centros de Costo)

### 📍 Ubicación
- **Ruta UI**: `/Accounting/CostCenters`
- **Vista**: `CMS.UI/Views/Accounting/CostCenters.cshtml`
- **JavaScript**: `CMS.UI/wwwroot/js/costCenters.js`
- **API Controller**: `CMS.API/Controllers/CostCenterController.cs`
- **Service**: `CMS.Data/Services/CostCenterService.cs`
- **Entity**: `CMS.Entities/Operational/CostCenter.cs`
- **Tabla BD**: `{company_schema}.cost_center` (ej: `sinai.cost_center`)
- **Script SQL**: `CMS.Data/Scripts/107_create_cost_center_table.sql`

### 🎨 Características

#### Interfaz de Usuario
- ✅ **Header** con título y botón "Nuevo Centro de Costo"
- ✅ **Filtros completos**:
  - Búsqueda por código/nombre
  - Tipo de centro (Operational, Administrative, ServiceCenter, Auxiliary)
  - Categoría
  - Estado (Activo/Inactivo)
  - Permite imputación (Sí/No)
- ✅ **Vista dual con tabs**:
  - **Tarjetas**: Grid responsive con paginación (12 por página)
  - **Árbol**: Jerarquía visual con indentación
- ✅ **Tarjetas visuales**:
  - Indicador de nivel jerárquico (círculo de color)
  - Badge de tipo con color
  - Código y nombre
  - Categoría y centro padre
  - Indicadores de estado (activo/bloqueado/posting)
  - Botones editar/eliminar
- ✅ **Modal CRUD con 5 tabs**:
  - **General**: Código, nombre, descripción, padre, nivel, ubicación
  - **Clasificación**: Tipo, categoría, profit center, business area, company code
  - **Presupuesto**: Monto anual, moneda (CRC/USD/EUR), permitir sobre-presupuesto
  - **Vigencia**: Válido desde/hasta
  - **Controles**: Permite imputación, bloqueado, activo

#### Funcionalidades
- ✅ Crear nuevo centro con validación de código jerárquico
- ✅ Editar centro existente
- ✅ Eliminar centro (valida que no tenga hijos ni transacciones)
- ✅ Auto-cálculo de nivel jerárquico según padre
- ✅ Validación de formato de código (1-00-00-00-00)
- ✅ Validación de unicidad de código
- ✅ Prevención de ciclos en jerarquía
- ✅ Dropdown dinámico de centros padre
- ✅ Control presupuestario con moneda
- ✅ Vigencia temporal (válido desde/hasta)
- ✅ Paginación en vista de tarjetas
- ✅ Vista de árbol jerárquico interactiva

#### Características del Catálogo
- **Estructura jerárquica** hasta 5 niveles
- **Código tipo SAP**: `1-00-00-00-00` (división), `1-10-00-00-00` (departamento), etc.
- **4 tipos de centros**:
  - 🔵 Operational (Operacional)
  - 🟣 Administrative (Administrativo)
  - 🟢 ServiceCenter (Centro de Servicio)
  - 🟠 Auxiliary (Auxiliar)
- **Dimensiones**: Localización, Departamento, División
- **Control presupuestario**: Monto anual, moneda, sobre-presupuesto
- **Vigencia temporal**: Válido desde/hasta para control histórico
- **Catálogo precargado**: 15+ centros de costo estándar

#### Paleta de Colores
| Tipo              | Color Badge | Color Indicador |
|-------------------|-------------|-----------------|
| Operational       | `#3b82f6`   | Azul            |
| Administrative    | `#8b5cf6`   | Violeta         |
| ServiceCenter     | `#10b981`   | Verde           |
| Auxiliary         | `#f59e0b`   | Naranja         |

#### Indicadores de Nivel Jerárquico
| Nivel | Color       | Descripción          |
|-------|-------------|----------------------|
| 1     | 🔴 `#ef4444` | Raíz / Divisiones   |
| 2     | 🟠 `#f59e0b` | Departamentos       |
| 3     | 🔵 `#3b82f6` | Áreas               |
| 4     | 🟢 `#10b981` | Sub-áreas           |
| 5     | 🟣 `#8b5cf6` | Niveles específicos |

---

## ⏳ 3. Journal Entries (Asientos de Diario) - PENDIENTE

### 📍 Ubicación Preparada
- **Ruta UI**: `/Accounting/JournalEntries` (⚠️ Acción existe pero sin vista)
- **API Controller**: ❌ **No implementado aún**
- **Service**: ❌ **No implementado aún**
- **Entity**: ✅ `CMS.Entities/Operational/JournalEntry.cs` y `JournalEntryLine.cs`
- **Tabla BD**: ✅ `{company_schema}.journal_entry` y `journal_entry_line`
- **Script SQL**: ✅ `CMS.Data/Scripts/103_create_journal_entry_tables.sql`

### 🔧 Estado Actual
- ✅ Tablas creadas en BD con schema actualizado
- ✅ Entidades C# definidas
- ✅ Acción del controller preparada (`JournalEntries()`)
- ❌ **Falta**: Vista Razor
- ❌ **Falta**: JavaScript cliente
- ❌ **Falta**: Service layer
- ❌ **Falta**: API Controller

### 📋 Estructura de Tablas (Ya Creadas)

#### `journal_entry` (Header)
- `id_journal_entry` (PK, serial)
- `entry_number` (varchar 30, unique)
- `entry_date` (date, NOT NULL)
- `posting_date` (date, NOT NULL)
- `period` (varchar 6) - formato YYYYMM
- `fiscal_year` (integer)
- `description` (varchar 1000)
- `reference` (varchar 100)
- `document_type` (varchar 30)
- `document_number` (varchar 50)
- `status` (varchar 20) - Draft, Posted, Reversed, Cancelled
- `currency` (varchar 3) - ISO 4217
- `exchange_rate` (decimal 18,6)
- `total_debit` (decimal 18,2)
- `total_credit` (decimal 18,2)
- Campos de aprobación (`approved_by`, `approved_date`)
- Campos de reversión (`reversed_by`, `reversed_date`, `reversal_reason`)
- Campos de origen (`source_module`, `source_document_id`)
- Auditoría estándar

#### `journal_entry_line` (Detail - Composite Key)
- `id_journal_entry` (PK1, FK a journal_entry)
- `id_journal_entry_line` (PK2, line number per journal - 1, 2, 3...)
- `id_chart_of_accounts` (FK a chart_of_accounts)
- `description` (varchar 500)
- `debit_amount` (decimal 18,2)
- `credit_amount` (decimal 18,2)
- `currency` (varchar 3)
- **Dimensiones**:
  - `id_cost_center` (FK a cost_center - **YA LISTO PARA USAR**)
  - `id_profit_center` (integer - futuro)
  - `id_project` (integer - futuro)
  - `id_department` (integer - futuro)
  - `business_area` (varchar 20)
  - `company_code` (varchar 10)
- **Impuestos**:
  - `tax_code` (varchar 10)
  - `tax_rate` (decimal 5,2)
  - `tax_amount` (decimal 18,2)
- Campos de conciliación bancaria
- Auditoría estándar

### 🚀 Características Planificadas
- Captura de asientos de diario con detalle
- Validación de cuadre (Débito = Crédito)
- Multi-moneda con tipo de cambio
- Estado del asiento (Borrador, Contabilizado, Reversado, Cancelado)
- Aprobación de asientos
- Reversión de asientos
- Dimensiones contables (Cost Center, Profit Center, Project, etc.)
- Impuestos por línea
- Referencia a documentos origen (facturas, pagos, etc.)
- Exportación a Excel/PDF
- Cierre de períodos

---

## 📊 Tabla Comparativa de Pantallas

| Característica                  | Chart of Accounts | Cost Centers | Journal Entries |
|---------------------------------|-------------------|--------------|-----------------|
| **Estado**                      | ✅ Completo       | ✅ Completo  | ⏳ Pendiente    |
| **Vista Razor**                 | ✅                | ✅           | ❌              |
| **JavaScript**                  | ✅                | ✅           | ❌              |
| **API Controller**              | ✅                | ✅           | ❌              |
| **Service Layer**               | ✅                | ✅           | ❌              |
| **Entidad C#**                  | ✅                | ✅           | ✅              |
| **Tabla BD**                    | ✅                | ✅           | ✅              |
| **Script SQL**                  | ✅                | ✅           | ✅              |
| **Catálogo Seed**               | ✅ (50+ cuentas)  | ✅ (15+ CCs) | N/A             |
| **CRUD Completo**               | ✅                | ✅           | ❌              |
| **Filtros**                     | ✅ (5 filtros)    | ✅ (5 filtros)| ❌              |
| **Vista Dual (Lista/Árbol)**    | ✅                | ✅           | ❌              |
| **Modal con Tabs**              | ✅ (4 tabs)       | ✅ (5 tabs)  | ❌              |
| **Jerarquía Visual**            | ✅                | ✅           | N/A             |
| **Paginación**                  | ✅                | ✅           | ❌              |
| **Validación de Código**        | ✅                | ✅           | N/A             |
| **Prevención de Ciclos**        | ✅                | ✅           | N/A             |
| **Auto-cálculo de Nivel**       | ✅                | ✅           | N/A             |
| **Control Presupuestario**      | ❌                | ✅           | N/A             |
| **Vigencia Temporal**           | ❌                | ✅           | N/A             |
| **Multi-moneda**                | ❌                | ✅           | ✅ (planificado)|
| **Dimensiones Contables**       | ❌                | N/A          | ✅ (planificado)|
| **Aprobación de Transacciones** | N/A               | N/A          | ✅ (planificado)|

---

## 🎨 Características Comunes de las Pantallas

### Diseño UI/UX
- ✅ **Tema oscuro** optimizado con texto claro
- ✅ **Bootstrap 5** para componentes
- ✅ **Bootstrap Icons** para iconografía
- ✅ **Responsive design** (mobile-friendly)
- ✅ **Modales con tabs** para organización de campos
- ✅ **Badges con colores** para categorización visual
- ✅ **Loading states** y feedback visual
- ✅ **Confirmaciones** antes de eliminar
- ✅ **Mensajes de error/éxito** claros

### Funcionalidades Técnicas
- ✅ **Autenticación JWT** desde sesión
- ✅ **Multi-compañía** (aislamiento por `CompanyId`)
- ✅ **API REST** con autorización
- ✅ **Service layer** con validaciones de negocio
- ✅ **EF Core** con queries optimizadas
- ✅ **Logging** estructurado
- ✅ **Manejo de errores** robusto
- ✅ **Escape de HTML** para prevenir XSS
- ✅ **Validación cliente** y servidor

---

## 📝 Scripts SQL Ejecutados

### Chart of Accounts
- ✅ `104_create_chart_of_accounts_table.sql` - Tabla y catálogo

### Cost Centers
- ✅ `107_create_cost_center_table.sql` - Tabla y catálogo
- ✅ `108_add_cost_center_menu.sql` - Entrada de menú
- ✅ `109_add_cost_center_permissions.sql` - Permisos CRUD

### Journal Entries
- ✅ `103_create_journal_entry_tables.sql` - Tablas header y detail
- ✅ `106_alter_journal_entry_line_composite_key.sql` - Composite key para líneas

---

## 🎯 Resumen de Estado

### ✅ Implementado (2 pantallas)
1. **Chart of Accounts** - 100% completo y funcional
2. **Cost Centers** - 100% completo y funcional

### ⏳ Pendiente (1 pantalla)
3. **Journal Entries** - Estructuras de BD listas, falta UI/Service/API

### 🔮 Futuro (Próximas pantallas sugeridas)
- **Bank Reconciliation** (Conciliación Bancaria)
- **Fixed Assets** (Activos Fijos)
- **Depreciation** (Depreciación)
- **General Ledger Reports** (Reportes de Libro Mayor)
- **Trial Balance** (Balance de Comprobación)
- **Financial Statements** (Estados Financieros)
- **Budget Management** (Gestión de Presupuestos)
- **Closing Operations** (Cierre de Períodos)

---

## 📚 Documentación Disponible

- 📄 `CMS.Documentation/MOD-ACC-002_Cost_Centers_Implementation.md` - Documentación completa de Cost Centers
- 📄 Todas las vistas Razor tienen comentarios de encabezado con propósito/tabla/descripción
- 📄 Todos los scripts SQL tienen comentarios de ejecución y propósito
- 📄 Todos los servicios tienen XML docs en métodos públicos

---

## ✅ 3. Journal Entries (Asientos de Diario) 🆕

### 📍 Ubicación
- **Ruta UI**: `/Accounting/JournalEntries`
- **Vista**: `CMS.UI/Views/Accounting/JournalEntries.cshtml`
- **JavaScript**: `CMS.UI/wwwroot/js/journalEntries.js`
- **API Controller**: `CMS.API/Controllers/JournalEntryController.cs`
- **Service**: `CMS.Data/Services/JournalEntryService.cs`
- **Entities**: 
  - `CMS.Entities/Operational/JournalEntry.cs` (Encabezado)
  - `CMS.Entities/Operational/JournalEntryLine.cs` (Líneas)
- **Tablas BD**: 
  - `{company_schema}.journal_entry` (Encabezado)
  - `{company_schema}.journal_entry_line` (Detalle)
- **Scripts SQL**: 
  - `CMS.Data/Scripts/103_create_journal_entry_tables.sql`
  - `CMS.Data/Scripts/106_alter_journal_entry_line_composite_key.sql`

### 🎨 Características

#### Interfaz de Usuario
- ✅ **Header** con título y botón "Nuevo Asiento"
- ✅ **Filtros completos**:
  - Búsqueda por número/descripción/referencia
  - Estado (Draft, Posted, Reversed, Cancelled)
  - Tipo de asiento (Manual, Automatic, Reversal, Adjustment, Closing, Opening)
  - Rango de fechas (Desde/Hasta)
  - Botón "Limpiar filtros"
- ✅ **Tabla de lista con paginación**:
  - Número de asiento
  - Fecha contable
  - Período (badge)
  - Descripción + Referencia
  - Tipo (badge con color)
  - Total Débito (verde)
  - Total Crédito (azul)
  - Estado (badge con color)
  - Acciones dinámicas según estado
- ✅ **Modal CRUD maestro-detalle con 3 tabs**:
  - **Tab Encabezado**: 
    - Número de asiento (auto-generado)
    - Tipo de asiento
    - Fecha de asiento y fecha contable
    - Descripción (textarea)
    - Referencia, período, año fiscal
    - Moneda y tipo de cambio
    - Requiere aprobación (checkbox)
  - **Tab Líneas**: 
    - Indicadores de cuadre en tiempo real (Débito/Crédito/Diferencia)
    - Botón "Agregar Línea"
    - Grid editable con scroll
    - Columnas: #, Cuenta, Descripción, Débito, Crédito, Centro Costo, Acciones
    - Doble-click para editar línea
  - **Tab Opciones**: 
    - Módulo origen
    - Tipo documento origen
    - ID documento origen
    - Número documento origen
- ✅ **Modal secundario - Edición de línea**:
  - Cuenta contable (select con cuentas de detalle)
  - Descripción de la línea
  - Débito / Crédito (no ambos simultáneamente)
  - Centro de costo (select opcional)
  - Referencia

#### Funcionalidades
- ✅ **CRUD completo**:
  - Crear nuevo asiento en borrador
  - Editar asiento en borrador
  - Eliminar asiento en borrador
  - Ver asiento contabilizado (read-only)
- ✅ **Operaciones contables**:
  - Contabilizar asiento (Draft → Posted)
  - Revertir asiento contabilizado (crea asiento inverso automático)
  - Cancelar asiento en borrador
  - Aprobar asiento (si requiere aprobación)
- ✅ **Validaciones en tiempo real**:
  - Cuadre contable (Débito = Crédito)
  - Tolerancia de 0.01 para redondeo
  - Indicador visual verde/rojo según cuadre
  - Mínimo 2 líneas por asiento
  - No débito y crédito simultáneos por línea
  - Montos no negativos
- ✅ **Generación automática**:
  - Número de asiento: Formato `JE-YYYY-MM-NNNN`
  - Líneas numeradas secuencialmente (1, 2, 3...)
  - Período calculado según fecha contable
- ✅ **Integración con catálogos**:
  - Carga cuentas contables desde `/api/ChartOfAccounts`
  - Carga centros de costo desde `/api/CostCenter`
  - Solo cuentas de detalle (`isDetail=true`)
  - Solo centros de costo con imputación (`isPostingAllowed=true`)

#### Características del Modelo de Datos

**Encabezado (`journal_entry`):**
- ✅ Numeración automática por período
- ✅ 6 tipos de asiento (Manual, Automatic, Reversal, Adjustment, Closing, Opening)
- ✅ 4 estados (Draft, Posted, Reversed, Cancelled)
- ✅ Multi-moneda con tipo de cambio
- ✅ Trazabilidad de origen (módulo + documento)
- ✅ Flujo de aprobación opcional
- ✅ Auditoría completa (creado por, contabilizado por, aprobado por, etc.)

**Detalle (`journal_entry_line`):**
- ✅ PK compuesta: `(id_journal_entry, id_journal_entry_line)`
- ✅ `id_journal_entry_line` es secuencial por asiento (1, 2, 3...)
- ✅ FK a `chart_of_accounts` (sin redundancia)
- ✅ Débito y crédito separados
- ✅ Dimensiones analíticas:
  - Centro de costo
  - Proyecto
  - Departamento
  - Socio de negocio
- ✅ Impuestos (código, tasa, monto)
- ✅ Reconciliación (flag, fecha, referencia)
- ✅ Fecha de vencimiento

#### Flujo de Estados
```
Draft → Posted → Reversed
  ↓
Cancelled
```

**Reglas:**
- Solo `Draft` puede editarse o eliminarse
- Solo `Posted` puede revertirse
- `Reversed` y `Cancelled` son estados finales

#### Paleta de Colores
| Estado    | Color Badge | Descripción      |
|-----------|-------------|------------------|
| Draft     | `#fbbf24`   | 📝 Amarillo      |
| Posted    | `#10b981`   | ✅ Verde         |
| Reversed  | `#ef4444`   | 🔄 Rojo          |
| Cancelled | `#6b7280`   | ❌ Gris          |

**Montos:**
- Débito: Verde (`#10b981`)
- Crédito: Azul (`#3b82f6`)

---

## 🚀 Próximos Pasos Recomendados

### Fase 2 - Reportes Contables

1. **Balance de Comprobación**:
   - Sumas y saldos por cuenta
   - Filtros por período, fecha, cuenta
   - Exportar a Excel/PDF

2. **Mayor General**:
   - Detalle de movimientos por cuenta
   - Saldos acumulados
   - Drill-down a asientos

3. **Libro Diario**:
   - Listado cronológico de asientos
   - Totales por día/mes
   - Exportar

4. **Estados Financieros**:
   - Balance General
   - Estado de Resultados
   - Estado de Flujo de Efectivo
   - Comparativos (actual vs anterior)

### Fase 3 - Integraciones Automáticas

1. **Desde Ventas**:
   - Factura de cliente → Asiento automático
   - Pago de cliente → Asiento de caja

2. **Desde Compras**:
   - Factura de proveedor → Asiento de compra
   - Pago a proveedor → Asiento de banco

3. **Desde Inventario**:
   - Ajuste de inventario → Asiento de ajuste
   - Traslado → Asiento de costo

4. **Desde Nómina**:
   - Pago de planilla → Asiento de nómina
   - Provisión de vacaciones → Asiento de provisión

### Fase 4 - Mejoras Avanzadas

1. **Plantillas de asientos**:
   - Guardar asientos recurrentes
   - Copiar de asiento anterior
   - Asientos predefinidos por tipo

2. **Workflow de aprobación**:
   - Múltiples niveles
   - Notificaciones
   - Historial de aprobaciones

3. **Adjuntos**:
   - Subir facturas, recibos, etc.
   - Ver documentos soporte
   - Gestión de archivos

4. **Conciliación bancaria**:
   - Importar extractos
   - Match automático
   - Asientos de ajuste

---

**Última actualización**: 2025-01-XX  
**Implementado por**: BITI Solutions S.A  
**Proyecto**: CMS - Clean Architecture .NET 9
