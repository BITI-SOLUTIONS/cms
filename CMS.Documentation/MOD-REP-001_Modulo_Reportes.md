# Módulo de Reportes
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Reportes y Business Intelligence permite generar, visualizar y exportar información del sistema de manera flexible y personalizada.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Reports & BI → General

**Permisos necesarios:**
- `Reports.View` - Ver lista de reportes
- `Reports.Execute` - Ejecutar reportes

---

## 3. Tipos de Reportes

### 3.1 Categorías

| Categoría | Descripción |
|-----------|-------------|
| **Inventario** | Artículos, stock, movimientos |
| **Ventas** | Facturación, clientes, vendedores |
| **Compras** | Órdenes, proveedores |
| **Contabilidad** | Estados financieros |
| **Administración** | Usuarios, auditoría |

### 3.2 Reportes Predefinidos

El sistema incluye reportes predefinidos como:
- Lista de Artículos
- Historial de Impresión de Etiquetas
- Usuarios del Sistema
- Actividad del Sistema

---

## 4. Ejecutar un Reporte

### 4.1 Paso a Paso

1. **Acceder a Reportes**
   - Menú → Reports & BI → General

2. **Seleccionar Reporte**
   - Busque por nombre o categoría
   - Clic en el reporte deseado

3. **Configurar Filtros**
   - Complete los filtros disponibles:
     - Fechas (Desde/Hasta)
     - Búsqueda de texto
     - Estados
     - Categorías

4. **Ejecutar**
   - Clic en **"▶ Ejecutar Reporte"**
   - Espere mientras se procesa

5. **Ver Resultados**
   - Los datos se muestran en tabla
   - Navegue con la paginación

### 4.2 Interfaz del Reporte

```
┌─────────────────────────────────────────────────────────┐
│ 📊 Nombre del Reporte                                   │
├─────────────────────────────────────────────────────────┤
│ Filtros:                                                │
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐        │
│ │ Buscar...   │ │ Fecha Desde │ │ Fecha Hasta │        │
│ └─────────────┘ └─────────────┘ └─────────────┘        │
│                                                         │
│ [▶ Ejecutar Reporte]  [✖ Limpiar Filtros]              │
├─────────────────────────────────────────────────────────┤
│ Resultados: 150 registros    [Excel] [PDF] [CSV]       │
├─────────────────────────────────────────────────────────┤
│ Código │ Nombre        │ Categoría │ Precio   │ Stock │
│ ART001 │ Producto A    │ Electr.   │ ₡10,000  │   50  │
│ ART002 │ Producto B    │ Ropa      │ ₡ 5,000  │  120  │
│ ...    │ ...           │ ...       │ ...      │  ...  │
└─────────────────────────────────────────────────────────┘
```

---

## 5. Filtros

### 5.1 Tipos de Filtros

| Tipo | Descripción | Ejemplo |
|------|-------------|---------|
| **Texto** | Búsqueda parcial | Buscar "laptop" |
| **Fecha** | Rango de fechas | Del 01/01 al 31/01 |
| **Selección** | Lista de opciones | Categoría: Electrónica |
| **Checkbox** | Sí/No | Solo activos: ✅ |

### 5.2 Combinar Filtros

Los filtros se combinan con AND lógico:
- Buscar "laptop" + Categoría "Electrónica" + Activo
- Mostrará laptops activos de la categoría Electrónica

### 5.3 Limpiar Filtros

Clic en **"✖ Limpiar Filtros"** para reiniciar todos los filtros.

---

## 6. Exportación

### 6.1 Formatos Disponibles

| Formato | Icono | Descripción |
|---------|-------|-------------|
| Excel | 📊 | Archivo .xlsx con formato |
| PDF | 📄 | Documento para impresión |
| CSV | 📋 | Datos separados por comas |

### 6.2 Exportar Datos

1. Ejecute el reporte con los filtros deseados
2. Clic en el botón del formato deseado:
   - **[Excel]** - Descarga .xlsx
   - **[PDF]** - Descarga .pdf
   - **[CSV]** - Descarga .csv
3. El archivo se descarga automáticamente

### 6.3 Contenido Exportado

La exportación incluye:
- ✅ Todos los registros filtrados (no solo la página actual)
- ✅ Todas las columnas visibles
- ✅ Fecha y hora de generación
- ✅ Filtros aplicados

---

## 7. Columnas del Reporte

### 7.1 Ordenar Columnas

- Clic en el **encabezado de columna** para ordenar
- Clic nuevamente para invertir el orden
- Indicador ▲ (ascendente) o ▼ (descendente)

### 7.2 Tipos de Datos

| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Texto | Normal | "Producto ABC" |
| Número | Con decimales | 1,234.56 |
| Fecha | DD/MM/YYYY HH:mm | 15/03/2026 14:30 |
| Moneda | Con símbolo | ₡ 25,000.00 |
| Booleano | Sí/No | Sí |

---

## 8. Paginación

### 8.1 Navegación

```
◀ Anterior  |  Página 1 de 15  |  Siguiente ▶

Mostrando 1-10 de 150 registros
```

### 8.2 Registros por Página

Puede seleccionar cuántos registros ver:
- 10 registros
- 25 registros
- 50 registros
- 100 registros

---

## 9. Favoritos

### 9.1 Marcar como Favorito

1. En la lista de reportes
2. Clic en el ícono ⭐ junto al reporte
3. El reporte aparecerá en la sección "Favoritos"

### 9.2 Acceso Rápido

Los reportes favoritos aparecen primero para acceso rápido.

---

## 10. Reportes Disponibles

### 10.1 Lista de Artículos (ITM001)

**Descripción:** Lista completa de artículos del inventario

**Filtros:**
- Búsqueda (código, nombre, marca)
- Estado (Activo/Inactivo)
- Clasificación

**Columnas:**
- Código, Nombre, Marca, Descripción
- Clasificaciones, Unidad de Medida
- Stock, Precio, Estado

### 10.2 Historial de Impresión de Etiquetas (LBL001)

**Descripción:** Registro de todas las etiquetas impresas

**Filtros:**
- Búsqueda (código, nombre del artículo)
- Fecha desde/hasta

**Columnas:**
- Código y nombre del artículo
- Cantidad impresa
- Fecha de impresión
- Usuario que imprimió
- Impresora utilizada

---

## 11. Preguntas Frecuentes

### ¿Por qué mi reporte no muestra datos?
- Verifique los filtros aplicados
- Asegúrese de tener datos en el rango de fechas
- Confirme que tiene permisos para ver los datos

### ¿Puedo crear mis propios reportes?
Los reportes personalizados requieren configuración por un administrador.

### ¿Cuántos registros puedo exportar?
No hay límite, pero reportes muy grandes pueden tardar más.

### ¿Los reportes incluyen datos de todas las compañías?
No, solo de la compañía activa en la sesión.

---

## 12. Soporte

Para asistencia con reportes:
- **Email:** soporte@biti-solutions.com
- **Solicitar nuevo reporte:** Contacte al administrador

---

**© 2026 BITI Solutions S.A.**
