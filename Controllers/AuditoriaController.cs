using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.Auditoria;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models.Enums;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    /// <summary>
    /// Controller para la consulta y gestión de auditoría del sistema
    /// Proporciona endpoints para obtener historial de acciones y estadísticas
    /// </summary>
    [ApiController]
    [Route("api/auditoria")]
    [Authorize]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<AuditoriaController> _logger;

        public AuditoriaController(
            IAuditoriaService auditoriaService,
            ILogger<AuditoriaController> logger)
        {
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        #region Endpoints de Consulta General

        /// <summary>
        /// Obtiene registros de auditoría con filtros avanzados y paginación
        /// </summary>
        /// <param name="filtros">Parámetros de filtrado, ordenamiento y paginación</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        /// <response code="200">Registros de auditoría obtenidos exitosamente</response>
        /// <response code="400">Parámetros de consulta inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para acceder a auditoría</response>
        [HttpGet]
        [AdminOnly] // Solo administradores pueden ver auditoría completa
        [ProducesResponseType(typeof(AuditoriaPaginadaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerRegistros([FromQuery] ConsultarAuditoriaDto filtros)
        {
            try
            {
                _logger.LogInformation("Obteniendo registros de auditoría con filtros: {@Filtros}", filtros);

                var resultado = await _auditoriaService.ObtenerRegistrosAsync(filtros);

                _logger.LogInformation("Se obtuvieron {Count} registros de auditoría de un total de {Total}", 
                    resultado.Registros.Count, resultado.TotalRegistros);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registros de auditoría");
                return StatusCode(500, new { message = "Error interno del servidor al obtener registros de auditoría" });
            }
        }

        /// <summary>
        /// Obtiene un registro de auditoría específico por su ID
        /// </summary>
        /// <param name="id">ID del registro de auditoría</param>
        /// <returns>Registro de auditoría</returns>
        /// <response code="200">Registro encontrado</response>
        /// <response code="404">Registro no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para acceder a auditoría</response>
        [HttpGet("{id}")]
        [AdminOnly]
        [ProducesResponseType(typeof(AuditoriaDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de auditoría es requerido" });
                }

                var registro = await _auditoriaService.ObtenerPorIdAsync(id);

                if (registro == null)
                {
                    return NotFound(new { message = "Registro de auditoría no encontrado" });
                }

                return Ok(registro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registro de auditoría {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Endpoints por Módulo

        /// <summary>
        /// Obtiene el historial de auditoría para un módulo específico
        /// </summary>
        /// <param name="modulo">Módulo del sistema (ej: GESTION_USUARIOS)</param>
        /// <param name="pagina">Número de página (base 1)</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Historial de auditoría del módulo</returns>
        /// <response code="200">Historial obtenido exitosamente</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para acceder a auditoría</response>
        [HttpGet("modulo/{modulo}")]
        [AdminOnly]
        [ProducesResponseType(typeof(AuditoriaPaginadaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerHistorialPorModulo(
            ModuloSistema modulo,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                if (pagina < 1)
                    return BadRequest(new { message = "La página debe ser mayor a 0" });

                if (tamanoPagina < 1 || tamanoPagina > 100)
                    return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 100" });

                _logger.LogInformation("Obteniendo historial de auditoría para módulo: {Modulo}, página: {Pagina}", modulo, pagina);

                var resultado = await _auditoriaService.ObtenerHistorialPorModuloAsync(modulo, pagina, tamanoPagina);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial por módulo {Modulo}", modulo);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el historial de auditoría para la gestión de usuarios (para frontend)
        /// </summary>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <param name="usuarioId">Filtrar por usuario específico (opcional)</param>
        /// <param name="tipoAccion">Filtrar por tipo de acción específica (opcional)</param>
        /// <param name="fechaDesde">Filtrar desde fecha (opcional)</param>
        /// <param name="fechaHasta">Filtrar hasta fecha (opcional)</param>
        /// <returns>Historial de auditoría de gestión de usuarios</returns>
        [HttpGet("gestion-usuarios")]
        [AdminOnly]
        [ProducesResponseType(typeof(AuditoriaPaginadaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerHistorialGestionUsuarios(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 20,
            [FromQuery] string? usuarioId = null,
            [FromQuery] TipoAccionAuditoria? tipoAccion = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                if (pagina < 1)
                    return BadRequest(new { message = "La página debe ser mayor a 0" });

                if (tamanoPagina < 1 || tamanoPagina > 100)
                    return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 100" });

                var filtros = new ConsultarAuditoriaDto
                {
                    Modulo = ModuloSistema.GESTION_USUARIOS,
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina,
                    UsuarioAfectadoId = usuarioId,
                    TipoAccion = tipoAccion,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    SoloVisibles = true
                };

                _logger.LogInformation("Obteniendo historial de gestión de usuarios con filtros: {@Filtros}", filtros);

                var resultado = await _auditoriaService.ObtenerRegistrosAsync(filtros);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de gestión de usuarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el historial simplificado para el frontend de gestión de usuarios
        /// Optimizado para mostrar en componentes de UI con datos esenciales
        /// </summary>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <param name="usuarioId">Filtrar por usuario específico (opcional)</param>
        /// <returns>Historial simplificado para UI</returns>
        [HttpGet("gestion-usuarios/simple")]
        [AdminOnly]
        [ProducesResponseType(typeof(AuditoriaSimplePaginadaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerHistorialGestionUsuariosSimple(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 15,
            [FromQuery] string? usuarioId = null)
        {
            try
            {
                if (pagina < 1)
                    return BadRequest(new { message = "La página debe ser mayor a 0" });

                if (tamanoPagina < 1 || tamanoPagina > 50)
                    return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 50" });

                var filtros = new ConsultarAuditoriaDto
                {
                    Modulo = ModuloSistema.GESTION_USUARIOS,
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina,
                    UsuarioAfectadoId = usuarioId,
                    SoloVisibles = true,
                    OrdenarPor = "FechaHora",
                    OrdenDescendente = true
                };

                var resultado = await _auditoriaService.ObtenerRegistrosAsync(filtros);

                // Convertir a formato simplificado para el frontend
                var registrosSimples = resultado.Registros.Select(r => new AuditoriaSimpleDto
                {
                    Id = r.Id,
                    Accion = r.MensajeCorto,
                    UsuarioEjecutor = r.UsuarioEjecutorNombre,
                    UsuarioAfectado = r.UsuarioAfectadoNombre,
                    MensajeCorto = r.MensajeCorto,
                    Motivo = r.Motivo,
                    FechaHora = r.FechaHora,
                    Severidad = r.Severidad.ToString(),
                    IpAddress = r.IpAddress
                }).ToList();

                var resultadoSimple = new AuditoriaSimplePaginadaDto
                {
                    Registros = registrosSimples,
                    TotalRegistros = resultado.TotalRegistros,
                    PaginaActual = resultado.PaginaActual,
                    TotalPaginas = resultado.TotalPaginas,
                    TienePaginaAnterior = resultado.TienePaginaAnterior,
                    TienePaginaSiguiente = resultado.TienePaginaSiguiente
                };

                return Ok(resultadoSimple);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial simplificado de gestión de usuarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Endpoints por Usuario

        /// <summary>
        /// Obtiene el historial de auditoría para un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="comoEjecutor">Si true, busca acciones ejecutadas por el usuario. Si false, acciones realizadas sobre el usuario</param>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Historial de auditoría del usuario</returns>
        [HttpGet("usuario/{usuarioId}")]
        [AdminOnly]
        [ProducesResponseType(typeof(AuditoriaPaginadaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerHistorialPorUsuario(
            string usuarioId,
            [FromQuery] bool comoEjecutor = true,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioId))
                    return BadRequest(new { message = "ID de usuario es requerido" });

                if (pagina < 1)
                    return BadRequest(new { message = "La página debe ser mayor a 0" });

                if (tamanoPagina < 1 || tamanoPagina > 100)
                    return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 100" });

                _logger.LogInformation("Obteniendo historial para usuario: {UsuarioId}, como ejecutor: {ComoEjecutor}", usuarioId, comoEjecutor);

                var resultado = await _auditoriaService.ObtenerHistorialPorUsuarioAsync(usuarioId, comoEjecutor, pagina, tamanoPagina);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial para usuario {UsuarioId}", usuarioId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el historial de auditoría para un registro específico
        /// </summary>
        /// <param name="tabla">Tabla de la base de datos</param>
        /// <param name="registroId">ID del registro</param>
        /// <returns>Lista de acciones realizadas sobre el registro</returns>
        [HttpGet("registro/{tabla}/{registroId}")]
        [AdminOnly]
        [ProducesResponseType(typeof(List<AuditoriaDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerHistorialPorRegistro(string tabla, string registroId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tabla))
                    return BadRequest(new { message = "Nombre de tabla es requerido" });

                if (string.IsNullOrWhiteSpace(registroId))
                    return BadRequest(new { message = "ID de registro es requerido" });

                var resultado = await _auditoriaService.ObtenerHistorialPorRegistroAsync(tabla, registroId);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial para registro {Tabla}.{RegistroId}", tabla, registroId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Endpoints de Estadísticas

        /// <summary>
        /// Obtiene estadísticas generales de auditoría del sistema
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <returns>Estadísticas de auditoría</returns>
        [HttpGet("estadisticas")]
        [AdminOnly]
        [ProducesResponseType(typeof(EstadisticasAuditoriaDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerEstadisticas(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de auditoría desde: {FechaDesde} hasta: {FechaHasta}", fechaDesde, fechaHasta);

                var estadisticas = await _auditoriaService.ObtenerEstadisticasAsync(fechaDesde, fechaHasta);

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de auditoría");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las últimas acciones críticas del sistema
        /// </summary>
        /// <param name="limite">Número máximo de registros a retornar</param>
        /// <returns>Lista de acciones críticas recientes</returns>
        [HttpGet("acciones-criticas")]
        [AdminOnly]
        [ProducesResponseType(typeof(List<AuditoriaDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ObtenerAccionesCriticasRecientes([FromQuery] int limite = 10)
        {
            try
            {
                if (limite < 1 || limite > 50)
                    return BadRequest(new { message = "El límite debe estar entre 1 y 50" });

                var accionesCriticas = await _auditoriaService.ObtenerAccionesCriticasRecientesAsync(limite);

                return Ok(accionesCriticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acciones críticas recientes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Endpoints de Utilidad

        /// <summary>
        /// Limpia registros de auditoría antiguos según política de retención
        /// </summary>
        /// <param name="diasRetencion">Días de retención (por defecto 365)</param>
        /// <returns>Número de registros eliminados</returns>
        [HttpDelete("limpiar-antiguos")]
        [AdminOnly]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> LimpiarRegistrosAntiguos([FromQuery] int diasRetencion = 365)
        {
            try
            {
                if (diasRetencion < 30)
                    return BadRequest(new { message = "Los días de retención deben ser al menos 30" });

                var adminId = User.GetUserId() ?? "Sistema";
                
                _logger.LogWarning("Iniciando limpieza de registros de auditoría antiguos. Días retención: {DiasRetencion}, Admin: {AdminId}", 
                    diasRetencion, adminId);

                var registrosEliminados = await _auditoriaService.LimpiarRegistrosAntiguosAsync(diasRetencion);

                _logger.LogWarning("Limpieza completada. Registros eliminados: {RegistrosEliminados}", registrosEliminados);

                return Ok(new 
                { 
                    message = "Limpieza de registros completada",
                    registrosEliminados,
                    diasRetencion,
                    fechaLimpieza = DateTime.UtcNow,
                    ejecutadoPor = adminId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar registros antiguos de auditoría");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion
    }
}
