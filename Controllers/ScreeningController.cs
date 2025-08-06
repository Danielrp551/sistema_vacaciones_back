using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SISTEMA_VACACIONES.DTOs.Screening;
using SISTEMA_VACACIONES.Interfaces;

namespace SISTEMA_VACACIONES.Controllers
{
    [Route("api/screening")]
    [ApiController]
    [EnableRateLimiting("ScraperPolicy")]
    public class ScreeningController : ControllerBase
    {
        private readonly ILogger<ScreeningController> _logger;
        private readonly IScraperService _scraperService;

        public ScreeningController(IScraperService scraperService, ILogger<ScreeningController> logger)
        {
            _scraperService = scraperService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]ScreeningRequestDto requestDto)
        {
            _logger.LogInformation("Iniciando solicitud de screening para la entidad: {EntityName}", requestDto.EntityName);

            if (string.IsNullOrWhiteSpace(requestDto.EntityName))
            {
                _logger.LogWarning("El nombre de la entidad está vacío en la solicitud.");
                return BadRequest("El nombre de la entidad es obligatorio.");
            }

            try
            {
                var response = await _scraperService.ScreenEntityAsync(requestDto.EntityName);
                _logger.LogInformation("Solicitud de screening completada con éxito para la entidad: {EntityName}", requestDto.EntityName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar el screening para la entidad: {EntityName}", requestDto.EntityName);
                return StatusCode(500, "Error interno del servidor. Por favor, contacte al administrador.");
            }          
        }
    }
}