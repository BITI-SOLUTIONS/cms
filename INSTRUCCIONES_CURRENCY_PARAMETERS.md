# Instrucciones - Parámetros de Moneda en Global Parameters

## ✅ Cambios Implementados (VERSIÓN CORREGIDA)

### 🔴 IMPORTANTE: Cambios vs. versión anterior
1. **NO se modifica `admin.company`** - La moneda ya existe en `admin.country.id_currency`
2. **Los parámetros guardan INTEGER** (ID de la moneda), NO el código string
3. **Ejemplo**: Se guarda `33` (ID) en lugar de `"CRC"` (código)

### 1. Base de Datos
- **Script SQL**: `CMS.Data/Scripts/113_add_currency_parameters.sql`
  - **NO modifica** la tabla `admin.company` (la moneda viene de `admin.country`)
  - Crea parámetros `currency_local` y `currency_exchange` con tipo `currency-select` y `data_type = integer`
  - Inicializa valores basados en la moneda del país de la compañía
  - Lógica: Si local = USD → exchange = EUR, sino → exchange = USD

### 2. Backend (API)
- **CurrencyController.cs**: Nuevo controlador con endpoints:
  - `GET /api/currency/active` - Obtiene todas las monedas activas
  - `GET /api/currency/code/{code}` - Obtiene una moneda por código ISO
  - **DTO incluye `Id` (integer)** además de Code, Name, Symbol

### 3. Frontend (JavaScript)
- **globalParameters.js**: Extendido con soporte completo para `currency-select`:
  - Carga automática de monedas desde API
  - Renderizado de selectores usando **`currency.id`** como value
  - Validación de monedas diferentes (compara IDs)
  - Sugerencias automáticas: USD → EUR, cualquier otra → USD
  - Visualización con badges usando el ID para buscar la moneda

### 4. Compilación
- ✅ Build exitoso sin errores

---

## 📋 Pasos para Activar

### 1. Ejecutar el Script SQL

```bash
psql -h localhost -U cmssystem -d cms -f CMS.Data/Scripts/113_add_currency_parameters.sql
```

Este script:
- **NO modifica** `admin.company` (la moneda ya está en `admin.country`)
- Crea los dos parámetros globales `currency_local` y `currency_exchange`
- Los inicializa con el **ID de la moneda** (integer) basado en el país de la compañía

### 2. Verificar la Ejecución del Script

El script incluye verificación automática al final. Deberías ver:

```sql
-- Parámetros creados con sus valores INTEGER
code              | value_integer | currency_code | currency_name
------------------+---------------+---------------+------------------
currency_local    | 33            | CRC           | Costa Rican Colón
currency_exchange | 155           | USD           | US Dollar

-- Relación Company → Country → Currency
company_name | country_name | currency_code | id_currency
-------------+--------------+---------------+-------------
SINAI        | Costa Rica   | CRC           | 33
```

**Nota**: Los valores son **IDs numéricos**, no códigos string.

### 3. Reiniciar la API (si está corriendo)

Si tienes la API corriendo en desarrollo, reiníciala para que cargue el nuevo controlador.

### 4. Probar la Funcionalidad

#### A. Navegar a Global Parameters
1. Iniciar sesión en el CMS
2. Ir a **Settings > Global Parameters**
3. Seleccionar el módulo **Settings** en el dropdown

#### B. Verificar que aparecen los nuevos parámetros
Deberías ver dos nuevos parámetros:
- **Moneda Local** (`currency_local`)
- **Moneda de Cambio** (`currency_exchange`)

Ambos deben mostrar:
- Un badge con la moneda actual (ej: "₡ CRC - Costa Rican Colón")
- Tipo de dato: `currency-select`
- Valor interno: ID numérico (ej: 33)
- Botón de edición

#### C. Editar Moneda Local
1. Click en el botón de editar de **Moneda Local**
2. Deberías ver un select con todas las monedas activas
3. El `value` de cada option es el **ID numérico** (ej: `<option value="33">CRC - Costa Rican Colón</option>`)
4. Selecciona una moneda (ej: USD con ID 155)
5. Deberías ver un mensaje sugeriendo EUR como moneda de cambio
6. Guardar

#### D. Editar Moneda de Cambio
1. Click en el botón de editar de **Moneda de Cambio**
2. Selecciona una moneda diferente a la local (usando su ID)
3. Si intentas seleccionar la misma que la local, debe mostrar error
4. Guardar

---

## 🧪 Pruebas Recomendadas

### Caso 1: Moneda Local = USD
- Establecer `currency_local` = 155 (ID de USD)
- El sistema debe sugerir EUR para `currency_exchange`
- Verificar que no permite guardar 155 en `currency_exchange`

### Caso 2: Moneda Local = CRC (u otra)
- Establecer `currency_local` = 33 (ID de CRC)
- El sistema debe sugerir USD (155) para `currency_exchange`
- Verificar que no permite guardar 33 en `currency_exchange`

### Caso 3: Verificar en Base de Datos
```sql
-- Verificar que se guardan IDs, no códigos
SELECT code, data_type, value_integer, value_string
FROM admin.global_parameter
WHERE code IN ('currency_local', 'currency_exchange');

-- Resultado esperado:
-- code              | data_type       | value_integer | value_string
-- ------------------+-----------------+---------------+-------------
-- currency_local    | currency-select | 33            | NULL
-- currency_exchange | currency-select | 155           | NULL
```

### Caso 4: API de Monedas
Probar los endpoints directamente:

```bash
# Obtener monedas activas (debe incluir el campo "id")
curl -H "Authorization: Bearer {TU_TOKEN}" \
  http://localhost:5000/api/currency/active

# Resultado esperado:
# [
#   {"id": 33, "code": "CRC", "name": "Costa Rican Colón", "symbol": "₡", ...},
#   {"id": 155, "code": "USD", "name": "US Dollar", "symbol": "$", ...},
#   ...
# ]
```

---

## 🎯 Lógica de Negocio Implementada

### Reglas de Validación
1. ✅ `currency_local` y `currency_exchange` deben ser **siempre diferentes** (compara IDs)
2. ✅ Si `currency_local` = USD (ID) → sugerir EUR (ID) para `currency_exchange`
3. ✅ Si `currency_local` ≠ USD → sugerir USD (ID) para `currency_exchange`
4. ✅ Ambos parámetros son obligatorios (no pueden estar vacíos)
5. ✅ Solo se muestran monedas activas (`is_active = true`)
6. ✅ Se guardan como **INTEGER** (ID de la moneda), no como STRING (código)

### Relación Company → Country → Currency
```
admin.company.id_country → admin.country.id_currency → admin.currency.id_currency
                                                              ↓
                                                    admin.global_parameter.value_integer
```

La moneda oficial de la compañía se obtiene a través de su país:
```sql
SELECT c.company_name, curr.code, curr.id_currency
FROM admin.company c
INNER JOIN admin.country co ON c.id_country = co.id_country
INNER JOIN admin.currency curr ON co.id_currency = curr.id_currency;
```

### Visualización
- Los valores se muestran como badges con:
  - Símbolo de la moneda (ej: $, €, ₡)
  - Código ISO (ej: USD, EUR, CRC)
  - Nombre completo (ej: US Dollar, Euro, Costa Rican Colón)
- Internamente se almacena y compara el **ID numérico**

---

## 🔧 Troubleshooting

### Problema: No aparecen los parámetros
**Solución**: Verificar que el script SQL se ejecutó correctamente:
```sql
SELECT * FROM admin.global_parameter 
WHERE code IN ('currency_local', 'currency_exchange');
```

### Problema: El select de monedas está vacío
**Solución**: Verificar que hay monedas activas en la BD:
```sql
SELECT id_currency, code, name FROM admin.currency WHERE is_active = true;
```

### Problema: Se guarda NULL en lugar del ID
**Solución**: 
1. Verificar que el `<option value="33">` tiene el ID como value, no el código
2. Verificar en el navegador (F12 → Network) que el payload contiene `value: 33` (integer)
3. Verificar que el backend acepta `value_integer` en el DTO

### Problema: La validación no funciona
**Solución**: 
1. Verificar que ambos parámetros usan `valueInteger` en lugar de `valueString`
2. Verificar en la consola del navegador que las comparaciones son numéricas (`===` entre integers)

---

## 📊 Estructura de Datos

### admin.country (existente, no modificada)
```sql
id_currency INTEGER REFERENCES admin.currency(id_currency)
```

### admin.global_parameter (nuevos registros)
```
code: 'currency_local'
data_type: 'currency-select'
value_integer: ID de la moneda (ej: 33 para CRC, 155 para USD)
value_string: NULL

code: 'currency_exchange'
data_type: 'currency-select'
value_integer: ID de la moneda (ej: 155 para USD, 42 para EUR)
value_string: NULL
```

### admin.currency (tabla existente)
```
id_currency: PK (INTEGER) - Este es el valor que se guarda en los parámetros
code: Código ISO (USD, EUR, CRC, etc.)
name: Nombre completo
symbol: Símbolo ($, €, ₡, etc.)
is_active: Para filtrar en el select
```

### Ejemplo de datos:
```sql
-- Monedas en admin.currency
id_currency | code | name              | symbol
------------+------+-------------------+--------
33          | CRC  | Costa Rican Colón | ₡
155         | USD  | US Dollar         | $
42          | EUR  | Euro              | €

-- Parámetros en admin.global_parameter
code              | value_integer | (se muestra como)
------------------+---------------+-------------------
currency_local    | 33            | ₡ CRC - Costa Rican Colón
currency_exchange | 155           | $ USD - US Dollar
```

---

## 🎨 Capturas Esperadas

### Vista de Parámetros
Deberías ver algo como:
```
[Moneda Local]        [currency-select] [ACTIVE]
currency_local
₡ CRC - Costa Rican Colón    [✏️ Editar]
(Valor interno: 33)

[Moneda de Cambio]    [currency-select] [ACTIVE]
currency_exchange
$ USD - US Dollar             [✏️ Editar]
(Valor interno: 155)
```

### Modal de Edición
```
Editar Parámetro
─────────────────
Nombre: Moneda Local
Clave: currency_local
Descripción: Moneda oficial de la compañía...

Valor:
[Select con monedas]
  <option value="33">₡ CRC - Costa Rican Colón</option>
  <option value="155">$ USD - US Dollar</option>
  <option value="42">€ EUR - Euro</option>
  ...

[Moneda oficial para todas las transacciones...]

☑️ Parámetro Activo

[Cancelar] [Guardar Cambios]
```

---

## ✅ Checklist Final

- [ ] Script SQL ejecutado correctamente
- [ ] Parámetros `currency_local` y `currency_exchange` creados
- [ ] Valores son **INTEGER** (ID), no STRING (código)
- [ ] API `/api/currency/active` responde con campo `id`
- [ ] Global Parameters muestra los dos nuevos parámetros
- [ ] Select de monedas usa `value="{id}"` (numérico)
- [ ] Validación impide seleccionar IDs iguales
- [ ] Sugerencias funcionan (USD ID→EUR ID, otro→USD ID)
- [ ] Se pueden guardar valores correctamente
- [ ] Los badges muestran símbolo + código + nombre
- [ ] En BD se ve `value_integer` con el ID correcto

---

## 🔍 Verificación Final en BD

```sql
-- 1. Verificar que los parámetros tienen valores INTEGER
SELECT 
    code,
    parameter_name,
    data_type,
    value_integer,  -- Debe tener valor (ej: 33, 155)
    value_string,   -- Debe ser NULL
    default_value
FROM admin.global_parameter
WHERE code IN ('currency_local', 'currency_exchange');

-- 2. Verificar que los IDs corresponden a monedas reales
SELECT 
    gp.code AS parametro,
    gp.value_integer AS id_guardado,
    c.code AS codigo_moneda,
    c.name AS nombre_moneda,
    c.symbol AS simbolo
FROM admin.global_parameter gp
LEFT JOIN admin.currency c ON gp.value_integer = c.id_currency
WHERE gp.code IN ('currency_local', 'currency_exchange');

-- 3. Verificar que la compañía tiene moneda desde su país
SELECT 
    comp.company_name,
    country.name AS pais,
    curr.id_currency AS id_moneda,
    curr.code AS codigo_moneda,
    curr.name AS nombre_moneda
FROM admin.company comp
INNER JOIN admin.country country ON comp.id_country = country.id_country
INNER JOIN admin.currency curr ON country.id_currency = curr.id_currency
WHERE comp.is_active = TRUE;
```

---

**Estado**: ✅ Implementación CORREGIDA y lista para pruebas
**Cambio clave**: Se guardan IDs (integer) en lugar de códigos (string)
**Siguiente paso**: Ejecutar script SQL y probar en la UI
- **CurrencyController.cs**: Nuevo controlador con endpoints:
  - `GET /api/currency/active` - Obtiene todas las monedas activas
  - `GET /api/currency/code/{code}` - Obtiene una moneda por código ISO

### 4. Frontend (JavaScript)
- **globalParameters.js**: Extendido con soporte completo para `currency-select`:
  - Carga automática de monedas desde API
  - Renderizado de selectores de moneda con nombre, código y símbolo
  - Validación de monedas diferentes entre `currency_local` y `currency_exchange`
  - Sugerencias automáticas: USD → EUR, cualquier otra → USD
  - Visualización con badges de las monedas seleccionadas

### 5. Compilación
- ✅ Build exitoso sin errores

---

## 📋 Pasos para Activar

### 1. Ejecutar el Script SQL

```bash
psql -h localhost -U cmssystem -d cms -f CMS.Data/Scripts/113_add_currency_parameters.sql
```

Este script:
- Agrega el campo `id_currency` a la tabla `admin.company`
- Asigna CRC a las compañías de Costa Rica y USD a las demás
- Crea los dos parámetros globales `currency_local` y `currency_exchange`

### 2. Verificar la Ejecución del Script

El script incluye verificación automática al final. Deberías ver:
- Campo `id_currency` en `admin.company`
- Monedas asignadas a las compañías
- Parámetros creados en `admin.global_parameter`

### 3. Reiniciar la API (si está corriendo)

Si tienes la API corriendo en desarrollo, reiníciala para que cargue las nuevas entidades y el controlador.

### 4. Probar la Funcionalidad

#### A. Navegar a Global Parameters
1. Iniciar sesión en el CMS
2. Ir a **Settings > Global Parameters**
3. Seleccionar el módulo **Settings** en el dropdown

#### B. Verificar que aparecen los nuevos parámetros
Deberías ver dos nuevos parámetros:
- **Moneda Local** (`currency_local`)
- **Moneda de Cambio** (`currency_exchange`)

Ambos deben mostrar:
- Un badge con la moneda actual (ej: "₡ CRC - Costa Rican Colón")
- Tipo de dato: `currency-select`
- Botón de edición

#### C. Editar Moneda Local
1. Click en el botón de editar de **Moneda Local**
2. Deberías ver un select con todas las monedas activas
3. Selecciona una moneda (ej: USD)
4. Deberías ver un mensaje sugeriendo EUR como moneda de cambio
5. Guardar

#### D. Editar Moneda de Cambio
1. Click en el botón de editar de **Moneda de Cambio**
2. Selecciona una moneda diferente a la local
3. Si intentas seleccionar la misma que la local, debe mostrar error
4. Guardar

---

## 🧪 Pruebas Recomendadas

### Caso 1: Moneda Local = USD
- Establecer `currency_local` = USD
- El sistema debe sugerir EUR para `currency_exchange`
- Verificar que no permite guardar USD en `currency_exchange`

### Caso 2: Moneda Local = CRC (u otra)
- Establecer `currency_local` = CRC
- El sistema debe sugerir USD para `currency_exchange`
- Verificar que no permite guardar CRC en `currency_exchange`

### Caso 3: Cambio de Moneda Local
- Cambiar `currency_local` de USD a EUR
- El sistema debe sugerir USD para `currency_exchange`
- La validación debe impedir monedas iguales

### Caso 4: API de Monedas
Probar los endpoints directamente:

```bash
# Obtener monedas activas
curl -H "Authorization: Bearer {TU_TOKEN}" \
  http://localhost:5000/api/currency/active

# Obtener moneda por código
curl -H "Authorization: Bearer {TU_TOKEN}" \
  http://localhost:5000/api/currency/code/USD
```

---

## 🎯 Lógica de Negocio Implementada

### Reglas de Validación
1. ✅ `currency_local` y `currency_exchange` deben ser **siempre diferentes**
2. ✅ Si `currency_local` = USD → sugerir EUR para `currency_exchange`
3. ✅ Si `currency_local` ≠ USD → sugerir USD para `currency_exchange`
4. ✅ Ambos parámetros son obligatorios (no pueden estar vacíos)
5. ✅ Solo se muestran monedas activas (`is_active = true`)

### Sincronización con Company
- El campo `admin.company.id_currency` representa la moneda oficial de la compañía
- El parámetro `currency_local` se sincroniza con este campo
- Para uso futuro: se puede crear lógica que actualice automáticamente uno cuando cambie el otro

### Visualización
- Los valores se muestran como badges con:
  - Símbolo de la moneda (ej: $, €, ₡)
  - Código ISO (ej: USD, EUR, CRC)
  - Nombre completo (ej: US Dollar, Euro, Costa Rican Colón)

---

## 🔧 Troubleshooting

### Problema: No aparecen los parámetros
**Solución**: Verificar que el script SQL se ejecutó correctamente:
```sql
SELECT * FROM admin.global_parameter 
WHERE code IN ('currency_local', 'currency_exchange');
```

### Problema: El select de monedas está vacío
**Solución**: Verificar que hay monedas activas en la BD:
```sql
SELECT * FROM admin.currency WHERE is_active = true;
```

### Problema: Error al guardar
**Solución**: 
1. Verificar que el token JWT es válido
2. Verificar que el usuario tiene permisos en Global Parameters
3. Revisar logs de la API para más detalles

### Problema: No se cargan las monedas
**Solución**: 
1. Verificar que CurrencyController.cs está registrado
2. Verificar que la API está corriendo
3. Abrir la consola del navegador (F12) y buscar errores

---

## 📊 Estructura de Datos

### admin.company (actualizada)
```sql
id_currency INTEGER REFERENCES admin.currency(id_currency)
```

### admin.global_parameter (nuevos registros)
```
code: 'currency_local'
data_type: 'currency-select'
value_string: código ISO de la moneda (ej: 'CRC', 'USD')

code: 'currency_exchange'
data_type: 'currency-select'
value_string: código ISO de la moneda (ej: 'USD', 'EUR')
```

### admin.currency (tabla existente)
```
id_currency: PK
code: Código ISO (USD, EUR, CRC, etc.)
name: Nombre completo
symbol: Símbolo ($, €, ₡, etc.)
is_active: Para filtrar en el select
```

---

## 🎨 Capturas Esperadas

### Vista de Parámetros
Deberías ver algo como:
```
[Moneda Local]        [currency-select] [ACTIVE]
currency_local
₡ CRC - Costa Rican Colón    [✏️ Editar]

[Moneda de Cambio]    [currency-select] [ACTIVE]
currency_exchange
$ USD - US Dollar             [✏️ Editar]
```

### Modal de Edición
```
Editar Parámetro
─────────────────
Nombre: Moneda Local
Clave: currency_local
Descripción: Moneda oficial de la compañía...

Valor:
[Select con monedas]
  ₡ CRC - Costa Rican Colón
  $ USD - US Dollar
  € EUR - Euro
  ...

[Moneda oficial para todas las transacciones...]

☑️ Parámetro Activo

[Cancelar] [Guardar Cambios]
```

---

## ✅ Checklist Final

- [ ] Script SQL ejecutado correctamente
- [ ] Campo `id_currency` existe en `admin.company`
- [ ] Compañías tienen moneda asignada
- [ ] Parámetros `currency_local` y `currency_exchange` creados
- [ ] API `/api/currency/active` responde correctamente
- [ ] Global Parameters muestra los dos nuevos parámetros
- [ ] Select de monedas se despliega correctamente
- [ ] Validación impide seleccionar monedas iguales
- [ ] Sugerencias funcionan (USD→EUR, otro→USD)
- [ ] Se pueden guardar valores correctamente
- [ ] Los badges muestran símbolo + código + nombre

---

**Estado**: ✅ Implementación completa y lista para pruebas
**Siguiente paso**: Ejecutar script SQL y probar en la UI
