# Módulo de Finanzas
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Finanzas gestiona las cuentas por cobrar, cuentas por pagar, flujo de caja, conciliaciones bancarias y gestión de tesorería.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Finance

**Permisos necesarios:**
- `Finance.View` - Ver módulo
- `Finance.Receivables.Manage` - Gestionar CxC
- `Finance.Payables.Manage` - Gestionar CxP
- `Finance.Banking.View` - Ver bancos

---

## 3. Cuentas por Cobrar

### 3.1 Vista General

Dashboard mostrando:
- Total por cobrar
- Facturas vencidas
- Próximas a vencer
- Antigüedad de saldos

### 3.2 Antigüedad de Cartera

| Rango | Monto | % |
|-------|-------|---|
| Al día | ₡5,000,000 | 50% |
| 1-30 días | ₡2,000,000 | 20% |
| 31-60 días | ₡1,500,000 | 15% |
| 61-90 días | ₡1,000,000 | 10% |
| > 90 días | ₡500,000 | 5% |

### 3.3 Registro de Cobros

1. Ir a **Finance → Cobros**
2. Seleccionar cliente
3. Ver facturas pendientes
4. Registrar pago:
   - Monto recibido
   - Forma de pago
   - Referencia bancaria
   - Facturas que cancela
5. Confirmar cobro

### 3.4 Aplicación de Pagos

- **Automática:** Por antigüedad
- **Manual:** Selección de facturas
- **Parcial:** Abono a factura
- **Anticipos:** Pagos sin factura

---

## 4. Cuentas por Pagar

### 4.1 Vista General

- Total por pagar
- Vencimientos próximos
- Pagos programados
- Flujo de pagos proyectado

### 4.2 Programación de Pagos

1. Ir a **Finance → Pagos**
2. Seleccionar facturas a pagar
3. Programar fecha de pago
4. Seleccionar cuenta bancaria
5. Generar propuesta de pago

### 4.3 Ejecución de Pagos

| Método | Proceso |
|--------|---------|
| Transferencia | Generar archivo para banco |
| Cheque | Imprimir cheque |
| Efectivo | Registrar desembolso |

### 4.4 Estados del Pago

```
Programado → Aprobado → Ejecutado → Confirmado
```

---

## 5. Bancos

### 5.1 Cuentas Bancarias

| Campo | Descripción |
|-------|-------------|
| Banco | Nombre del banco |
| Tipo | Corriente, Ahorro, etc. |
| Número | Número de cuenta |
| Moneda | Colones, Dólares |
| Saldo | Saldo actual |

### 5.2 Movimientos Bancarios

- Depósitos recibidos
- Transferencias enviadas
- Cheques emitidos
- Débitos automáticos
- Comisiones bancarias

### 5.3 Conciliación Bancaria

1. Importar estado de cuenta del banco
2. Sistema propone coincidencias
3. Revisar y confirmar partidas
4. Identificar partidas pendientes
5. Generar reporte de conciliación

**Partidas en Conciliación:**

| Tipo | Ejemplo |
|------|---------|
| Cheques en tránsito | Cheques emitidos no cobrados |
| Depósitos en tránsito | Depósitos no reflejados |
| Notas débito | Comisiones no registradas |
| Notas crédito | Intereses ganados |

---

## 6. Flujo de Caja

### 6.1 Proyección de Flujo

```
Semana 1    Semana 2    Semana 3    Semana 4
┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
│+₡5,000  │ │+₡3,000  │ │+₡4,000  │ │+₡6,000  │ Ingresos
│-₡4,000  │ │-₡5,000  │ │-₡3,500  │ │-₡4,500  │ Egresos
├─────────┤ ├─────────┤ ├─────────┤ ├─────────┤
│ ₡1,000  │ │-₡2,000  │ │  ₡500   │ │ ₡1,500  │ Neto
└─────────┘ └─────────┘ └─────────┘ └─────────┘
```

### 6.2 Categorías de Flujo

**Ingresos:**
- Cobros de clientes
- Otros ingresos
- Préstamos recibidos

**Egresos:**
- Pagos a proveedores
- Nómina
- Impuestos
- Gastos operativos

---

## 7. Gestión de Crédito

### 7.1 Límites de Crédito

- Asignación por cliente
- Evaluación crediticia
- Alertas de sobregiro

### 7.2 Políticas de Cobro

| Días Vencido | Acción |
|--------------|--------|
| 1-15 | Recordatorio amable |
| 16-30 | Llamada de cobro |
| 31-60 | Carta formal |
| > 60 | Gestión legal |

---

## 8. Reportes Financieros

| Reporte | Descripción |
|---------|-------------|
| Antigüedad CxC | Cartera por vencimiento |
| Antigüedad CxP | Deudas por vencimiento |
| Flujo de Caja | Proyección de efectivo |
| Estado de Cuenta | Movimientos por cliente/proveedor |
| Conciliación | Estado de cuentas bancarias |
| Cobranza | Eficiencia de cobro |

---

## 9. Integración

- **Ventas:** Generación de CxC
- **Compras:** Generación de CxP
- **Contabilidad:** Registro de movimientos
- **Bancos:** Importación de movimientos

---

## 10. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
