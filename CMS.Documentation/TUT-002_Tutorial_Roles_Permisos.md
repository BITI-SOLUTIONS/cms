# Tutorial: Configurar Roles y Permisos
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## Objetivo

Aprender a crear roles, asignar permisos y configurar la autorización de usuarios en el sistema CMS.

---

## Requisitos Previos

✅ Permiso `Admin.Roles.View` para gestionar roles  
✅ Permiso `Admin.Permissions.View` para gestionar permisos  
✅ Permiso `Admin.Users.Edit` para asignar roles a usuarios

---

## Conceptos Clave

### ¿Qué es un Rol?

Un **rol** es una agrupación de permisos que define un perfil de acceso. Ejemplos:
- Administrador
- Vendedor
- Contador
- Auditor

### ¿Qué es un Permiso?

Un **permiso** es una acción específica que se puede realizar. Formato: `Módulo.Acción`

Ejemplos:
- `Inventory.Items.View` - Ver artículos
- `Sales.Orders.Create` - Crear pedidos
- `Admin.Users.Delete` - Eliminar usuarios

### Regla Importante

> Los permisos son **POR COMPAÑÍA**. Un usuario puede tener diferentes roles y permisos en cada compañía a la que pertenece.

---

## Paso 1: Crear un Nuevo Rol

### 1.1 Acceder a Gestión de Roles

1. Menú → **Administration** → **Roles & Permissions**
2. Verá la lista de roles existentes

### 1.2 Crear el Rol

1. Clic en **"➕ Nuevo Rol"**
2. Complete los datos:

| Campo | Ejemplo | Descripción |
|-------|---------|-------------|
| Nombre | Vendedor | Nombre del rol |
| Descripción | Personal de ventas | Descripción clara |
| Activo | ✅ | Si está disponible |

3. Clic en **"Guardar"**

```
┌─────────────────────────────────────────────────────┐
│ Nuevo Rol                                           │
├─────────────────────────────────────────────────────┤
│                                                      │
│ Nombre *        [Vendedor                    ]      │
│                                                      │
│ Descripción     [Personal del área de ventas]      │
│                                                      │
│ Activo          [✅]                                 │
│                                                      │
│             [Cancelar]  [Guardar]                   │
└─────────────────────────────────────────────────────┘
```

---

## Paso 2: Asignar Permisos al Rol

### 2.1 Editar el Rol

1. En la lista de roles, clic en el rol creado
2. Vaya a la pestaña **"Permisos"**

### 2.2 Seleccionar Permisos

Los permisos están organizados por módulo. Para un rol de Vendedor, marque:

```
📦 Sales (Ventas)
  ☑ Sales.View
  ☑ Sales.Quotes.View
  ☑ Sales.Quotes.Create
  ☑ Sales.Orders.View
  ☑ Sales.Orders.Create
  ☐ Sales.Orders.Delete     ← No marcar

📋 Inventory (Inventario)
  ☑ Inventory.Items.View    ← Solo ver
  ☐ Inventory.Items.Create  ← No crear

📈 Reports (Reportes)
  ☑ Reports.View
  ☑ Reports.Execute
```

### 2.3 Guardar Permisos

1. Después de marcar los permisos necesarios
2. Clic en **"Guardar Permisos"**

---

## Paso 3: Asignar Rol a Usuario

### 3.1 Ir a Gestión de Usuarios

1. Menú → **Administration** → **Users**
2. Busque el usuario

### 3.2 Asignar a Compañía con Rol

1. Clic en el usuario para editarlo
2. Sección **"Compañías"**
3. Clic en **"➕ Agregar Compañía"**
4. Seleccione:
   - **Compañía:** SINAI
   - **Rol:** Vendedor
5. Clic en **"Agregar"**

```
┌─────────────────────────────────────────────────────┐
│ Asignar Compañía                                    │
├─────────────────────────────────────────────────────┤
│                                                      │
│ Compañía *      [SINAI                ▼]            │
│                                                      │
│ Rol *           [Vendedor             ▼]            │
│                                                      │
│             [Cancelar]  [Agregar]                   │
└─────────────────────────────────────────────────────┘
```

### 3.3 Qué Sucede Internamente

Cuando asigna un rol:
1. Se registra el rol del usuario en la compañía
2. Los permisos del rol se **COPIAN** al usuario
3. Los permisos copiados pueden modificarse individualmente

---

## Paso 4: Personalizar Permisos del Usuario

### 4.1 Acceder a Permisos por Compañía

1. En el usuario, clic en **"Permisos por Compañía"**
2. Seleccione la compañía

### 4.2 Modificar Permisos Individuales

Puede:
- ✅ **Agregar** permisos adicionales al usuario
- ❌ **Denegar** permisos específicos (sobrescriben el rol)

**Ejemplo:** El usuario tiene rol Vendedor pero necesita crear artículos:

```
Permisos de: Juan Pérez en SINAI
Rol: Vendedor

Permisos del Rol:                    Ajuste Individual:
☑ Sales.View                         (heredado)
☑ Sales.Orders.Create                (heredado)
☑ Inventory.Items.View               (heredado)
☑ Inventory.Items.Create             ← AGREGADO manualmente
```

### 4.3 Denegar un Permiso

Si necesita **quitar** un permiso específico del usuario (aunque el rol lo tenga):

```
☐ Sales.Orders.Delete    [Denegado]
```

> **Importante:** Las denegaciones SIEMPRE ganan sobre los permisos permitidos.

---

## Paso 5: Verificar Permisos Efectivos

### 5.1 Ver Permisos Finales

En la pantalla de permisos del usuario, verá:
- ✅ **Permitido** - Usuario puede realizar la acción
- ❌ **Denegado** - Usuario NO puede realizar la acción
- ○ **Sin asignar** - No tiene el permiso

### 5.2 Probar Acceso

1. El usuario cierra sesión
2. Inicia sesión nuevamente
3. Verifica que solo ve los menús y funciones permitidos

---

## Diagrama del Flujo de Permisos

```
┌─────────────────────────────────────────────────────────────┐
│                  FLUJO DE PERMISOS                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Crear Rol                                               │
│         │                                                   │
│         ▼                                                   │
│  2. Asignar Permisos al Rol (plantilla)                    │
│         │                                                   │
│         ▼                                                   │
│  3. Asignar Rol a Usuario en Compañía                      │
│         │                                                   │
│         ▼                                                   │
│  4. Permisos se COPIAN a user_company_permission           │
│         │                                                   │
│         ▼                                                   │
│  5. (Opcional) Ajustar permisos individuales               │
│         │                                                   │
│         ▼                                                   │
│  6. Usuario accede según permisos efectivos                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Mejores Prácticas

### ✅ Hacer

| Práctica | Razón |
|----------|-------|
| Crear roles por función laboral | Fácil gestión |
| Usar nombres descriptivos | Claridad |
| Principio de mínimo privilegio | Seguridad |
| Documentar roles especiales | Trazabilidad |

### ❌ Evitar

| Práctica | Razón |
|----------|-------|
| Dar todos los permisos | Riesgo de seguridad |
| Duplicar roles similares | Difícil mantenimiento |
| Asignar permisos sin rol | Inconsistencia |

---

## Roles Recomendados

### Para una empresa típica:

| Rol | Permisos Principales |
|-----|---------------------|
| **Super Admin** | Todos los permisos |
| **Administrador** | Admin.*, Users.*, Config.* |
| **Gerente de Ventas** | Sales.*, Reports.Sales.* |
| **Vendedor** | Sales.Orders.*, Inventory.Items.View |
| **Contador** | Accounting.*, Finance.*, Reports.* |
| **Bodeguero** | Inventory.*, Warehouse.* |
| **Auditor** | *.View (solo lectura) |

---

## Solución de Problemas

### El usuario no ve un menú esperado
1. Verifique el permiso requerido del menú
2. Confirme que el usuario tiene ese permiso en la compañía
3. Revise que no haya una denegación

### El usuario ve demasiados menús
1. Revise los permisos asignados
2. Verifique que el rol no tenga permisos excesivos
3. Agregue denegaciones si es necesario

### Los cambios de permisos no se reflejan
1. El usuario debe cerrar sesión
2. Volver a iniciar sesión
3. El JWT se regenera con los nuevos permisos

---

## Siguiente Tutorial

➡️ **Tutorial: Crear y Ejecutar Reportes**

---

**© 2026 BITI Solutions S.A.**
