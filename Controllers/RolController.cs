using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.DTOs.Rol;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Controllers
{
    [Route("api/rol")]
    [ApiController]
    [Authorize]
    public class RolController : ControllerBase
    {
        private readonly IRolRepository _rolRepository;

        private readonly IPersonaRepository _personaRepository;

        public RolController(
            IRolRepository rolRepository,
            IPersonaRepository personaRepository
        )
        {
            _rolRepository = rolRepository;
            _personaRepository = personaRepository;
        } 

        [HttpGet, Route("get-rol-pagination/{usuarioId}")]
        public async Task<IActionResult> GetRolPagination([FromQuery] RolesQueryObject queryObject,string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (total,rolesDto) = await _rolRepository.GetRolPagination(queryObject,usuarioId);

            return Ok(new {
                Total = total,
                Roles = rolesDto
            });            
        }

        [HttpGet, Route("get-permisos/{usuarioId}")]
        public async Task<IActionResult> GetPermisos(string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (total, permisosDto) = await _rolRepository.GetPermisos(usuarioId);

            return Ok(new {
                Total = total,
                Permisos = permisosDto
            });
        }

        [HttpPost, Route("crear-rol/{usuarioId}")]
        public async Task<IActionResult> CrearRol([FromBody] CreateRolRequestDto createRolDto, string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userNombre = await _personaRepository.GetNombreByIdAsync(usuarioId);

            var (Success, ErrorMessage, CreatedRol) = await _rolRepository.CrearRol(createRolDto, userNombre);

            if (!Success)
                return BadRequest(ErrorMessage);

            return Ok(CreatedRol);
        }

        [HttpPut, Route("actualizar-permiso/{usuarioId}")]
        public async Task<IActionResult> ActualizarPermiso([FromBody] UpdatePermisoRequestDto updatePermisoDto, string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userNombre = await _personaRepository.GetNombreByIdAsync(usuarioId);

            var (Success, ErrorMessage, UpdatedPermiso) = await _rolRepository.ActualizarPermiso(updatePermisoDto, userNombre);

            if (!Success)
                return BadRequest(ErrorMessage);

            return Ok(UpdatedPermiso);
        }

        [HttpPut, Route("actualizar-rol/{usuarioId}")]
        public async Task<IActionResult> ActualizarRol([FromBody] UpdateRolRequestDto updateRolDto, string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userNombre = await _personaRepository.GetNombreByIdAsync(usuarioId);

            var (Success, ErrorMessage, UpdatedRol) = await _rolRepository.ActualizarRol(updateRolDto, userNombre);

            if (!Success)
                return BadRequest(ErrorMessage);

            return Ok(UpdatedRol);
        }

    }
}