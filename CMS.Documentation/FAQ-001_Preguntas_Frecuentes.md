# Preguntas Frecuentes (FAQ)
## Sistema CMS - BITI Solutions S.A.

**Versión:** 1.0  
**Fecha:** Marzo 2026

---

## Índice

1. [Acceso y Login](#1-acceso-y-login)
2. [Navegación](#2-navegación)
3. [Usuarios y Permisos](#3-usuarios-y-permisos)
4. [Inventario](#4-inventario)
5. [Reportes](#5-reportes)
6. [Problemas Técnicos](#6-problemas-técnicos)
7. [Soporte](#7-soporte)

---

## 1. Acceso y Login

### ❓ ¿Cuál es la URL para acceder al sistema?
**Producción:** https://cms.biti-solutions.com  
**Desarrollo:** https://localhost:5001

---

### ❓ ¿Olvidé mi contraseña, qué hago?
1. En la pantalla de login, clic en **"¿Olvidó su contraseña?"**
2. Ingrese su email registrado
3. Recibirá un correo con instrucciones
4. El enlace es válido por 30 minutos

---

### ❓ ¿Por qué no puedo iniciar sesión?
Posibles causas:
- ❌ Email no verificado (revise su correo)
- ❌ Contraseña incorrecta
- ❌ Usuario desactivado
- ❌ No está asignado a la compañía seleccionada
- ❌ Cuenta bloqueada por intentos fallidos

**Solución:** Contacte a su administrador.

---

### ❓ ¿Puedo usar mi cuenta de Microsoft?
Sí, si su compañía tiene configurado Azure AD:
1. En la pantalla de login, clic en **"Iniciar con Microsoft"**
2. Use sus credenciales corporativas de Microsoft 365

---

### ❓ ¿Cómo cambio mi contraseña?
1. Inicie sesión
2. Clic en su nombre de usuario (esquina superior derecha)
3. Seleccione **"Cambiar Contraseña"**
4. Ingrese la contraseña actual y la nueva

---

### ❓ ¿Por qué mi sesión se cierra automáticamente?
Por seguridad, las sesiones expiran después de 8 horas de inactividad.

---

## 2. Navegación

### ❓ ¿Dónde está el menú?
El menú está en el lado izquierdo de la pantalla. Si no lo ve, puede estar colapsado. Busque el ícono **☰** para expandirlo.

---

### ❓ ¿Por qué no veo algunos módulos en el menú?
Los módulos visibles dependen de:
- Sus permisos asignados
- Los módulos activos para su compañía
- Si el menú está habilitado

Contacte a su administrador si necesita acceso.

---

### ❓ ¿Cómo vuelvo al inicio?
- Clic en el **logo de CMS** en la esquina superior izquierda
- O clic en **Dashboard** en el menú

---

### ❓ ¿Cómo cambio de compañía?
1. Cierre sesión
2. En la pantalla inicial, ingrese el código de la otra compañía
3. Inicie sesión nuevamente

---

## 3. Usuarios y Permisos

### ❓ ¿Qué es un rol?
Un **rol** es una agrupación de permisos que define qué puede hacer un usuario (ej: Administrador, Usuario, Auditor).

---

### ❓ ¿Qué es un permiso?
Un **permiso** es una acción específica que se puede realizar (ej: `Inventory.Items.Create` = crear artículos).

---

### ❓ ¿Por qué tengo diferentes accesos en diferentes compañías?
Los permisos son **por compañía**. Puede tener:
- Rol de Administrador en Compañía A
- Rol de Usuario en Compañía B
- Sin acceso a Compañía C

---

### ❓ ¿Cómo sé qué permisos tengo?
Contacte a su administrador o revise en:
**Administration → Users → [Su usuario] → Permisos**

---

### ❓ ¿Puedo tener acceso a múltiples compañías?
Sí, un usuario puede estar asignado a varias compañías con diferentes roles y permisos en cada una.

---

## 4. Inventario

### ❓ ¿Cómo creo un nuevo artículo?
1. Menú → **Inventory** → **Items**
2. Clic en **"➕ Nuevo Artículo"**
3. Complete los datos obligatorios
4. Clic en **"Guardar"**

---

### ❓ ¿Puedo cambiar el código de un artículo?
No, el código es único e inmutable. Para cambiar un código:
1. Cree un nuevo artículo con el código correcto
2. Desactive el artículo anterior

---

### ❓ ¿Cómo elimino un artículo?
Los artículos **no se eliminan**, se desactivan:
1. Busque el artículo
2. Clic en **"Eliminar"**
3. El artículo se desactiva pero conserva historial

---

### ❓ ¿Cómo imprimo etiquetas?
1. Menú → **Inventory** → **Label Items**
2. Busque los artículos
3. Seleccione cantidad de etiquetas
4. Configure opciones (nombre, precio, código de barras)
5. Clic en **"Imprimir"**

---

### ❓ ¿Qué significan las clasificaciones?
Las clasificaciones organizan artículos en categorías jerárquicas:
- **Clasificación 1:** Tipo (ej: Electrónica)
- **Clasificación 2:** Subtipo (ej: Smartphones)
- **Clasificación 3:** Marca (ej: Samsung)

---

## 5. Reportes

### ❓ ¿Cómo ejecuto un reporte?
1. Menú → **Reports & BI** → **General**
2. Seleccione el reporte
3. Configure filtros (fechas, búsqueda, etc.)
4. Clic en **"▶ Ejecutar Reporte"**

---

### ❓ ¿Cómo exporto un reporte?
Después de ejecutar el reporte, clic en:
- **[Excel]** para archivo .xlsx
- **[PDF]** para documento PDF
- **[CSV]** para datos en texto

---

### ❓ ¿El reporte exporta todos los datos o solo la página actual?
Exporta **todos los datos** que cumplen los filtros, no solo la página visible.

---

### ❓ ¿Por qué mi reporte no muestra datos?
- Verifique los filtros aplicados
- Confirme el rango de fechas
- Asegúrese de tener permisos
- Verifique que hay datos en el sistema

---

### ❓ ¿Puedo crear mis propios reportes?
Los reportes personalizados requieren configuración por un administrador. Contacte a soporte para solicitarlos.

---

## 6. Problemas Técnicos

### ❓ La página no carga / está muy lenta
1. Recargue la página (F5)
2. Borre caché del navegador
3. Verifique su conexión a internet
4. Pruebe con otro navegador

---

### ❓ Veo un error "500" o "Error del servidor"
1. Espere unos segundos y reintente
2. Si persiste, tome captura de pantalla
3. Contacte a soporte con el detalle

---

### ❓ No puedo guardar un registro
Posibles causas:
- Campos obligatorios vacíos
- Datos duplicados (ej: código ya existe)
- Sin permisos para la acción
- Error de conexión

---

### ❓ ¿Qué navegadores son compatibles?
| Navegador | Versión Mínima |
|-----------|----------------|
| Chrome | 90+ |
| Firefox | 88+ |
| Edge | 90+ |
| Safari | 14+ |

---

### ❓ ¿Funciona en dispositivos móviles?
El sistema es responsive y funciona en tablets y smartphones, aunque la experiencia óptima es en computadora de escritorio.

---

## 7. Soporte

### ❓ ¿Cómo contacto a soporte?
- **Email:** soporte@biti-solutions.com
- **Teléfono:** +506 XXXX-XXXX
- **Horario:** Lunes a Viernes, 8:00 AM - 5:00 PM (CST)

---

### ❓ ¿Qué información debo proporcionar al soporte?
- Su nombre de usuario
- Compañía
- Descripción del problema
- Pasos para reproducirlo
- Capturas de pantalla (si aplica)
- Mensaje de error exacto

---

### ❓ ¿Hay tiempo de respuesta garantizado?
| Prioridad | Tiempo de Respuesta |
|-----------|---------------------|
| Crítico (sistema caído) | 1 hora |
| Alto (funcionalidad bloqueada) | 4 horas |
| Medio (problema parcial) | 24 horas |
| Bajo (consulta/mejora) | 48 horas |

---

### ❓ ¿Dónde encuentro más documentación?
En el Dashboard, clic en **"documentación del sistema"** para acceder a manuales, tutoriales y guías.

---

### ❓ ¿Cómo solicito una nueva funcionalidad?
Envíe su solicitud a:
- **Email:** soporte@biti-solutions.com
- **Asunto:** [SOLICITUD] Descripción breve

Incluya:
- Descripción de la funcionalidad
- Beneficio esperado
- Casos de uso

---

**¿No encontró su pregunta?**

Contacte a soporte@biti-solutions.com

---

**© 2026 BITI Solutions S.A.**
