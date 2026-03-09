# ============================================================================
# Script: Generate-SQL-From-PDFs.ps1
# Descripción: Genera un script SQL con los PDFs codificados en Base64/Hex
#              para insertar directamente en PostgreSQL
# Autor: EAMR - BITI Solutions S.A
# Fecha: Marzo 2026
#
# USO:
#   .\Generate-SQL-From-PDFs.ps1 -PdfFolder "PDFs" -OutputFile "insert_pdfs.sql"
#
# Luego ejecuta el SQL generado en tu cliente PostgreSQL (DBeaver, pgAdmin, psql)
# ============================================================================

param(
    [string]$PdfFolder = "CMS.Documentation/PDFs",
    [string]$OutputFile = "CMS.Data/Scripts/022_insert_documentation_pdfs.sql"
)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  PDF to SQL Generator" -ForegroundColor Cyan
Write-Host "  BITI Solutions S.A." -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Mapeo de archivos PDF a document_code
$documentMapping = @{
    "CMS_Manual_General.pdf" = "DOC-GEN-001"
    "CMS_Guia_Inicio_Rapido.pdf" = "DOC-GEN-002"
    "CMS_Seguridad_Permisos.pdf" = "DOC-GEN-003"
    "CMS_Modulo_Inventario.pdf" = "MOD-INV-001"
    "CMS_Modulo_Reportes.pdf" = "MOD-REP-001"
    "CMS_Modulo_Administracion.pdf" = "MOD-ADM-001"
    "CMS_Modulo_CRM.pdf" = "MOD-CRM-001"
    "CMS_Modulo_Ventas.pdf" = "MOD-VEN-001"
    "CMS_Modulo_Compras.pdf" = "MOD-COM-001"
    "CMS_Modulo_POS.pdf" = "MOD-POS-001"
    "CMS_Modulo_Contabilidad.pdf" = "MOD-CON-001"
    "CMS_Modulo_Finanzas.pdf" = "MOD-FIN-001"
    "CMS_Modulo_RRHH.pdf" = "MOD-RRH-001"
    "CMS_Modulo_Proyectos.pdf" = "MOD-PRO-001"
    "CMS_Modulo_Facturacion_Electronica.pdf" = "MOD-FAC-001"
    "CMS_Tutorial_Crear_Usuario.pdf" = "TUT-001"
    "CMS_Tutorial_Roles_Permisos.pdf" = "TUT-002"
    "CMS_Tutorial_Reportes.pdf" = "TUT-003"
    "CMS_FAQ.pdf" = "FAQ-001"
}

# Verificar carpeta
if (-not (Test-Path $PdfFolder)) {
    Write-Host "? ERROR: Carpeta no encontrada: $PdfFolder" -ForegroundColor Red
    Write-Host "   Ejecute primero: .\Convert-Documentation-To-PDF.ps1" -ForegroundColor Yellow
    exit 1
}

# Iniciar contenido SQL
$sqlContent = @"
-- ============================================================================
-- Script: 022_insert_documentation_pdfs.sql
-- Descripción: Inserta los archivos PDF de documentación en la BD
-- Generado: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- Autor: Generado automáticamente por Generate-SQL-From-PDFs.ps1
-- ============================================================================

-- NOTA: Este script contiene los PDFs codificados en formato hexadecimal
-- Ejecutar en PostgreSQL usando psql, DBeaver, pgAdmin o similar

BEGIN;

"@

$processedCount = 0
$totalSize = 0

Write-Host "?? Procesando PDFs...`n" -ForegroundColor Cyan

foreach ($mapping in $documentMapping.GetEnumerator()) {
    $pdfFile = Join-Path $PdfFolder $mapping.Key
    $documentCode = $mapping.Value
    
    Write-Host "   ?? $($mapping.Key) ... " -NoNewline
    
    if (Test-Path $pdfFile) {
        try {
            # Leer archivo como bytes
            $fileBytes = [System.IO.File]::ReadAllBytes($pdfFile)
            $fileSize = $fileBytes.Length
            $totalSize += $fileSize
            
            # Convertir a hexadecimal para PostgreSQL (formato \x...)
            $hexString = [BitConverter]::ToString($fileBytes) -replace '-', ''
            
            # Agregar al SQL
            $sqlContent += @"

-- Documento: $($mapping.Key) -> $documentCode
UPDATE admin.system_documentation 
SET 
    file_data = '\x$hexString'::bytea,
    file_size_bytes = $fileSize,
    record_date = NOW(),
    updated_by = 'SYSTEM'
WHERE document_code = '$documentCode';

"@
            
            $fileSizeKB = [math]::Round($fileSize / 1KB, 1)
            Write-Host "? ($fileSizeKB KB)" -ForegroundColor Green
            $processedCount++
        }
        catch {
            Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "?? No encontrado" -ForegroundColor Yellow
    }
}

# Agregar verificación al final
$sqlContent += @"

-- Verificar resultado
SELECT 
    document_code,
    document_title,
    file_name,
    CASE 
        WHEN file_data IS NOT NULL AND octet_length(file_data) > 0 
        THEN '? Cargado (' || pg_size_pretty(file_size_bytes::bigint) || ')'
        ELSE '? Pendiente'
    END AS estado
FROM admin.system_documentation
ORDER BY sort_order;

COMMIT;

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================
"@

# Guardar archivo SQL
$sqlContent | Out-File -FilePath $OutputFile -Encoding UTF8
$sqlFileSizeMB = [math]::Round((Get-Item $OutputFile).Length / 1MB, 2)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? PDFs procesados: $processedCount" -ForegroundColor Green
Write-Host "  ?? Tamaño total: $([math]::Round($totalSize / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host "  ?? Archivo SQL: $OutputFile ($sqlFileSizeMB MB)" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "? Para cargar los PDFs, ejecute el SQL generado:" -ForegroundColor Yellow
Write-Host "   psql -h localhost -U cmssystem -d cms -f `"$OutputFile`"" -ForegroundColor White
Write-Host "`n   O abra el archivo en DBeaver/pgAdmin y ejecútelo.`n" -ForegroundColor White
