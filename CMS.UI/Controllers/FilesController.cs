// ================================================================================
// ARCHIVO: CMS.UI/Controllers/FilesController.cs
// PROPÓSITO: Controlador UI para gestión de archivos
// AUTOR: EAMR - BITI Solutions S.A
// FECHA: Marzo 2026
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.UI.Services;
using System.Net.Http.Headers;

namespace CMS.UI.Controllers
{
    /// <summary>
    /// Controlador para el módulo de gestión de archivos.
    /// Consume el API de archivos y presenta las vistas.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FilesController(
            ILogger<FilesController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Vista principal: File Master (explorador de archivos)
        /// GET: /Files/Master
        /// </summary>
        [HttpGet("Master")]
        public IActionResult Master()
        {
            ViewData["Title"] = "File Master";
            ViewData["Subtitle"] = "Explorador de archivos";

            // Pasar URL del API al JavaScript
            var environment = _configuration["Environment"] ?? "Development";
            var apiBaseUrl = _configuration[$"ApiSettings:{environment}:BaseUrl"] ?? "https://localhost:7001";
            ViewBag.ApiBaseUrl = apiBaseUrl;

            return View();
        }

        /// <summary>
        /// Subir archivo al API
        /// POST: /Files/Upload
        /// </summary>
        [HttpPost("Upload")]
        [RequestSizeLimit(100_000_000)] // 100MB
        public async Task<IActionResult> Upload(IFormFile file, string? title = null, int? folderId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No se proporcionó archivo" });
                }

                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Preparar FormData para enviar al API
                using var content = new MultipartFormDataContent();

                // Agregar el archivo
                using var fileStream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileContent = new ByteArrayContent(memoryStream.ToArray());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                content.Add(fileContent, "file", file.FileName);

                // Agregar metadatos opcionales
                content.Add(new StringContent(title ?? file.FileName), "title");
                if (folderId.HasValue)
                {
                    content.Add(new StringContent(folderId.Value.ToString()), "folderId");
                }

                // Enviar al API
                var response = await client.PostAsync("/api/file/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📁 Archivo subido exitosamente: {FileName}", file.FileName);
                    return Ok(new { success = true, message = "Archivo subido exitosamente", data = result });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Error subiendo archivo: {StatusCode} - {Error}", response.StatusCode, error);
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error subiendo archivo", error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error subiendo archivo: {FileName}", file?.FileName);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear carpeta
        /// POST: /Files/CreateFolder
        /// </summary>
        [HttpPost("CreateFolder")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Name))
                {
                    return BadRequest(new { success = false, message = "El nombre de la carpeta es requerido" });
                }

                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Generar código único para la carpeta
                var folderCode = $"FOLDER-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

                var payload = new
                {
                    code = folderCode,
                    name = request.Name,
                    description = request.Description,
                    parentId = request.ParentId,
                    icon = "bi-folder",
                    color = "#fbbf24"
                };

                // Enviar al API
                var response = await client.PostAsJsonAsync("/api/file/folders", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📁 Carpeta creada exitosamente: {Name}", request.Name);
                    return Ok(new { success = true, message = "Carpeta creada exitosamente", data = result });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Error creando carpeta: {StatusCode} - {Error}", response.StatusCode, error);
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error creando carpeta", error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creando carpeta: {Name}", request?.Name);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Vista: Mis Archivos (archivos del usuario actual)
        /// GET: /Files/MyFiles
        /// </summary>
        [HttpGet]
        public IActionResult MyFiles()
        {
            ViewData["Title"] = "Mis Archivos";
            ViewData["Subtitle"] = "Archivos creados por mí";
            return View();
        }

        /// <summary>
        /// Vista: Compartidos Conmigo
        /// GET: /Files/SharedWithMe
        /// </summary>
        [HttpGet]
        public IActionResult SharedWithMe()
        {
            ViewData["Title"] = "Compartidos Conmigo";
            ViewData["Subtitle"] = "Archivos que otros usuarios han compartido conmigo";
            return View();
        }

        /// <summary>
        /// Vista: Papelera
        /// GET: /Files/Trash
        /// </summary>
        [HttpGet]
        public IActionResult Trash()
        {
            ViewData["Title"] = "Papelera";
            ViewData["Subtitle"] = "Archivos eliminados (se borran permanentemente después de 30 días)";
            return View();
        }

        /// <summary>
        /// Vista: Categorías (Administración)
        /// GET: /Files/Categories
        /// </summary>
        [HttpGet]
        public IActionResult Categories()
        {
            ViewData["Title"] = "Categorías de Archivos";
            ViewData["Subtitle"] = "Administrar categorías globales";
            return View();
        }

        /// <summary>
        /// Vista principal del módulo
        /// GET: /Files
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Master));
        }

        /// <summary>
        /// Obtener contenido de una carpeta (carpetas y archivos)
        /// GET: /Files/GetContents?folderId=null&extension=pdf,docx&isFavorite=true&orderBy=created_at&descending=true
        /// </summary>
        [HttpGet("GetContents")]
        public async Task<IActionResult> GetContents(
            int? folderId = null, 
            string? extension = null,
            bool? isFavorite = null,
            string? orderBy = null,
            bool descending = false)
        {
            try
            {
                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Obtener carpetas
                var foldersUrl = folderId.HasValue 
                    ? $"/api/file/folders?parentId={folderId.Value}" 
                    : "/api/file/folders";
                var foldersResponse = await client.GetAsync(foldersUrl);
                var foldersData = foldersResponse.IsSuccessStatusCode 
                    ? await foldersResponse.Content.ReadAsStringAsync() 
                    : "[]";

                // Construir URL con filtros para archivos
                var filesQueryParams = new List<string>();

                if (folderId.HasValue)
                    filesQueryParams.Add($"folderId={folderId.Value}");

                if (!string.IsNullOrEmpty(extension))
                    filesQueryParams.Add($"extension={Uri.EscapeDataString(extension)}");

                if (isFavorite.HasValue)
                    filesQueryParams.Add($"isFavorite={isFavorite.Value.ToString().ToLower()}");

                if (!string.IsNullOrEmpty(orderBy))
                    filesQueryParams.Add($"orderBy={Uri.EscapeDataString(orderBy)}");

                if (descending)
                    filesQueryParams.Add("descending=true");

                var filesUrl = filesQueryParams.Count > 0 
                    ? $"/api/file?{string.Join("&", filesQueryParams)}" 
                    : "/api/file";

                var filesResponse = await client.GetAsync(filesUrl);
                var filesData = filesResponse.IsSuccessStatusCode 
                    ? await filesResponse.Content.ReadAsStringAsync() 
                    : "{\"files\":[],\"totalCount\":0}";

                _logger.LogInformation("📂 Contenido cargado: FolderId={FolderId}, Extension={Extension}, IsFavorite={IsFavorite}", 
                    folderId, extension, isFavorite);

                return Ok(new { 
                    success = true, 
                    folders = System.Text.Json.JsonDocument.Parse(foldersData).RootElement,
                    files = System.Text.Json.JsonDocument.Parse(filesData).RootElement
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo contenido de carpeta: {FolderId}", folderId);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Buscar archivos (recursivo desde una carpeta o global)
        /// GET: /Files/SearchFiles?search=xxx&folderId=null
        /// </summary>
        [HttpGet("SearchFiles")]
        public async Task<IActionResult> SearchFiles(string search, int? folderId = null)
        {
            try
            {
                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Buscar carpetas que coincidan con el término
                var allFoldersResponse = await client.GetAsync("/api/file/folders");
                var allFoldersData = allFoldersResponse.IsSuccessStatusCode 
                    ? await allFoldersResponse.Content.ReadAsStringAsync() 
                    : "[]";

                // Filtrar carpetas por nombre (lado cliente ya que la API no tiene búsqueda de carpetas)
                var allFolders = System.Text.Json.JsonDocument.Parse(allFoldersData).RootElement;
                var matchingFolders = new List<System.Text.Json.JsonElement>();

                foreach (var folder in allFolders.EnumerateArray())
                {
                    var name = folder.GetProperty("name").GetString() ?? "";
                    var description = folder.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "";

                    if (name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingFolders.Add(folder);
                    }
                }

                // Buscar archivos usando el parámetro search de la API
                var filesUrl = $"/api/file?search={Uri.EscapeDataString(search)}&pageSize=100";
                var filesResponse = await client.GetAsync(filesUrl);
                var filesData = filesResponse.IsSuccessStatusCode 
                    ? await filesResponse.Content.ReadAsStringAsync() 
                    : "{\"files\":[],\"totalCount\":0}";

                _logger.LogInformation("🔍 Búsqueda '{Search}': encontrados", search);

                return Ok(new { 
                    success = true, 
                    folders = matchingFolders,
                    files = System.Text.Json.JsonDocument.Parse(filesData).RootElement
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error buscando archivos: {Search}", search);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Descargar un archivo
        /// GET: /Files/Download/{id}
        /// </summary>
        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return RedirectToAction("SelectCompany", "Account");
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Obtener el archivo del API
                var response = await client.GetAsync($"/api/file/{id}/download");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"file_{id}";

                    _logger.LogInformation("📥 Archivo descargado: {FileName}", fileName);
                    return File(content, contentType, fileName);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { message = "Archivo no encontrado" });
                }
                else
                {
                    _logger.LogWarning("⚠️ Error descargando archivo: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, new { message = "Error descargando archivo" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error descargando archivo: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Mover una carpeta (drag-and-drop)
        /// POST: /Files/MoveFolder
        /// </summary>
        [HttpPost("MoveFolder")]
        public async Task<IActionResult> MoveFolder([FromBody] MoveFolderRequest request)
        {
            try
            {
                if (request.FolderId <= 0)
                {
                    return BadRequest(new { success = false, message = "ID de carpeta inválido" });
                }

                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Enviar al API
                var payload = new { newParentId = request.NewParentId };
                var response = await client.PutAsJsonAsync($"/api/file/folders/{request.FolderId}/move", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📁 Carpeta {FolderId} movida a parentId {NewParentId}", 
                        request.FolderId, request.NewParentId);
                    return Ok(new { success = true, message = "Carpeta movida exitosamente", data = result });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Error moviendo carpeta: {StatusCode} - {Error}", response.StatusCode, error);
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error moviendo carpeta", error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error moviendo carpeta: {FolderId}", request?.FolderId);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar un archivo (soft delete)
        /// DELETE: /Files/Delete/{id}
        /// </summary>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Enviar al API
                var response = await client.DeleteAsync($"/api/file/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("🗑️ Archivo {Id} eliminado exitosamente", id);
                    return Ok(new { success = true, message = "Archivo eliminado exitosamente" });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ Error eliminando archivo: {StatusCode} - {Error}", response.StatusCode, error);
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error eliminando archivo", error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error eliminando archivo: {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar una carpeta (soft delete)
        /// DELETE: /Files/DeleteFolder/{id}
        /// </summary>
        [HttpDelete("DeleteFolder/{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            try
            {
                // Obtener token JWT de la sesión
                var apiToken = HttpContext.Session.GetString("ApiToken");
                if (string.IsNullOrEmpty(apiToken))
                {
                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                }

                // Crear cliente HTTP autenticado
                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                // Enviar al API
                var response = await client.DeleteAsync($"/api/file/folders/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("🗑️ Carpeta {Id} eliminada exitosamente", id);
                    return Ok(new { success = true, message = "Carpeta eliminada exitosamente" });
                }
                else
                {
                                var error = await response.Content.ReadAsStringAsync();
                                    _logger.LogWarning("⚠️ Error eliminando carpeta: {StatusCode} - {Error}", response.StatusCode, error);
                                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error eliminando carpeta", error });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Error eliminando carpeta: {Id}", id);
                                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
                            }
                        }

                        /// <summary>
                        /// Renombrar un archivo
                        /// PUT: /Files/Rename/{id}
                        /// </summary>
                        [HttpPut("Rename/{id}")]
                        public async Task<IActionResult> Rename(int id, [FromBody] RenameFileRequest request)
                        {
                            try
                            {
                                if (string.IsNullOrWhiteSpace(request?.NewName))
                                {
                                    return BadRequest(new { success = false, message = "El nuevo nombre es requerido" });
                                }

                                var apiToken = HttpContext.Session.GetString("ApiToken");
                                if (string.IsNullOrEmpty(apiToken))
                                {
                                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                                }

                                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                                var payload = new { title = request.NewName };
                                var response = await client.PutAsJsonAsync($"/api/file/{id}", payload);

                                if (response.IsSuccessStatusCode)
                                {
                                    _logger.LogInformation("✏️ Archivo {Id} renombrado a: {NewName}", id, request.NewName);
                                    return Ok(new { success = true, message = "Archivo renombrado exitosamente" });
                                }
                                else
                                {
                                    var error = await response.Content.ReadAsStringAsync();
                                    _logger.LogWarning("⚠️ Error renombrando archivo: {StatusCode}", response.StatusCode);
                                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error renombrando archivo", error });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Error renombrando archivo: {Id}", id);
                                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
                            }
                        }

                        /// <summary>
                        /// Mover un archivo a otra carpeta
                        /// PUT: /Files/MoveFile/{id}
                        /// </summary>
                        [HttpPut("MoveFile/{id}")]
                        public async Task<IActionResult> MoveFile(int id, [FromBody] MoveFileRequest request)
                        {
                            try
                            {
                                var apiToken = HttpContext.Session.GetString("ApiToken");
                                if (string.IsNullOrEmpty(apiToken))
                                {
                                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                                }

                                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                                var payload = new { folderId = request.NewFolderId };
                                var response = await client.PutAsJsonAsync($"/api/file/{id}/move", payload);

                                if (response.IsSuccessStatusCode)
                                {
                                    _logger.LogInformation("📦 Archivo {Id} movido a carpeta: {FolderId}", id, request.NewFolderId);
                                    return Ok(new { success = true, message = "Archivo movido exitosamente" });
                                }
                                else
                                {
                                    var error = await response.Content.ReadAsStringAsync();
                                    _logger.LogWarning("⚠️ Error moviendo archivo: {StatusCode}", response.StatusCode);
                                    return StatusCode((int)response.StatusCode, new { success = false, message = "Error moviendo archivo", error });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Error moviendo archivo: {Id}", id);
                                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
                            }
                        }

                        /// <summary>
                        /// Vista previa de archivo (streaming para imágenes/PDF)
                        /// GET: /Files/Preview/{id}
                        /// </summary>
                        [HttpGet("Preview/{id}")]
                        public async Task<IActionResult> Preview(int id)
                        {
                            try
                            {
                                var apiToken = HttpContext.Session.GetString("ApiToken");
                                if (string.IsNullOrEmpty(apiToken))
                                {
                                    return RedirectToAction("SelectCompany", "Account");
                                }

                                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                                var response = await client.GetAsync($"/api/file/{id}/download");

                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsByteArrayAsync();
                                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                                    // Para vista previa, no forzar descarga (inline disposition)
                                    _logger.LogInformation("👁️ Vista previa de archivo: {Id}", id);
                                    return File(content, contentType);
                                }
                                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                                {
                                    return NotFound(new { message = "Archivo no encontrado" });
                                }
                                else
                                {
                                    _logger.LogWarning("⚠️ Error obteniendo preview: {StatusCode}", response.StatusCode);
                                    return StatusCode((int)response.StatusCode, new { message = "Error obteniendo vista previa" });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ Error obteniendo preview: {Id}", id);
                                return StatusCode(500, new { message = "Error interno del servidor" });
                            }
                        }

                        /// <summary>
                        /// Renombrar una carpeta
                        /// PUT: /Files/RenameFolder/{id}
                        /// </summary>
                        [HttpPut("RenameFolder/{id}")]
                        public async Task<IActionResult> RenameFolder(int id, [FromBody] RenameFolderRequest request)
                        {
                            try
                            {
                                if (string.IsNullOrWhiteSpace(request?.NewName))
                                {
                                    return BadRequest(new { success = false, message = "El nuevo nombre es requerido" });
                                }

                                var apiToken = HttpContext.Session.GetString("ApiToken");
                                if (string.IsNullOrEmpty(apiToken))
                                {
                                    return Unauthorized(new { success = false, message = "Sesión expirada" });
                                }

                                var client = _httpClientFactory.CreateClient("cmsapi-authenticated");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                                var payload = new { name = request.NewName };
                                var response = await client.PutAsJsonAsync($"/api/file/folders/{id}", payload);

                                if (response.IsSuccessStatusCode)
                                {
                                                        _logger.LogInformation("✏️ Carpeta {Id} renombrada a: {NewName}", id, request.NewName);
                                                        return Ok(new { success = true, message = "Carpeta renombrada exitosamente" });
                                                    }
                                                    else
                                                    {
                                                        var error = await response.Content.ReadAsStringAsync();
                                                        _logger.LogWarning("⚠️ Error renombrando carpeta: {StatusCode}", response.StatusCode);
                                                        return StatusCode((int)response.StatusCode, new { success = false, message = "Error renombrando carpeta", error });
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError(ex, "❌ Error renombrando carpeta: {Id}", id);
                                                    return StatusCode(500, new { success = false, message = "Error interno del servidor" });
                                                }
                                            }
                                        }

                                        /// <summary>
                                        /// Request para crear carpeta
                                        /// </summary>
                                        public class CreateFolderRequest
                                        {
                                            public string Name { get; set; } = string.Empty;
                                            public string? Description { get; set; }
                                            public int? ParentId { get; set; }
                                        }

                                        /// <summary>
                                        /// Request para mover carpeta (drag-and-drop)
                                        /// </summary>
                                        public class MoveFolderRequest
                                        {
                                            public int FolderId { get; set; }
                                            public int? NewParentId { get; set; }
                                        }

                                        /// <summary>
                                        /// Request para renombrar archivo
                                        /// </summary>
                                        public class RenameFileRequest
                                        {
                                            public string NewName { get; set; } = string.Empty;
                                        }

                                        /// <summary>
                                        /// Request para mover archivo
                                        /// </summary>
                                        public class MoveFileRequest
                                        {
                                            public int? NewFolderId { get; set; }
                                        }

                                        /// <summary>
                                        /// Request para renombrar carpeta
                                        /// </summary>
                                        public class RenameFolderRequest
                                        {
                                            public string NewName { get; set; } = string.Empty;
                                        }
                                    }
