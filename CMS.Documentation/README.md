# 📚 Documentación del Sistema CMS

Este directorio contiene la documentación oficial del Sistema CMS de BITI Solutions S.A.

## 📁 Estructura de Archivos

```
CMS.Documentation/
├── README.md                              # Este archivo
├── Convert-Documentation-To-PDF.ps1       # Script para convertir MD a PDF
├── Generate-SQL-From-PDFs.ps1             # Script para generar SQL con PDFs
│
├── DOC-GEN-001_Manual_General.md          # Manual General del Sistema
├── DOC-GEN-002_Guia_Inicio_Rapido.md      # Guía de Inicio Rápido
├── DOC-GEN-003_Seguridad_Permisos.md      # Política de Seguridad
│
├── MOD-*.md                               # Documentación de módulos
├── TUT-*.md                               # Tutoriales
├── FAQ-*.md                               # Preguntas Frecuentes
│
└── PDFs/                                  # PDFs generados
```

## 🚀 Pasos para Generar y Cargar la Documentación

### Paso 1: Instalar Dependencias

#### Windows (con Chocolatey)
```powershell
choco install pandoc wkhtmltopdf
```

#### Windows (con Winget)
```powershell
winget install JohnMacFarlane.Pandoc
winget install wkhtmltopdf.wkhtmltopdf
```

#### Ubuntu/Debian
```bash
sudo apt-get install pandoc wkhtmltopdf
```

### Paso 2: Convertir Markdown a PDF

```powershell
cd CMS.Documentation
.\Convert-Documentation-To-PDF.ps1
```

Esto generará los archivos PDF en la carpeta `PDFs/`.

### Paso 3: Generar Script SQL con los PDFs

```powershell
.\Generate-SQL-From-PDFs.ps1
```

Esto genera el archivo `CMS.Data/Scripts/022_insert_documentation_pdfs.sql` con los PDFs codificados.

### Paso 4: Ejecutar el SQL en PostgreSQL

**Opción A - Usando psql:**
```bash
psql -h 10.0.0.1 -U cmssystem -d cms -f "CMS.Data/Scripts/022_insert_documentation_pdfs.sql"
```

**Opción B - Usando DBeaver/pgAdmin:**
1. Abra DBeaver o pgAdmin
2. Conéctese a la base de datos `cms`
3. Abra el archivo `022_insert_documentation_pdfs.sql`
4. Ejecute el script (F5)

### Paso 5: Verificar

1. Acceda al sistema: https://cms.biti-solutions.com
2. Vaya al Dashboard
3. Clic en **"documentación del sistema"**
4. Verifique que los documentos aparecen con el estado "Cargado"

## 📋 Lista de Documentos

### Documentación General
| Código | Título | Archivo |
|--------|--------|---------|
| DOC-GEN-001 | Manual General del Sistema CMS | CMS_Manual_General.pdf |
| DOC-GEN-002 | Guía de Inicio Rápido | CMS_Guia_Inicio_Rapido.pdf |
| DOC-GEN-003 | Política de Seguridad y Permisos | CMS_Seguridad_Permisos.pdf |

### Documentación por Módulo
| Código | Título | Archivo |
|--------|--------|---------|
| MOD-INV-001 | Módulo de Inventario | CMS_Modulo_Inventario.pdf |
| MOD-REP-001 | Módulo de Reportes | CMS_Modulo_Reportes.pdf |
| MOD-ADM-001 | Módulo de Administración | CMS_Modulo_Administracion.pdf |
| MOD-CRM-001 | Módulo de CRM | CMS_Modulo_CRM.pdf |
| MOD-VEN-001 | Módulo de Ventas | CMS_Modulo_Ventas.pdf |
| MOD-COM-001 | Módulo de Compras | CMS_Modulo_Compras.pdf |
| MOD-POS-001 | Módulo de POS | CMS_Modulo_POS.pdf |
| MOD-CON-001 | Módulo de Contabilidad | CMS_Modulo_Contabilidad.pdf |
| MOD-FIN-001 | Módulo de Finanzas | CMS_Modulo_Finanzas.pdf |
| MOD-RRH-001 | Módulo de RRHH | CMS_Modulo_RRHH.pdf |
| MOD-PRO-001 | Módulo de Proyectos | CMS_Modulo_Proyectos.pdf |
| MOD-FAC-001 | Módulo de Facturación Electrónica | CMS_Modulo_Facturacion_Electronica.pdf |

### Tutoriales
| Código | Título | Archivo |
|--------|--------|---------|
| TUT-001 | Cómo crear un nuevo usuario | CMS_Tutorial_Crear_Usuario.pdf |
| TUT-002 | Configurar roles y permisos | CMS_Tutorial_Roles_Permisos.pdf |
| TUT-003 | Crear y ejecutar reportes | CMS_Tutorial_Reportes.pdf |

### FAQ
| Código | Título | Archivo |
|--------|--------|---------|
| FAQ-001 | Preguntas Frecuentes | CMS_FAQ.pdf |
| MOD-REP-001 | Módulo de Reportes | CMS_Modulo_Reportes.pdf |
| MOD-ADM-001 | Módulo de Administración | CMS_Modulo_Administracion.pdf |
| MOD-CRM-001 | Módulo de CRM | CMS_Modulo_CRM.pdf |
| MOD-VEN-001 | Módulo de Ventas | CMS_Modulo_Ventas.pdf |
| MOD-COM-001 | Módulo de Compras | CMS_Modulo_Compras.pdf |
| MOD-POS-001 | Módulo de Punto de Venta | CMS_Modulo_POS.pdf |
| MOD-CON-001 | Módulo de Contabilidad | CMS_Modulo_Contabilidad.pdf |
| MOD-FIN-001 | Módulo de Finanzas | CMS_Modulo_Finanzas.pdf |
| MOD-RRH-001 | Módulo de Recursos Humanos | CMS_Modulo_RRHH.pdf |
| MOD-PRO-001 | Módulo de Proyectos | CMS_Modulo_Proyectos.pdf |
| MOD-FAC-001 | Módulo de Facturación Electrónica | CMS_Modulo_Facturacion_Electronica.pdf |

### Tutoriales
| Código | Título | Archivo |
|--------|--------|---------|
| TUT-001 | Tutorial: Cómo crear un nuevo usuario | CMS_Tutorial_Crear_Usuario.pdf |
| TUT-002 | Tutorial: Configurar roles y permisos | CMS_Tutorial_Roles_Permisos.pdf |
| TUT-003 | Tutorial: Crear y ejecutar reportes | CMS_Tutorial_Reportes.pdf |

### FAQ
| Código | Título | Archivo |
|--------|--------|---------|
| FAQ-001 | Preguntas Frecuentes | CMS_FAQ.pdf |

## ✏️ Edición de Documentos

1. Edite el archivo `.md` correspondiente
2. Ejecute `.\Convert-Documentation-To-PDF.ps1` para regenerar el PDF
3. Ejecute `.\Upload-Documentation-PDFs.ps1` para actualizar en el sistema

## 🔧 API de Documentación

### Endpoints Disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/documentation` | Lista documentos agrupados |
| GET | `/api/documentation/list` | Lista simple de documentos |
| GET | `/api/documentation/{id}` | Detalle de documento |
| GET | `/api/documentation/{id}/download` | Descargar PDF |
| GET | `/api/documentation/{id}/view` | Ver PDF inline |
| POST | `/api/documentation/{id}/upload` | Subir PDF |

### Ejemplo de Uso

```javascript
// Obtener lista de documentos
fetch('/api/documentation', {
  headers: { 'Authorization': 'Bearer ' + token }
})
.then(response => response.json())
.then(data => console.log(data));
```

## 📞 Soporte

Para preguntas sobre la documentación:
- **Email:** soporte@biti-solutions.com
- **Teléfono:** +506 XXXX-XXXX

---

**© 2026 BITI Solutions S.A. - Todos los derechos reservados**
