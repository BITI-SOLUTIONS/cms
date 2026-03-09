# Manual General del Sistema CMS
## BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026  
**Autor:** BITI Solutions S.A.

---

## Tabla de Contenidos

1. [Introducción](#1-introducción)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Requisitos del Sistema](#3-requisitos-del-sistema)
4. [Acceso al Sistema](#4-acceso-al-sistema)
5. [Navegación Principal](#5-navegación-principal)
6. [Módulos del Sistema](#6-módulos-del-sistema)
7. [Gestión Multi-Compañía](#7-gestión-multi-compañía)
8. [Soporte Técnico](#8-soporte-técnico)

---

## 1. Introducción

### 1.1 ¿Qué es CMS?

CMS (Content Management System) de BITI Solutions es una plataforma empresarial integral diseñada para gestionar las operaciones de negocios de manera eficiente y centralizada. El sistema ofrece una solución completa que abarca desde la gestión de inventarios hasta la facturación electrónica, pasando por contabilidad, recursos humanos y mucho más.

### 1.2 Características Principales

- **Multi-compañía:** Gestione múltiples empresas desde una sola plataforma
- **Multi-usuario:** Control de acceso basado en roles y permisos
- **Modular:** Active solo los módulos que necesita
- **Seguro:** Autenticación con Azure AD y sistema de permisos granular
- **Responsive:** Acceda desde cualquier dispositivo
- **Integrado:** Todos los módulos trabajan de forma coordinada

### 1.3 Beneficios

| Beneficio | Descripción |
|-----------|-------------|
| Centralización | Toda la información de su negocio en un solo lugar |
| Eficiencia | Automatización de procesos repetitivos |
| Control | Visibilidad completa de las operaciones |
| Seguridad | Protección de datos con estándares empresariales |
| Escalabilidad | Crece con su negocio |

---

## 2. Arquitectura del Sistema

### 2.1 Componentes Principales

```
┌─────────────────────────────────────────────────────────┐
│                    CMS Platform                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │   CMS.UI    │  │   CMS.API   │  │  PostgreSQL │      │
│  │  (Frontend) │◄─►│  (Backend)  │◄─►│  (Database) │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Estructura Multi-Tenant

El sistema utiliza una arquitectura multi-tenant donde:

- **Base de Datos Central (cms):** Almacena usuarios, roles, permisos, menús y configuración del sistema
- **Bases de Datos Operacionales:** Cada compañía tiene su propia base de datos para datos operacionales

### 2.3 Tecnologías Utilizadas

| Componente | Tecnología |
|------------|------------|
| Frontend | ASP.NET Core MVC / Razor Pages |
| Backend | .NET 9.0 Web API |
| Base de Datos | PostgreSQL 15 |
| Autenticación | Azure AD / JWT |
| Hosting | Kubernetes (k3s) |

---

## 3. Requisitos del Sistema

### 3.1 Navegadores Compatibles

| Navegador | Versión Mínima |
|-----------|----------------|
| Google Chrome | 90+ |
| Mozilla Firefox | 88+ |
| Microsoft Edge | 90+ |
| Safari | 14+ |

### 3.2 Requisitos de Red

- Conexión a Internet estable
- Puerto HTTPS (443) habilitado
- JavaScript habilitado en el navegador

### 3.3 Resolución de Pantalla

- **Mínima:** 1024 x 768 píxeles
- **Recomendada:** 1920 x 1080 píxeles

---

## 4. Acceso al Sistema

### 4.1 URL de Acceso

- **Producción:** https://cms.biti-solutions.com
- **Desarrollo:** https://localhost:5001

### 4.2 Proceso de Login

1. **Seleccionar Compañía**
   - Ingrese el código de su compañía (ej: "sinai", "eamr")
   - El sistema validará que la compañía existe y está activa

2. **Autenticación**
   - **Con Azure AD:** Se redirige a Microsoft para autenticación
   - **Login Local:** Ingrese email y contraseña

3. **Dashboard**
   - Una vez autenticado, accederá al Dashboard principal

### 4.3 Recuperación de Contraseña

Si olvidó su contraseña:
1. En la pantalla de login, haga clic en "¿Olvidó su contraseña?"
2. Ingrese su email registrado
3. Recibirá un correo con instrucciones para restablecer su contraseña
4. El enlace de recuperación es válido por 30 minutos

---

## 5. Navegación Principal

### 5.1 Estructura del Menú

El menú principal se encuentra en el lado izquierdo de la pantalla y está organizado por módulos:

```
📊 Dashboard
👥 CRM
🛒 Sales (Ventas)
📄 Billing (Facturación)
🏪 POS (Punto de Venta)
📦 Purchasing (Compras)
📋 Inventory (Inventario)
🚚 Warehouse (Almacén)
⚙️ Manufacturing (Manufactura)
🔧 Maintenance (Mantenimiento)
🧮 Accounting (Contabilidad)
🏦 Finance (Finanzas)
👔 Human Resources (RRHH)
👤 Employee Portal
📁 Projects (Proyectos)
📈 Reports & BI (Reportes)
📜 E-Invoicing (Factura Electrónica)
⚙️ Settings (Configuración)
🔐 Administration (Administración)
```

### 5.2 Barra Superior

- **Logo:** Clic para ir al Dashboard
- **Nombre de Usuario:** Muestra el usuario actual
- **Compañía Activa:** Muestra la compañía seleccionada
- **Cerrar Sesión:** Finaliza la sesión actual

### 5.3 Accesos Rápidos

En el Dashboard encontrará tarjetas de acceso rápido a:
- Gestionar Usuarios
- Roles y Permisos
- Menú del Sistema
- Compañías

---

## 6. Módulos del Sistema

### 6.1 Dashboard
Vista general con estadísticas del sistema, actividad reciente y estado de los servicios.

### 6.2 Inventory (Inventario)
- Gestión de artículos/productos
- Control de stock
- Categorías y clasificaciones
- Impresión de etiquetas

### 6.3 Reports & BI
- Reportes predefinidos
- Reportes personalizables
- Exportación a Excel, PDF, CSV
- Filtros avanzados

### 6.4 Administration
- Gestión de usuarios
- Roles y permisos
- Configuración del sistema
- Auditoría

---

## 7. Gestión Multi-Compañía

### 7.1 Concepto

El sistema permite gestionar múltiples compañías desde una sola instalación. Cada compañía tiene:
- Su propia base de datos operacional
- Usuarios y permisos específicos
- Configuraciones independientes

### 7.2 Cambio de Compañía

Para cambiar de compañía:
1. Cierre sesión
2. En la pantalla de selección de compañía, ingrese el código de la nueva compañía
3. Autentíquese nuevamente

### 7.3 Permisos por Compañía

Los permisos son específicos por compañía. Un usuario puede tener:
- Rol de Administrador en Compañía A
- Rol de Usuario en Compañía B
- Sin acceso a Compañía C

---

## 8. Soporte Técnico

### 8.1 Contacto

| Canal | Información |
|-------|-------------|
| Email | soporte@biti-solutions.com |
| Teléfono | +506 XXXX-XXXX |
| Horario | Lunes a Viernes, 8:00 AM - 5:00 PM (CST) |

### 8.2 Antes de Contactar Soporte

Tenga lista la siguiente información:
- Nombre de usuario y compañía
- Descripción detallada del problema
- Capturas de pantalla si es posible
- Pasos para reproducir el problema

### 8.3 Actualizaciones

El sistema se actualiza periódicamente con:
- Nuevas funcionalidades
- Correcciones de errores
- Mejoras de seguridad

Las actualizaciones se aplican automáticamente y se notifican a los usuarios.

---

**© 2026 BITI Solutions S.A. - Todos los derechos reservados**
