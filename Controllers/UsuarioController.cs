using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]    
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioRepository usuarioRepository, ILogger<UsuarioController> logger)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        [HttpGet, Route("get-usuarios")]
        [AdminOnly] // Solo administradores pueden ver todos los usuarios
        public async Task<IActionResult> GetUsuarios([FromQuery] UsuariosQueryObject queryObject)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Obtener el userId del token JWT para auditoría
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a listado de usuarios con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Administrador {UserId} consultando listado de usuarios", userId);

                var (total, usuarios) = await _usuarioRepository.GetUsuarios(queryObject, userId);

                return Ok(new{
                    Total = total,
                    Usuarios = usuarios,
                    ConsultadoPor = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de usuarios por usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }
        
    }
}