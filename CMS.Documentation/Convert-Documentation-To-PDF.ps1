# ============================================================================
# Script: Convert-Documentation-To-PDF.ps1
# Descripcion: Convierte archivos Markdown de documentacion a PDF
# Autor: EAMR - BITI Solutions S.A
# Fecha: Marzo 2026
#
# REQUISITOS:
# - PowerShell 5.1+
# - Pandoc (https://pandoc.org/installing.html)
# - wkhtmltopdf (para renderizado PDF)
#
# INSTALACION DE DEPENDENCIAS:
# Windows (con Chocolatey):
#   choco install pandoc wkhtmltopdf
#
# ============================================================================

param(
    [string]$SourceFolder = ".",
    [string]$OutputFolder = "PDFs"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CMS Documentation PDF Generator" -ForegroundColor Cyan
Write-Host "  BITI Solutions S.A." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Pandoc esta instalado
$pandocPath = Get-Command pandoc -ErrorAction SilentlyContinue
if (-not $pandocPath) {
    Write-Host "ERROR: Pandoc no esta instalado." -ForegroundColor Red
    Write-Host "   Instalalo con: choco install pandoc wkhtmltopdf" -ForegroundColor Yellow
    exit 1
}

Write-Host "Pandoc encontrado: $($pandocPath.Source)" -ForegroundColor Green

# Verificar carpeta de origen
if (-not (Test-Path $SourceFolder)) {
    Write-Host "ERROR: Carpeta de origen no encontrada: $SourceFolder" -ForegroundColor Red
    exit 1
}

# Crear carpeta de salida si no existe
if (-not (Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
    Write-Host "Carpeta de salida creada: $OutputFolder" -ForegroundColor Green
}

# Mapeo de archivos MD a nombres PDF (TODOS los documentos)
$documentMapping = @{
    "DOC-GEN-001_Manual_General.md" = "CMS_Manual_General.pdf"
    "DOC-GEN-002_Guia_Inicio_Rapido.md" = "CMS_Guia_Inicio_Rapido.pdf"
    "DOC-GEN-003_Seguridad_Permisos.md" = "CMS_Seguridad_Permisos.pdf"
    "MOD-INV-001_Modulo_Inventario.md" = "CMS_Modulo_Inventario.pdf"
    "MOD-REP-001_Modulo_Reportes.md" = "CMS_Modulo_Reportes.pdf"
    "MOD-ADM-001_Modulo_Administracion.md" = "CMS_Modulo_Administracion.pdf"
    "MOD-CRM-001_Modulo_CRM.md" = "CMS_Modulo_CRM.pdf"
    "MOD-VEN-001_Modulo_Ventas.md" = "CMS_Modulo_Ventas.pdf"
    "MOD-COM-001_Modulo_Compras.md" = "CMS_Modulo_Compras.pdf"
    "MOD-POS-001_Modulo_POS.md" = "CMS_Modulo_POS.pdf"
    "MOD-CON-001_Modulo_Contabilidad.md" = "CMS_Modulo_Contabilidad.pdf"
    "MOD-FIN-001_Modulo_Finanzas.md" = "CMS_Modulo_Finanzas.pdf"
    "MOD-RRH-001_Modulo_RRHH.md" = "CMS_Modulo_RRHH.pdf"
    "MOD-PRO-001_Modulo_Proyectos.md" = "CMS_Modulo_Proyectos.pdf"
    "MOD-FAC-001_Modulo_Facturacion_Electronica.md" = "CMS_Modulo_Facturacion_Electronica.pdf"
    "TUT-001_Tutorial_Crear_Usuario.md" = "CMS_Tutorial_Crear_Usuario.pdf"
    "TUT-002_Tutorial_Roles_Permisos.md" = "CMS_Tutorial_Roles_Permisos.pdf"
    "TUT-003_Tutorial_Reportes.md" = "CMS_Tutorial_Reportes.pdf"
    "FAQ-001_Preguntas_Frecuentes.md" = "CMS_FAQ.pdf"
}

# Crear archivo CSS para estilo
$cssPath = Join-Path $OutputFolder "style.css"
$cssLines = @(
    "body { font-family: Arial, sans-serif; font-size: 11pt; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px; }"
    "h1 { color: #0066cc; border-bottom: 3px solid #0066cc; padding-bottom: 10px; font-size: 24pt; }"
    "h2 { color: #0066cc; border-bottom: 1px solid #ccc; padding-bottom: 5px; font-size: 18pt; margin-top: 30px; }"
    "h3 { color: #333; font-size: 14pt; margin-top: 20px; }"
    "table { border-collapse: collapse; width: 100%; margin: 15px 0; }"
    "th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }"
    "th { background-color: #0066cc; color: white; }"
    "tr:nth-child(even) { background-color: #f9f9f9; }"
    "code { background-color: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: Consolas, monospace; font-size: 10pt; }"
    "pre { background-color: #f4f4f4; padding: 15px; border-radius: 5px; overflow-x: auto; font-size: 9pt; }"
    "blockquote { border-left: 4px solid #0066cc; padding-left: 15px; margin-left: 0; color: #666; font-style: italic; }"
    "hr { border: none; border-top: 2px solid #eee; margin: 30px 0; }"
    "a { color: #0066cc; }"
)
$cssLines -join "`n" | Out-File -FilePath $cssPath -Encoding UTF8
Write-Host "CSS de estilo creado" -ForegroundColor Green

# Contadores
$converted = 0
$failed = 0
$skipped = 0

# Verificar wkhtmltopdf
$wkhtmlPath = Get-Command wkhtmltopdf -ErrorAction SilentlyContinue
$useWkhtml = $null -ne $wkhtmlPath
if ($useWkhtml) {
    Write-Host "wkhtmltopdf encontrado: $($wkhtmlPath.Source)" -ForegroundColor Green
} else {
    Write-Host "wkhtmltopdf no encontrado - se generaran HTMLs" -ForegroundColor Yellow
}

# Procesar cada archivo
Write-Host ""
Write-Host "Convirtiendo documentos..." -ForegroundColor Cyan
Write-Host ""

foreach ($mapping in $documentMapping.GetEnumerator()) {
    $mdFile = Join-Path $SourceFolder $mapping.Key
    $pdfFile = Join-Path $OutputFolder $mapping.Value
    
    Write-Host "   $($mapping.Key) ... " -NoNewline
    
    if (Test-Path $mdFile) {
        try {
            # Primero convertir a HTML
            $htmlFile = Join-Path $OutputFolder ($mapping.Value -replace "\.pdf$", ".html")
            $pandocArgsHtml = @($mdFile, "-o", $htmlFile, "--standalone", "--css=$cssPath", "--metadata=title:CMS Documentation")

            $processHtml = Start-Process -FilePath "pandoc" -ArgumentList $pandocArgsHtml -Wait -PassThru -NoNewWindow

            if ($useWkhtml -and (Test-Path $htmlFile)) {
                # Convertir HTML a PDF con wkhtmltopdf
                $wkArgs = @(
                    "--quiet",
                    "--enable-local-file-access",
                    "--margin-top", "20mm",
                    "--margin-bottom", "20mm",
                    "--margin-left", "15mm",
                    "--margin-right", "15mm",
                    $htmlFile,
                    $pdfFile
                )

                $processPdf = Start-Process -FilePath "wkhtmltopdf" -ArgumentList $wkArgs -Wait -PassThru -NoNewWindow

                if ($processPdf.ExitCode -eq 0 -and (Test-Path $pdfFile)) {
                    $fileSize = (Get-Item $pdfFile).Length
                    $fileSizeKB = [math]::Round($fileSize / 1KB, 1)
                    Write-Host "OK ($fileSizeKB KB)" -ForegroundColor Green
                    $converted++
                    # Limpiar HTML temporal
                    Remove-Item $htmlFile -Force -ErrorAction SilentlyContinue
                }
                else {
                    Write-Host "ERROR PDF" -ForegroundColor Red
                    $failed++
                }
            }
            elseif (Test-Path $htmlFile) {
                $fileSize = (Get-Item $htmlFile).Length
                $fileSizeKB = [math]::Round($fileSize / 1KB, 1)
                Write-Host "HTML ($fileSizeKB KB)" -ForegroundColor Yellow
                $converted++
            }
            else {
                Write-Host "ERROR" -ForegroundColor Red
                $failed++
            }
        }
        catch {
            Write-Host "ERROR: $_" -ForegroundColor Red
            $failed++
        }
    }
    else {
        Write-Host "NO ENCONTRADO" -ForegroundColor Yellow
        $skipped++
    }
}

# Limpiar archivo de error si esta vacio
$errorLog = Join-Path $OutputFolder "error.log"
if (Test-Path $errorLog) {
    $errorContent = Get-Content $errorLog -Raw
    if ([string]::IsNullOrWhiteSpace($errorContent)) {
        Remove-Item $errorLog -Force
    }
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Convertidos: $converted" -ForegroundColor Green
Write-Host "  Fallidos: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host "  Omitidos: $skipped" -ForegroundColor $(if ($skipped -gt 0) { "Yellow" } else { "Green" })
Write-Host "  Salida: $OutputFolder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($converted -gt 0) {
    Write-Host "Siguiente paso: Ejecute .\Generate-SQL-From-PDFs.ps1 para generar el SQL" -ForegroundColor Yellow
}
