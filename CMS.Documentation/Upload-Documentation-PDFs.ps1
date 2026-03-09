# ============================================================================
# Script: Upload-Documentation-PDFs.ps1
# Descripción: Sube los archivos PDF de documentación al sistema CMS via API
# Autor: EAMR - BITI Solutions S.A
# Fecha: Marzo 2026
#
# USO:
#   .\Upload-Documentation-PDFs.ps1 -ApiBaseUrl "https://localhost:7001" -Token "tu_jwt_token"
#
# REQUISITOS:
# - PowerShell 7+
# - Los PDFs deben existir en CMS.Documentation/PDFs/
# - Un token JWT válido con permisos de administrador
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiBaseUrl,
    
    [Parameter(Mandatory=$true)]
    [string]$Token,
    
    [string]$PdfFolder = "CMS.Documentation/PDFs"
)

# Colores para output
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n========================================" "Cyan"
Write-ColorOutput "  CMS Documentation PDF Uploader" "Cyan"
Write-ColorOutput "  BITI Solutions S.A." "Cyan"
Write-ColorOutput "========================================`n" "Cyan"

# Mapeo de archivos PDF a document_code
$documentMapping = @{
    "CMS_Manual_General.pdf" = @{ code = "DOC-GEN-001"; id = 1 }
    "CMS_Guia_Inicio_Rapido.pdf" = @{ code = "DOC-GEN-002"; id = 2 }
    "CMS_Seguridad_Permisos.pdf" = @{ code = "DOC-GEN-003"; id = 3 }
    "CMS_Modulo_Inventario.pdf" = @{ code = "MOD-INV-001"; id = 4 }
    "CMS_Modulo_Reportes.pdf" = @{ code = "MOD-REP-001"; id = 5 }
    "CMS_Modulo_Administracion.pdf" = @{ code = "MOD-ADM-001"; id = 6 }
    "CMS_Modulo_CRM.pdf" = @{ code = "MOD-CRM-001"; id = 7 }
    "CMS_Modulo_Ventas.pdf" = @{ code = "MOD-VEN-001"; id = 8 }
    "CMS_Modulo_Compras.pdf" = @{ code = "MOD-COM-001"; id = 9 }
    "CMS_Modulo_POS.pdf" = @{ code = "MOD-POS-001"; id = 10 }
    "CMS_Modulo_Contabilidad.pdf" = @{ code = "MOD-CON-001"; id = 11 }
    "CMS_Modulo_Finanzas.pdf" = @{ code = "MOD-FIN-001"; id = 12 }
    "CMS_Modulo_RRHH.pdf" = @{ code = "MOD-RRH-001"; id = 13 }
    "CMS_Modulo_Proyectos.pdf" = @{ code = "MOD-PRO-001"; id = 14 }
    "CMS_Modulo_Facturacion_Electronica.pdf" = @{ code = "MOD-FAC-001"; id = 15 }
    "CMS_Tutorial_Crear_Usuario.pdf" = @{ code = "TUT-001"; id = 16 }
    "CMS_Tutorial_Roles_Permisos.pdf" = @{ code = "TUT-002"; id = 17 }
    "CMS_Tutorial_Reportes.pdf" = @{ code = "TUT-003"; id = 18 }
    "CMS_FAQ.pdf" = @{ code = "FAQ-001"; id = 19 }
}

# Verificar carpeta
if (-not (Test-Path $PdfFolder)) {
    Write-ColorOutput "❌ ERROR: Carpeta no encontrada: $PdfFolder" "Red"
    Write-ColorOutput "   Ejecute primero: .\Convert-Documentation-To-PDF.ps1" "Yellow"
    exit 1
}

# Headers para la API
$headers = @{
    "Authorization" = "Bearer $Token"
}

# Primero, obtener la lista de documentos de la API para obtener los IDs correctos
Write-ColorOutput "📡 Obteniendo lista de documentos del servidor..." "Cyan"

try {
    $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/documentation/list" -Headers $headers -Method Get
    
    # Crear mapeo por document_code
    $serverDocs = @{}
    foreach ($doc in $response) {
        $serverDocs[$doc.documentCode] = $doc.id
    }
    
    Write-ColorOutput "✅ Se encontraron $($response.Count) documentos en el servidor`n" "Green"
}
catch {
    Write-ColorOutput "❌ Error al conectar con la API: $($_.Exception.Message)" "Red"
    Write-ColorOutput "   Verifique la URL y el token" "Yellow"
    exit 1
}

# Contadores
$uploaded = 0
$skipped = 0
$failed = 0

# Procesar cada PDF
Write-ColorOutput "📤 Subiendo PDFs...`n" "Cyan"

foreach ($mapping in $documentMapping.GetEnumerator()) {
    $pdfFile = Join-Path $PdfFolder $mapping.Key
    $documentCode = $mapping.Value.code
    
    Write-Host "   📄 $($mapping.Key) ... " -NoNewline
    
    # Verificar si el archivo existe
    if (-not (Test-Path $pdfFile)) {
        Write-ColorOutput "⏭️ No existe" "Yellow"
        $skipped++
        continue
    }
    
    # Obtener ID del servidor
    if (-not $serverDocs.ContainsKey($documentCode)) {
        Write-ColorOutput "⚠️ Código no encontrado en BD: $documentCode" "Yellow"
        $skipped++
        continue
    }
    
    $documentId = $serverDocs[$documentCode]
    
    try {
        # Preparar el archivo para subir
        $fileBytes = [System.IO.File]::ReadAllBytes($pdfFile)
        $fileName = [System.IO.Path]::GetFileName($pdfFile)
        
        # Crear el contenido multipart
        $boundary = [System.Guid]::NewGuid().ToString()
        $LF = "`r`n"
        
        $bodyLines = @(
            "--$boundary",
            "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
            "Content-Type: application/pdf",
            "",
            [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
            "--$boundary--"
        ) -join $LF
        
        # Usar Invoke-WebRequest para upload multipart
        $uploadUrl = "$ApiBaseUrl/api/documentation/$documentId/upload"
        
        # Alternativa más simple usando -Form (PowerShell 7+)
        $form = @{
            file = Get-Item -Path $pdfFile
        }
        
        $result = Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $headers -Form $form
        
        if ($result.success) {
            $fileSizeKB = [math]::Round($result.fileSize / 1KB, 1)
            Write-ColorOutput "✅ ($fileSizeKB KB)" "Green"
            $uploaded++
        }
        else {
            Write-ColorOutput "❌ $($result.error)" "Red"
            $failed++
        }
    }
    catch {
        Write-ColorOutput "❌ $($_.Exception.Message)" "Red"
        $failed++
    }
}

# Resumen
Write-ColorOutput "`n========================================" "Cyan"
Write-ColorOutput "  RESUMEN" "Cyan"
Write-ColorOutput "========================================" "Cyan"
Write-ColorOutput "  ✅ Subidos:   $uploaded" "Green"
Write-ColorOutput "  ⏭️ Omitidos: $skipped" "Yellow"
if ($failed -gt 0) {
    Write-ColorOutput "  ❌ Fallidos: $failed" "Red"
}
Write-ColorOutput "========================================`n" "Cyan"

Write-ColorOutput "✅ Proceso completado!" "Green"
Write-ColorOutput "`nPuede verificar los documentos en:" "Yellow"
Write-ColorOutput "$ApiBaseUrl/Documentation`n" "Cyan"
