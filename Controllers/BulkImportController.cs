using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Helpers;

namespace sistema_vacaciones_back.Controllers
{
    /// <summary>
    /// Controlador para la gestión de bulk import de usuarios.
    /// Maneja la carga masiva de usuarios desde archivos Excel/CSV.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BulkImportController : ControllerBase
    {
        private readonly IBulkImportService _bulkImportService;
        private readonly IBulkImportFileProcessor _fileProcessor;
        private readonly ILogger<BulkImportController> _logger;

        public BulkImportController(
            IBulkImportService bulkImportService,
            IBulkImportFileProcessor fileProcessor,
            ILogger<BulkImportController> logger)
        {
            _bulkImportService = bulkImportService ?? throw new ArgumentNullException(nameof(bulkImportService));
            _fileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Procesa un archivo de bulk import de usuarios.
        /// POST /api/bulkimport/process
        /// </summary>
        /// <param name="file">Archivo Excel o CSV con los datos de usuarios</param>
        /// <param name="configuracion">Configuración del bulk import (JSON)</param>
        /// <returns>Resultado detallado del procesamiento</returns>
        [HttpPost("process")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<BulkImportResultDto>> ProcessBulkImport(
            IFormFile file,
            [FromForm] string? configuracion = null)
        {
            try
            {
                // Validaciones básicas
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Debe proporcionar un archivo válido", error = "FILE_REQUIRED" });
                }

                // Validar constraints del archivo
                var constraintError = _fileProcessor.ValidateFileConstraints(file.Length, file.FileName);
                if (constraintError != null)
                {
                    return BadRequest(new { message = constraintError, error = "FILE_CONSTRAINT_ERROR" });
                }

                // Obtener información del usuario autenticado
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = "USER_NOT_AUTHENTICATED" });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Iniciando bulk import. Usuario: {AdminId}, Archivo: {FileName}, Tamaño: {FileSize}", 
                    adminId, file.FileName, file.Length);

                // Procesar archivo y extraer usuarios
                using var stream = file.OpenReadStream();
                var (usuarios, metadata) = await ProcessFileAsync(stream, file.FileName);

                if (!metadata.EsExitoso)
                {
                    return BadRequest(new 
                    { 
                        message = "Error al procesar el archivo", 
                        errores = metadata.ErroresParsing,
                        metadata = metadata,
                        error = "FILE_PROCESSING_ERROR" 
                    });
                }

                if (usuarios.Count == 0)
                {
                    return BadRequest(new 
                    { 
                        message = "El archivo no contiene usuarios válidos para procesar",
                        metadata = metadata,
                        error = "NO_VALID_USERS" 
                    });
                }

                // Parsear configuración
                var config = ParseConfiguration(configuracion);

                // Crear request para el servicio
                var request = new BulkImportRequestDto
                {
                    Usuarios = usuarios,
                    Configuracion = config,
                    Metadata = new BulkImportMetadataDto
                    {
                        NombreArchivo = file.FileName,
                        TipoArchivo = metadata.TipoArchivo,
                        TamanoArchivo = file.Length,
                        FechaProcesamiento = DateTime.UtcNow,
                        TotalFilas = metadata.TotalFilas,
                        FilasConDatos = metadata.FilasConDatos,
                        FilasOmitidas = metadata.FilasOmitidas,
                        ErroresParsing = metadata.ErroresParsing,
                        Advertencias = metadata.Advertencias,
                        TiempoProcesamiento = metadata.TiempoProcesamiento,
                        ColumnasEncontradas = metadata.ColumnasEncontradas
                    }
                };

                // Procesar bulk import
                var resultado = await _bulkImportService.ProcessBulkImportAsync(request, adminId, ipAddress, userAgent);

                _logger.LogInformation("Bulk import completado. Usuarios creados: {UsuariosCreados}, Errores: {Errores}", 
                    resultado.UsuariosCreados.Count, resultado.RegistrosFallidos.Count);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante bulk import. Archivo: {FileName}", file?.FileName ?? "unknown");
                return StatusCode(500, new 
                { 
                    message = "Error interno del servidor durante el bulk import", 
                    error = "INTERNAL_SERVER_ERROR",
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Valida un archivo de bulk import sin procesarlo.
        /// POST /api/bulkimport/validate
        /// </summary>
        /// <param name="file">Archivo Excel o CSV a validar</param>
        /// <returns>Resultado de la validación con errores encontrados</returns>
        [HttpPost("validate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<object>> ValidateBulkImportFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Debe proporcionar un archivo válido", error = "FILE_REQUIRED" });
                }

                // Validar constraints del archivo
                var constraintError = _fileProcessor.ValidateFileConstraints(file.Length, file.FileName);
                if (constraintError != null)
                {
                    return BadRequest(new { message = constraintError, error = "FILE_CONSTRAINT_ERROR" });
                }

                _logger.LogInformation("Validando archivo de bulk import: {FileName}", file.FileName);

                // Procesar archivo para validación
                using var stream = file.OpenReadStream();
                var (usuarios, metadata) = await ProcessFileAsync(stream, file.FileName);

                // Validación adicional con el servicio
                var request = new BulkImportRequestDto
                {
                    Usuarios = usuarios,
                    Configuracion = new BulkImportConfiguracionDto(), // Configuración por defecto para validación
                    Metadata = new BulkImportMetadataDto
                    {
                        NombreArchivo = file.FileName,
                        TipoArchivo = metadata.TipoArchivo,
                        TamanoArchivo = file.Length
                    }
                };

                var erroresValidacion = await _bulkImportService.ValidateRequestAsync(request);

                var resultado = new
                {
                    esValido = metadata.EsExitoso && erroresValidacion.Count == 0,
                    metadata = metadata,
                    usuariosEncontrados = usuarios.Count,
                    erroresArchivo = metadata.ErroresParsing,
                    erroresValidacion = erroresValidacion,
                    advertencias = metadata.Advertencias,
                    columnasEncontradas = metadata.ColumnasEncontradas,
                    columnasEsperadas = _fileProcessor.GetExpectedColumns()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar archivo de bulk import: {FileName}", file?.FileName ?? "unknown");
                return StatusCode(500, new 
                { 
                    message = "Error interno del servidor durante la validación", 
                    error = "VALIDATION_ERROR",
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Descarga la plantilla Excel para bulk import.
        /// GET /api/bulkimport/template/excel
        /// </summary>
        /// <returns>Archivo Excel con la plantilla</returns>
        [HttpGet("template/excel")]
        public ActionResult DownloadExcelTemplate()
        {
            try
            {
                _logger.LogInformation("Generando plantilla Excel para bulk import");

                var template = _fileProcessor.GenerateExcelTemplate();
                var fileName = $"Plantilla_BulkImport_Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(template.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar plantilla Excel");
                return StatusCode(500, new 
                { 
                    message = "Error al generar la plantilla Excel", 
                    error = "TEMPLATE_GENERATION_ERROR" 
                });
            }
        }

        /// <summary>
        /// Descarga la plantilla CSV para bulk import.
        /// GET /api/bulkimport/template/csv
        /// </summary>
        /// <returns>Archivo CSV con la plantilla</returns>
        [HttpGet("template/csv")]
        public ActionResult DownloadCsvTemplate()
        {
            try
            {
                _logger.LogInformation("Generando plantilla CSV para bulk import");

                var template = _fileProcessor.GenerateCsvTemplate();
                var fileName = $"Plantilla_BulkImport_Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(template.ToArray(), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar plantilla CSV");
                return StatusCode(500, new 
                { 
                    message = "Error al generar la plantilla CSV", 
                    error = "TEMPLATE_GENERATION_ERROR" 
                });
            }
        }

        /// <summary>
        /// Obtiene las columnas esperadas para el bulk import.
        /// GET /api/bulkimport/columns
        /// </summary>
        /// <returns>Lista de columnas esperadas</returns>
        [HttpGet("columns")]
        public ActionResult<object> GetExpectedColumns()
        {
            try
            {
                var columnas = _fileProcessor.GetExpectedColumns();
                return Ok(new 
                { 
                    columnas = columnas,
                    total = columnas.Count,
                    descripcion = "Columnas requeridas para el bulk import de usuarios",
                    formato = "Las columnas deben aparecer en este orden exacto en el archivo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener columnas esperadas");
                return StatusCode(500, new 
                { 
                    message = "Error al obtener las columnas esperadas", 
                    error = "COLUMNS_ERROR" 
                });
            }
        }

        #region Métodos Privados

        /// <summary>
        /// Procesa un archivo y retorna usuarios y metadata.
        /// </summary>
        private async Task<(List<BulkImportUsuarioDto> usuarios, BulkImportMetadataDto metadata)> ProcessFileAsync(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            
            return extension switch
            {
                ".xlsx" => await _fileProcessor.ProcessExcelFileAsync(stream, fileName),
                ".csv" => await _fileProcessor.ProcessCsvFileAsync(stream, fileName),
                _ => throw new NotSupportedException($"Tipo de archivo no soportado: {extension}")
            };
        }

        /// <summary>
        /// Parsea la configuración del bulk import desde JSON.
        /// </summary>
        private BulkImportConfiguracionDto ParseConfiguration(string? configuracionJson)
        {
            if (string.IsNullOrWhiteSpace(configuracionJson))
            {
                // Configuración por defecto
                return new BulkImportConfiguracionDto
                {
                    DepartamentoPorDefecto = null,
                    RolesPorDefecto = new List<string> { "Empleado" },
                    GenerarPasswordsAutomaticamente = true,
                    ContinuarConErrores = false,
                    EmpresaPorDefecto = null,
                    ValidarJefesExistentes = true,
                    EnviarNotificacionEmail = false
                };
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<BulkImportConfiguracionDto>(configuracionJson) 
                    ?? new BulkImportConfiguracionDto();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al parsear configuración, usando configuración por defecto");
                return new BulkImportConfiguracionDto();
            }
        }

        #endregion
    }
}
