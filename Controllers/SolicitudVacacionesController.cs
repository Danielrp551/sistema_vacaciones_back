using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using sistema_vacaciones_back.DTOs.SolicitudVacaciones;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/solicitud-vacaciones")]
    [Authorize]
    public class SolicitudVacacionesController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISolicitudVacacionesRepository _solicitudVacacionesRepository;
        private readonly ILogger<SolicitudVacacionesController> _logger;

        public SolicitudVacacionesController(
            IUsuarioRepository usuarioRepository,
            ISolicitudVacacionesRepository solicitudVacacionesRepository,
            ILogger<SolicitudVacacionesController> logger
            )
        {
            _usuarioRepository = usuarioRepository;
            _solicitudVacacionesRepository = solicitudVacacionesRepository;
            _logger = logger;
        }

        [HttpPost, Route("crear-solicitud-vacaciones")]
        [OwnerOnly] // Atributo de seguridad personalizado
        public async Task<IActionResult> CrearSolicitudVacaciones([FromBody] CreateSolicitudRequestDto createSolicitudDto)
        {
            try
            {
                if(!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT con validación robusta
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso con token inválido o sin userId");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} creando solicitud de vacaciones", userId);

                if (createSolicitudDto.DiasSolicitados <= 0)
                    return BadRequest("Los días solicitados deben ser mayores a 0");

                if (createSolicitudDto.FechaInicio > createSolicitudDto.FechaFin)
                    return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

                if (createSolicitudDto.FechaInicio < DateTime.Today)
                    return BadRequest("La fecha de inicio no puede ser anterior a hoy");

                if (createSolicitudDto.TipoVacaciones != "libres" && createSolicitudDto.TipoVacaciones != "bloque")
                    return BadRequest("El tipo de vacaciones debe ser 'libres' o 'bloque'");

                var solicitud = createSolicitudDto.ToSolicitudVacacionesFromCreateDto(userId);

                var (Success, ErrorMessage, CreatedSolicitud) =
                                await _solicitudVacacionesRepository.CrearSolicitudVacaciones(solicitud, userId);
                if (!Success)
                {
                    _logger.LogWarning("Error al crear solicitud para usuario {UserId}: {Error}", userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                _logger.LogInformation("Solicitud de vacaciones creada exitosamente para usuario {UserId}", userId);
                return Ok(new { Message = "Solicitud creada exitosamente", SolicitudId = CreatedSolicitud?.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear solicitud de vacaciones para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet, Route("get-solicitudes-pagination")]
        [OwnerOnly] // Atributo de seguridad personalizado
        public async Task<IActionResult> GetSolicitudesPagination([FromQuery] SolicitudesQueryObject queryObject)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT con validación robusta
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a solicitudes con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} consultando solicitudes de vacaciones", userId);

                var solicitudes = await _solicitudVacacionesRepository.GetSolicitudesPagination(queryObject, userId);

                var solicitudesDto = solicitudes.Select(s => s.ToSolicitudVacacionesDto()).ToList();

                return Ok(new {
                    Total = solicitudes.Count,
                    Solicitudes = solicitudesDto,
                    Usuario = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes de vacaciones para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de una solicitud de vacaciones
        /// </summary>
        [HttpGet, Route("{solicitudId}")]
        public async Task<IActionResult> GetSolicitudDetail(string solicitudId)
        {
            try
            {
                if (string.IsNullOrEmpty(solicitudId))
                    return BadRequest("El ID de la solicitud es requerido");

                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a detalle de solicitud con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} consultando detalle de solicitud {SolicitudId}", userId, solicitudId);

                var solicitud = await _solicitudVacacionesRepository.GetSolicitudByIdAsync(solicitudId);
                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no encontrada", solicitudId);
                    return NotFound("Solicitud no encontrada");
                }

                // Verificar permisos: solo el propietario o un administrador/jefe puede ver
                var canApprove = await _solicitudVacacionesRepository.CanUserApproveSolicitudesAsync(userId);
                if (solicitud.SolicitanteId != userId && !canApprove)
                {
                    _logger.LogWarning("Usuario {UserId} sin permisos para ver solicitud {SolicitudId}", userId, solicitudId);
                    return Forbid("No tienes permisos para ver esta solicitud");
                }

                var solicitudDetailDto = solicitud.ToSolicitudVacacionesDetailDto(userId, canApprove);

                _logger.LogInformation("Detalle de solicitud {SolicitudId} consultado exitosamente por usuario {UserId}", solicitudId, userId);
                return Ok(solicitudDetailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de solicitud {SolicitudId} para usuario {UserId}", solicitudId, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Cancela una solicitud de vacaciones
        /// </summary>
        [HttpPut, Route("{solicitudId}/cancelar")]
        [OwnerOnly]
        public async Task<IActionResult> CancelarSolicitud(string solicitudId, [FromBody] CancelarSolicitudRequestDto cancelarDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(solicitudId))
                    return BadRequest("El ID de la solicitud es requerido");

                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de cancelar solicitud con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} cancelando solicitud {SolicitudId}", userId, solicitudId);

                var (success, errorMessage) = await _solicitudVacacionesRepository.CancelarSolicitudAsync(
                    solicitudId, 
                    userId, 
                    cancelarDto.MotivoCancelacion);

                if (!success)
                {
                    _logger.LogWarning("Error al cancelar solicitud {SolicitudId} para usuario {UserId}: {Error}", 
                        solicitudId, userId, errorMessage);
                    return BadRequest(errorMessage);
                }

                _logger.LogInformation("Solicitud {SolicitudId} cancelada exitosamente por usuario {UserId}", solicitudId, userId);
                return Ok(new { Message = "Solicitud cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar solicitud {SolicitudId} para usuario {UserId}", solicitudId, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Aprueba o rechaza una solicitud de vacaciones (solo para administradores y jefe directo)
        /// </summary>
        [HttpPut, Route("{solicitudId}/aprobar")]
        [Authorize]
        public async Task<IActionResult> AprobarRechazarSolicitud(string solicitudId, [FromBody] AprobarSolicitudRequestDto aprobarDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(solicitudId))
                    return BadRequest("El ID de la solicitud es requerido");

                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de aprobar solicitud con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                // Verificar si el usuario puede aprobar esta solicitud específica
                var canApprove = await _solicitudVacacionesRepository.CanUserApproveSpecificSolicitudAsync(userId, solicitudId);
                if (!canApprove)
                {
                    _logger.LogWarning("Usuario {UserId} no autorizado para aprobar solicitud {SolicitudId}", userId, solicitudId);
                    return Forbid("No tiene permisos para aprobar esta solicitud. Solo el jefe directo o administradores pueden aprobar solicitudes.");
                }

                _logger.LogInformation("Usuario {UserId} procesando solicitud {SolicitudId} con acción {Accion}", 
                    userId, solicitudId, aprobarDto.Accion);

                var (success, errorMessage) = await _solicitudVacacionesRepository.AprobarRechazarSolicitudAsync(
                    solicitudId, 
                    userId, 
                    aprobarDto.Accion, 
                    aprobarDto.Comentarios);

                if (!success)
                {
                    _logger.LogWarning("Error al procesar solicitud {SolicitudId} para usuario {UserId}: {Error}", 
                        solicitudId, userId, errorMessage);
                    return BadRequest(errorMessage);
                }

                var mensaje = aprobarDto.Accion == "aprobar" ? "aprobada" : "rechazada";
                _logger.LogInformation("Solicitud {SolicitudId} {Accion} exitosamente por usuario {UserId}", 
                    solicitudId, mensaje, userId);
                
                return Ok(new { Message = $"Solicitud {mensaje} exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud {SolicitudId} para usuario {UserId}", solicitudId, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista mis solicitudes de vacaciones (endpoint específico y claro)
        /// </summary>
        [HttpGet, Route("mis-solicitudes")]
        [OwnerOnly]
        public async Task<IActionResult> GetMisSolicitudes([FromQuery] SolicitudesQueryObject queryObject)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a mis solicitudes con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} consultando sus solicitudes de vacaciones", userId);

                var solicitudes = await _solicitudVacacionesRepository.GetSolicitudesPagination(queryObject, userId);
                var solicitudesDto = solicitudes.Select(s => s.ToSolicitudVacacionesDetailDto(userId)).ToList();

                return Ok(new {
                    Total = solicitudes.Count,
                    Solicitudes = solicitudesDto,
                    Usuario = userId,
                    Pagina = queryObject.PageNumber,
                    TamanoPagina = queryObject.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mis solicitudes para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("gestion-solicitudes")]
        public async Task<IActionResult> GetSolicitudesParaGestion([FromQuery] SolicitudesQueryObject queryObject)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var supervisorId = User.GetUserId();
                if (string.IsNullOrEmpty(supervisorId))
                {
                    _logger.LogWarning("Intento de acceso a gestión de solicitudes con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Supervisor {SupervisorId} consultando solicitudes para gestión", supervisorId);

                // Verificar si el usuario actual es administrador
                var isAdmin = await _solicitudVacacionesRepository.CanUserApproveSolicitudesAsync(supervisorId);

                // Obtener solicitudes del equipo según el nivel especificado
                var (solicitudes, totalCount) = await _solicitudVacacionesRepository.GetSolicitudesEquipo(queryObject, supervisorId);
                var solicitudesDto = solicitudes.Select(s => s.ToSolicitudVacacionesDetailDto(supervisorId, isAdmin)).ToList();

                // Obtener estadísticas con los mismos filtros aplicados
                var estadisticas = await _solicitudVacacionesRepository.GetEstadisticasEquipo(supervisorId, queryObject);

                return Ok(new {
                    Total = solicitudes.Count,  // Total de la página actual
                    TotalCompleto = totalCount,  // Total completo para calcular páginas
                    Solicitudes = solicitudesDto,
                    Supervisor = supervisorId,
                    Pagina = queryObject.PageNumber,
                    TamanoPagina = queryObject.PageSize,
                    Estadisticas = estadisticas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes para gestión por supervisor {SupervisorId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("equipo-empleados")]
        public async Task<IActionResult> GetEmpleadosEquipo([FromQuery] bool incluirSubordinadosNivelN = false)
        {
            try
            {
                var supervisorId = User.GetUserId();
                if (string.IsNullOrEmpty(supervisorId))
                {
                    _logger.LogWarning("Intento de acceso a empleados del equipo con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Supervisor {SupervisorId} consultando empleados del equipo", supervisorId);

                var empleados = await _usuarioRepository.GetEmpleadosEquipo(supervisorId, incluirSubordinadosNivelN);
                var empleadosDto = empleados.Select(u => new {
                    Id = u.Id,
                    NombreCompleto = $"{u.Persona.Nombres} {u.Persona.ApellidoPaterno} {u.Persona.ApellidoMaterno}",
                    Email = u.Email,
                    EsDirecto = u.JefeId == supervisorId // Indica si es empleado directo
                }).ToList();

                return Ok(new {
                    Empleados = empleadosDto,
                    Total = empleados.Count,
                    IncluirSubordinadosNivelN = incluirSubordinadosNivelN
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados del equipo para supervisor {SupervisorId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}