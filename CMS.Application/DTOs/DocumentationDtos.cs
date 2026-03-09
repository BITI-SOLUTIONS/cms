// ================================================================================
// ARCHIVO: CMS.Application/DTOs/DocumentationDtos.cs
// PROPÓSITO: DTOs para documentación del sistema
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-03-08
// ================================================================================

namespace CMS.Application.DTOs
{
    /// <summary>
    /// DTO para listar documentos (sin el contenido binario)
    /// </summary>
    public class DocumentationListDto
    {
        public int Id { get; set; }
        public string DocumentCode { get; set; } = default!;
        public string DocumentTitle { get; set; } = default!;
        public string? DocumentDescription { get; set; }
        public string DocumentCategory { get; set; } = default!;
        public string? ModuleName { get; set; }
        public string FileName { get; set; } = default!;
        public long? FileSizeBytes { get; set; }
        public string Version { get; set; } = default!;
        public bool HasFile { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Tamaño formateado (ej: "2.5 MB")
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
        
        private static string FormatFileSize(long? bytes)
        {
            if (!bytes.HasValue || bytes.Value == 0) return "N/A";
            
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes.Value;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// DTO para detalle de documento (incluye todo excepto file_data)
    /// </summary>
    public class DocumentationDetailDto
    {
        public int Id { get; set; }
        public string DocumentCode { get; set; } = default!;
        public string DocumentTitle { get; set; } = default!;
        public string? DocumentDescription { get; set; }
        public string DocumentCategory { get; set; } = default!;
        public string? ModuleName { get; set; }
        public string FileName { get; set; } = default!;
        public long? FileSizeBytes { get; set; }
        public string ContentType { get; set; } = default!;
        public string Version { get; set; } = default!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public string? RequiredPermission { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; } = default!;
    }

    /// <summary>
    /// DTO para crear/actualizar documento
    /// </summary>
    public class DocumentationCreateDto
    {
        public string DocumentCode { get; set; } = default!;
        public string DocumentTitle { get; set; } = default!;
        public string? DocumentDescription { get; set; }
        public string DocumentCategory { get; set; } = "GENERAL";
        public string? ModuleName { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublic { get; set; } = true;
        public string? RequiredPermission { get; set; }
    }

    /// <summary>
    /// DTO para categoría agrupada
    /// </summary>
    public class DocumentationCategoryDto
    {
        public string CategoryCode { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public string Icon { get; set; } = default!;
        public int DocumentCount { get; set; }
        public List<DocumentationListDto> Documents { get; set; } = new();
    }
}
