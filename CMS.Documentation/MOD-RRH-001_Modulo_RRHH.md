# Módulo de Recursos Humanos
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Recursos Humanos gestiona la información de empleados, nómina, vacaciones, permisos y control de asistencia.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Human Resources

**Permisos necesarios:**
- `HR.View` - Ver módulo
- `HR.Employees.Manage` - Gestionar empleados
- `HR.Payroll.Process` - Procesar nómina
- `HR.TimeOff.Approve` - Aprobar vacaciones

---

## 3. Empleados

### 3.1 Ficha del Empleado

| Sección | Datos |
|---------|-------|
| **Personal** | Nombre, cédula, fecha nacimiento, foto |
| **Contacto** | Dirección, teléfono, email, emergencia |
| **Laboral** | Puesto, departamento, jefe, fecha ingreso |
| **Salarial** | Salario base, deducciones, beneficios |
| **Documentos** | Contratos, evaluaciones, capacitaciones |

### 3.2 Crear Empleado

1. Ir a **HR → Empleados**
2. Clic en **"➕ Nuevo Empleado"**
3. Complete información:
   - Datos personales
   - Información de contacto
   - Datos laborales
   - Información bancaria
4. Adjunte documentos
5. Guarde

### 3.3 Estados del Empleado

| Estado | Descripción |
|--------|-------------|
| Activo | Empleado en funciones |
| Vacaciones | En período de vacaciones |
| Incapacidad | Con licencia médica |
| Suspendido | Suspensión temporal |
| Liquidado | Ya no labora |

---

## 4. Estructura Organizacional

### 4.1 Departamentos

- Crear departamentos
- Asignar jefaturas
- Definir jerarquía

### 4.2 Puestos

- Nombre del puesto
- Descripción de funciones
- Rango salarial
- Competencias requeridas

### 4.3 Organigrama

Visualización gráfica de la estructura de la empresa.

---

## 5. Nómina

### 5.1 Conceptos de Pago

**Devengos (Ingresos):**
- Salario base
- Horas extra
- Comisiones
- Bonificaciones
- Viáticos

**Deducciones:**
- CCSS (9.34% empleado)
- Impuesto sobre la renta
- Préstamos
- Embargos
- Otros descuentos

### 5.2 Proceso de Nómina

1. **Preparación**
   - Verificar novedades (incapacidades, permisos)
   - Registrar horas extra
   - Agregar comisiones/bonos

2. **Cálculo**
   - Ejecutar cálculo de nómina
   - Revisar resultados
   - Ajustes si necesario

3. **Aprobación**
   - Revisión por RRHH
   - Aprobación por gerencia

4. **Pago**
   - Generar archivo de pagos
   - Enviar a banco
   - Confirmar transferencias

5. **Distribución**
   - Enviar comprobantes de pago
   - Generar reportes

### 5.3 Ejemplo de Cálculo

```
EMPLEADO: Juan Pérez
PERÍODO: Marzo 2026

INGRESOS:
  Salario base ........... ₡800,000
  Horas extra ............ ₡ 50,000
  Bono desempeño ......... ₡ 25,000
  ─────────────────────────────────
  TOTAL DEVENGADO ........ ₡875,000

DEDUCCIONES:
  CCSS (9.34%) ........... ₡ 81,725
  Impuesto renta ......... ₡ 45,000
  Préstamo empresa ....... ₡ 20,000
  ─────────────────────────────────
  TOTAL DEDUCCIONES ...... ₡146,725

  SALARIO NETO ........... ₡728,275
```

---

## 6. Vacaciones y Permisos

### 6.1 Solicitud de Vacaciones

1. Empleado ingresa solicitud
2. Sistema calcula días disponibles
3. Jefe directo aprueba/rechaza
4. RRHH confirma
5. Se registra en calendario

### 6.2 Cálculo de Vacaciones

- **Acumulación:** 1 día por mes trabajado
- **Máximo acumulable:** 2 años (según política)
- **Pago de vacaciones:** Salario + carga social

### 6.3 Tipos de Permisos

| Tipo | Días | Pago |
|------|------|------|
| Matrimonio | 3 | Con goce |
| Nacimiento (padre) | 3 | Con goce |
| Duelo familiar | 3 | Con goce |
| Personal | Variable | Sin goce |
| Médico | Según certificado | Con goce |

---

## 7. Control de Asistencia

### 7.1 Métodos de Registro

- Reloj biométrico
- Tarjeta de proximidad
- App móvil
- Registro manual

### 7.2 Información Registrada

| Dato | Descripción |
|------|-------------|
| Entrada | Hora de llegada |
| Salida almuerzo | Inicio de descanso |
| Entrada almuerzo | Fin de descanso |
| Salida | Hora de retiro |

### 7.3 Reportes de Asistencia

- Asistencia diaria
- Tardanzas
- Ausencias
- Horas trabajadas
- Horas extra

---

## 8. Evaluaciones de Desempeño

### 8.1 Proceso

1. Definir período de evaluación
2. Establecer objetivos/competencias
3. Autoevaluación del empleado
4. Evaluación del jefe
5. Reunión de retroalimentación
6. Plan de desarrollo

### 8.2 Escalas de Evaluación

| Calificación | Descripción |
|--------------|-------------|
| 5 | Excepcional |
| 4 | Supera expectativas |
| 3 | Cumple expectativas |
| 2 | Necesita mejora |
| 1 | Insatisfactorio |

---

## 9. Capacitación

### 9.1 Registro de Capacitaciones

- Nombre del curso
- Proveedor
- Fecha y duración
- Empleados participantes
- Costo
- Certificación obtenida

### 9.2 Plan de Capacitación

- Necesidades detectadas
- Presupuesto asignado
- Calendario de cursos
- Seguimiento de avance

---

## 10. Reportes de RRHH

| Reporte | Descripción |
|---------|-------------|
| Planilla | Detalle de nómina |
| Vacaciones | Saldos y solicitudes |
| Rotación | Ingresos y salidas |
| Asistencia | Control de asistencia |
| Costos Laborales | Gastos de personal |
| Evaluaciones | Resultados de desempeño |

---

## 11. Integración

- **Contabilidad:** Registro de gastos de nómina
- **Finanzas:** Pagos de nómina
- **Portal del Empleado:** Autoservicio

---

## 12. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
