using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.Auditoria;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Security;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Controllers
{
    /// <summary>
    /// Controlador para la gestión de permisos del sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PermisoController : ControllerBase
    {
        private readonly IPermisoRepository _permisoRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<PermisoController> _logger;

        public PermisoController(
            IPermisoRepository permisoRepository,
            IAuditoriaService auditoriaService,
            ILogger<PermisoController> logger)
        {
            _permisoRepository = permisoRepository;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista paginada de permisos con filtros y estadísticas
        /// </summary>
        /// <param name="queryObject">Parámetros de consulta y filtrado</param>
        /// <returns>Lista paginada de permisos con estadísticas</returns>
        /// <response code="200">Lista de permisos obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> GetPermisos([FromQuery] PermisosQueryObject queryObject)
        {
            try
            {
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a permisos con token inválido");
                    return Unauthorized();
                }

                _logger.LogInformation("Administrador {UserId} consultando permisos", userId);

                var response = await _permisoRepository.GetAllPermisosAsync(queryObject);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de permisos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un permiso específico por su ID
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <returns>Datos del permiso</returns>
        /// <response code="200">Permiso encontrado</response>
        /// <response code="404">Permiso no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpGet("{id}")]
        [AdminOnly]
        public async Task<IActionResult> GetPermiso(string id)
        {
            try
            {
                var permiso = await _permisoRepository.GetPermisoByIdAsync(id);
                if (permiso == null)
                {
                    return NotFound("Permiso no encontrado");
                }

                var numeroRoles = await _permisoRepository.GetNumeroRolesConPermisoAsync(id);
                var permisoDto = permiso.ToPermisoAdminDto(numeroRoles);

                return Ok(permisoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso {PermisoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo permiso
        /// </summary>
        /// <param name="createPermisoDto">Datos del permiso a crear</param>
        /// <returns>Permiso creado</returns>
        /// <response code="201">Permiso creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="409">Ya existe un permiso con ese nombre o código</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreatePermiso([FromBody] CreatePermisoDto createPermisoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userNombre = User.GetUserName();
                if (string.IsNullOrEmpty(userNombre))
                {
                    return Unauthorized();
                }

                // Verificar duplicados por nombre
                var existeNombre = await _permisoRepository.ExistsPermisoByNombreAsync(createPermisoDto.Nombre);
                if (existeNombre)
                {
                    return Conflict("Ya existe un permiso con ese nombre");
                }

                // Crear permiso
                var nuevoPermiso = createPermisoDto.ToPermisoFromCreateDto(userNombre);
                var permisoCreado = await _permisoRepository.CreatePermisoAsync(nuevoPermiso);

                // Obtener userId
                var userId = User.GetUserId();

                // Obtener IP
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Registrar auditoría
                await _auditoriaService.RegistrarAccionSimpleAsync(
                    TipoAccionAuditoria.CreacionPermiso,
                    ModuloSistema.GESTION_PERMISOS,
                    "Permisos",
                    permisoCreado.Id,
                    userId,
                    null,
                    $"Permiso '{permisoCreado.Nombre}' creado en módulo '{permisoCreado.Modulo}'",
                    ipAddress,
                    SeveridadAuditoria.INFO
                );

                var numeroRoles = await _permisoRepository.GetNumeroRolesConPermisoAsync(permisoCreado.Id);
                var permisoDto = permisoCreado.ToPermisoAdminDto(numeroRoles);

                return CreatedAtAction(nameof(GetPermiso), new { id = permisoCreado.Id }, permisoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza un permiso existente
        /// </summary>
        /// <param name="id">ID del permiso a actualizar</param>
        /// <param name="updatePermisoDto">Datos actualizados del permiso</param>
        /// <returns>Permiso actualizado</returns>
        /// <response code="200">Permiso actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Permiso no encontrado</response>
        /// <response code="409">Ya existe otro permiso con ese nombre</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpPut("{id}")]
        [AdminOnly]
        public async Task<IActionResult> UpdatePermiso(string id, [FromBody] UpdatePermisoDto updatePermisoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userNombre = User.GetUserName();
                if (string.IsNullOrEmpty(userNombre))
                {
                    return Unauthorized();
                }

                var permisoExistente = await _permisoRepository.GetPermisoByIdAsync(id);
                if (permisoExistente == null)
                {
                    return NotFound("Permiso no encontrado");
                }

                // Verificar duplicados por nombre (excluyendo el permiso actual)
                var existeNombre = await _permisoRepository.ExistsPermisoByNombreAsync(updatePermisoDto.Nombre, id);
                if (existeNombre)
                {
                    return Conflict("Ya existe otro permiso con ese nombre");
                }

                // Actualizar permiso
                var permisoActualizado = permisoExistente.UpdateFromDto(updatePermisoDto, userNombre);
                await _permisoRepository.UpdatePermisoAsync(permisoActualizado);

                // Obtener userId
                var userId = User.GetUserId();

                // Obtener IP
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Registrar auditoría
                await _auditoriaService.RegistrarAccionSimpleAsync(
                    TipoAccionAuditoria.ActualizacionPermiso,
                    ModuloSistema.GESTION_PERMISOS,
                    "Permisos",
                    permisoActualizado.Id,
                    userId,
                    null,
                    $"Permiso '{permisoActualizado.Nombre}' actualizado",
                    ipAddress,
                    SeveridadAuditoria.INFO
                );

                var numeroRoles = await _permisoRepository.GetNumeroRolesConPermisoAsync(permisoActualizado.Id);
                var permisoDto = permisoActualizado.ToPermisoAdminDto(numeroRoles);

                return Ok(permisoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso {PermisoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un permiso (soft delete)
        /// </summary>
        /// <param name="id">ID del permiso a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="200">Permiso eliminado exitosamente</response>
        /// <response code="400">No se puede eliminar el permiso porque está asignado a roles</response>
        /// <response code="404">Permiso no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> DeletePermiso(string id)
        {
            try
            {
                var userNombre = User.GetUserName();
                if (string.IsNullOrEmpty(userNombre))
                {
                    return Unauthorized();
                }

                // Obtener user 
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                //Obtener IP
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var permiso = await _permisoRepository.GetPermisoByIdAsync(id);
                if (permiso == null)
                {
                    return NotFound("Permiso no encontrado");
                }

                // Verificar si se puede eliminar
                var puedeEliminar = await _permisoRepository.CanDeletePermisoAsync(id);
                if (!puedeEliminar)
                {
                    return BadRequest("No se puede eliminar el permiso porque está asignado a uno o más roles");
                }

                // Eliminar permiso
                var eliminado = await _permisoRepository.DeletePermisoAsync(id);
                if (!eliminado)
                {
                    return StatusCode(500, "Error al eliminar el permiso");
                }

                // Registrar auditoría
                await _auditoriaService.RegistrarAccionSimpleAsync(
                    TipoAccionAuditoria.EliminacionPermiso,
                    ModuloSistema.GESTION_PERMISOS,
                    "Permisos",
                    id,
                    userId,
                    null,
                    motivo: $"Permiso '{permiso.Nombre}' eliminado",
                    ipAddress,
                    SeveridadAuditoria.INFO
                );

                return Ok(new { message = "Permiso eliminado exitosamente", id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso {PermisoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene la lista de módulos disponibles para filtros
        /// </summary>
        /// <returns>Lista de módulos únicos</returns>
        /// <response code="200">Lista de módulos obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpGet("modulos")]
        [AdminOnly]
        public async Task<IActionResult> GetModulos()
        {
            try
            {
                var modulos = await _permisoRepository.GetModulosDisponiblesAsync();
                return Ok(modulos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos disponibles");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de permisos para dashboard
        /// </summary>
        /// <returns>Estadísticas de permisos</returns>
        /// <response code="200">Estadísticas obtenidas exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Usuario sin permisos administrativos</response>
        [HttpGet("estadisticas")]
        [AdminOnly]
        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                var estadisticas = await _permisoRepository.GetEstadisticasPermisosAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de permisos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
