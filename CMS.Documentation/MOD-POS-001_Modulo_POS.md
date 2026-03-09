# Módulo de Punto de Venta (POS)
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo POS (Point of Sale) permite realizar ventas de mostrador de forma rápida y eficiente. Diseñado para tiendas, restaurantes y negocios con atención directa al público.

---

## 2. Acceso al Módulo

**Ruta:** Menú ? POS

**Permisos necesarios:**
- `POS.View` - Acceder al POS
- `POS.Sales.Create` - Realizar ventas
- `POS.Returns.Create` - Procesar devoluciones
- `POS.Cash.Manage` - Gestionar caja

---

## 3. Apertura de Caja

### 3.1 Proceso de Apertura

1. Ir a **POS ? Abrir Caja**
2. Contar efectivo inicial
3. Ingresar monto de apertura
4. Confirmar apertura

### 3.2 Información Registrada

| Campo | Descripción |
|-------|-------------|
| Fecha/Hora | Momento de apertura |
| Cajero | Usuario que abre |
| Monto Inicial | Efectivo contado |
| Terminal | Punto de venta |

---

## 4. Pantalla de Ventas

### 4.1 Interfaz Principal

```
?????????????????????????????????????????????????????????????
? [Buscar producto...]                    Usuario: Juan    ?
?????????????????????????????????????????????????????????????
?                                                           ?
? Categorías: [Todos] [Bebidas] [Comida] [Accesorios]      ?
?                                                           ?
? ??????????? ??????????? ??????????? ???????????          ?
? ?Producto1? ?Producto2? ?Producto3? ?Producto4?          ?
? ? ?1,500  ? ? ?2,000  ? ? ?3,500  ? ? ?1,200  ?          ?
? ??????????? ??????????? ??????????? ???????????          ?
?                                                           ?
?????????????????????????????????????????????????????????????
? Carrito:                                     TOTAL: ?6,700?
? - Producto1 x 2 .............. ?3,000                    ?
? - Producto3 x 1 .............. ?3,500                    ?
? - Descuento .................. -?200                     ?
?                                                           ?
? [Descuento] [Cliente] [Limpiar] [?? COBRAR]              ?
?????????????????????????????????????????????????????????????
```

### 4.2 Agregar Productos

**Métodos:**
- Clic en el producto
- Búsqueda por nombre/código
- Escanear código de barras
- Ingresar código manualmente

### 4.3 Modificar Cantidades

- Clic en + o - 
- Ingresar cantidad directamente
- Eliminar línea con X

---

## 5. Cobro

### 5.1 Formas de Pago

| Forma | Descripción |
|-------|-------------|
| Efectivo | Pago en efectivo |
| Tarjeta | Crédito/Débito |
| Transferencia | SINPE/Transferencia |
| Mixto | Combinación de formas |

### 5.2 Proceso de Cobro

1. Clic en **"?? COBRAR"**
2. Seleccione forma de pago
3. Si es efectivo, ingrese monto recibido
4. Sistema calcula vuelto
5. Confirme la venta
6. Imprima/envíe comprobante

### 5.3 Factura Electrónica

- Seleccionar tipo: Tiquete o Factura
- Si es factura, ingresar datos del cliente
- Generación automática de XML
- Envío a Hacienda

---

## 6. Devoluciones

### 6.1 Procesar Devolución

1. Ir a **POS ? Devoluciones**
2. Buscar venta original (número, fecha)
3. Seleccionar productos a devolver
4. Indicar motivo
5. Procesar reembolso

### 6.2 Formas de Reembolso

- Efectivo
- Nota de crédito
- Reverso a tarjeta

---

## 7. Cierre de Caja

### 7.1 Proceso de Cierre

1. Ir a **POS ? Cerrar Caja**
2. Sistema muestra:
   - Ventas del día
   - Desglose por forma de pago
   - Monto esperado en caja
3. Contar efectivo real
4. Registrar monto contado
5. Sistema calcula diferencia
6. Confirmar cierre

### 7.2 Reporte de Cierre

| Concepto | Esperado | Real | Diferencia |
|----------|----------|------|------------|
| Apertura | ?50,000 | - | - |
| Ventas Efectivo | ?150,000 | ?149,500 | -?500 |
| Ventas Tarjeta | ?200,000 | ?200,000 | ?0 |
| Devoluciones | -?5,000 | -?5,000 | ?0 |
| **Total Efectivo** | ?195,000 | ?194,500 | **-?500** |

---

## 8. Funciones Adicionales

### 8.1 Apartar Productos

- Reservar productos para cliente
- Abono parcial
- Fecha límite para recoger

### 8.2 Pedidos a Domicilio

- Registrar dirección de entrega
- Asignar repartidor
- Seguimiento de estado

### 8.3 Comandas (Restaurantes)

- Enviar a cocina
- Estados: Pendiente, Preparando, Listo
- Agrupación por mesa

---

## 9. Configuración

### 9.1 Opciones

| Opción | Descripción |
|--------|-------------|
| Impresora térmica | Configurar impresora |
| Tamaño de papel | 80mm, 58mm |
| Logo en tiquete | Activar/desactivar |
| Datos en tiquete | Información del negocio |

---

## 10. Reportes POS

| Reporte | Descripción |
|---------|-------------|
| Ventas del Día | Resumen de ventas |
| Por Cajero | Ventas por usuario |
| Por Forma de Pago | Desglose de pagos |
| Productos Vendidos | Detalle de artículos |
| Histórico de Cierres | Cierres anteriores |

---

## 11. Soporte

Para asistencia con el POS:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
