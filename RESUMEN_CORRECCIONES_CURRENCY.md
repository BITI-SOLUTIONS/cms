# 🎯 RESUMEN DE CORRECCIONES - Parámetros de Moneda

## ⚠️ Cambios Críticos vs. Versión Anterior

### ❌ VERSIÓN ANTERIOR (INCORRECTA)
```
1. Intentaba agregar id_currency a admin.company
2. Guardaba códigos STRING ("CRC", "USD") en los parámetros
3. Comparaba strings en el JavaScript
```

### ✅ VERSIÓN ACTUAL (CORRECTA)
```
1. NO modifica admin.company (la moneda ya está en admin.country)
2. Guarda IDs INTEGER (33, 155) en los parámetros
3. Compara integers en el JavaScript
```

---

## 📋 Cambios Realizados

### 1. **Company.cs y AppDbContext.cs**
- ✅ **REVERTIDOS** - No agregan campo `IdCurrency` a Company
- ✅ La moneda se obtiene de: `Company → Country → Currency`

### 2. **Script SQL** (`113_add_currency_parameters.sql`)
- ✅ **NO modifica** `admin.company`
- ✅ Crea parámetros con `value_integer` (no `value_string`)
- ✅ Inicializa con IDs numéricos basados en `admin.country.id_currency`
- ✅ Lógica: Si país tiene USD → exchange = EUR ID, sino → exchange = USD ID

### 3. **CurrencyController.cs**
- ✅ DTO incluye campo `Id` (integer) - **CRÍTICO para el frontend**
- ✅ Endpoints devuelven lista con `{id: 33, code: "CRC", ...}`

### 4. **globalParameters.js**
- ✅ `renderEditValueField`: Usa `currency.id` como value del option
- ✅ `saveParameter`: Convierte a `parseInt()` antes de guardar
- ✅ `formatValue`: Busca currency por ID (`c.id == value`)
- ✅ `onCurrencyChange`: Compara IDs de USD/EUR, no códigos
- ✅ Validación: Compara IDs numéricos

---

## 🔍 Flujo de Datos Correcto

### Obtener Moneda de la Compañía:
```sql
SELECT 
	c.company_name,
	curr.id_currency,  -- Este es el valor que se usa
	curr.code,
	curr.name
FROM admin.company c
INNER JOIN admin.country co ON c.id_country = co.id_country
INNER JOIN admin.currency curr ON co.id_currency = curr.id_currency;

-- Resultado para SINAI:
-- company_name | id_currency | code | name
-- SINAI        | 33          | CRC  | Costa Rican Colón
```

### Guardar en Parámetro:
```sql
INSERT INTO admin.global_parameter (..., value_integer, ...)
VALUES (..., 33, ...);  -- Se guarda 33, NO "CRC"
```

### Leer desde Frontend:
```javascript
// API devuelve:
{id: 33, code: "CRC", name: "Costa Rican Colón", symbol: "₡"}

// Select renderiza:
<option value="33">CRC - Costa Rican Colón (₡)</option>

// Al guardar:
value = parseInt(valueInput.value);  // 33 (integer)

// Al comparar:
if (value === otherValue) { ... }  // 33 === 155 → false
```

---

## 📊 Estructura de Datos Final

### admin.global_parameter
```
id | code              | data_type       | value_integer | value_string | default_value
---+-------------------+-----------------+---------------+--------------+--------------
X  | currency_local    | currency-select | 33            | NULL         | 33
Y  | currency_exchange | currency-select | 155           | NULL         | 155
```

**Nota**: `default_value` es TEXT pero contiene el string "33", mientras `value_integer` contiene el integer 33.

---

## ✅ Checklist de Correcciones

### Backend C#
- [x] Company.cs NO tiene campo IdCurrency
- [x] AppDbContext.cs NO configura FK a Currency desde Company
- [x] CurrencyController.cs devuelve DTO con campo `Id` (integer)

### Base de Datos
- [x] Script SQL NO modifica admin.company
- [x] Script SQL crea parámetros con `value_integer`
- [x] Script SQL inicializa con IDs basados en country.id_currency

### Frontend JavaScript
- [x] renderEditValueField usa `c.id` como value
- [x] saveParameter convierte a parseInt()
- [x] formatValue busca por ID (`c.id == value`)
- [x] onCurrencyChange compara IDs de USD/EUR
- [x] Validación compara integers

### Compilación
- [x] Build exitoso sin errores

---

## 🚀 Siguiente Paso

**Ejecutar el script SQL:**

```powershell
$env:PGPASSWORD='tu_password'; psql -h localhost -U cmssystem -d cms -f 'CMS.Data\Scripts\113_add_currency_parameters.sql'
```

**Verificar resultado esperado:**

```sql
-- Debe mostrar IDs numéricos, NO códigos string
SELECT code, value_integer, value_string
FROM admin.global_parameter
WHERE code IN ('currency_local', 'currency_exchange');

-- Resultado esperado:
-- code              | value_integer | value_string
-- currency_local    | 33            | NULL
-- currency_exchange | 155           | NULL
```

---

## 💡 Puntos Clave para Recordar

1. **La moneda de la compañía VIENE DEL PAÍS** (`admin.country.id_currency`)
2. **Los parámetros guardan IDs** (33, 155), NO códigos ("CRC", "USD")
3. **El frontend trabaja con IDs** en los selects y comparaciones
4. **La visualización usa el código** (CRC, USD) pero internamente es ID
5. **USD y EUR se buscan por código** en JavaScript para determinar la sugerencia, pero se devuelve su ID

---

**Estado**: ✅ CORRECCIONES COMPLETADAS
**Versión**: 2.0 (Corregida según estructura real de BD)
