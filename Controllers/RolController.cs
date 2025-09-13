using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.DTOs.Rol;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models.Enums;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    [Route("api/rol")]
    [ApiController]
    [Authorize]
    public class RolController : ControllerBase
    {
        private readonly IRolRepository _rolRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<RolController> _logger;

        public RolController(
            IRolRepository rolRepository,
            IPersonaRepository personaRepository,
            IAuditoriaService auditoriaService,
            ILogger<RolController> logger
        )
        {
            _rolRepository = rolRepository;
            _personaRepository = personaRepository;
            _auditoriaService = auditoriaService;
            _logger = logger;
        } 

        [HttpGet, Route("get-rol-pagination")]
        [AdminOnly] // Solo administradores pueden gestionar roles
        public async Task<IActionResult> GetRolPagination([FromQuery] RolesQueryObject queryObject)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a roles con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} consultando roles paginados", userId);

                var (total, rolesDto) = await _rolRepository.GetRolPagination(queryObject, userId);

                // Respuesta estructurada similar a otros endpoints
                var response = new {
                    Roles = rolesDto,
                    Total = rolesDto.Count,
                    TotalCompleto = total,
                    PaginaActual = queryObject.PageNumber,
                    TamanoPagina = queryObject.PageSize,
                    TotalPaginas = (int)Math.Ceiling((double)total / queryObject.PageSize),
                    TienePaginaAnterior = queryObject.PageNumber > 1,
                    TienePaginaSiguiente = queryObject.PageNumber < (int)Math.Ceiling((double)total / queryObject.PageSize),
                    ConsultadoPor = userId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles paginados para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }            
        }

        [HttpGet, Route("get-permisos")]
        [AdminOnly] // Solo administradores pueden ver permisos
        public async Task<IActionResult> GetPermisos()
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a permisos con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} consultando permisos", userId);

                var (total, permisosDto) = await _rolRepository.GetPermisos(userId);

                return Ok(new {
                    Total = total,
                    Permisos = permisosDto,
                    ConsultadoPor = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet, Route("get-rol/{id}")]
        [AdminOnly] // Solo administradores pueden ver detalles de roles
        public async Task<IActionResult> GetRolById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("El ID del rol es requerido");
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a rol con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} consultando rol {RolId}", userId, id);

                var rolDto = await _rolRepository.GetRolById(id, userId);

                if (rolDto == null)
                {
                    _logger.LogWarning("Rol {RolId} no encontrado para usuario {UserId}", id, userId);
                    return NotFound($"No se encontró el rol con ID: {id}");
                }

                return Ok(new {
                    Rol = rolDto,
                    ConsultadoPor = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol {RolId} para usuario {UserId}", id, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost, Route("crear-rol")]
        [AdminOnly] // Solo administradores pueden crear roles
        public async Task<IActionResult> CrearRol([FromBody] CreateRolRequestDto createRolDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de crear rol con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} creando rol: {RolName}", userId, createRolDto.Name);

                // Obtener nombre del usuario para auditoría
                string userNombre = await _personaRepository.GetNombreByIdAsync(userId) ?? "Usuario desconocido";

                var (Success, ErrorMessage, CreatedRol) = await _rolRepository.CrearRol(createRolDto, userNombre);

                if (!Success)
                {
                    _logger.LogWarning("Error al crear rol para usuario {UserId}: {Error}", userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.CREAR_ROL,
                        ModuloSistema.GESTION_ROLES,
                        "AspNetRoles",
                        CreatedRol?.Id ?? "",
                        userId,
                        motivo: $"Nuevo rol creado: {createRolDto.Name}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para creación de rol: {RolId} por admin: {AdminId}", CreatedRol?.Id, userId);
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning(auditEx, "Error al registrar auditoría para creación de rol por usuario {UserId}", userId);
                    // No interrumpimos el flujo si falla la auditoría
                }

                _logger.LogInformation("Rol creado exitosamente por usuario {UserId}", userId);
                return Ok(new { Rol = CreatedRol, CreadoPor = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut, Route("actualizar-permiso")]
        [AdminOnly] // Solo administradores pueden actualizar permisos
        public async Task<IActionResult> ActualizarPermiso([FromBody] UpdatePermisoRequestDto updatePermisoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de actualizar permiso con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} actualizando permiso", userId);

                // Obtener nombre del usuario para auditoría
                string userNombre = await _personaRepository.GetNombreByIdAsync(userId) ?? "Usuario desconocido";

                var (Success, ErrorMessage, UpdatedPermiso) = await _rolRepository.ActualizarPermiso(updatePermisoDto, userNombre);

                if (!Success)
                {
                    _logger.LogWarning("Error al actualizar permiso para usuario {UserId}: {Error}", userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.MODIFICAR_PERMISOS_ROL,
                        ModuloSistema.GESTION_ROLES,
                        "Permisos", // Tabla de permisos
                        UpdatedPermiso?.Id ?? "",
                        userId,
                        motivo: $"Permiso actualizado: {UpdatedPermiso?.Nombre}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para actualización de permiso: {PermisoId} por admin: {AdminId}", UpdatedPermiso?.Id, userId);
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning(auditEx, "Error al registrar auditoría para actualización de permiso por usuario {UserId}", userId);
                    // No interrumpimos el flujo si falla la auditoría
                }

                _logger.LogInformation("Permiso actualizado exitosamente por usuario {UserId}", userId);
                return Ok(new { Permiso = UpdatedPermiso, ActualizadoPor = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut, Route("actualizar-rol")]
        [AdminOnly] // Solo administradores pueden actualizar roles
        public async Task<IActionResult> ActualizarRol([FromBody] UpdateRolRequestDto updateRolDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de actualizar rol con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} actualizando rol", userId);

                // Obtener nombre del usuario para auditoría
                string userNombre = await _personaRepository.GetNombreByIdAsync(userId) ?? "Usuario desconocido";

                var (Success, ErrorMessage, UpdatedRol) = await _rolRepository.ActualizarRol(updateRolDto, userNombre);

                if (!Success)
                {
                    _logger.LogWarning("Error al actualizar rol para usuario {UserId}: {Error}", userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.EDITAR_ROL,
                        ModuloSistema.GESTION_ROLES,
                        "AspNetRoles",
                        UpdatedRol?.Id ?? "",
                        userId,
                        motivo: $"Rol actualizado: {UpdatedRol?.Name}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para actualización de rol: {RolId} por admin: {AdminId}", UpdatedRol?.Id, userId);
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning(auditEx, "Error al registrar auditoría para actualización de rol por usuario {UserId}", userId);
                    // No interrumpimos el flujo si falla la auditoría
                }

                _logger.LogInformation("Rol actualizado exitosamente por usuario {UserId}", userId);
                return Ok(new { Rol = UpdatedRol, ActualizadoPor = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete, Route("eliminar-rol/{id}")]
        [AdminOnly] // Solo administradores pueden eliminar roles
        public async Task<IActionResult> EliminarRol(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("El ID del rol es requerido");
                }

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de eliminar rol con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} eliminando rol {RolId}", userId, id);

                // Obtener nombre del usuario para auditoría
                string userNombre = await _personaRepository.GetNombreByIdAsync(userId) ?? "Usuario desconocido";

                var (Success, ErrorMessage) = await _rolRepository.EliminarRol(id, userNombre);

                if (!Success)
                {
                    _logger.LogWarning("Error al eliminar rol {RolId} para usuario {UserId}: {Error}", id, userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.ELIMINAR_ROL,
                        ModuloSistema.GESTION_ROLES,
                        "AspNetRoles",
                        id,
                        userId,
                        motivo: $"Rol eliminado: {id}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.WARNING
                    );

                    _logger.LogInformation("Acción de auditoría registrada para eliminación de rol: {RolId} por admin: {AdminId}", id, userId);
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning(auditEx, "Error al registrar auditoría para eliminación de rol por usuario {UserId}", userId);
                    // No interrumpimos el flujo si falla la auditoría
                }

                _logger.LogInformation("Rol {RolId} eliminado exitosamente por usuario {UserId}", id, userId);
                return Ok(new { Message = "Rol eliminado exitosamente", EliminadoPor = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol {RolId} para usuario {UserId}", id, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut, Route("cambiar-estado-rol/{id}")]
        [AdminOnly] // Solo administradores pueden cambiar estado de roles
        public async Task<IActionResult> CambiarEstadoRol(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("El ID del rol es requerido");
                }

                // Obtener el userId del token JWT
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de cambiar estado de rol con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} cambiando estado de rol {RolId}", userId, id);

                // Obtener información del rol antes del cambio para auditoría
                var rolActual = await _rolRepository.GetRolById(id, userId);
                if (rolActual == null)
                {
                    _logger.LogWarning("Rol {RolId} no encontrado para cambio de estado por usuario {UserId}", id, userId);
                    return NotFound($"No se encontró el rol con ID: {id}");
                }

                // Obtener nombre del usuario para auditoría
                string userNombre = await _personaRepository.GetNombreByIdAsync(userId) ?? "Usuario desconocido";

                var (Success, ErrorMessage, nuevoEstado) = await _rolRepository.CambiarEstadoRol(id, userNombre);

                if (!Success)
                {
                    _logger.LogWarning("Error al cambiar estado de rol {RolId} para usuario {UserId}: {Error}", id, userId, ErrorMessage);
                    return BadRequest(ErrorMessage);
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    // Determinar el tipo de acción según el nuevo estado
                    var tipoAccion = nuevoEstado?.ToLower() == "activo" 
                        ? TipoAccionAuditoria.ACTIVAR_ROL 
                        : TipoAccionAuditoria.DESACTIVAR_ROL;

                    // Mensaje descriptivo con el nombre del rol
                    var accionTexto = nuevoEstado?.ToLower() == "activo" ? "activado" : "desactivado";
                    var motivoDescriptivo = $"Rol '{rolActual.Name}' {accionTexto} (estado cambiado de {rolActual.Estado} a {nuevoEstado})";

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        tipoAccion,
                        ModuloSistema.GESTION_ROLES,
                        "AspNetRoles",
                        id,
                        userId,
                        motivo: motivoDescriptivo,
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para cambio de estado de rol: {RolId} por admin: {AdminId}", id, userId);
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning(auditEx, "Error al registrar auditoría para cambio de estado de rol por usuario {UserId}", userId);
                    // No interrumpimos el flujo si falla la auditoría
                }

                _logger.LogInformation("Estado de rol {RolId} cambiado a {NuevoEstado} por usuario {UserId}", id, nuevoEstado, userId);
                return Ok(new { 
                    Message = $"Estado del rol cambiado a {nuevoEstado} exitosamente", 
                    NuevoEstado = nuevoEstado,
                    CambiadoPor = userId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de rol {RolId} para usuario {UserId}", id, User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}