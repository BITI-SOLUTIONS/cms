# Módulo de CRM
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de CRM (Customer Relationship Management) permite gestionar las relaciones con clientes y prospectos. Incluye gestión de contactos, empresas, oportunidades de venta, seguimiento de actividades y análisis de pipeline.

---

## 2. Acceso al Módulo

**Ruta:** Menú ? CRM

**Permisos necesarios:**
- `CRM.View` - Ver módulo CRM
- `CRM.Contacts.View` - Ver contactos
- `CRM.Contacts.Create` - Crear contactos
- `CRM.Contacts.Edit` - Editar contactos
- `CRM.Opportunities.View` - Ver oportunidades

---

## 3. Gestión de Contactos

### 3.1 Lista de Contactos

Información mostrada:
- Nombre completo
- Empresa asociada
- Email y teléfono
- Cargo/Posición
- Fecha último contacto

### 3.2 Crear Contacto

1. Clic en **"? Nuevo Contacto"**
2. Complete datos:
   - Nombre y apellido
   - Email (requerido)
   - Teléfono
   - Empresa
   - Cargo
3. Guarde el contacto

### 3.3 Campos del Contacto

| Campo | Tipo | Requerido |
|-------|------|-----------|
| Nombre | Texto | ? |
| Apellido | Texto | ? |
| Email | Email | ? |
| Teléfono | Texto | ? |
| Móvil | Texto | ? |
| Empresa | Relación | ? |
| Cargo | Texto | ? |
| Dirección | Texto | ? |
| Notas | Texto largo | ? |

---

## 4. Gestión de Empresas

### 4.1 Registro de Empresas

- Nombre comercial
- Razón social
- Cédula jurídica
- Industria/Sector
- Dirección
- Contactos asociados

### 4.2 Categorización

- **Prospecto:** Empresa potencial
- **Cliente:** Empresa con compras realizadas
- **Partner:** Socio comercial
- **Proveedor:** También es proveedor

---

## 5. Oportunidades de Venta

### 5.1 Pipeline de Ventas

Estados del pipeline:
1. **Prospección:** Oportunidad identificada
2. **Calificación:** Evaluando necesidades
3. **Propuesta:** Cotización enviada
4. **Negociación:** En negociación
5. **Ganada:** Venta cerrada ?
6. **Perdida:** No se cerró ?

### 5.2 Crear Oportunidad

1. Clic en **"? Nueva Oportunidad"**
2. Seleccione contacto/empresa
3. Ingrese:
   - Nombre de la oportunidad
   - Valor estimado
   - Fecha cierre esperada
   - Etapa del pipeline
   - Probabilidad (%)

### 5.3 Seguimiento

- Registrar llamadas
- Registrar reuniones
- Enviar emails
- Agregar notas
- Programar tareas

---

## 6. Actividades

### 6.1 Tipos de Actividades

| Tipo | Icono | Descripción |
|------|-------|-------------|
| Llamada | ?? | Llamada telefónica |
| Reunión | ?? | Reunión presencial o virtual |
| Email | ?? | Correo enviado/recibido |
| Tarea | ? | Tarea pendiente |
| Nota | ?? | Nota o comentario |

### 6.2 Calendario

- Vista mensual, semanal, diaria
- Integración con calendario externo (opcional)
- Recordatorios automáticos

---

## 7. Reportes CRM

### 7.1 Reportes Disponibles

| Reporte | Descripción |
|---------|-------------|
| Pipeline de Ventas | Oportunidades por etapa |
| Forecast | Proyección de ventas |
| Actividades | Resumen de actividades |
| Contactos por Empresa | Lista de contactos agrupados |
| Conversión | Tasa de conversión del pipeline |

---

## 8. Integración

### 8.1 Con otros módulos

- **Ventas:** Convertir oportunidad ganada en pedido
- **Facturación:** Ver historial de facturas del cliente
- **Soporte:** Ver tickets del cliente

---

## 9. Soporte

Para asistencia con el módulo CRM:
- **Email:** soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
