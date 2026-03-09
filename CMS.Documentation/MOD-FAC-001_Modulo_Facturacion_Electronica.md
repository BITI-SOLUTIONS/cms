# Módulo de Facturación Electrónica
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Facturación Electrónica permite emitir, recibir y gestionar comprobantes electrónicos cumpliendo con la normativa del Ministerio de Hacienda de Costa Rica.

---

## 2. Acceso al Módulo

**Ruta:** Menú → E-Invoicing

**Permisos necesarios:**
- `EInvoicing.View` - Ver módulo
- `EInvoicing.Issue` - Emitir comprobantes
- `EInvoicing.Receive` - Recibir comprobantes
- `EInvoicing.Void` - Anular comprobantes

---

## 3. Tipos de Comprobantes

### 3.1 Comprobantes de Venta

| Tipo | Código | Uso |
|------|--------|-----|
| Factura Electrónica | 01 | Venta a contribuyente |
| Tiquete Electrónico | 04 | Venta a consumidor final |
| Nota de Crédito | 03 | Corrección de factura |
| Nota de Débito | 02 | Cargos adicionales |

### 3.2 Comprobantes de Compra

| Tipo | Código | Uso |
|------|--------|-----|
| Factura de Compra | 08 | Compra a proveedor |
| Aceptación | -- | Aceptar factura recibida |
| Rechazo | -- | Rechazar factura recibida |

---

## 4. Emisión de Comprobantes

### 4.1 Crear Factura Electrónica

1. Ir a **E-Invoicing → Nueva Factura**
2. Complete encabezado:
   - Tipo de documento
   - Cliente (cédula requerida para FE)
   - Condición de venta
   - Medio de pago
3. Agregue líneas:
   - Producto/Servicio
   - Cantidad
   - Precio unitario
   - Impuestos
   - Descuentos
4. Revise totales
5. Clic en **"Emitir y Enviar"**

### 4.2 Campos Obligatorios

| Campo | Descripción |
|-------|-------------|
| Cédula emisor | Cédula de la empresa |
| Cédula receptor | Cédula del cliente |
| Fecha emisión | Fecha del documento |
| Consecutivo | Número automático |
| Detalle | Al menos una línea |
| Total | Monto total |

### 4.3 Condiciones de Venta

| Código | Descripción |
|--------|-------------|
| 01 | Contado |
| 02 | Crédito |
| 03 | Consignación |
| 04 | Apartado |
| 05 | Arrendamiento con opción de compra |
| 06 | Arrendamiento en función financiera |
| 99 | Otros |

### 4.4 Medios de Pago

| Código | Descripción |
|--------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia - Depósito bancario |
| 05 | Recaudado por terceros |
| 99 | Otros |

---

## 5. Proceso de Envío

### 5.1 Flujo de Emisión

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Generar   │────►│   Firmar    │────►│   Enviar    │
│     XML     │     │   con Llave │     │  a Hacienda │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Documento  │◄────│  Procesar   │◄────│   Recibir   │
│   Final     │     │  Respuesta  │     │  Respuesta  │
└─────────────┘     └─────────────┘     └─────────────┘
```

### 5.2 Estados del Documento

| Estado | Descripción |
|--------|-------------|
| Borrador | En edición |
| Pendiente | Listo para enviar |
| Enviado | Enviado a Hacienda |
| Procesando | En proceso en Hacienda |
| Aceptado | Validado por Hacienda ✅ |
| Rechazado | Error, debe corregirse ❌ |

### 5.3 Respuestas de Hacienda

| Código | Significado |
|--------|-------------|
| 1 | Aceptado |
| 2 | Aceptado parcialmente |
| 3 | Rechazado |

---

## 6. Recepción de Comprobantes

### 6.1 Consulta de Comprobantes

1. Ir a **E-Invoicing → Recibidos**
2. Sistema consulta comprobantes en Hacienda
3. Descarga automática de documentos

### 6.2 Aceptación/Rechazo

Para cada comprobante recibido:
1. Revisar el documento
2. Decidir: Aceptar o Rechazar
3. Si rechaza, indicar motivo
4. Enviar respuesta a Hacienda

### 6.3 Motivos de Rechazo

- Servicio no prestado
- Mercadería no recibida
- Error en datos del receptor
- Error en montos

---

## 7. Notas de Crédito

### 7.1 Crear Nota de Crédito

1. Ir a **E-Invoicing → Nueva Nota Crédito**
2. Seleccionar factura de referencia
3. Indicar motivo:
   - Anula documento de referencia
   - Corrige texto
   - Corrige monto
4. Especificar productos/montos afectados
5. Emitir

### 7.2 Códigos de Referencia

| Código | Descripción |
|--------|-------------|
| 01 | Anula documento de referencia |
| 02 | Corrige texto del documento |
| 03 | Corrige monto |
| 04 | Referencia a otro documento |
| 05 | Sustituye comprobante provisional |
| 99 | Otros |

---

## 8. Impuestos

### 8.1 IVA (Impuesto al Valor Agregado)

| Tarifa | Descripción |
|--------|-------------|
| 13% | Tarifa general |
| 4% | Canasta básica |
| 2% | Educación y salud privada |
| 1% | Bienes de capital |
| 0% | Exento |

### 8.2 Otros Impuestos

- Impuesto selectivo de consumo
- Impuesto específico
- IVA bienes usados

---

## 9. Configuración

### 9.1 Certificado Digital (Llave Criptográfica)

1. Ir a **Settings → Factura Electrónica**
2. Cargar archivo de certificado (.p12)
3. Ingresar PIN del certificado
4. Validar conexión con Hacienda

### 9.2 Consecutivos

| Serie | Rango | Uso |
|-------|-------|-----|
| 001 | 00000001-99999999 | Facturas |
| 002 | 00000001-99999999 | Tiquetes |
| 003 | 00000001-99999999 | Notas Crédito |

### 9.3 Ambiente

| Ambiente | Descripción |
|----------|-------------|
| Producción | Documentos reales |
| Pruebas (Stag) | Para desarrollo |

---

## 10. Reportes de Facturación

| Reporte | Descripción |
|---------|-------------|
| Libro de Ventas | Facturas emitidas |
| Libro de Compras | Facturas recibidas |
| IVA por Pagar | Impuestos a pagar |
| IVA Acreditable | Impuestos a favor |
| Documentos Rechazados | Errores |
| Resumen Mensual | Totales del mes |

---

## 11. Formatos de Exportación

### 11.1 XML

Formato estándar de Hacienda para:
- Archivo del documento
- Intercambio con otros sistemas

### 11.2 PDF

Representación gráfica del comprobante para:
- Envío al cliente
- Impresión
- Archivo

### 11.3 JSON

Para integración con sistemas externos.

---

## 12. Errores Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| Certificado inválido | PIN incorrecto | Verificar PIN |
| Cédula inválida | Formato incorrecto | Verificar número |
| Consecutivo duplicado | Ya se usó | Verificar secuencia |
| Sin respuesta | Timeout | Reintentar |
| Rechazado | Datos incorrectos | Revisar mensaje |

---

## 13. Normativa

### 13.1 Resoluciones Aplicables

- DGT-R-48-2016: Comprobantes electrónicos
- DGT-R-12-2018: Modificaciones
- DGT-R-20-2019: Tiquete electrónico

### 13.2 Plazos

- Emisión: Momento de la venta
- Envío a Hacienda: Máximo 8 días
- Respuesta de aceptación: 8 días

---

## 14. Soporte

Para asistencia con facturación electrónica:
- **Email:** soporte@biti-solutions.com
- **Hacienda:** https://www.hacienda.go.cr

---

**© 2026 BITI Solutions S.A.**
