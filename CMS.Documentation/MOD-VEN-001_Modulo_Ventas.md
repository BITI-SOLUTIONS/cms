# Módulo de Ventas
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Ventas gestiona todo el ciclo de ventas de la empresa: desde cotizaciones hasta la generación de pedidos y facturación.

---

## 2. Acceso al Módulo

**Ruta:** Menú ? Sales

**Permisos necesarios:**
- `Sales.View` - Ver módulo de ventas
- `Sales.Quotes.Create` - Crear cotizaciones
- `Sales.Orders.Create` - Crear pedidos
- `Sales.Orders.Approve` - Aprobar pedidos

---

## 3. Cotizaciones

### 3.1 Crear Cotización

1. Clic en **"? Nueva Cotización"**
2. Seleccione cliente
3. Agregue productos/servicios:
   - Buscar artículo
   - Cantidad
   - Precio unitario
   - Descuento (si aplica)
4. Revise totales
5. Guarde o envíe al cliente

### 3.2 Estados de Cotización

| Estado | Descripción |
|--------|-------------|
| Borrador | En edición |
| Enviada | Enviada al cliente |
| Aceptada | Cliente aceptó |
| Rechazada | Cliente rechazó |
| Vencida | Pasó fecha de vigencia |
| Convertida | Convertida a pedido |

### 3.3 Validez

- Configurar días de vigencia
- Alerta antes de vencer
- Renovación de cotización

---

## 4. Pedidos de Venta

### 4.1 Crear Pedido

**Desde cotización:**
1. Abra cotización aceptada
2. Clic en **"Convertir a Pedido"**

**Directo:**
1. Clic en **"? Nuevo Pedido"**
2. Complete información del cliente
3. Agregue líneas de productos

### 4.2 Información del Pedido

| Campo | Descripción |
|-------|-------------|
| Número | Consecutivo automático |
| Fecha | Fecha del pedido |
| Cliente | Cliente seleccionado |
| Vendedor | Responsable de la venta |
| Condiciones | Forma de pago |
| Entrega | Fecha y dirección de entrega |
| Líneas | Productos y cantidades |

### 4.3 Flujo del Pedido

```
Borrador ? Confirmado ? En Proceso ? Despachado ? Facturado ? Completado
```

---

## 5. Clientes

### 5.1 Registro de Clientes

- Datos generales (nombre, cédula)
- Información de contacto
- Direcciones de entrega
- Condiciones comerciales
- Límite de crédito

### 5.2 Historial

- Pedidos realizados
- Facturas emitidas
- Pagos recibidos
- Saldo pendiente

---

## 6. Precios y Descuentos

### 6.1 Listas de Precios

- Precio regular
- Precio mayorista
- Precio distribuidor
- Precios especiales por cliente

### 6.2 Descuentos

| Tipo | Descripción |
|------|-------------|
| Por producto | Descuento en artículo específico |
| Por cliente | Descuento especial para cliente |
| Por volumen | Descuento por cantidad |
| Promocional | Descuento temporal |

---

## 7. Despacho

### 7.1 Preparación

1. Pedido confirmado aparece en lista
2. Verificar stock disponible
3. Preparar productos
4. Generar guía de despacho

### 7.2 Entrega

- Registro de fecha y hora
- Firma de recibido (opcional)
- Actualización de estado

---

## 8. Reportes de Ventas

| Reporte | Descripción |
|---------|-------------|
| Ventas por Período | Total de ventas en rango |
| Ventas por Vendedor | Rendimiento por vendedor |
| Ventas por Producto | Productos más vendidos |
| Ventas por Cliente | Mejores clientes |
| Comparativo | Comparación entre períodos |

---

## 9. Integración

- **Inventario:** Reserva de stock
- **Facturación:** Generación de factura
- **CRM:** Historial del cliente
- **Contabilidad:** Registro de ingresos

---

## 10. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
