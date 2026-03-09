# Módulo de Inventario
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Inventario permite gestionar todos los artículos, productos y materiales de su empresa. Incluye funciones para categorización, control de stock, precios e impresión de etiquetas.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Inventory

**Permisos necesarios:**
- `Inventory.Items.View` - Ver artículos
- `Inventory.Items.Create` - Crear artículos
- `Inventory.Items.Edit` - Editar artículos
- `Inventory.Items.Delete` - Eliminar artículos

---

## 3. Gestión de Artículos

### 3.1 Lista de Artículos

Al acceder a **Inventory → Items**, verá la lista de artículos con:

| Columna | Descripción |
|---------|-------------|
| Código | Código único del artículo |
| Nombre | Nombre del artículo |
| Marca | Marca del producto |
| Categoría | Clasificación del artículo |
| Precio | Precio de venta |
| Stock | Cantidad disponible |
| Estado | Activo/Inactivo |

### 3.2 Búsqueda y Filtros

Puede filtrar artículos por:
- **Búsqueda de texto:** Código, nombre, marca, descripción
- **Estado:** Activo, Inactivo, Todos
- **Clasificación:** Categoría del artículo

### 3.3 Crear Nuevo Artículo

1. Clic en **"➕ Nuevo Artículo"**
2. Complete los campos obligatorios:
   - Código (único)
   - Nombre
   - Unidad de medida
3. Complete campos opcionales:
   - Descripción
   - Marca
   - Clasificaciones
   - Precios
4. Clic en **"Guardar"**

### 3.4 Campos del Artículo

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| Código | Texto | ✅ | Código único (ej: ART-001) |
| Nombre | Texto | ✅ | Nombre del artículo |
| Descripción | Texto | ❌ | Descripción detallada |
| Marca | Texto | ❌ | Marca del producto |
| Unidad de Medida | Selección | ✅ | Unidad, Kg, Lt, etc. |
| Clasificación 1-6 | Selección | ❌ | Categorías jerárquicas |
| Precio Base | Número | ❌ | Precio de venta |
| Activo | Checkbox | ✅ | Estado del artículo |

### 3.5 Editar Artículo

1. Busque el artículo en la lista
2. Clic en el ícono **✏️ Editar**
3. Modifique los campos necesarios
4. Clic en **"Guardar"**

### 3.6 Eliminar/Desactivar Artículo

> ⚠️ **Importante:** Los artículos no se eliminan, se desactivan para preservar el historial.

1. Busque el artículo
2. Clic en **🗑️ Eliminar**
3. Confirme la acción

---

## 4. Clasificaciones

### 4.1 Concepto

Las clasificaciones permiten organizar artículos en categorías jerárquicas. El sistema soporta hasta 6 niveles de clasificación.

### 4.2 Ejemplo de Jerarquía

```
Clasificación 1: Tipo de Producto
├── Electrónica
├── Ropa
└── Alimentos

Clasificación 2: Subtipo
├── Smartphones
├── Laptops
└── Accesorios

Clasificación 3: Marca
├── Apple
├── Samsung
└── HP
```

### 4.3 Asignar Clasificaciones

Al crear o editar un artículo:
1. Seleccione la Clasificación 1 (ej: Electrónica)
2. Seleccione la Clasificación 2 (ej: Smartphones)
3. Continue según sea necesario

---

## 5. Unidades de Medida

### 5.1 Unidades Disponibles

| Código | Nombre | Tipo |
|--------|--------|------|
| UND | Unidad | Cantidad |
| KG | Kilogramo | Peso |
| LT | Litro | Volumen |
| MT | Metro | Longitud |
| M2 | Metro Cuadrado | Área |
| M3 | Metro Cúbico | Volumen |
| DOC | Docena | Cantidad |
| PAR | Par | Cantidad |

### 5.2 Conversiones

El sistema puede manejar conversiones entre unidades compatibles (ej: KG a gramos).

---

## 6. Impresión de Etiquetas

### 6.1 Acceso

**Ruta:** Inventory → Label Items

### 6.2 Proceso de Impresión

1. **Buscar artículos** a imprimir
2. **Configurar cantidad** de etiquetas por artículo
3. **Seleccionar impresora** (si hay varias)
4. **Configurar opciones:**
   - Imprimir nombre
   - Imprimir precio
   - Imprimir código de barras
   - Imprimir borde
   - Símbolo de moneda
5. **Vista previa** de las etiquetas
6. Clic en **"Imprimir"**

### 6.3 Formato de Etiqueta

```
┌─────────────────────────┐
│  NOMBRE DEL ARTÍCULO    │
│  ₡ 15,000.00            │
│  |||||||||||||||||||    │
│  1234567890123          │
└─────────────────────────┘
```

### 6.4 Historial de Impresión

El sistema registra cada impresión con:
- Fecha y hora
- Usuario que imprimió
- Artículos y cantidades
- Impresora utilizada

---

## 7. Reportes de Inventario

### 7.1 Reportes Disponibles

| Reporte | Descripción |
|---------|-------------|
| Lista de Artículos | Todos los artículos con detalles |
| Historial de Etiquetas | Registro de impresiones |
| Stock por Categoría | Inventario agrupado |

### 7.2 Ejecutar Reporte

1. Ir a **Reports & BI → General**
2. Seleccionar el reporte de inventario
3. Configurar filtros
4. Clic en **"Ejecutar"**
5. Exportar si necesita

---

## 8. Preguntas Frecuentes

### ¿Cómo cambio el código de un artículo?
El código es único e inmutable. Debe crear un nuevo artículo y desactivar el anterior.

### ¿Puedo importar artículos masivamente?
Sí, contacte al administrador para la plantilla de importación.

### ¿Qué pasa si elimino un artículo?
El artículo se desactiva pero se conserva el historial.

---

## 9. Soporte

Para asistencia con el módulo de Inventario:
- **Email:** soporte@biti-solutions.com
- **Documentación:** Menú → Dashboard → Documentación del Sistema

---

**© 2026 BITI Solutions S.A.**
