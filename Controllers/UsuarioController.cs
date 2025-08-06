using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]    
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet, Route("get-usuarios/{usuarioId}")]
        public async Task<IActionResult> GetUsuarios([FromQuery] UsuariosQueryObject queryObject, string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (total, usuarios) = await _usuarioRepository.GetUsuarios(queryObject, usuarioId);

            return Ok(new{
                Total = total,
                Usuarios = usuarios
            });
        }
        
    }
}