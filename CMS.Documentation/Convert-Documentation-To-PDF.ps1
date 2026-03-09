# ============================================================================
# Script: Convert-Documentation-To-PDF.ps1
# Descripción: Convierte archivos Markdown de documentación a PDF
# Autor: EAMR - BITI Solutions S.A
# Fecha: Marzo 2026
#
# REQUISITOS:
# - PowerShell 7+
# - Pandoc (https://pandoc.org/installing.html)
# - wkhtmltopdf o LaTeX (para renderizado PDF)
#
# INSTALACIÓN DE DEPENDENCIAS:
# Windows (con Chocolatey):
#   choco install pandoc wkhtmltopdf
#
# Windows (con Winget):
#   winget install JohnMacFarlane.Pandoc
#   winget install wkhtmltopdf.wkhtmltopdf
#
# Ubuntu/Debian:
#   sudo apt-get install pandoc wkhtmltopdf
#
# ============================================================================

param(
    [string]$SourceFolder = "CMS.Documentation",
    [string]$OutputFolder = "CMS.Documentation/PDFs",
    [switch]$OpenOutputFolder
)

# Colores para output
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n========================================" "Cyan"
Write-ColorOutput "  CMS Documentation PDF Generator" "Cyan"
Write-ColorOutput "  BITI Solutions S.A." "Cyan"
Write-ColorOutput "========================================`n" "Cyan"

# Verificar que Pandoc está instalado
$pandocPath = Get-Command pandoc -ErrorAction SilentlyContinue
if (-not $pandocPath) {
    Write-ColorOutput "❌ ERROR: Pandoc no está instalado." "Red"
    Write-ColorOutput "   Instálalo con: choco install pandoc" "Yellow"
    Write-ColorOutput "   O descarga de: https://pandoc.org/installing.html" "Yellow"
    exit 1
}

Write-ColorOutput "✅ Pandoc encontrado: $($pandocPath.Source)" "Green"

# Verificar carpeta de origen
if (-not (Test-Path $SourceFolder)) {
    Write-ColorOutput "❌ ERROR: Carpeta de origen no encontrada: $SourceFolder" "Red"
    exit 1
}

# Crear carpeta de salida si no existe
if (-not (Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
    Write-ColorOutput "📁 Carpeta de salida creada: $OutputFolder" "Green"
}

# Mapeo de archivos MD a nombres PDF
$documentMapping = @{
    "DOC-GEN-001_Manual_General.md" = "CMS_Manual_General.pdf"
    "DOC-GEN-002_Guia_Inicio_Rapido.md" = "CMS_Guia_Inicio_Rapido.pdf"
    "DOC-GEN-003_Seguridad_Permisos.md" = "CMS_Seguridad_Permisos.pdf"
    "MOD-INV-001_Modulo_Inventario.md" = "CMS_Modulo_Inventario.pdf"
    "MOD-REP-001_Modulo_Reportes.md" = "CMS_Modulo_Reportes.pdf"
    "MOD-ADM-001_Modulo_Administracion.md" = "CMS_Modulo_Administracion.pdf"
    "TUT-001_Tutorial_Crear_Usuario.md" = "CMS_Tutorial_Crear_Usuario.pdf"
    "FAQ-001_Preguntas_Frecuentes.md" = "CMS_FAQ.pdf"
}

# CSS para estilo del PDF
$cssContent = @"
body {
    font-family: 'Segoe UI', Arial, sans-serif;
    font-size: 11pt;
    line-height: 1.6;
    color: #333;
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
}

h1 {
    color: #0066cc;
    border-bottom: 3px solid #0066cc;
    padding-bottom: 10px;
    font-size: 24pt;
}

h2 {
    color: #0066cc;
    border-bottom: 1px solid #ccc;
    padding-bottom: 5px;
    font-size: 18pt;
    margin-top: 30px;
}

h3 {
    color: #333;
    font-size: 14pt;
    margin-top: 20px;
}

table {
    border-collapse: collapse;
    width: 100%;
    margin: 15px 0;
}

th, td {
    border: 1px solid #ddd;
    padding: 10px;
    text-align: left;
}

th {
    background-color: #0066cc;
    color: white;
}

tr:nth-child(even) {
    background-color: #f9f9f9;
}

code {
    background-color: #f4f4f4;
    padding: 2px 6px;
    border-radius: 3px;
    font-family: 'Consolas', monospace;
    font-size: 10pt;
}

pre {
    background-color: #f4f4f4;
    padding: 15px;
    border-radius: 5px;
    overflow-x: auto;
    font-size: 9pt;
}

blockquote {
    border-left: 4px solid #0066cc;
    padding-left: 15px;
    margin-left: 0;
    color: #666;
    font-style: italic;
}

hr {
    border: none;
    border-top: 2px solid #eee;
    margin: 30px 0;
}

a {
    color: #0066cc;
}

/* Footer */
footer {
    margin-top: 50px;
    padding-top: 20px;
    border-top: 1px solid #ccc;
    text-align: center;
    font-size: 9pt;
    color: #666;
}
"@

# Guardar CSS temporal
$cssPath = Join-Path $OutputFolder "style.css"
$cssContent | Out-File -FilePath $cssPath -Encoding UTF8
Write-ColorOutput "📄 CSS de estilo creado" "Green"

# Contadores
$converted = 0
$failed = 0

# Procesar cada archivo
Write-ColorOutput "`n📚 Convirtiendo documentos...`n" "Cyan"

foreach ($mapping in $documentMapping.GetEnumerator()) {
    $mdFile = Join-Path $SourceFolder $mapping.Key
    $pdfFile = Join-Path $OutputFolder $mapping.Value
    
    if (Test-Path $mdFile) {
        Write-Host "   📄 $($mapping.Key) -> $($mapping.Value) ... " -NoNewline
        
        try {
            # Usar pandoc para convertir MD a PDF
            # Opción 1: Con wkhtmltopdf (más simple)
            $result = & pandoc $mdFile `
                --from markdown `
                --to html5 `
                --css $cssPath `
                --pdf-engine wkhtmltopdf `
                --pdf-engine-opt "--enable-local-file-access" `
                --metadata title="BITI Solutions - CMS" `
                -o $pdfFile 2>&1
            
            if ($LASTEXITCODE -eq 0 -and (Test-Path $pdfFile)) {
                $fileSize = (Get-Item $pdfFile).Length
                $fileSizeKB = [math]::Round($fileSize / 1KB, 1)
                Write-ColorOutput "✅ ($fileSizeKB KB)" "Green"
                $converted++
            }
            else {
                # Intentar alternativa con LaTeX si falla wkhtmltopdf
                Write-Host "⚠️ " -NoNewline -ForegroundColor Yellow
                
                $result = & pandoc $mdFile `
                    --from markdown `
                    --to pdf `
                    -V geometry:margin=1in `
                    -V fontsize=11pt `
                    -o $pdfFile 2>&1
                
                if ($LASTEXITCODE -eq 0 -and (Test-Path $pdfFile)) {
                    $fileSize = (Get-Item $pdfFile).Length
                    $fileSizeKB = [math]::Round($fileSize / 1KB, 1)
                    Write-ColorOutput "✅ (LaTeX, $fileSizeKB KB)" "Green"
                    $converted++
                }
                else {
                    Write-ColorOutput "❌ Error" "Red"
                    Write-ColorOutput "      $result" "DarkGray"
                    $failed++
                }
            }
        }
        catch {
            Write-ColorOutput "❌ $($_.Exception.Message)" "Red"
            $failed++
        }
    }
    else {
        Write-Host "   📄 $($mapping.Key) ... " -NoNewline
        Write-ColorOutput "⏭️ No encontrado" "Yellow"
    }
}

# Limpiar CSS temporal
# Remove-Item $cssPath -ErrorAction SilentlyContinue

# Resumen
Write-ColorOutput "`n========================================" "Cyan"
Write-ColorOutput "  RESUMEN" "Cyan"
Write-ColorOutput "========================================" "Cyan"
Write-ColorOutput "  ✅ Convertidos: $converted" "Green"
if ($failed -gt 0) {
    Write-ColorOutput "  ❌ Fallidos:    $failed" "Red"
}
Write-ColorOutput "  📁 Ubicación:   $OutputFolder" "Cyan"
Write-ColorOutput "========================================`n" "Cyan"

# Listar archivos PDF generados
Write-ColorOutput "📋 Archivos PDF generados:" "Cyan"
Get-ChildItem -Path $OutputFolder -Filter "*.pdf" | ForEach-Object {
    $sizeKB = [math]::Round($_.Length / 1KB, 1)
    Write-ColorOutput "   - $($_.Name) ($sizeKB KB)" "White"
}

# Abrir carpeta si se solicitó
if ($OpenOutputFolder) {
    Start-Process $OutputFolder
}

Write-ColorOutput "`n✅ Proceso completado!" "Green"
Write-ColorOutput "`nPara cargar los PDFs al sistema, use el endpoint API:" "Yellow"
Write-ColorOutput "POST /api/documentation/{id}/upload" "Yellow"
Write-ColorOutput "`nO ejecute el script SQL:" "Yellow"
Write-ColorOutput "CMS.Data/Scripts/021_upload_documentation_pdfs.sql`n" "Yellow"
