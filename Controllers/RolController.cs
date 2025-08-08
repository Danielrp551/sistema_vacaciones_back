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
        private readonly ILogger<RolController> _logger;

        public RolController(
            IRolRepository rolRepository,
            IPersonaRepository personaRepository,
            ILogger<RolController> logger
        )
        {
            _rolRepository = rolRepository;
            _personaRepository = personaRepository;
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

                return Ok(new {
                    Total = total,
                    Roles = rolesDto,
                    ConsultadoPor = userId
                });
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

                _logger.LogInformation("Rol actualizado exitosamente por usuario {UserId}", userId);
                return Ok(new { Rol = UpdatedRol, ActualizadoPor = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}