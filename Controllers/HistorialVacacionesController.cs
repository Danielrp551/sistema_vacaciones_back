using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.HistorialVacaciones;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/historial-vacaciones")]
    [Authorize]
    public class HistorialVacacionesController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISolicitudVacacionesRepository _solicitudVacacionesRepository;

        public HistorialVacacionesController(
            IUsuarioRepository usuarioRepository,
            ISolicitudVacacionesRepository solicitudVacacionesRepository)
        {
            _usuarioRepository = usuarioRepository;
            _solicitudVacacionesRepository = solicitudVacacionesRepository;
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> ObtenerHistorialVacaciones(string usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            //var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            //System.Console.WriteLine("USER ID: {0}", userId);
            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized("Usuario no autorizado");

            //var usuario = await _usuarioRepository.GetByIdAsync(userId);
            //if (usuario == null)
            //    return NotFound("Usuario no encontrado");

            DateTime fechaIngreso = usuario.Persona.FechaIngreso;
            DateTime fechaActual = DateTime.Today.AddDays(1).AddTicks(-1); 
            System.Console.WriteLine("FECHA ACTUAL: {0}", fechaActual);
            System.Console.WriteLine("FECHA INGRESO: {0}", fechaIngreso);

            // 1️⃣ Generar lista de períodos de vacaciones por año desde la fecha de ingreso hasta hoy
            var historial = GenerarPeriodosVacaciones(fechaIngreso, fechaActual);

            System.Console.WriteLine("HISTORIAL: {0}", historial);

            // 2️⃣ Obtener solicitudes de vacaciones aprobadas del usuario
            var solicitudes = await _solicitudVacacionesRepository.ObtenerSolicitudesAprobadasPendientes(usuarioId);

            // 3️⃣ Restar días gastados según el orden correcto
            AplicarDeducciones(historial, solicitudes);

            var response = new
            {
                FechaIngreso = fechaIngreso, // Formato ISO
                Historial = historial
            };

            return Ok(response);
        }

        private List<HistorialVacacionesDto> GenerarPeriodosVacaciones(DateTime fechaIngreso, DateTime fechaActual)
        {
            var historial = new List<HistorialVacacionesDto>();
            DateTime periodoInicio = fechaIngreso;

            while (periodoInicio <= fechaActual)
            {
                int año = periodoInicio.Year;
                if (periodoInicio > fechaActual)
                    break;
                double diasVacaciones = 30.0;

                // ✅ Calcular proporcionalidad de días considerando la fecha exacta de ingreso
                if (periodoInicio.AddYears(1) > fechaActual)
                {
                    int diasEnElAnio = DateTime.IsLeapYear(periodoInicio.Year) ? 366 : 365;
                    double diasTrabajados = (fechaActual - periodoInicio).TotalDays;
                    System.Console.WriteLine("DIAS TRABAJADOS: {0}", diasTrabajados);
                    diasVacaciones = diasTrabajados* (30.0/diasEnElAnio); // Sin redondear
                    System.Console.WriteLine("DIAS VACACIONES: {0}", diasVacaciones);
                }

                // ✅ Distribuir vacaciones, asegurando los días de bloque sean 7, 8 o 15
                double diasLibres;
                double diasBloque;
                
                //if(diasVacaciones >= 15) diasBloque = 15;
                //else if(diasVacaciones >= 8) diasBloque = 8;
                //else if(diasVacaciones >= 7) diasBloque = 7;
                //else diasBloque = 0;
                
                if(diasVacaciones >= 15) {
                    diasBloque = 15;
                    diasLibres = diasVacaciones - diasBloque;
                }else{
                    diasBloque = diasVacaciones;
                    diasLibres = 0;
                }

                //diasLibres = diasVacaciones - diasBloque;

                historial.Add(new HistorialVacacionesDto
                {
                    Periodo = año,
                    Vencidas = 0,
                    Pendientes = 0,
                    Truncas = 0,
                    DiasLibres = diasLibres,
                    DiasBloque = diasBloque
                });

                // ✅ Avanzar exactamente al próximo período usando la fecha exacta de ingreso
                periodoInicio = periodoInicio.AddYears(1);
            }

            // ✅ Clasificar correctamente los períodos en Vencidas, Pendientes y Truncas
            DateTime haceDosAnios = fechaActual.AddYears(-2);
            DateTime haceUnAnio = fechaActual.AddYears(-1);

            foreach (var item in historial)
            {
                DateTime fechaPeriodo = new DateTime(item.Periodo, fechaIngreso.Month, fechaIngreso.Day);

                if (fechaPeriodo < haceDosAnios)
                    item.Vencidas = item.DiasLibres + item.DiasBloque;
                else if (fechaPeriodo < haceUnAnio)
                    item.Pendientes = item.DiasLibres + item.DiasBloque;
                else
                    item.Truncas = item.DiasLibres + item.DiasBloque;
            }

            return historial;
        }

        private void AplicarDeducciones(List<HistorialVacacionesDto> historial, List<SolicitudVacaciones> solicitudes)
        {
            foreach (var periodo in historial)
            {
                // Buscamos todas las solicitudes que correspondan a este período
                var solicitudesDelPeriodo = solicitudes
                    .Where(s => s.Periodo == periodo.Periodo)
                    .ToList();

                // 1. Sumar días libres pedidos en este período
                int totalLibres = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "libres")
                    .Sum(s => s.DiasSolicitados);

                // 2. Sumar días de fin de semana, solo para solicitudes "libres"
                int totalFinde = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "libres")
                    .Sum(s => s.DiasFinde);

                // 3. Sumar días bloque pedidos en este período
                int totalBloque = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "bloque")
                    .Sum(s => s.DiasSolicitados);

                // 4. Restar en el período
                // Los fines de semana se restan junto con los días libres
                int dias_finde_sumar;
                if (totalFinde >=4) dias_finde_sumar = 0;
                else{
                    dias_finde_sumar = (int)totalLibres/5;
                    dias_finde_sumar *=2; 
                    dias_finde_sumar -= totalFinde;
                }
                periodo.DiasLibres -= totalLibres + dias_finde_sumar;
                if (periodo.DiasLibres < 0) periodo.DiasLibres = 0;

                periodo.DiasBloque -= totalBloque;
                if (periodo.DiasBloque < 0) periodo.DiasBloque = 0;
            }
        }
    }
}
