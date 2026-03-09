# Política de Seguridad y Permisos
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026  
**Clasificación:** Interno

---

## 1. Introducción

Este documento describe las políticas de seguridad, el sistema de roles y permisos, y las mejores prácticas de seguridad para el uso del Sistema CMS.

---

## 2. Sistema de Autenticación

### 2.1 Métodos de Autenticación

El sistema soporta dos métodos de autenticación:

| Método | Descripción | Uso Recomendado |
|--------|-------------|-----------------|
| Azure AD | Autenticación corporativa Microsoft | Empresas con Microsoft 365 |
| Login Local | Email y contraseña | Usuarios sin cuenta Microsoft |

### 2.2 Requisitos de Contraseña

Para cuentas locales, las contraseñas deben cumplir:

- ✅ Mínimo 8 caracteres
- ✅ Al menos una letra mayúscula
- ✅ Al menos una letra minúscula
- ✅ Al menos un número
- ✅ Al menos un carácter especial (!@#$%^&*)

### 2.3 Política de Bloqueo

| Parámetro | Valor |
|-----------|-------|
| Intentos fallidos antes de bloqueo | 5 |
| Duración del bloqueo | 30 minutos |
| Expiración de sesión inactiva | 8 horas |

### 2.4 Verificación de Email

- Los nuevos usuarios reciben un email de verificación
- El enlace de verificación expira en 30 minutos
- El usuario NO puede iniciar sesión hasta verificar su email

---

## 3. Sistema de Roles y Permisos

### 3.1 Arquitectura de Permisos

```
┌─────────────────────────────────────────────────────────┐
│                    ARQUITECTURA                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Usuario ────► Compañía ────► Permisos                  │
│                    │                                     │
│                    └────► Rol (informativo)              │
│                                                          │
│  Los permisos son POR COMPAÑÍA, no globales             │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Usuario** | Persona que accede al sistema |
| **Compañía** | Entidad empresarial en el sistema |
| **Rol** | Agrupación descriptiva de permisos (Administrador, Usuario, etc.) |
| **Permiso** | Acción específica que se puede realizar |

### 3.3 Roles Predefinidos

| Rol | Descripción | Permisos Típicos |
|-----|-------------|------------------|
| **Super Admin** | Acceso total al sistema | Todos |
| **Administrador** | Gestión de usuarios y configuración | Admin.*, Users.* |
| **Usuario** | Acceso operacional básico | Menu.View, Items.View |
| **Auditor** | Solo lectura para auditoría | *.View |

### 3.4 Estructura de Permisos

Los permisos siguen el formato: `Módulo.Acción`

**Ejemplos:**
- `Admin.Users.View` - Ver usuarios
- `Admin.Users.Create` - Crear usuarios
- `Admin.Users.Edit` - Editar usuarios
- `Admin.Users.Delete` - Eliminar usuarios
- `Inventory.Items.View` - Ver artículos
- `Reports.Execute` - Ejecutar reportes

### 3.5 Permisos Especiales

| Permiso | Efecto |
|---------|--------|
| `System.ViewAllCompanies` | Ver todas las compañías del sistema |
| `Admin.Roles.View` | Gestionar roles |
| `Admin.Permissions.View` | Gestionar permisos |

---

## 4. Permisos por Compañía

### 4.1 Regla Fundamental

> **IMPORTANTE:** Los permisos de un usuario son ESPECÍFICOS por compañía.

Un usuario puede tener:
- Rol de **Administrador** en Compañía A
- Rol de **Usuario** en Compañía B
- **Sin acceso** a Compañía C

### 4.2 Asignación de Permisos

```
Proceso de Asignación:

1. Usuario se asocia a una Compañía
2. Se asigna un Rol al usuario en esa Compañía
3. Los permisos del Rol se COPIAN al usuario
4. Se pueden modificar permisos individuales
5. Las DENEGACIONES siempre ganan
```

### 4.3 Evaluación de Permisos

```
Flujo de Evaluación:

┌─────────────────────────────────────┐
│ ¿Usuario tiene permiso X en Comp Y?│
├─────────────────────────────────────┤
│                                     │
│ 1. Buscar en user_company_permission│
│    WHERE user_id = X                │
│    AND company_id = Y               │
│                                     │
│ 2. Si is_allowed = FALSE → DENEGADO │
│                                     │
│ 3. Si is_allowed = TRUE → PERMITIDO │
│                                     │
│ 4. Si no existe → DENEGADO          │
│                                     │
└─────────────────────────────────────┘
```

---

## 5. Gestión de Usuarios

### 5.1 Creación de Usuarios

1. Solo usuarios con permiso `Admin.Users.Create` pueden crear usuarios
2. Se genera una contraseña temporal automáticamente
3. Se envía email de verificación
4. El usuario debe verificar email y cambiar contraseña

### 5.2 Desactivación de Usuarios

- Los usuarios se **desactivan**, no se eliminan
- Un usuario desactivado no puede iniciar sesión
- Se conserva el historial y auditoría

### 5.3 Auditoría de Usuarios

Todas las acciones se registran:
- Logins exitosos y fallidos
- Cambios de contraseña
- Modificaciones de permisos
- Creación/edición de registros

---

## 6. Mejores Prácticas de Seguridad

### 6.1 Para Usuarios

| ✅ Hacer | ❌ No Hacer |
|---------|------------|
| Usar contraseñas únicas | Compartir contraseñas |
| Cerrar sesión al terminar | Dejar sesiones abiertas |
| Reportar actividad sospechosa | Ignorar alertas de seguridad |
| Verificar URL antes de ingresar | Acceder desde enlaces no verificados |

### 6.2 Para Administradores

| ✅ Hacer | ❌ No Hacer |
|---------|------------|
| Principio de mínimo privilegio | Dar permisos excesivos |
| Revisar permisos periódicamente | Olvidar usuarios inactivos |
| Documentar cambios de permisos | Hacer cambios sin registro |
| Usar Azure AD cuando sea posible | Depender solo de login local |

### 6.3 Principio de Mínimo Privilegio

> Cada usuario debe tener SOLO los permisos necesarios para realizar su trabajo.

**Ejemplo:**
- Un vendedor NO necesita `Admin.Users.Edit`
- Un contador NO necesita `Inventory.Items.Delete`

---

## 7. Tokens y Sesiones

### 7.1 JWT (JSON Web Tokens)

El sistema utiliza JWT para autenticación:

| Parámetro | Valor |
|-----------|-------|
| Algoritmo | HS256 |
| Expiración | 8 horas |
| Renovación | Automática |

### 7.2 Contenido del Token

El JWT incluye:
- ID de usuario
- ID de compañía activa
- Lista de permisos
- Roles del usuario
- Tiempo de expiración

### 7.3 Seguridad de Tokens

- Los tokens se transmiten solo por HTTPS
- Se almacenan de forma segura en el servidor
- No se exponen en URLs o logs

---

## 8. Auditoría y Logs

### 8.1 Eventos Registrados

| Evento | Información Registrada |
|--------|------------------------|
| Login | Usuario, IP, fecha, resultado |
| Logout | Usuario, fecha |
| Cambios de datos | Usuario, tabla, antes/después |
| Errores | Stack trace, contexto |

### 8.2 Retención de Logs

| Tipo de Log | Retención |
|-------------|-----------|
| Logs de acceso | 90 días |
| Logs de cambios | 1 año |
| Logs de errores | 30 días |

### 8.3 Acceso a Logs

Solo usuarios con permiso `Admin.Logs.View` pueden acceder a los logs del sistema.

---

## 9. Contacto de Seguridad

Para reportar incidentes de seguridad:

- **Email:** seguridad@biti-solutions.com
- **Urgente:** +506 XXXX-XXXX

### Qué Reportar

- Accesos no autorizados
- Comportamiento sospechoso del sistema
- Posibles vulnerabilidades
- Pérdida o robo de credenciales

---

**© 2026 BITI Solutions S.A. - Documento Confidencial**
