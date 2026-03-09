# Módulo de Administración
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## 1. Introducción

El módulo de Administración permite gestionar usuarios, roles, permisos, menús y la configuración general del sistema CMS.

---

## 2. Acceso al Módulo

**Ruta:** Menú → Administration

**Permisos necesarios:**
- `Admin.Dashboard.View` - Ver dashboard administrativo
- `Admin.Users.View` - Gestionar usuarios
- `Admin.Roles.View` - Gestionar roles
- `Admin.Permissions.View` - Gestionar permisos
- `Admin.Menus.View` - Gestionar menús
- `Admin.Companies.View` - Gestionar compañías

---

## 3. Dashboard de Administración

### 3.1 Acceso
**Ruta:** Administration → Dashboard

### 3.2 Contenido

El dashboard muestra estadísticas del sistema:

| Estadística | Descripción |
|-------------|-------------|
| Usuarios Activos | Total de usuarios con acceso |
| Roles Configurados | Número de roles en el sistema |
| Permisos | Total de permisos disponibles |
| Módulos Activos | Menús principales habilitados |

---

## 4. Gestión de Usuarios

### 4.1 Lista de Usuarios

**Ruta:** Administration → Users

Información mostrada:
- Nombre completo
- Email
- Roles asignados
- Estado (Activo/Inactivo)
- Último login

### 4.2 Crear Usuario

1. Clic en **"➕ Nuevo Usuario"**
2. Complete los datos obligatorios:
   - Nombre y apellido
   - Email (único en el sistema)
   - Nombre de usuario
3. El sistema genera una contraseña temporal
4. Se envía email de verificación
5. Clic en **"Guardar"**

### 4.3 Campos del Usuario

| Campo | Requerido | Descripción |
|-------|-----------|-------------|
| Email | ✅ | Correo único para login |
| Nombre | ✅ | Primer nombre |
| Apellido | ✅ | Apellido |
| Usuario | ✅ | Nombre para mostrar |
| Teléfono | ❌ | Número de contacto |
| Zona Horaria | ❌ | Zona horaria del usuario |
| Activo | ✅ | Si puede acceder al sistema |

### 4.4 Asignar Usuario a Compañía

1. Abra el usuario a editar
2. Sección "Compañías Asignadas"
3. Clic en **"Agregar Compañía"**
4. Seleccione la compañía
5. Asigne un rol
6. Guarde los cambios

### 4.5 Gestionar Permisos por Compañía

1. En la lista de usuarios, clic en el usuario
2. Clic en **"Permisos por Compañía"**
3. Seleccione la compañía
4. Marque/desmarque permisos individuales
5. Guarde los cambios

```
┌─────────────────────────────────────────────────────────┐
│ Permisos de: Juan Pérez en Compañía: SINAI             │
├─────────────────────────────────────────────────────────┤
│ Rol asignado: Administrador                             │
├─────────────────────────────────────────────────────────┤
│ Permisos:                                               │
│ ☑ Admin.Users.View                                      │
│ ☑ Admin.Users.Create                                    │
│ ☑ Admin.Users.Edit                                      │
│ ☐ Admin.Users.Delete    ← Denegado específicamente     │
│ ☑ Inventory.Items.View                                  │
│ ...                                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 5. Gestión de Roles

### 5.1 Lista de Roles

**Ruta:** Administration → Roles & Permissions

Roles predefinidos:
- Super Admin
- Administrador
- Usuario
- Auditor

### 5.2 Crear Rol

1. Clic en **"➕ Nuevo Rol"**
2. Ingrese nombre del rol
3. Agregue descripción
4. Seleccione permisos base
5. Guarde

### 5.3 Asignar Permisos a Rol

1. Seleccione el rol
2. En la sección "Permisos"
3. Marque los permisos deseados
4. Guarde

> **Nota:** Los permisos del rol sirven como plantilla. Al asignar un rol a un usuario, estos permisos se copian y pueden modificarse individualmente.

---

## 6. Gestión de Permisos

### 6.1 Lista de Permisos

**Ruta:** Administration → Permisos

### 6.2 Estructura de Permisos

```
Formato: [Módulo].[Acción]

Ejemplos:
- Admin.Users.View
- Admin.Users.Create
- Admin.Users.Edit
- Admin.Users.Delete
- Inventory.Items.View
- Reports.Execute
```

### 6.3 Crear Permiso

1. Clic en **"➕ Nuevo Permiso"**
2. Ingrese:
   - Clave (ej: `Module.Action`)
   - Nombre descriptivo
   - Descripción
3. Guarde

---

## 7. Gestión de Menús

### 7.1 Lista de Menús

**Ruta:** Administration → Menús

### 7.2 Estructura del Menú

```
Menú Principal (id_parent = 0)
├── Dashboard
├── Inventory
│   ├── Items
│   └── Label Items
├── Reports & BI
│   └── General
└── Administration
    ├── Users
    ├── Roles
    └── ...
```

### 7.3 Crear Menú

1. Clic en **"➕ Nuevo Menú"**
2. Complete:
   - Nombre
   - URL (ruta)
   - Ícono (clase Bootstrap Icons)
   - Orden
   - Menú padre (para submenús)
   - Permiso requerido
3. Guarde

### 7.4 Campos del Menú

| Campo | Descripción |
|-------|-------------|
| Nombre | Texto que se muestra |
| URL | Ruta de navegación |
| Ícono | Clase de ícono (bi-*) |
| Orden | Posición en el menú |
| Parent | Menú padre (0 = principal) |
| Permiso | Permiso para ver el menú |
| Activo | Si está visible |

---

## 8. Gestión de Compañías

### 8.1 Lista de Compañías

**Ruta:** Administration → Company Management

### 8.2 Crear Compañía

1. Clic en **"➕ Nueva Compañía"**
2. Complete datos básicos:
   - Nombre
   - Código (schema)
   - Cédula jurídica
   - Dirección
3. Configure conexión a BD
4. Guarde

### 8.3 Configuración de Compañía

| Sección | Campos |
|---------|--------|
| **General** | Nombre, código, descripción |
| **Contacto** | Email, teléfono, dirección |
| **Base de Datos** | Connection strings (dev/prod) |
| **Azure AD** | Configuración de autenticación |
| **Factura Electrónica** | Credenciales de Hacienda |

---

## 9. Auditoría

### 9.1 Audit Trail

**Ruta:** Administration → Audit Trail

Registra:
- Quién hizo el cambio
- Cuándo se hizo
- Qué se cambió
- Valores antes/después

### 9.2 System Logs

**Ruta:** Administration → System Logs

Muestra logs técnicos del sistema para diagnóstico.

---

## 10. Configuración del Sistema

### 10.1 System Config

**Ruta:** Administration → System Config

Configuraciones disponibles:
- SMTP (correo)
- Seguridad
- Notificaciones
- Parámetros generales

---

## 11. Buenas Prácticas

### 11.1 Para Usuarios

| ✅ Hacer | ❌ Evitar |
|---------|----------|
| Usar roles estándar | Crear usuarios sin rol |
| Documentar cambios | Modificar sin registro |
| Revisar permisos periódicamente | Dar acceso excesivo |

### 11.2 Para Roles

- Crear roles por función laboral
- No duplicar roles con mínimas diferencias
- Usar nombres descriptivos

### 11.3 Para Permisos

- Aplicar principio de mínimo privilegio
- Usar denegaciones solo cuando necesario
- Documentar permisos especiales

---

## 12. Preguntas Frecuentes

### ¿Cómo desbloqueo un usuario?
1. Administration → Users
2. Busque el usuario
3. Edite y desmarque el bloqueo
4. Guarde

### ¿Por qué un usuario no ve un menú?
- Verifique que tiene el permiso requerido
- Confirme que está asignado a la compañía
- Revise que el menú está activo

### ¿Cómo reseteo la contraseña de un usuario?
1. Edite el usuario
2. Clic en "Resetear Contraseña"
3. Se enviará email con nueva contraseña temporal

---

## 13. Soporte

Para asistencia administrativa:
- **Email:** soporte@biti-solutions.com
- **Urgencias:** +506 XXXX-XXXX

---

**© 2026 BITI Solutions S.A.**
