# Módulo de Contabilidad
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Contabilidad permite gestionar la información financiera de la empresa según las Normas Internacionales de Información Financiera (NIIF) y regulaciones locales.

---

## 2. Acceso al Módulo

**Ruta:** Menú ? Accounting

**Permisos necesarios:**
- `Accounting.View` - Ver módulo
- `Accounting.Journal.Create` - Crear asientos
- `Accounting.Journal.Approve` - Aprobar asientos
- `Accounting.Close` - Cerrar períodos

---

## 3. Plan de Cuentas

### 3.1 Estructura

```
1. ACTIVOS
   1.1 Activo Corriente
       1.1.01 Caja y Bancos
       1.1.02 Cuentas por Cobrar
       1.1.03 Inventarios
   1.2 Activo No Corriente
       1.2.01 Propiedad, Planta y Equipo
       1.2.02 Activos Intangibles

2. PASIVOS
   2.1 Pasivo Corriente
       2.1.01 Cuentas por Pagar
       2.1.02 Impuestos por Pagar
   2.2 Pasivo No Corriente

3. PATRIMONIO
   3.1 Capital Social
   3.2 Utilidades Retenidas

4. INGRESOS
   4.1 Ingresos Operacionales
   4.2 Otros Ingresos

5. GASTOS
   5.1 Gastos Operacionales
   5.2 Gastos Financieros
```

### 3.2 Crear Cuenta

1. Ir a **Accounting ? Plan de Cuentas**
2. Clic en **"? Nueva Cuenta"**
3. Complete:
   - Código de cuenta
   - Nombre
   - Tipo (Activo, Pasivo, etc.)
   - Nivel (Mayor, Auxiliar)
   - Naturaleza (Débito/Crédito)

### 3.3 Tipos de Cuenta

| Tipo | Naturaleza Normal |
|------|-------------------|
| Activo | Débito |
| Pasivo | Crédito |
| Patrimonio | Crédito |
| Ingreso | Crédito |
| Gasto | Débito |

---

## 4. Asientos Contables

### 4.1 Crear Asiento

1. Ir a **Accounting ? Asientos**
2. Clic en **"? Nuevo Asiento"**
3. Ingrese:
   - Fecha
   - Descripción
   - Referencia (opcional)
4. Agregue líneas:
   - Cuenta
   - Débito o Crédito
   - Centro de costo (opcional)
5. Verifique que cuadre (Débito = Crédito)
6. Guarde

### 4.2 Ejemplo de Asiento

**Venta de mercadería al contado:**

| Cuenta | Débito | Crédito |
|--------|--------|---------|
| 1.1.01 Caja | ?100,000 | |
| 4.1.01 Ventas | | ?87,719 |
| 2.1.05 IVA por Pagar | | ?12,281 |
| **Totales** | **?100,000** | **?100,000** |

### 4.3 Estados del Asiento

| Estado | Descripción |
|--------|-------------|
| Borrador | En edición |
| Pendiente | Esperando aprobación |
| Aprobado | Listo para contabilizar |
| Contabilizado | Registrado en mayor |
| Anulado | Reversado |

---

## 5. Libros Contables

### 5.1 Libro Diario

Registro cronológico de todos los asientos contables.

### 5.2 Libro Mayor

Resumen de movimientos por cuenta contable.

### 5.3 Balance de Comprobación

| Cuenta | Saldo Anterior | Débitos | Créditos | Saldo Final |
|--------|----------------|---------|----------|-------------|
| Caja | ?500,000 | ?200,000 | ?150,000 | ?550,000 |
| Bancos | ?2,000,000 | ?500,000 | ?300,000 | ?2,200,000 |
| ... | ... | ... | ... | ... |

---

## 6. Estados Financieros

### 6.1 Balance General

- Activos = Pasivos + Patrimonio
- Clasificación: Corriente / No Corriente
- Comparativo con período anterior

### 6.2 Estado de Resultados

```
Ingresos Operacionales         ?10,000,000
(-) Costo de Ventas            (?6,000,000)
= Utilidad Bruta               ?4,000,000

(-) Gastos Operacionales       (?2,500,000)
= Utilidad Operacional         ?1,500,000

(+/-) Otros Ingresos/Gastos    ?100,000
= Utilidad Antes de Impuestos  ?1,600,000

(-) Impuesto sobre la Renta    (?480,000)
= Utilidad Neta                ?1,120,000
```

### 6.3 Estado de Flujos de Efectivo

- Actividades de operación
- Actividades de inversión
- Actividades de financiamiento

---

## 7. Períodos Contables

### 7.1 Gestión de Períodos

| Estado | Descripción |
|--------|-------------|
| Abierto | Se pueden registrar asientos |
| Cerrado | No permite nuevos registros |
| Ajuste | Solo asientos de ajuste |

### 7.2 Cierre Mensual

1. Verificar asientos pendientes
2. Realizar ajustes necesarios
3. Generar reportes del período
4. Cerrar período

### 7.3 Cierre Anual

1. Cierre de cuentas de resultados
2. Traslado de utilidad/pérdida
3. Apertura de nuevo ejercicio

---

## 8. Centros de Costo

### 8.1 Configuración

- Departamentos
- Proyectos
- Sucursales
- Líneas de negocio

### 8.2 Reportes por Centro de Costo

- Gastos por departamento
- Rentabilidad por proyecto
- Comparativo entre centros

---

## 9. Reportes Contables

| Reporte | Descripción |
|---------|-------------|
| Balance de Comprobación | Saldos de todas las cuentas |
| Libro Diario | Asientos por período |
| Libro Mayor | Movimientos por cuenta |
| Balance General | Estado de situación |
| Estado de Resultados | Pérdidas y ganancias |
| Auxiliar de Cuenta | Detalle de movimientos |

---

## 10. Integración

- **Ventas/Facturación:** Registro automático de ingresos
- **Compras:** Registro automático de gastos
- **Inventario:** Valoración y costo de ventas
- **Nómina:** Gastos de personal
- **Bancos:** Conciliación bancaria

---

## 11. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
