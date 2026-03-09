# Tutorial: Cómo Crear un Nuevo Usuario
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## Objetivo

Aprender a crear y configurar un nuevo usuario en el sistema CMS paso a paso.

---

## Requisitos Previos

✅ Tener acceso al módulo de Administración  
✅ Permiso `Admin.Users.Create`  
✅ Conocer la compañía a la que se asignará el usuario

---

## Paso 1: Acceder a Gestión de Usuarios

1. Inicie sesión en el sistema
2. En el menú lateral, navegue a:
   - **Administration** → **Users**

```
📂 Administration
   └── 👥 Users  ← Clic aquí
```

3. Verá la lista de usuarios existentes

---

## Paso 2: Iniciar Creación

1. En la esquina superior derecha, clic en **"➕ Nuevo Usuario"**

```
┌─────────────────────────────────────────────────────┐
│ 👥 Gestión de Usuarios           [➕ Nuevo Usuario] │
├─────────────────────────────────────────────────────┤
│ Usuario │ Email │ Rol │ Estado │ Acciones          │
│ ...     │ ...   │ ... │ ...    │ ...               │
└─────────────────────────────────────────────────────┘
```

2. Se abrirá el formulario de nuevo usuario

---

## Paso 3: Completar Información Básica

### Campos Obligatorios

| Campo | Ejemplo | Notas |
|-------|---------|-------|
| **Email** | juan.perez@empresa.com | Debe ser único |
| **Nombre** | Juan | Primer nombre |
| **Apellido** | Pérez | Apellido |
| **Nombre de Usuario** | juan.perez | Para mostrar en el sistema |

### Campos Opcionales

| Campo | Ejemplo | Notas |
|-------|---------|-------|
| Teléfono | +506 8888-8888 | Contacto |
| Zona Horaria | America/Costa_Rica | Para fechas |

### Ejemplo Visual

```
┌─────────────────────────────────────────────────────┐
│ Nuevo Usuario                                        │
├─────────────────────────────────────────────────────┤
│                                                      │
│ Email *           [juan.perez@empresa.com    ]      │
│                                                      │
│ Nombre *          [Juan                      ]      │
│                                                      │
│ Apellido *        [Pérez                     ]      │
│                                                      │
│ Nombre Usuario *  [juan.perez                ]      │
│                                                      │
│ Teléfono          [+506 8888-8888           ]      │
│                                                      │
│ Estado            [✅ Activo]                        │
│                                                      │
│             [Cancelar]  [Guardar]                   │
└─────────────────────────────────────────────────────┘
```

---

## Paso 4: Guardar Usuario

1. Verifique que los datos son correctos
2. Clic en **"Guardar"**
3. El sistema:
   - ✅ Crea el usuario
   - ✅ Genera contraseña temporal (válida 30 minutos)
   - ✅ Envía email de verificación

### Mensaje de Confirmación

```
✅ Usuario creado exitosamente

Se ha enviado un email de verificación a:
juan.perez@empresa.com

El usuario debe:
1. Verificar su email
2. Usar la contraseña temporal para primer acceso
3. Cambiar la contraseña
```

---

## Paso 5: Asignar a Compañía

Después de crear el usuario, debe asignarlo a al menos una compañía:

1. En la lista de usuarios, busque el nuevo usuario
2. Clic en **"Editar"** o en el nombre del usuario
3. Vaya a la sección **"Compañías"**
4. Clic en **"➕ Agregar Compañía"**

```
┌─────────────────────────────────────────────────────┐
│ Compañías Asignadas                                 │
├─────────────────────────────────────────────────────┤
│                                                      │
│ Compañía         │ Rol              │ Acciones      │
│ ────────────────────────────────────────────────────│
│ (Ninguna asignada)                                  │
│                                                      │
│            [➕ Agregar Compañía]                     │
└─────────────────────────────────────────────────────┘
```

5. Seleccione la compañía
6. Seleccione el rol (ej: Usuario, Administrador)
7. Clic en **"Agregar"**

---

## Paso 6: Ajustar Permisos (Opcional)

Si necesita ajustar permisos específicos:

1. En el usuario, clic en **"Permisos por Compañía"**
2. Seleccione la compañía
3. Verá los permisos heredados del rol
4. Puede:
   - ✅ Agregar permisos adicionales
   - ❌ Denegar permisos específicos

```
┌─────────────────────────────────────────────────────┐
│ Permisos: Juan Pérez en SINAI                       │
├─────────────────────────────────────────────────────┤
│ Rol: Usuario                                         │
│                                                      │
│ ☑ Menu.View                                         │
│ ☑ Inventory.Items.View                              │
│ ☐ Inventory.Items.Create    (heredado, desmarcado) │
│ ☑ Reports.View                                      │
│ ☑ Reports.Execute                                   │
│                                                      │
│                    [Guardar]                        │
└─────────────────────────────────────────────────────┘
```

---

## Paso 7: Verificación del Usuario

El nuevo usuario debe:

### 7.1 Recibir Email

```
┌─────────────────────────────────────────────────────┐
│ 📧 Email de Verificación                            │
├─────────────────────────────────────────────────────┤
│                                                      │
│ Bienvenido al Sistema CMS de BITI Solutions         │
│                                                      │
│ Se ha creado una cuenta para usted.                 │
│                                                      │
│ Contraseña temporal: Abc123!@#                      │
│ (Válida por 30 minutos)                             │
│                                                      │
│ [Verificar mi cuenta]                               │
│                                                      │
│ Si no solicitó esta cuenta, ignore este email.     │
└─────────────────────────────────────────────────────┘
```

### 7.2 Primer Acceso

1. El usuario hace clic en "Verificar mi cuenta"
2. Ingresa con la contraseña temporal
3. El sistema solicita nueva contraseña
4. ¡Listo! El usuario puede usar el sistema

---

## Resumen del Proceso

```
┌─────────────────────────────────────────────────────┐
│                FLUJO DE CREACIÓN                     │
├─────────────────────────────────────────────────────┤
│                                                      │
│  1. Crear Usuario                                   │
│         │                                           │
│         ▼                                           │
│  2. Sistema envía email de verificación             │
│         │                                           │
│         ▼                                           │
│  3. Asignar a Compañía + Rol                       │
│         │                                           │
│         ▼                                           │
│  4. Ajustar permisos (opcional)                    │
│         │                                           │
│         ▼                                           │
│  5. Usuario verifica email                          │
│         │                                           │
│         ▼                                           │
│  6. Usuario cambia contraseña                       │
│         │                                           │
│         ▼                                           │
│  ✅ Usuario activo en el sistema                    │
│                                                      │
└─────────────────────────────────────────────────────┘
```

---

## Solución de Problemas

### El usuario no recibe el email
- Verifique que el email es correcto
- Pida que revise carpeta de spam
- Contacte soporte si persiste

### El enlace de verificación expiró
- El administrador puede reenviar el email
- O resetear la contraseña desde el panel

### El usuario no puede acceder a un módulo
- Verifique que tiene permisos en esa compañía
- Confirme que el menú está activo

---

## Siguiente Tutorial

➡️ **Tutorial: Configurar Roles y Permisos**

---

**© 2026 BITI Solutions S.A.**
