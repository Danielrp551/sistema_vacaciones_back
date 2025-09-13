using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Models.Enums;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    /// <summary>
    /// Controller para la administración de usuarios y permisos
    /// Proporciona endpoints para la gestión completa de usuarios del sistema
    /// </summary>
    [ApiController]
    [Route("api/usuarios-admin")]
    [Authorize]
    public class UsuarioAdminController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<UsuarioAdminController> _logger;
        private readonly IAuditoriaService _auditoriaService;

        public UsuarioAdminController(
            IUsuarioRepository usuarioRepository, 
            ILogger<UsuarioAdminController> logger,
            IAuditoriaService auditoriaService)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
            _auditoriaService = auditoriaService;
        }

        #region Endpoints de Consulta y Listado

        /// <summary>
        /// Obtiene una lista paginada de usuarios con filtros avanzados para administración
        /// </summary>
        /// <param name="queryObject">Parámetros de filtrado, ordenamiento y paginación</param>
        /// <returns>Lista paginada de usuarios con metadatos</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="400">Parámetros de consulta inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para acceder a usuarios administrativos</response>
        [HttpGet]
        [AdminOnly] // Solo administradores pueden ver la lista completa de usuarios
        [ProducesResponseType(typeof(UsuariosAdminResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsuariosAdmin([FromQuery] UsuariosAdminQueryObject queryObject)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de usuarios administrativos con filtros: {@QueryObject}", queryObject);

                var resultado = await _usuarioRepository.GetUsuariosAdmin(queryObject);

                _logger.LogInformation("Lista de usuarios obtenida exitosamente. Total: {Total}, Página: {Pagina}", 
                    resultado.TotalCompleto, resultado.PaginaActual);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Parámetros inválidos en GetUsuariosAdmin: {Message}", ex.Message);
                return BadRequest(new { message = "Parámetros de consulta inválidos", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de usuarios administrativos");
                return StatusCode(500, new { message = "Error interno del servidor al obtener usuarios" });
            }
        }

        /// <summary>
        /// Obtiene los detalles completos de un usuario específico para administración
        /// </summary>
        /// <param name="id">ID único del usuario</param>
        /// <returns>Detalles completos del usuario</returns>
        /// <response code="200">Usuario encontrado exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para ver detalles de usuarios</response>
        [HttpGet("{id}")]
        [AdminOnly]
        [ProducesResponseType(typeof(UsuarioDetalleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsuarioById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de usuario es requerido" });
                }

                _logger.LogInformation("Obteniendo detalles del usuario: {UsuarioId}", id);

                var usuario = await _usuarioRepository.GetUsuarioAdminByIdAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UsuarioId}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation("Detalles del usuario obtenidos exitosamente: {UsuarioId}", id);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del usuario: {UsuarioId}", id);
                return StatusCode(500, new { message = "Error interno del servidor al obtener detalles del usuario" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales del sistema de usuarios
        /// </summary>
        /// <returns>Estadísticas de usuarios del sistema</returns>
        /// <response code="200">Estadísticas obtenidas exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para ver estadísticas</response>
        [HttpGet("estadisticas")]
        [AdminOnly]
        [ProducesResponseType(typeof(UsuariosEstadisticasDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de usuarios del sistema");

                var estadisticas = await _usuarioRepository.GetUsuariosEstadisticasAsync();

                _logger.LogInformation("Estadísticas obtenidas exitosamente. Total usuarios: {Total}", 
                    estadisticas.TotalUsuarios);

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de usuarios");
                return StatusCode(500, new { message = "Error interno del servidor al obtener estadísticas" });
            }
        }

        #endregion

        #region Endpoints de Datos de Apoyo (Dropdowns)

        /// <summary>
        /// Obtiene lista de departamentos para dropdowns y selectores
        /// </summary>
        /// <param name="soloActivos">Si true, solo retorna departamentos activos</param>
        /// <returns>Lista de departamentos simplificada</returns>
        /// <response code="200">Lista de departamentos obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("dropdowns/departamentos")]
        [ProducesResponseType(typeof(List<DepartamentoSimpleDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDepartamentos([FromQuery] bool soloActivos = true)
        {
            try
            {
                _logger.LogInformation("Obteniendo departamentos para dropdown. Solo activos: {SoloActivos}", soloActivos);

                var departamentos = await _usuarioRepository.GetDepartamentosSimpleAsync(soloActivos);

                _logger.LogInformation("Departamentos obtenidos exitosamente. Total: {Total}", departamentos.Count);

                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener departamentos para dropdown");
                return StatusCode(500, new { message = "Error interno del servidor al obtener departamentos" });
            }
        }

        /// <summary>
        /// Obtiene lista de roles disponibles en el sistema
        /// </summary>
        /// <returns>Lista de nombres de roles</returns>
        /// <response code="200">Lista de roles obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para ver roles</response>
        [HttpGet("dropdowns/roles")]
        [AdminOnly] // Solo administradores pueden ver todos los roles
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                _logger.LogInformation("Obteniendo roles del sistema para dropdown");

                var roles = await _usuarioRepository.GetAllRolesAsync();

                _logger.LogInformation("Roles obtenidos exitosamente. Total: {Total}", roles.Count);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles para dropdown");
                return StatusCode(500, new { message = "Error interno del servidor al obtener roles" });
            }
        }

        /// <summary>
        /// Obtiene lista de usuarios que pueden ser asignados como jefes/supervisores
        /// </summary>
        /// <param name="soloActivos">Si true, solo retorna usuarios activos</param>
        /// <returns>Lista de usuarios simplificada para jefes</returns>
        /// <response code="200">Lista de posibles jefes obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("dropdowns/jefes")]
        [ProducesResponseType(typeof(List<UsuarioSimpleDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetJefes([FromQuery] bool soloActivos = true)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios para dropdown de jefes. Solo activos: {SoloActivos}", soloActivos);

                var jefes = await _usuarioRepository.GetUsuariosSimpleAsync(soloActivos);

                _logger.LogInformation("Usuarios para jefes obtenidos exitosamente. Total: {Total}", jefes.Count);

                return Ok(jefes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios para dropdown de jefes");
                return StatusCode(500, new { message = "Error interno del servidor al obtener posibles jefes" });
            }
        }

        #endregion

        #region Endpoints de Validación

        /// <summary>
        /// Valida si un email ya está en uso por otro usuario
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <param name="excludeUserId">ID de usuario a excluir de la validación (opcional, para edición)</param>
        /// <returns>True si el email ya existe, False si está disponible</returns>
        /// <response code="200">Validación completada exitosamente</response>
        /// <response code="400">Email no proporcionado</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("validate/email")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateEmail([FromQuery] string email, [FromQuery] string? excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { message = "Email es requerido para la validación" });
                }

                _logger.LogInformation("Validando email: {Email}, Excluir: {ExcludeUserId}", email, excludeUserId);

                var existe = await _usuarioRepository.EmailExistsAsync(email, excludeUserId);

                _logger.LogInformation("Validación de email completada. Email: {Email}, Existe: {Existe}", email, existe);

                return Ok(existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar email: {Email}", email);
                return StatusCode(500, new { message = "Error interno del servidor al validar email" });
            }
        }

        /// <summary>
        /// Valida si un DNI ya está en uso por otra persona
        /// </summary>
        /// <param name="dni">DNI a validar</param>
        /// <param name="excludeUserId">ID de usuario a excluir de la validación (opcional, para edición)</param>
        /// <returns>True si el DNI ya existe, False si está disponible</returns>
        /// <response code="200">Validación completada exitosamente</response>
        /// <response code="400">DNI no proporcionado</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("validate/dni")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateDni([FromQuery] string dni, [FromQuery] string? excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dni))
                {
                    return BadRequest(new { message = "DNI es requerido para la validación" });
                }

                _logger.LogInformation("Validando DNI: {Dni}, Excluir: {ExcludeUserId}", dni, excludeUserId);

                var existe = await _usuarioRepository.DniExistsAsync(dni, excludeUserId);

                _logger.LogInformation("Validación de DNI completada. DNI: {Dni}, Existe: {Existe}", dni, existe);

                return Ok(existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar DNI: {Dni}", dni);
                return StatusCode(500, new { message = "Error interno del servidor al validar DNI" });
            }
        }

        #endregion

        #region Endpoints de Gestión CRUD

        /// <summary>
        /// Crea un nuevo usuario en el sistema con su información personal y roles
        /// </summary>
        /// <param name="createDto">Datos del usuario a crear</param>
        /// <returns>Resultado de la creación con ID del usuario y contraseña temporal</returns>
        /// <response code="201">Usuario creado exitosamente</response>
        /// <response code="400">Datos inválidos o usuario ya existe</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para crear usuarios</response>
        [HttpPost]
        [AdminOnly] // Solo administradores pueden crear usuarios
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos inválidos para crear usuario: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Iniciando creación de usuario: {Email}", createDto.Email);

                // ✅ Validar que el jefe existe (si se proporciona)
                if (!string.IsNullOrWhiteSpace(createDto.JefeId))
                {
                    var jefeExiste = await _usuarioRepository.GetByIdAsync(createDto.JefeId);
                    if (jefeExiste == null)
                    {
                        _logger.LogWarning("Jefe no encontrado: {JefeId}", createDto.JefeId);
                        return BadRequest(new { message = "El jefe seleccionado no existe" });
                    }
                }

                // ✅ Crear objetos Usuario y Persona desde el DTO
                var persona = new Persona
                {
                    Nombres = createDto.Nombres,
                    ApellidoPaterno = createDto.ApellidoPaterno,
                    ApellidoMaterno = createDto.ApellidoMaterno,
                    Dni = createDto.Dni,
                    Extranjero = createDto.Extranjero,
                    FechaIngreso = createDto.FechaIngreso,
                    Empresa = createDto.Empresa
                };

                var usuario = new Usuario
                {
                    Email = createDto.Email,
                    DepartamentoId = createDto.DepartamentoId,
                    // Normalizar JefeId: convertir string vacío/whitespace a null
                    JefeId = string.IsNullOrWhiteSpace(createDto.JefeId) ? null : createDto.JefeId
                };

                // ✅ Obtener ID del usuario actual para auditoría
                var currentUserId = User.GetUserId() ?? "Sistema";
                usuario.CreatedBy = currentUserId;

                // ✅ Llamar al repositorio para crear el usuario
                var (success, errors, userId) = await _usuarioRepository.CreateUsuarioAsync(
                    usuario, persona, createDto.ContrasenaTemporal ?? string.Empty, createDto.Roles ?? new List<string>());

                if (!success)
                {
                    _logger.LogWarning("Error al crear usuario {Email}: {@Errors}", createDto.Email, errors);
                    return BadRequest(new { message = "Error al crear usuario", errors });
                }

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("Usuario creado pero ID no disponible para {Email}", createDto.Email);
                    return StatusCode(500, new { message = "Usuario creado pero ID no disponible" });
                }

                _logger.LogInformation("Usuario creado exitosamente: {UserId}, Email: {Email}", userId, createDto.Email);

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.CREAR_USUARIO,
                        ModuloSistema.GESTION_USUARIOS,
                        "AspNetUsers",
                        userId,
                        currentUserId,
                        usuarioAfectadoId: userId,
                        motivo: $"Nuevo usuario creado: {createDto.Email}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para creación de usuario: {UserId} por admin: {AdminId}", userId, currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar auditoría para creación de usuario: {UserId}", userId);
                    // No fallar la operación por error de auditoría, pero registrarlo
                }

                // ✅ Retornar resultado exitoso con información relevante
                return CreatedAtAction(
                    nameof(GetUsuarioById),
                    new { id = userId },
                    new
                    {
                        message = "Usuario creado exitosamente",
                        userId,
                        email = createDto.Email,
                        contrasenaTemporal = string.IsNullOrEmpty(createDto.ContrasenaTemporal) 
                            ? "Se generó contraseña automática (revisar logs de seguridad)" 
                            : "Se utilizó contraseña personalizada"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argumentos inválidos al crear usuario: {Message}", ex.Message);
                return BadRequest(new { message = "Datos inválidos", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al crear usuario: {Email}", createDto.Email);
                return StatusCode(500, new { message = "Error interno del servidor al crear usuario" });
            }
        }

        /// <summary>
        /// Actualiza la información de un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="updateDto">Nuevos datos del usuario</param>
        /// <returns>Resultado de la actualización</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para actualizar usuarios</response>
        [HttpPut("{id}")]
        [AdminOnly]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateUsuario(string id, [FromBody] UpdateUsuarioDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de usuario es requerido" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Datos inválidos para actualizar usuario {Id}: {@ModelState}", id, ModelState);
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Iniciando actualización de usuario: {Id}", id);

                // ✅ Obtener usuario existente
                var usuarioExistente = await _usuarioRepository.GetByIdAsync(id);
                if (usuarioExistente == null)
                {
                    _logger.LogWarning("Usuario no encontrado para actualización: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // ✅ Validar duplicados de email (excluyendo el usuario actual)
                if (await _usuarioRepository.EmailExistsAsync(updateDto.Email, id))
                {
                    _logger.LogWarning("Email ya existe para otro usuario: {Email}", updateDto.Email);
                    return BadRequest(new { message = "El email ya está en uso por otro usuario" });
                }

                // ✅ Validar duplicados de DNI (excluyendo el usuario actual)
                if (await _usuarioRepository.DniExistsAsync(updateDto.Dni, id))
                {
                    _logger.LogWarning("DNI ya existe para otro usuario: {Dni}", updateDto.Dni);
                    return BadRequest(new { message = "El DNI ya está en uso por otro usuario" });
                }

                // ✅ Validar que el jefe existe (si se proporciona)
                if (!string.IsNullOrWhiteSpace(updateDto.JefeId))
                {
                    var jefeExiste = await _usuarioRepository.GetByIdAsync(updateDto.JefeId);
                    if (jefeExiste == null)
                    {
                        _logger.LogWarning("Jefe no encontrado: {JefeId}", updateDto.JefeId);
                        return BadRequest(new { message = "El jefe seleccionado no existe" });
                    }

                    // Validar que el usuario no se esté asignando como jefe de sí mismo
                    if (updateDto.JefeId == id)
                    {
                        _logger.LogWarning("Usuario intentando asignarse como jefe de sí mismo: {UserId}", id);
                        return BadRequest(new { message = "Un usuario no puede ser jefe de sí mismo" });
                    }
                }

                // ✅ Actualizar propiedades del usuario
                usuarioExistente.Email = updateDto.Email;
                // Normalizar DepartamentoId: convertir string vacío/whitespace a valor por defecto si es necesario
                usuarioExistente.DepartamentoId = updateDto.DepartamentoId;
                // Normalizar JefeId: convertir string vacío/whitespace a null
                usuarioExistente.JefeId = string.IsNullOrWhiteSpace(updateDto.JefeId) ? null : updateDto.JefeId;
                usuarioExistente.UpdatedOn = DateTime.UtcNow;
                usuarioExistente.UpdatedBy = User.GetUserId() ?? "Sistema";

                // ✅ Actualizar información personal si existe
                if (usuarioExistente.Persona != null)
                {
                    usuarioExistente.Persona.Nombres = updateDto.Nombres;
                    usuarioExistente.Persona.ApellidoPaterno = updateDto.ApellidoPaterno;
                    usuarioExistente.Persona.ApellidoMaterno = updateDto.ApellidoMaterno;
                    usuarioExistente.Persona.Dni = updateDto.Dni;
                    usuarioExistente.Persona.Extranjero = updateDto.Extranjero;
                    usuarioExistente.Persona.FechaIngreso = updateDto.FechaIngreso;
                    usuarioExistente.Persona.Empresa = updateDto.Empresa;
                }

                // ✅ Actualizar usuario y roles
                var (success, errors) = await _usuarioRepository.UpdateUsuarioAsync(
                    usuarioExistente, updateDto.Roles ?? new List<string>());

                if (!success)
                {
                    _logger.LogWarning("Error al actualizar usuario {Id}: {@Errors}", id, errors);
                    return BadRequest(new { message = "Error al actualizar usuario", errors });
                }

                _logger.LogInformation("Usuario actualizado exitosamente: {Id}", id);

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();
                    var currentUserId = User.GetUserId() ?? "Sistema";

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.EDITAR_USUARIO,
                        ModuloSistema.GESTION_USUARIOS,
                        "AspNetUsers",
                        id,
                        currentUserId,
                        usuarioAfectadoId: id,
                        motivo: $"Usuario actualizado: {updateDto.Email}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.INFO
                    );

                    _logger.LogInformation("Acción de auditoría registrada para actualización de usuario: {Id} por admin: {AdminId}", id, currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar auditoría para actualización de usuario: {Id}", id);
                    // No fallar la operación por error de auditoría, pero registrarlo
                }

                return Ok(new { message = "Usuario actualizado exitosamente", userId = id });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argumentos inválidos al actualizar usuario {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = "Datos inválidos", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al actualizar usuario: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor al actualizar usuario" });
            }
        }

        /// <summary>
        /// Elimina un usuario del sistema (soft delete)
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Resultado de la eliminación</returns>
        /// <response code="200">Usuario eliminado exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="400">No se puede eliminar el usuario (tiene dependencias)</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para eliminar usuarios</response>
        [HttpDelete("{id}")]
        [AdminOnly]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de usuario es requerido" });
                }

                _logger.LogInformation("Iniciando eliminación de usuario: {Id}", id);

                // ✅ Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para eliminación: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // ✅ Verificar dependencias (subordinados)
                var numeroSubordinados = await _usuarioRepository.GetNumeroSubordinadosAsync(id);
                if (numeroSubordinados > 0)
                {
                    _logger.LogWarning("No se puede eliminar usuario {Id} - tiene {Count} subordinados", id, numeroSubordinados);
                    return BadRequest(new { 
                        message = "No se puede eliminar el usuario porque tiene subordinados a cargo",
                        subordinados = numeroSubordinados
                    });
                }

                // ✅ Prevenir auto-eliminación
                var currentUserId = User.GetUserId();
                if (id == currentUserId)
                {
                    _logger.LogWarning("Usuario {Id} intentó eliminarse a sí mismo", id);
                    return BadRequest(new { message = "No puedes eliminar tu propia cuenta" });
                }

                // ✅ Realizar soft delete
                usuario.IsDeleted = true;
                usuario.UpdatedOn = DateTime.UtcNow;
                usuario.UpdatedBy = currentUserId ?? "Sistema";

                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Usuario eliminado exitosamente: {Id}", id);

                return Ok(new { 
                    message = "Usuario eliminado exitosamente", 
                    userId = id,
                    eliminadoPor = currentUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al eliminar usuario: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor al eliminar usuario" });
            }
        }

        #endregion

        #region Endpoints de Seguridad y Contraseñas

        /// <summary>
        /// Reinicia la contraseña de un usuario, generando una nueva contraseña temporal
        /// </summary>
        /// <param name="id">ID del usuario cuya contraseña se reiniciará</param>
        /// <param name="resetDto">Datos para el reset de contraseña (opcional contraseña personalizada)</param>
        /// <returns>Resultado del reset con nueva contraseña temporal</returns>
        /// <response code="200">Contraseña reiniciada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para resetear contraseñas</response>
        [HttpPost("{id}/reset-password")]
        [AdminOnly]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto? resetDto = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de usuario es requerido" });
                }

                _logger.LogInformation("Iniciando reset de contraseña para usuario: {Id}", id);

                // ✅ Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para reset de contraseña: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // ✅ Obtener ID del admin que realiza el reset
                var adminId = User.GetUserId() ?? "Sistema";

                // ✅ Llamar al repositorio para resetear contraseña (usando contraseña generada automáticamente)
                var (success, errors, contrasenaTemporal) = await _usuarioRepository.ResetPasswordAsync(
                    id, null, adminId);

                if (!success)
                {
                    _logger.LogWarning("Error al resetear contraseña para usuario {Id}: {@Errors}", id, errors);
                    return BadRequest(new { message = "Error al resetear contraseña", errors });
                }

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.REINICIAR_PASSWORD,
                        ModuloSistema.GESTION_USUARIOS,
                        "AspNetUsers",
                        id,
                        adminId,
                        usuarioAfectadoId: id,
                        motivo: resetDto?.Motivo,
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.SECURITY
                    );

                    _logger.LogInformation("Acción de auditoría registrada para reset de contraseña de usuario: {Id} por admin: {AdminId}", id, adminId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar auditoría para reset de contraseña de usuario: {Id}", id);
                    // No fallar la operación por error de auditoría, pero registrarlo
                }

                _logger.LogInformation("Contraseña reseteada exitosamente para usuario: {Id} por admin: {AdminId}", id, adminId);

                // ✅ Respuesta sin exponer la contraseña en logs
                return Ok(new
                {
                    message = "Contraseña reiniciada exitosamente",
                    userId = id,
                    contrasenaTemporal = contrasenaTemporal,
                    forzarCambio = true,
                    resetPor = adminId,
                    fechaReset = DateTime.UtcNow,
                    motivoRegistrado = !string.IsNullOrEmpty(resetDto?.Motivo)
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argumentos inválidos al resetear contraseña para usuario {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = "Datos inválidos", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al resetear contraseña para usuario: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor al resetear contraseña" });
            }
        }

        /// <summary>
        /// Activa o desactiva un usuario (toggle del estado)
        /// </summary>
        /// <param name="id">ID del usuario cuyo estado se cambiará</param>
        /// <returns>Resultado del cambio de estado</returns>
        /// <response code="200">Estado cambiado exitosamente</response>
        /// <response code="400">No se puede cambiar el estado</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para cambiar estado de usuarios</response>
        [HttpPost("{id}/toggle-status")]
        [AdminOnly]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID de usuario es requerido" });
                }

                _logger.LogInformation("Iniciando cambio de estado para usuario: {Id}", id);

                // ✅ Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para cambio de estado: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // ✅ Prevenir desactivación propia
                var currentUserId = User.GetUserId();
                if (id == currentUserId)
                {
                    _logger.LogWarning("Usuario {Id} intentó cambiar su propio estado", id);
                    return BadRequest(new { message = "No puedes cambiar tu propio estado" });
                }

                // ✅ Si se va a desactivar, verificar dependencias
                if (!usuario.IsDeleted)
                {
                    var numeroSubordinados = await _usuarioRepository.GetNumeroSubordinadosAsync(id);
                    if (numeroSubordinados > 0)
                    {
                        _logger.LogWarning("No se puede desactivar usuario {Id} - tiene {Count} subordinados activos", id, numeroSubordinados);
                        return BadRequest(new { 
                            message = "No se puede desactivar el usuario porque tiene subordinados activos a cargo",
                            subordinados = numeroSubordinados
                        });
                    }
                }

                // ✅ Cambiar estado
                var estadoAnterior = !usuario.IsDeleted;
                usuario.IsDeleted = !usuario.IsDeleted;
                usuario.UpdatedOn = DateTime.UtcNow;
                usuario.UpdatedBy = currentUserId ?? "Sistema";

                await _usuarioRepository.UpdateAsync(usuario);

                var nuevoEstado = !usuario.IsDeleted;
                var accion = nuevoEstado ? "activado" : "desactivado";

                _logger.LogInformation("Usuario {Id} {Accion} exitosamente por admin: {AdminId}", id, accion, currentUserId);

                // ✅ REGISTRAR ACCIÓN DE AUDITORÍA
                try
                {
                    var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    var userAgent = Request.Headers["User-Agent"].ToString();
                    var tipoAccion = nuevoEstado ? TipoAccionAuditoria.ACTIVAR_USUARIO : TipoAccionAuditoria.DESACTIVAR_USUARIO;

                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        tipoAccion,
                        ModuloSistema.GESTION_USUARIOS,
                        "AspNetUsers",
                        id,
                        currentUserId ?? "Sistema",
                        usuarioAfectadoId: id,
                        motivo: $"Usuario {accion} desde estado {(estadoAnterior ? "Activo" : "Inactivo")}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.WARNING
                    );

                    _logger.LogInformation("Acción de auditoría registrada para cambio de estado de usuario: {Id} por admin: {AdminId}", id, currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar auditoría para cambio de estado de usuario: {Id}", id);
                    // No fallar la operación por error de auditoría, pero registrarlo
                }

                return Ok(new
                {
                    message = $"Usuario {accion} exitosamente",
                    userId = id,
                    estadoAnterior = estadoAnterior ? "Activo" : "Inactivo",
                    estadoActual = nuevoEstado ? "Activo" : "Inactivo",
                    cambiadoPor = currentUserId,
                    fechaCambio = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al cambiar estado del usuario: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor al cambiar estado del usuario" });
            }
        }

        #endregion
    }
}
