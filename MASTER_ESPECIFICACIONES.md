# 📋 MASTER DE ESPECIFICACIONES - BITI SOLUTIONS CMS

> **Última actualización:** 2026-02-09 20:21:17  
> **Versión:** 1.0  
> **Proyecto:** CMS (Central Management System)  
> **Empresa:** BITI Solutions S.A

---

## 🎯 1. CONTEXTO GENERAL

### Información del Usuario/Organización
- **Usuario GitHub:** BITI-SOLUTIONS
- **Tipo de cuenta:** Usuario (Organización)
- **Nombre de la empresa:** BITI Solutions S.A
- **Owner:** @BITI-SOLUTIONS
- **Repositorios principales:** 
  - `BITI-SOLUTIONS/cms` (Central Management System)
  - URL: https://github.com/BITI-SOLUTIONS/cms.git

### Descripción del Proyecto
**BITI Solutions S.A** es una empresa dedicada al desarrollo de software, consultoría de Business Intelligence, reportería y soporte de servidores. 

**Servicios que ofrece:**
- Desarrollo de software (Web, Desktop)
- Consultoría de Business Intelligence
- Desarrollo de reportería
- Power BI
- Soporte de servidores
- Soluciones integrales para gestionar datos y procesos de clientes

**Proyecto CMS (Central Management System):**
Sistema de gestión centralizado desarrollado en C# con arquitectura en capas (Clean Architecture):
- `CMS.API` - Backend API REST
- `CMS.Application` - Lógica de aplicación (CQRS/MediatR)
- `CMS.Data` - Capa de acceso a datos (Repository/EF Core)
- `CMS.Entities` - Entidades de dominio
- `CMS.UI` - Frontend (Razor Pages/MVC)
- `CMS.Shared` - Infraestructura compartida

---

## 🏗️ 2. ARQUITECTURA Y TECNOLOGÍA

### Stack Tecnológico Completo

#### Frontend
- **Framework:** ASP.NET Core MVC 9.0
- **UI Engine:** Razor Pages
- **Target Framework:** net9.0
- **Puerto interno (container):** 8081
- **Puerto externo (desarrollo):** 3000

#### Backend
- **Framework:** ASP.NET Core Web API 9.0
- **Target Framework:** net9.0
- **Arquitectura:** Clean Architecture + CQRS (MediatR)
- **Puerto interno (container):** 8080
- **Puerto externo (desarrollo):** 5000
- **Swagger:** Habilitado en `/swagger/index.html`

#### Base de Datos
- **Motor:** PostgreSQL 15 (Alpine)
- **Puerto:** 5432 (interno), 30432 (NodePort externo)
- **ORM:** Entity Framework Core 9.0
- **Provider:** Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0
- **Esquema principal:** `admin` (Ej: `admin.menu`)
- **Usuario:** `cmssystem` (almacenado en Kubernetes Secret)
- **Base de datos:** Definida en Secret `cms-db-secret`
- **Persistent Volume:** 10Gi (StorageClass: local-path)
- **Liveness Probe:** `pg_isready -U cmssystem` (delay 30s, period 10s)
- **Resources:**
  - Requests: 250m CPU, 256Mi RAM
  - Limits: 500m CPU, 512Mi RAM

#### Autenticación y Seguridad
- **Proveedor:** Azure AD (Azure Entra ID)
- **Protocolo:** OpenID Connect (OIDC)
- **Endpoint:** signin-oidc
- **Flujo:** OAuth 2.0
- **Azure App Registration:** ✅ Configurado (datos en base de datos)
- **⚠️ PROBLEMA ACTUAL:** El API no valida correctamente el token que viene del UI
- **Log actual:** `⚠️ Sin usuario autenticado - Devolviendo todos los menús activos`

#### Containerización
- **Runtime:** Docker
- **Orquestador (Producción):** Kubernetes k3s
- **Orquestador (Desarrollo):** Docker Compose
- **Container Registry:** Harbor (privado)
  - URL: https://registry.biti-solutions.com
  - Imágenes:
    - `registry.biti-solutions.com/cms-api:latest`
    - `registry.biti-solutions.com/cms-ui:latest`

#### Infraestructura Cloud
- **Proveedor:** DigitalOcean
- **Tipo de recurso:** Droplet
- **Servidor:** BITISERVER1
- **Sistema Operativo:** Linux
- **IP Pública:** 147.182.204.86
- **Kubernetes:** k3s (lightweight Kubernetes)
- **VPN:** WireGuard ✅ Configurado (acceso directo desde laptop)

#### Networking y DNS
- **Dominio principal:** https://cms.biti-solutions.com
- **Dominio registry:** https://registry.biti-solutions.com
- **DNS:** cms.biti-solutions.com → 147.182.204.86
- **Certificados SSL:** Let's Encrypt (renovación automática vía cert-manager)
- **Ingress Controller:** Traefik
- **Load Balancer IP:** 147.182.204.86
- **Puertos externos:**
  - HTTP: 80 (redirect a HTTPS)
  - HTTPS: 443

### Arquitectura del Proyecto (Estructura de Código)

```
BITI-SOLUTIONS/cms/
├── CMS.UI/               ← Presentation Layer (Razor Pages/MVC)
├── CMS.API/              ← Presentation Layer (Web API REST)  
├── CMS.Application/      ← Application Layer (CQRS/MediatR)
├── CMS.Data/             ← Data Access Layer (Repository/EF Core)
├── CMS.Entities/         ← Domain Layer (Entities/Value Objects)
├── CMS.Shared/           ← Shared Infrastructure
├── backup/               ← Carpeta de backups
├── Dockerfile            ← Dockerfile para CMS.API
├── Dockerfile.UI         ← Dockerfile para CMS.UI
├── docker-compose.yml    ← Orquestación local (desarrollo)
├── CMS.Solution.sln      ← Solución Visual Studio
├── .dockerignore
├── .gitignore
└── .gitattributes
```

En su lugar, usa **`connectionstrings.json`** para TODA la configuración:
- Ubicación: `CMS.API/connectionstrings.json` y `CMS.UI/connectionstrings.json`
- Son el **mismo archivo** (idéntico contenido)
- Está en `.gitignore` ✅ (NO se versiona)
- Contiene:
  - Connection strings de PostgreSQL
  - Configuración de Azure AD
  - Cualquier otra configuración necesaria

**En Kubernetes:**
- El archivo se monta desde un ConfigMap: `cms-connectionstrings`
- Volumen: `connectionstrings-volume`
- Path en API: `/app/connectionstrings.json`
- Path en UI: `/app/connectionstrings.json` y `/app/../CMS.API/connectionstrings.json`

---

## 🖥️ 3. INFRAESTRUCTURA DETALLADA DE DIGITALOCEAN

### Droplet Configuration
- **Nombre:** BITISERVER1
- **Proveedor:** DigitalOcean
- **IP Pública:** 147.182.204.86
- **Sistema Operativo:** Linux
- **Acceso:**
  - SSH: `ssh root@147.182.204.86`
  - DigitalOcean Console (Web)
- **VPN:** WireGuard configurado (permite acceso directo desde laptop)

### Kubernetes Cluster (k3s)

#### Control Plane
- **Distribución:** k3s (lightweight Kubernetes)
- **Nodos:** 1 (single-node cluster)
- **DNS:** CoreDNS (servicio: kube-dns)
- **Metrics:** metrics-server (para HPA y monitoring)
- **Storage Provisioner:** local-path-provisioner
- **Edad del cluster:** 20 días

#### Namespaces Activos

| Namespace | Propósito | Pods Activos |
|-----------|-----------|--------------|
| `cms` | Aplicación principal CMS | 4 |
| `harbor` | Docker registry privado | 1 |
| `cert-manager` | Gestión de certificados SSL | 3 |
| `kube-system` | Sistema Kubernetes (core) | 4 |
| `kube-public` | Recursos públicos | 0 |
| `kube-node-lease` | Node heartbeats | 0 |
| `default` | Namespace por defecto | 2 |

### Namespace: `cms` (Aplicación Principal)

#### Deployments

| Nombre | Replicas | Imagen | Revision | Estrategia | Estado |
|--------|----------|--------|----------|------------|--------|
| `cms-api-deployment` | 1/1 | registry.biti-solutions.com/cms-api:latest | 20 | RollingUpdate | ✅ Running |
| `cms-ui-deployment` | 1/1 | registry.biti-solutions.com/cms-ui:latest | 31 | RollingUpdate | ✅ Running |
| `cms-deployment` | 1/1 | [Investigar qué imagen] | ? | RollingUpdate | ✅ Running |

**Rolling Update Strategy:** 25% max unavailable, 25% max surge

#### StatefulSet: PostgreSQL

```yaml
Name: cms-postgres
Replicas: 1/1
Pod: cms-postgres-0
Image: postgres:15-alpine
Strategy: RollingUpdate (Partition: 0)
Storage:
  - Volume: postgres-storage
  - Size: 10Gi
  - StorageClass: local-path
  - Access Mode: ReadWriteOnce
  - Mount Path: /var/lib/postgresql/data/postgres
```

#### Pods en Namespace `cms`

| Pod | Estado | Restarts | Uptime |
|-----|--------|----------|--------|
| cms-api-deployment-f95cbf478-jpzc2 | Running | 0 | 3h+ |
| cms-ui-deployment-855c8469c9-r76lt | Running | 0 | 14h+ |
| cms-postgres-0 | Running | 0 | 14h+ |
| cms-deployment-7875d787f8-8bllp | Running | 0 | 14h+ |

#### Services en Namespace `cms`

| Service | Type | Cluster IP | Port | Target |
|---------|------|------------|------|--------|
| `cms-api-service` | ClusterIP | 10.43.112.7 | 8080 | cms-api-deployment:8080 |
| `cms-ui-service` | ClusterIP | 10.43.98.213 | 8081 | cms-ui-deployment:8081 |
| `cms-service` | ClusterIP | 10.43.88.58 | 80 | [Investigar routing] |
| `cms-postgres` | ClusterIP (Headless) | None | 5432 | cms-postgres-0:5432 |
| `cms-postgres-nodeport` | NodePort | 10.43.13.99 | 5432:30432 | cms-postgres:5432 |

#### Secrets en Namespace `cms`

| Secret | Type | Keys | Propósito |
|--------|------|------|-----------|
| `cms-db-secret` | Opaque | 9 | Credenciales de PostgreSQL (POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB, etc.) |
| `cms-tls` | kubernetes.io/tls | 2 | Certificado SSL legacy |
| `cms-tls-secret` | kubernetes.io/tls | 2 | Certificado SSL actual (Let's Encrypt) |
| `registry-secret` | kubernetes.io/dockerconfigjson | 1 | Credenciales para Harbor registry |

#### ConfigMaps en Namespace `cms`

| ConfigMap | Keys | Propósito |
|-----------|------|-----------|
| `cms-config` | 1 | Configuración general |
| `cms-connectionstrings` | 1 | **ARCHIVO connectionstrings.json** (monta en ambos pods) |
| `kube-root-ca.crt` | 1 | Certificado CA raíz de Kubernetes |

#### Ingress: `cms-ingress`

```yaml
Name: cms-ingress
Namespace: cms
Class: traefik
Address: 147.182.204.86
TLS: cms-tls-secret (terminates cms.biti-solutions.com)

Annotations:
  cert-manager.io/cluster-issuer: letsencrypt-prod
  traefik.ingress.kubernetes.io/router.middlewares: cms-https-headers@kubernetescrd
  traefik.ingress.kubernetes.io/router.tls: "true"

Rules:
  Host: cms.biti-solutions.com
  Paths:
    /swagger → cms-api-service:8080 (Pod IP: 10.42.0.187:8080)
    /api     → cms-api-service:8080 (Pod IP: 10.42.0.187:8080)
    /        → cms-ui-service:8081  (Pod IP: 10.42.0.198:8081)
```

### Namespace: `harbor` (Docker Registry)

| Resource | Details |
|----------|---------|
| **Pod** | docker-registry-7497dc8ddc-6mzrt |
| **Service** | ClusterIP 10.43.98.208:5000 |
| **Ingress** | registry-ingress → registry.biti-solutions.com |
| **Estado** | ✅ Running (4d 14h uptime) |
| **Propósito** | Almacén privado de imágenes Docker del CMS |

### Namespace: `cert-manager` (Gestión SSL)

| Pod | Estado | Restarts | Función |
|-----|--------|----------|---------|
| cert-manager-75bb65b7b9-zxvb7 | Running | 3 | Emisor de certificados (controller) |
| cert-manager-cainjector-5cd89979d6-6rrtf | Running | 3 | Inyección de CA en recursos |
| cert-manager-webhook-8fc5dcf5f-t8dv9 | Running | 10 | Webhook para validación |

**Issuer:** `letsencrypt-prod` (ClusterIssuer)  
**Renovación:** Automática cada 60 días  
**Uptime:** 19 días

### Namespace: `kube-system` (Sistema Kubernetes)

| Resource | Type | Details |
|----------|------|---------|
| **coredns** | Deployment | DNS interno del cluster (servicio: kube-dns) |
| **traefik** | Deployment + LoadBalancer | Ingress Controller principal |
| **metrics-server** | Deployment | Métricas de CPU/RAM para HPA |
| **local-path-provisioner** | Deployment | Provisioning de PersistentVolumes |
| **svclb-traefik** | DaemonSet | Service LoadBalancer helper |

#### Traefik LoadBalancer (CRÍTICO)

```yaml
Service: traefik
Type: LoadBalancer
Cluster IP: 10.43.139.45
External IP: 147.182.204.86
Ports:
  - 80:30194/TCP   (HTTP)
  - 443:32404/TCP  (HTTPS)
Uptime: 2d 20h
```

**Este es el punto de entrada principal para TODO el tráfico externo.**

---

## 🌐 4. NETWORKING Y FLUJO DE TRÁFICO

### External Access Flow (Producción)

```
Internet (HTTPS Request)
    ↓
https://cms.biti-solutions.com
    ↓
147.182.204.86:443 (IP pública de DigitalOcean)
    ↓
Traefik LoadBalancer (kube-system)
  - ClusterIP: 10.43.139.45
  - External IP: 147.182.204.86
    ↓
cms-ingress (Traefik IngressRoute - namespace: cms)
  - TLS Termination (cms-tls-secret)
  - Path routing:
    ├─ /swagger → cms-api-service:8080
    ├─ /api     → cms-api-service:8080
    └─ /        → cms-ui-service:8081
    ↓
┌─────────────────────────────┬─────────────────────────────┐
│ cms-ui-service:8081         │ cms-api-service:8080        │
│ (ClusterIP: 10.43.98.213)   │ (ClusterIP: 10.43.112.7)    │
└──────────────┬──────────────┴──────────────┬──────────────┘
               ↓                             ↓
   cms-ui-deployment Pod              cms-api-deployment Pod
   (10.42.0.198:8081)                (10.42.0.187:8080)
               │                             │
               └─────────────┬───────────────┘
                             ↓
                    cms-postgres-0:5432
                    (StatefulSet PostgreSQL)
```

### Internal Service Mesh

```
cms-ui-service:8081 → cms-ui-deployment (Port 8081)
cms-api-service:8080 → cms-api-deployment (Port 8080)
cms-service:80 → [Investigar qué deployment]
cms-postgres:5432 → cms-postgres-0 (Headless Service)
cms-postgres-nodeport:30432 → cms-postgres:5432 (Acceso externo DB)
```

### VPN Configuration (WireGuard)

**Propósito:** Conectar laptop de desarrollo directamente al servidor DigitalOcean

```
[PENDIENTE: Documentar configuración WireGuard]
- Archivo de configuración: .conf
- Puerto WireGuard: [típicamente 51820]
- IP interna VPN: [Ej: 10.8.0.x]
- Uso: Permite kubectl desde laptop sin exponer puertos
```

---

## 👥 5. EQUIPO Y ROLES

### Equipo Actual
- **@BITI-SOLUTIONS** - Owner / Full Stack Developer / DevOps / QA
- **Rol único:** Fundador, desarrollador principal, administrador de infraestructura
- **Tamaño del equipo:** 1 persona
- **Colaboradores externos:** Ninguno actualmente

### Zona Horaria y Horario
- **Timezone:** UTC-6 (Hora de Costa Rica - CST)
- **Horario de trabajo:** 8:00 AM - 12:00 AM (medianoche) CST
- **Disponibilidad para deployments:** Horario laboral (preferentemente mañanas)

### Herramientas de Trabajo

#### Desarrollo Local (Laptop - Windows)
- **IDE:** Visual Studio / Visual Studio Code
- **Terminal:** PowerShell
- **Docker:** Docker Desktop (para docker-compose local)
- **VPN:** WireGuard (conexión al servidor)
- **Acceso remoto:** 
  - SSH a DigitalOcean: `ssh root@147.182.204.86`
  - kubectl vía WireGuard VPN

#### Servidor Remoto (DigitalOcean)
- **Acceso:** DigitalOcean Console (Web) + SSH
- **Kubernetes:** kubectl nativo en BITISERVER1
- **Container management:** k3s + kubectl
- **Logs:** `kubectl logs -n cms <pod-name>`

---

## 📝 6. CONVENCIONES Y ESTÁNDARES

### Nomenclatura de Branches
```
main              → Producción (default branch)
develop           → Desarrollo (NO EXISTE AÚN - recomendado crear)
feature/[nombre]  → Nuevas funcionalidades
fix/[nombre]      → Correcciones de bugs
hotfix/[nombre]   → Correcciones urgentes en producción
```

**Estado actual:** Solo existe branch `main`  
**Recomendación:** Implementar GitFlow con branch `develop`

### Convenciones de Commits

**Formato recomendado (Conventional Commits):**
```
feat: nueva funcionalidad
fix: corrección de bug
docs: documentación
test: pruebas
refactor: refactorización de código
style: cambios de formato (no afectan lógica)
chore: tareas de mantenimiento
ci: cambios en CI/CD
perf: mejoras de performance
```

### Nomenclatura de Issues

```
[BUG] Descripción corta del error
[FEAT] Descripción de la nueva funcionalidad
[DOCS] Actualización de documentación
[REFACTOR] Refactorización de [componente]
[PERF] Mejora de performance en [área]
```

### Labels Estándar (GitHub)

**Prioridad:**
- `priority-critical` - Bloquea producción
- `priority-high` - Importante, resolver pronto
- `priority-medium` - Normal
- `priority-low` - Cuando haya tiempo

**Tipo:**
- `bug` - Errores
- `enhancement` - Mejoras
- `feature` - Nueva funcionalidad
- `documentation` - Documentación
- `question` - Preguntas
- `wontfix` - No se arreglará

**Áreas:**
- `area-api` - Backend/API
- `area-ui` - Frontend
- `area-database` - Base de datos
- `area-infrastructure` - DevOps/K8s
- `area-auth` - Autenticación

---

## 🔄 7. WORKFLOW Y PROCESOS

### Proceso de Desarrollo ACTUAL (Manual)

```
1. Hacer cambios en código localmente
2. Crear Pull Request a main
3. [NO HAY] Code review automático
4. [NO HAY] Tests automáticos
5. Merge manual a main
6. Build manual de imágenes Docker
7. Push manual a Harbor registry
8. Deployment manual con kubectl set image
```

### Proceso de Deployment ACTUAL

#### Build y Push (Manual)

```bash
# Desde laptop (con Docker)

# Build API
docker build -t registry.biti-solutions.com/cms-api:latest -f Dockerfile .

# Build UI
docker build -t registry.biti-solutions.com/cms-ui:latest -f Dockerfile.UI .

# Login a Harbor
docker login registry.biti-solutions.com

# Push imágenes
docker push registry.biti-solutions.com/cms-api:latest
docker push registry.biti-solutions.com/cms-ui:latest
```

#### Deploy (Manual desde DigitalOcean Console)

```bash
# API
kubectl set image deployment/cms-api-deployment \
  cms-api=registry.biti-solutions.com/cms-api:latest \
  --namespace=cms --record=true

# UI
kubectl set image deployment/cms-ui-deployment \
  cms-ui=registry.biti-solutions.com/cms-ui:latest \
  --namespace=cms --record=true

# Verificar rollout
kubectl rollout status deployment/cms-api-deployment -n cms
kubectl rollout status deployment/cms-ui-deployment -n cms
```

### ⚠️ PROBLEMAS IDENTIFICADOS

1. ❌ **No hay GitHub Actions** configurados
2. ❌ **No hay manifiestos de Kubernetes** en el repositorio
3. ❌ **Deployments completamente manuales**
4. ❌ **No hay tests automáticos**
5. ❌ **No hay proceso de rollback documentado**
6. ❌ **Falta autenticación Azure AD** en el API

---

## 🚨 8. PROBLEMA ACTUAL CRÍTICO

### Estado Actual del Sistema

| Componente | Estado | Observación |
|------------|--------|-------------|
| PostgreSQL | ✅ Funcionando | Queries ejecutándose correctamente |
| CMS.UI | ✅ Funcionando | Interfaz cargando, navegación OK |
| CMS.API | ⚠️ **PROBLEMA** | **No valida autenticación de Azure AD** |
| Kubernetes | ✅ Funcionando | Todos los pods running |
| Traefik/Ingress | ✅ Funcionando | Routing correcto |
| Certificados SSL | ✅ Funcionando | HTTPS válido |

### Problema Identificado: Autenticación Azure AD

**Síntoma:**
```
⚠️ Sin usuario autenticado - Devolviendo todos los menús activos
```

**Diagnóstico:**
1. ✅ UI puede hacer login con Azure AD
2. ✅ UI obtiene un token
3. ❌ **API NO VALIDA el token correctamente**
4. ❌ API detecta "sin usuario autenticado"
5. ⚠️ API devuelve data sin validar permisos

**Configuración actual:**
- **Tipo de autenticación:** Azure AD (Azure Entra ID)
- **Protocolo:** OpenID Connect (OIDC)
- **Endpoint:** signin-oidc
- **Flujo:** OAuth 2.0
- **Azure App Registration:** ✅ Configurado (credenciales en base de datos)

**Posibles causas:**
1. API no está configurado para validar tokens JWT de Azure AD
2. Middleware de autenticación no está en el pipeline correcto
3. Audience/Issuer mismatch entre UI y API
4. Claims no se están leyendo correctamente
5. CORS bloqueando el header Authorization

---

## 💾 9. BASE DE DATOS - POSTGRESQL

### Configuración del StatefulSet

```yaml
Name: cms-postgres
Namespace: cms
Image: postgres:15-alpine
Replicas: 1 (StatefulSet)
Pod: cms-postgres-0
```

### Recursos Asignados

| Recurso | Request | Limit |
|---------|---------|-------|
| CPU | 250m (0.25 cores) | 500m (0.5 cores) |
| RAM | 256Mi | 512Mi |

### Storage

| Parámetro | Valor |
|-----------|-------|
| **Volume Claim** | postgres-storage |
| **StorageClass** | local-path |
| **Capacity** | 10Gi |
| **Access Mode** | ReadWriteOnce |
| **Mount Path** | /var/lib/postgresql/data/postgres |

### Esquemas y Estructura

**Esquema principal:** `admin`

**Ejemplo de tablas conocidas:**
```sql
admin.menu
  - id_menu (PK)
  - name
  - url
  - icon
  - id_parent (FK self-reference)
  - menu_order
  - permission_key
  - is_active
  - created_by
  - updated_by
  - createdate
  - record_date
  - rowpointer (UUID)
```

**Resultado actual:** 159 menús activos

---

## 🛠️ 10. COMANDOS ÚTILES Y TROUBLESHOOTING

### Comandos de Acceso Rápido

#### Ver logs en tiempo real
```bash
# API
kubectl logs -n cms deployment/cms-api-deployment -f

# UI
kubectl logs -n cms deployment/cms-ui-deployment -f

# PostgreSQL
kubectl logs -n cms cms-postgres-0 -f
```

#### Ver estado general
```bash
# Todo en namespace cms
kubectl get all -n cms

# Pods con más detalle
kubectl get pods -n cms -o wide
```

#### Reiniciar deployments
```bash
# Reiniciar API (rolling restart)
kubectl rollout restart deployment/cms-api-deployment -n cms

# Reiniciar UI
kubectl rollout restart deployment/cms-ui-deployment -n cms
```

#### Deploy Manual
```bash
# Actualizar imagen del API
kubectl set image deployment/cms-api-deployment \
  cms-api=registry.biti-solutions.com/cms-api:latest \
  --namespace=cms --record=true

# Actualizar imagen del UI
kubectl set image deployment/cms-ui-deployment \
  cms-ui=registry.biti-solutions.com/cms-ui:latest \
  --namespace=cms --record=true
```

#### Rollback
```bash
# Ver historial
kubectl rollout history deployment/cms-api-deployment -n cms

# Rollback a la versión anterior
kubectl rollout undo deployment/cms-api-deployment -n cms
```

---

## 🚀 11. PRÓXIMOS PASOS Y MEJORAS PENDIENTES

### Crítico (Hacer AHORA)

1. **❌ Arreglar autenticación Azure AD en API**
2. **❌ Crear manifiestos de Kubernetes en el repo**
3. **❌ Implementar GitHub Actions para CI/CD**

### Alta Prioridad (Esta semana)

4. **Documentar proceso de backup de PostgreSQL**
5. **Crear branch `develop` e implementar GitFlow**
6. **Documentar configuración de WireGuard VPN**
7. **Implementar health checks en la aplicación**

---

## 💡 12. CONTEXTO PARA COPILOT

### Stack Clave
- .NET 9.0 + PostgreSQL 15 + Kubernetes k3s + Azure AD
- Puertos: API 8080, UI 8081, PostgreSQL 5432/30432
- Servidor: BITISERVER1 @ 147.182.204.86

### Problema Actual
- ⚠️ API no valida autenticación Azure AD
- UI funciona ✅, API no valida token ❌

---

## 📝 13. CHANGELOG

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2026-02-09 | 1.0 | Documento inicial completo |

## 🚀 14. Checklist para Deploy en DigitalOcean Droplet/Servidor
Dado lo que describes (Kubernetes, Docker, acceso SSH, registry privado), te dejo los comandos completos para:

Actualizar el código
Reconstruir imágenes Docker
Push de imágenes al registry
Actualizar el deployment en Kubernetes
Verificar y hacer rollback si es necesario

Necesito agregarle una pantalla de login al puro inicio que el usuario digite la company este dato se tiene que validar contrata la tabla SELECT * FROM admin.company espeficicamente con el campo company_schema, aqui se tiene que validar el tipo de validacion de usuario que de momento solo lo tengo implementado para que valide por azure ad, pero tambien puede ser que se validad por un usuario del propio sistema es decir correo electronico mas contraseña, para eso hay que crear un campo nuevo boolean en SELECT * FROM admin.company para saber si la compañlia utiliza o no azure ad, es decir todos los usuarios de la compañia se van a logear igual segun lo que tenga definido, tambien hay que crear una tabla nueva que relacione si el usuario puede acceder a esa compañia ya que un usurios va poder acceder a varias compañias ademas puede que en una de las compañias se valide con azure ad y en otra solo por correo mas contraseña si esta esta ultima los usuarios pueden solicitar reestablecer la contraseña si fue que se les olvido esto mandara un correo al correo registrado y con esto comprar que es el usuario, 3 intentos de poner una contraseña incorrecto se bloquea automaticamnet el usuario y debera reestablecer la contraseña

---

**Autor:** @BITI-SOLUTIONS con GitHub Copilot  
**URL:** https://cms.biti-solutions.com  
**Servidor:** BITISERVER1 (147.182.204.86)