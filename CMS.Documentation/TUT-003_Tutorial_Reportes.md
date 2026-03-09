# Tutorial: Crear y Ejecutar Reportes
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## Objetivo

Aprender a ejecutar reportes predefinidos, aplicar filtros y exportar los resultados en diferentes formatos.

---

## Requisitos Previos

✅ Permiso `Reports.View` para ver reportes  
✅ Permiso `Reports.Execute` para ejecutar reportes

---

## Paso 1: Acceder al Módulo de Reportes

### 1.1 Navegación

1. En el menú lateral, clic en **Reports & BI**
2. Clic en **General**

```
📂 Reports & BI
   └── 📊 General  ← Clic aquí
```

### 1.2 Vista de Reportes

Verá una lista de reportes disponibles organizados por categoría:

```
┌─────────────────────────────────────────────────────────────┐
│ 📊 Reportes Disponibles                                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ 📁 Inventario                                               │
│    ├── Lista de Artículos (ITM001)                         │
│    └── Historial de Etiquetas (LBL001)                     │
│                                                              │
│ 📁 Ventas                                                   │
│    ├── Ventas por Período (VEN001)                         │
│    └── Ventas por Cliente (VEN002)                         │
│                                                              │
│ 📁 Administración                                           │
│    ├── Usuarios del Sistema (USR001)                       │
│    └── Log de Actividad (LOG001)                           │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Paso 2: Seleccionar un Reporte

### 2.1 Elegir el Reporte

Para este tutorial, seleccionaremos **"Lista de Artículos"**:

1. Clic en la categoría **Inventario**
2. Clic en **Lista de Artículos (ITM001)**

### 2.2 Pantalla del Reporte

Se abre la pantalla de configuración del reporte:

```
┌─────────────────────────────────────────────────────────────┐
│ 📊 Lista de Artículos                        Código: ITM001 │
├─────────────────────────────────────────────────────────────┤
│ Descripción: Lista completa de artículos del inventario    │
│              con sus características y precios.             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ FILTROS:                                                     │
│ ┌────────────────────────────────────────────────────────┐  │
│ │ Buscar: [________________________]                     │  │
│ │ Estado: [Todos          ▼]                             │  │
│ │ Categoría: [Todas       ▼]                             │  │
│ └────────────────────────────────────────────────────────┘  │
│                                                              │
│ [▶ Ejecutar Reporte]  [✖ Limpiar Filtros]                  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Paso 3: Configurar Filtros

### 3.1 Tipos de Filtros

| Tipo de Filtro | Descripción | Ejemplo |
|----------------|-------------|---------|
| **Texto** | Búsqueda parcial | "laptop" |
| **Fecha** | Rango de fechas | 01/01/2026 - 31/03/2026 |
| **Selección** | Lista desplegable | Estado: Activo |
| **Checkbox** | Sí/No | Solo activos: ✅ |

### 3.2 Aplicar Filtros

Para este ejemplo:

1. **Buscar:** Escriba "laptop"
2. **Estado:** Seleccione "Activo"
3. **Categoría:** Seleccione "Electrónica"

```
FILTROS:
┌────────────────────────────────────────────────────────┐
│ Buscar:    [laptop_____________________]               │
│ Estado:    [Activo                    ▼]               │
│ Categoría: [Electrónica               ▼]               │
└────────────────────────────────────────────────────────┘
```

### 3.3 Comportamiento de Filtros

- Los filtros se combinan con **AND** (todos deben cumplirse)
- Campos vacíos = sin filtro en ese campo
- La búsqueda de texto es **parcial** (encuentra "laptop" en "Laptop HP")

---

## Paso 4: Ejecutar el Reporte

### 4.1 Ejecutar

1. Clic en **"▶ Ejecutar Reporte"**
2. Espere mientras se procesan los datos
3. Los resultados aparecen en una tabla

### 4.2 Resultados

```
┌─────────────────────────────────────────────────────────────┐
│ Resultados: 25 registros encontrados                        │
│                                      [Excel] [PDF] [CSV]    │
├─────────────────────────────────────────────────────────────┤
│ Código  │ Nombre           │ Categoría   │ Precio  │ Stock │
├─────────────────────────────────────────────────────────────┤
│ LAP-001 │ Laptop HP 15"    │ Electrónica │ ₡450,000│   15  │
│ LAP-002 │ Laptop Dell XPS  │ Electrónica │ ₡650,000│    8  │
│ LAP-003 │ Laptop Lenovo    │ Electrónica │ ₡380,000│   22  │
│ ...     │ ...              │ ...         │ ...     │  ...  │
└─────────────────────────────────────────────────────────────┘

◀ Anterior  │  Página 1 de 3  │  Siguiente ▶
```

---

## Paso 5: Trabajar con los Resultados

### 5.1 Ordenar Columnas

- Clic en el **encabezado de columna** para ordenar
- Clic nuevamente para invertir (ascendente ↔ descendente)
- El indicador ▲ o ▼ muestra el orden actual

**Ejemplo:** Ordenar por Precio descendente (más caro primero):
1. Clic en columna "Precio"
2. Clic nuevamente para invertir

### 5.2 Cambiar Paginación

En la parte inferior, puede:
- Navegar entre páginas (◀ ▶)
- Cambiar registros por página (10, 25, 50, 100)

### 5.3 Ver Detalle

Si está disponible:
- Clic en una fila para ver detalle
- O clic en ícono 👁️ para ver más información

---

## Paso 6: Exportar Resultados

### 6.1 Formatos Disponibles

| Formato | Botón | Uso Recomendado |
|---------|-------|-----------------|
| **Excel** | 📊 | Análisis adicional, gráficos |
| **PDF** | 📄 | Impresión, archivo oficial |
| **CSV** | 📋 | Importar a otros sistemas |

### 6.2 Exportar a Excel

1. Clic en botón **[Excel]**
2. Se descarga archivo `.xlsx`
3. Abra con Microsoft Excel o compatible

**El archivo incluye:**
- Todos los registros filtrados (no solo la página actual)
- Formato de celdas
- Encabezados de columnas

### 6.3 Exportar a PDF

1. Clic en botón **[PDF]**
2. Se descarga archivo `.pdf`
3. Abra con visor de PDF

**El archivo incluye:**
- Logo de la empresa
- Título del reporte
- Filtros aplicados
- Fecha de generación
- Datos en tabla formateada

### 6.4 Exportar a CSV

1. Clic en botón **[CSV]**
2. Se descarga archivo `.csv`
3. Abra con cualquier editor de texto o Excel

**El archivo incluye:**
- Datos separados por comas
- Sin formato, ideal para importación

---

## Paso 7: Guardar Filtros (Favoritos)

### 7.1 Marcar como Favorito

Si ejecuta un reporte frecuentemente con los mismos filtros:

1. Configure los filtros deseados
2. Clic en **⭐ Guardar como Favorito**
3. Ingrese un nombre: "Laptops Activas"
4. Guarde

### 7.2 Acceder a Favoritos

En la lista de reportes:
- Sección **"⭐ Mis Favoritos"** aparece primero
- Clic para ejecutar con filtros guardados

---

## Ejemplo Completo: Reporte de Ventas

### Escenario

Necesita un reporte de ventas del mes anterior por vendedor.

### Pasos

1. **Acceder:** Reports & BI → General → Ventas por Vendedor

2. **Configurar Filtros:**
   - Fecha Desde: 01/02/2026
   - Fecha Hasta: 28/02/2026
   - Vendedor: (Todos)

3. **Ejecutar:** Clic en ▶ Ejecutar Reporte

4. **Revisar Resultados:**
   ```
   │ Vendedor     │ # Ventas │ Total Ventas │ Promedio │
   │ Juan Pérez   │    45    │ ₡2,500,000   │ ₡55,555  │
   │ Ana García   │    38    │ ₡1,800,000   │ ₡47,368  │
   │ Carlos López │    52    │ ₡3,200,000   │ ₡61,538  │
   ```

5. **Exportar:** Clic en [PDF] para presentación

---

## Solución de Problemas

### El reporte no muestra datos
- ✅ Verifique los filtros aplicados
- ✅ Confirme que existen datos en el rango de fechas
- ✅ Limpie filtros y ejecute sin filtros

### La exportación está vacía
- El reporte debe tener datos antes de exportar
- Ejecute el reporte primero

### El reporte tarda mucho
- Reportes con muchos datos pueden tardar
- Aplique filtros para reducir el volumen
- Considere rangos de fechas más pequeños

### No veo el botón de exportación
- Verifique que tiene permiso `Reports.Export`
- Algunos reportes pueden no tener exportación habilitada

---

## Resumen

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO DE REPORTES                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Seleccionar Reporte                                     │
│         │                                                   │
│         ▼                                                   │
│  2. Configurar Filtros (opcional)                          │
│         │                                                   │
│         ▼                                                   │
│  3. Ejecutar Reporte                                        │
│         │                                                   │
│         ▼                                                   │
│  4. Revisar Resultados                                      │
│         │                                                   │
│         ├──► Ordenar columnas                               │
│         ├──► Navegar páginas                                │
│         │                                                   │
│         ▼                                                   │
│  5. Exportar (Excel/PDF/CSV)                               │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

**© 2026 BITI Solutions S.A.**
