# Módulo de Compras
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Compras gestiona el proceso de adquisición de productos y servicios, desde la solicitud hasta la recepción y pago a proveedores.

---

## 2. Acceso al Módulo

**Ruta:** Menú ? Purchasing

**Permisos necesarios:**
- `Purchasing.View` - Ver módulo
- `Purchasing.Requisitions.Create` - Crear solicitudes
- `Purchasing.Orders.Create` - Crear órdenes
- `Purchasing.Orders.Approve` - Aprobar órdenes

---

## 3. Proveedores

### 3.1 Registro de Proveedores

| Campo | Descripción |
|-------|-------------|
| Nombre | Nombre comercial |
| Razón Social | Nombre legal |
| Cédula/RUC | Identificación fiscal |
| Contacto | Persona de contacto |
| Teléfono | Número principal |
| Email | Correo electrónico |
| Dirección | Dirección física |
| Condiciones | Plazo de pago |

### 3.2 Evaluación de Proveedores

- Calidad de productos
- Cumplimiento de entregas
- Precios competitivos
- Servicio al cliente

---

## 4. Solicitudes de Compra

### 4.1 Crear Solicitud

1. Clic en **"? Nueva Solicitud"**
2. Seleccione departamento solicitante
3. Agregue productos/servicios requeridos
4. Indique fecha requerida
5. Agregue justificación
6. Envíe para aprobación

### 4.2 Flujo de Aprobación

```
Borrador ? Enviada ? En Revisión ? Aprobada ? En Proceso
                              ???? Rechazada
```

### 4.3 Niveles de Aprobación

| Monto | Aprobador |
|-------|-----------|
| < ?500,000 | Jefe de Área |
| ?500,000 - ?2,000,000 | Gerente |
| > ?2,000,000 | Director |

---

## 5. Órdenes de Compra

### 5.1 Crear Orden

1. Desde solicitud aprobada o directamente
2. Seleccione proveedor
3. Configure:
   - Productos y cantidades
   - Precios acordados
   - Condiciones de pago
   - Fecha de entrega esperada

### 5.2 Campos de la Orden

| Campo | Descripción |
|-------|-------------|
| Número OC | Consecutivo automático |
| Proveedor | Proveedor seleccionado |
| Moneda | Colones/Dólares |
| Condiciones | Forma de pago |
| Entrega | Lugar de recepción |
| Notas | Instrucciones adicionales |

### 5.3 Estados de la Orden

| Estado | Descripción |
|--------|-------------|
| Borrador | En edición |
| Enviada | Enviada al proveedor |
| Confirmada | Proveedor confirma |
| Recepción Parcial | Parte entregada |
| Recibida | Todo entregado |
| Facturada | Factura registrada |
| Pagada | Pago realizado |

---

## 6. Recepción de Mercadería

### 6.1 Proceso de Recepción

1. Ir a **Purchasing ? Recepciones**
2. Seleccionar OC pendiente
3. Registrar:
   - Cantidad recibida
   - Estado de productos
   - Número de guía/factura
4. Confirmar recepción
5. Productos ingresan a inventario

### 6.2 Discrepancias

| Tipo | Acción |
|------|--------|
| Faltante | Registrar y notificar |
| Sobrante | Registrar y devolver o facturar |
| Dañado | Registrar y rechazar |

---

## 7. Cuentas por Pagar

### 7.1 Registro de Facturas

1. Vincular factura a OC
2. Validar montos
3. Programar pago según condiciones

### 7.2 Programación de Pagos

- Por fecha de vencimiento
- Por prioridad
- Por disponibilidad de fondos

---

## 8. Reportes de Compras

| Reporte | Descripción |
|---------|-------------|
| Compras por Período | Total comprado |
| Compras por Proveedor | Gasto por proveedor |
| OC Pendientes | Órdenes sin recibir |
| Pagos Pendientes | Facturas por pagar |
| Análisis de Precios | Comparativo de precios |

---

## 9. Integración

- **Inventario:** Ingreso automático de productos
- **Contabilidad:** Registro de gastos y pasivos
- **Finanzas:** Cuentas por pagar

---

## 10. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
