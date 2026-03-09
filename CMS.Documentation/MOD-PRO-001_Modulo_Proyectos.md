# Módulo de Proyectos
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Proyectos permite planificar, ejecutar y dar seguimiento a proyectos de cualquier tipo, incluyendo gestión de tareas, recursos, presupuesto y cronogramas.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Projects

**Permisos necesarios:**
- `Projects.View` - Ver proyectos
- `Projects.Create` - Crear proyectos
- `Projects.Tasks.Manage` - Gestionar tareas
- `Projects.Resources.Assign` - Asignar recursos

---

## 3. Gestión de Proyectos

### 3.1 Crear Proyecto

1. Ir a **Projects → Lista**
2. Clic en **"➕ Nuevo Proyecto"**
3. Complete información:
   - Nombre del proyecto
   - Descripción
   - Cliente (opcional)
   - Fecha inicio y fin
   - Gerente de proyecto
   - Presupuesto
4. Guarde

### 3.2 Campos del Proyecto

| Campo | Descripción |
|-------|-------------|
| Código | Identificador único |
| Nombre | Nombre del proyecto |
| Cliente | Cliente asociado |
| Gerente | Responsable del proyecto |
| Fecha Inicio | Inicio planificado |
| Fecha Fin | Fin planificado |
| Presupuesto | Monto asignado |
| Estado | Fase actual |
| Prioridad | Alta, Media, Baja |

### 3.3 Estados del Proyecto

```
Planificación → En Progreso → En Pausa → Completado
                     └──────────→ Cancelado
```

---

## 4. Tareas

### 4.1 Crear Tarea

1. Dentro del proyecto, sección Tareas
2. Clic en **"➕ Nueva Tarea"**
3. Complete:
   - Nombre de la tarea
   - Descripción
   - Responsable
   - Fecha inicio y fin
   - Prioridad
   - Dependencias

### 4.2 Vista Kanban

```
┌─────────────┬─────────────┬─────────────┬─────────────┐
│  PENDIENTE  │ EN PROGRESO │  EN REVISIÓN │ COMPLETADO  │
├─────────────┼─────────────┼─────────────┼─────────────┤
│ ┌─────────┐ │ ┌─────────┐ │ ┌─────────┐ │ ┌─────────┐ │
│ │ Tarea 1 │ │ │ Tarea 3 │ │ │ Tarea 5 │ │ │ Tarea 2 │ │
│ └─────────┘ │ ├─────────┤ │ └─────────┘ │ ├─────────┤ │
│ ┌─────────┐ │ │ Tarea 4 │ │             │ │ Tarea 6 │ │
│ │ Tarea 7 │ │ └─────────┘ │             │ └─────────┘ │
│ └─────────┘ │             │             │             │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### 4.3 Vista de Lista

| Tarea | Responsable | Estado | Prioridad | Vencimiento |
|-------|-------------|--------|-----------|-------------|
| Diseño UI | Ana | En progreso | Alta | 15/03 |
| Backend API | Carlos | Pendiente | Alta | 20/03 |
| Testing | María | Pendiente | Media | 25/03 |

### 4.4 Diagrama de Gantt

Visualización temporal de tareas con:
- Barras de duración
- Dependencias (flechas)
- Hitos importantes
- Ruta crítica

---

## 5. Subtareas y Listas de Verificación

### 5.1 Subtareas

Dividir tareas grandes en subtareas más pequeñas:

```
Tarea: Implementar módulo de pagos
  └── Subtarea 1: Diseñar base de datos
  └── Subtarea 2: Crear API endpoints
  └── Subtarea 3: Desarrollar UI
  └── Subtarea 4: Pruebas unitarias
```

### 5.2 Checklists

Lista de verificación dentro de una tarea:
- [ ] Revisar requerimientos
- [ ] Aprobar diseño
- [x] Crear repositorio
- [ ] Documentar API

---

## 6. Recursos

### 6.1 Asignación de Recursos

- Asignar personas a tareas
- Definir horas estimadas
- Registrar horas trabajadas
- Ver disponibilidad

### 6.2 Capacidad del Equipo

| Recurso | Asignado | Disponible |
|---------|----------|------------|
| Juan Pérez | 40h | 0h |
| Ana García | 30h | 10h |
| Carlos López | 20h | 20h |

---

## 7. Tiempo y Horas

### 7.1 Registro de Horas

1. En la tarea, clic en **"Registrar Tiempo"**
2. Ingrese:
   - Fecha
   - Horas trabajadas
   - Descripción del trabajo
3. Guarde

### 7.2 Timesheet Semanal

| Día | Proyecto A | Proyecto B | Total |
|-----|------------|------------|-------|
| Lun | 4h | 4h | 8h |
| Mar | 6h | 2h | 8h |
| Mié | 5h | 3h | 8h |
| Jue | 8h | 0h | 8h |
| Vie | 4h | 4h | 8h |

---

## 8. Presupuesto y Costos

### 8.1 Control de Presupuesto

| Concepto | Presupuestado | Ejecutado | Variación |
|----------|---------------|-----------|-----------|
| Mano de obra | ₡2,000,000 | ₡1,800,000 | +₡200,000 |
| Materiales | ₡500,000 | ₡550,000 | -₡50,000 |
| Servicios | ₡300,000 | ₡280,000 | +₡20,000 |
| **Total** | **₡2,800,000** | **₡2,630,000** | **+₡170,000** |

### 8.2 Costos por Recurso

- Tarifa por hora del recurso
- Horas trabajadas
- Costo total calculado

---

## 9. Comunicación

### 9.1 Comentarios

- Comentarios en tareas
- Menciones (@usuario)
- Adjuntar archivos
- Historial de actividad

### 9.2 Notificaciones

- Asignación de tareas
- Cambios de estado
- Menciones
- Vencimientos próximos

---

## 10. Reportes de Proyectos

| Reporte | Descripción |
|---------|-------------|
| Estado del Proyecto | Resumen general |
| Tareas Pendientes | Tareas sin completar |
| Horas por Proyecto | Tiempo invertido |
| Costos vs Presupuesto | Control financiero |
| Productividad | Rendimiento del equipo |
| Gantt | Cronograma visual |

---

## 11. Plantillas

### 11.1 Plantillas de Proyecto

Crear proyectos base reutilizables:
- Estructura de tareas predefinida
- Roles estándar
- Checklists típicos

---

## 12. Integración

- **CRM:** Vincular con oportunidades ganadas
- **Facturación:** Facturar horas trabajadas
- **RRHH:** Asignación de personal
- **Contabilidad:** Registro de costos

---

## 13. Soporte

Para asistencia:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
