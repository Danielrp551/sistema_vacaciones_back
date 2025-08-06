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
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/solicitud-vacaciones")]
    [Authorize]
    public class SolicitudVacacionesController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISolicitudVacacionesRepository _solicitudVacacionesRepository;

        public SolicitudVacacionesController(
            IUsuarioRepository usuarioRepository,
            ISolicitudVacacionesRepository solicitudVacacionesRepository
            )
        {
            _usuarioRepository = usuarioRepository;
            _solicitudVacacionesRepository = solicitudVacacionesRepository;
        }

        [HttpPost, Route("crear-solicitud-vacaciones/{usuarioId}")]
        public async Task<IActionResult> CrearSolicitudVacaciones([FromBody] CreateSolicitudRequestDto createSolicitudDto, string usuarioId)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createSolicitudDto.DiasSolicitados <= 0)
                return BadRequest("Los días solicitados deben ser mayores a 0");

            if (createSolicitudDto.FechaInicio > createSolicitudDto.FechaFin)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

            if (createSolicitudDto.FechaInicio < DateTime.Today)
                return BadRequest("La fecha de inicio no puede ser anterior a hoy");

            if (createSolicitudDto.TipoVacaciones != "libres" && createSolicitudDto.TipoVacaciones != "bloque")
                return BadRequest("El tipo de vacaciones debe ser 'libres' o 'bloque'");

            //var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized("Usuario no autorizado");


            var solicitud = createSolicitudDto.ToSolicitudVacacionesFromCreateDto();

            var (Success, ErrorMessage, CreatedSolicitud) =
                            await _solicitudVacacionesRepository.CrearSolicitudVacaciones(solicitud, usuarioId);
            if (!Success)
                return BadRequest(ErrorMessage);

            return Ok("Solicitud creada exitosamente");
        }

        [HttpGet, Route("get-solicitudes-pagination/{usuarioId}")]
        public async Task<IActionResult> GetSolicitudesPagination([FromQuery] SolicitudesQueryObject queryObject,string usuarioId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized("Usuario no autorizado");

            var solicitudes = await _solicitudVacacionesRepository.GetSolicitudesPagination(queryObject,usuarioId);

            var solicitudesDto = solicitudes.Select(s => s.ToSolicitudVacacionesDto()).ToList();

            return Ok(new {
                Total = solicitudes.Count,
                Solicitudes = solicitudesDto
            });
        }
    }
}