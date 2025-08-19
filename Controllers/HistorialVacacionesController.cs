using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_vacaciones_back.DTOs.HistorialVacaciones;
using sistema_vacaciones_back.DTOs.SaldoVacaciones;
using sistema_vacaciones_back.Extensions;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Security;

namespace sistema_vacaciones_back.Controllers
{
    [ApiController]
    [Route("api/historial-vacaciones")]
    [Authorize]
    public class HistorialVacacionesController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISolicitudVacacionesRepository _solicitudVacacionesRepository;
        private readonly ILogger<HistorialVacacionesController> _logger;

        public HistorialVacacionesController(
            IUsuarioRepository usuarioRepository,
            ISolicitudVacacionesRepository solicitudVacacionesRepository,
            ILogger<HistorialVacacionesController> logger)
        {
            _usuarioRepository = usuarioRepository;
            _solicitudVacacionesRepository = solicitudVacacionesRepository;
            _logger = logger;
        }

        [HttpGet]
        [OwnerOnly] // Atributo de seguridad personalizado
        public async Task<IActionResult> ObtenerHistorialVacaciones()
        {
            try
            {
                // Obtener el userId del token JWT con validación robusta
                var userId = User.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de acceso a historial con token inválido");
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Usuario {UserId} consultando historial de vacaciones", userId);

                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado en la base de datos", userId);
                    return NotFound("Usuario no encontrado");
                }

                if (usuario?.Persona?.FechaIngreso == null)
                {
                    _logger.LogWarning("Usuario {UserId} no tiene fecha de ingreso configurada", userId);
                    return BadRequest("Usuario no tiene fecha de ingreso configurada");
                }

                DateTime fechaIngreso = usuario.Persona.FechaIngreso;
                DateTime fechaActual = DateTime.Today.AddDays(1).AddTicks(-1); 
                
                _logger.LogInformation("Procesando historial para usuario {UserId} desde {FechaIngreso} hasta {FechaActual}", 
                    userId, fechaIngreso, fechaActual);

                // 1️⃣ Generar lista de períodos de vacaciones por año desde la fecha de ingreso hasta hoy
                var historial = GenerarPeriodosVacaciones(fechaIngreso, fechaActual);

                // 2️⃣ Obtener solicitudes de vacaciones aprobadas del usuario
                var solicitudes = await _solicitudVacacionesRepository.ObtenerSolicitudesAprobadasPendientes(userId);

                // 3️⃣ Restar días gastados según el orden correcto
                AplicarDeducciones(historial, solicitudes);

                var response = new
                {
                    UsuarioId = userId,
                    FechaIngreso = fechaIngreso,
                    Historial = historial
                };

                _logger.LogInformation("Historial de vacaciones generado exitosamente para usuario {UserId}", userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de vacaciones para usuario {UserId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
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
                    .Where(s => s.Periodo == periodo.Periodo && (s.Estado == "aprobado" || s.Estado == "pendiente"))
                    .ToList();

                // 1. Sumar días libres pedidos en este período
                int totalLibres = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "libres")
                    .Sum(s => s.DiasSolicitados); // dias totales solicitados = dias habiles + dias finde

                // 2. Sumar días de fin de semana, solo para solicitudes "libres"
                int totalFinde = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "libres")
                    .Sum(s => s.DiasFinde);

                int totalHabiles = totalLibres - totalFinde;

                int totalLibresFindeRestar = 0;
                int totalLibresRestar = 0;
                switch (totalHabiles)
                {
                    case 1:
                        totalLibresFindeRestar = 0;
                        break;
                    case 2:
                        totalLibresFindeRestar = 0;
                        break;
                    case 3:
                        totalLibresFindeRestar = 0;
                        break;
                    case 4:
                        totalLibresFindeRestar = 0;
                        break;
                    case 5:
                        totalLibresFindeRestar = 2;
                        break;
                    case 6:
                        totalLibresFindeRestar = 2;
                        break;
                    case 7:
                        totalLibresFindeRestar = 2;
                        break;
                    case 8:
                        totalLibresFindeRestar = 2;
                        break;
                    case 9:
                        totalLibresFindeRestar = 2;
                        break;
                    case 10:
                        totalLibresFindeRestar = 4;
                        break;
                    case 11:
                        totalLibresFindeRestar = 4;
                        break;
                    default:
                        totalLibresFindeRestar = 4;
                        break;
                }
                totalLibresRestar = totalHabiles + totalLibresFindeRestar;
                // 3. Sumar días bloque pedidos en este período
                int totalBloque = solicitudesDelPeriodo
                    .Where(s => s.TipoVacaciones == "bloque")
                    .Sum(s => s.DiasSolicitados);

                // 4. Restar en el período
                // Los fines de semana se restan junto con los días libres
                //int dias_finde_sumar;
                //if (totalFinde >=4) dias_finde_sumar = 0;
                //else{
                //    dias_finde_sumar = (int)totalLibres/5;
                //    dias_finde_sumar *=2; 
                //    dias_finde_sumar -= totalFinde;
                //}
                //periodo.DiasLibres -= totalLibres + dias_finde_sumar;
                periodo.DiasLibres -= totalLibresRestar;
                if (periodo.DiasLibres < 0) periodo.DiasLibres = 0;
                
                periodo.DiasBloque -= totalBloque;
                if (periodo.DiasBloque < 0) periodo.DiasBloque = 0;
            }
        }

        /// <summary>
        /// Obtiene los saldos de vacaciones del equipo para supervisores
        /// </summary>
        [HttpGet("saldos-equipo")]
        public async Task<IActionResult> ObtenerSaldosEquipo(
            [FromQuery] string? empleadoId = null,
            [FromQuery] int? periodo = null,
            [FromQuery] bool incluirSubordinadosNivelN = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            try
            {
                var supervisorId = User.GetUserId();
                if (string.IsNullOrEmpty(supervisorId))
                {
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                _logger.LogInformation("Supervisor {SupervisorId} consultando saldos del equipo", supervisorId);

                // Obtener empleados del equipo
                var empleados = await ObtenerEmpleadosDelEquipo(supervisorId, incluirSubordinadosNivelN);

                // Filtrar por empleado específico si se proporciona
                if (!string.IsNullOrEmpty(empleadoId))
                {
                    empleados = empleados.Where(e => e.Id == empleadoId).ToList();
                }

                // Calcular saldos para cada empleado
                var saldosCompletos = new List<SaldoVacacionesDto>();
                foreach (var empleado in empleados)
                {
                    var saldos = await CalcularSaldosEmpleado(empleado, periodo);
                    saldosCompletos.AddRange(saldos);
                }

                // Aplicar ordenamiento
                saldosCompletos = AplicarOrdenamientoSaldos(saldosCompletos, sortBy, isDescending);

                // Aplicar paginación
                var totalCompleto = saldosCompletos.Count;
                var saldosPaginados = saldosCompletos
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Calcular estadísticas
                var estadisticas = CalcularEstadisticasSaldos(saldosCompletos);

                var response = new
                {
                    Total = saldosPaginados.Count,
                    TotalCompleto = totalCompleto,
                    Saldos = saldosPaginados,
                    Supervisor = supervisorId,
                    Pagina = pageNumber,
                    TamanoPagina = pageSize,
                    Estadisticas = estadisticas
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener saldos del equipo para supervisor {SupervisorId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene los empleados del equipo para filtros
        /// </summary>
        [HttpGet("empleados-equipo")]
        public async Task<IActionResult> ObtenerEmpleadosEquipo([FromQuery] bool incluirSubordinadosNivelN = false)
        {
            try
            {
                var supervisorId = User.GetUserId();
                if (string.IsNullOrEmpty(supervisorId))
                {
                    return Unauthorized("Token inválido - Usuario no identificado");
                }

                var empleados = await ObtenerEmpleadosDelEquipo(supervisorId, incluirSubordinadosNivelN);

                var empleadosDto = empleados.Select(e => new
                {
                    Id = e.Id,
                    NombreCompleto = e.Persona?.Nombres ?? "Sin nombre",
                    Email = e.Email ?? "Sin email",
                    EsDirecto = e.JefeId == supervisorId
                }).ToList();

                var response = new
                {
                    Empleados = empleadosDto,
                    Total = empleadosDto.Count,
                    IncluirSubordinadosNivelN = incluirSubordinadosNivelN
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados del equipo para supervisor {SupervisorId}", User.GetUserId());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task<List<Usuario>> ObtenerEmpleadosDelEquipo(string supervisorId, bool incluirSubordinadosNivelN)
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            var supervisor = usuarios.FirstOrDefault(u => u.Id == supervisorId);

            if (supervisor == null)
            {
                return new List<Usuario>();
            }

            var empleados = new List<Usuario>();

            if (incluirSubordinadosNivelN)
            {
                // Incluir todos los subordinados de todos los niveles
                empleados = ObtenerSubordinadosRecursivo(usuarios.ToList(), supervisorId).ToList();
            }
            else
            {
                // Solo subordinados directos
                empleados = usuarios.Where(u => u.JefeId == supervisorId && !u.IsDeleted).ToList();
            }

            return empleados;
        }

        private IEnumerable<Usuario> ObtenerSubordinadosRecursivo(List<Usuario> todosUsuarios, string jefeId)
        {
            var subordinadosDirectos = todosUsuarios.Where(u => u.JefeId == jefeId && !u.IsDeleted);

            foreach (var subordinado in subordinadosDirectos)
            {
                yield return subordinado;

                // Recursivamente obtener subordinados de este empleado
                foreach (var subSubordinado in ObtenerSubordinadosRecursivo(todosUsuarios, subordinado.Id))
                {
                    yield return subSubordinado;
                }
            }
        }

        private async Task<List<SaldoVacacionesDto>> CalcularSaldosEmpleado(Usuario empleado, int? periodoFiltro = null)
        {
            if (empleado.Persona?.FechaIngreso == null)
            {
                return new List<SaldoVacacionesDto>();
            }

            DateTime fechaIngreso = empleado.Persona.FechaIngreso;
            DateTime fechaActual = DateTime.Today.AddDays(1).AddTicks(-1);

            // Generar períodos de vacaciones
            var historial = GenerarPeriodosVacaciones(fechaIngreso, fechaActual);

            // Obtener solicitudes aprobadas del empleado
            var solicitudes = await _solicitudVacacionesRepository.ObtenerSolicitudesAprobadasPendientes(empleado.Id);

            // Aplicar deducciones
            AplicarDeducciones(historial, solicitudes);

            // Convertir a DTOs de saldos
            var saldos = historial.Select(h => new SaldoVacacionesDto
            {
                Id = $"{empleado.Id}-{h.Periodo}",
                EmpleadoId = empleado.Id,
                NombreEmpleado = empleado.Persona?.Nombres ?? "Sin nombre",
                Email = empleado.Email ?? "Sin email",
                Periodo = h.Periodo,
                DiasVencidas = (int)h.Vencidas,
                DiasPendientes = (int)h.Pendientes,
                DiasTruncas = (int)h.Truncas,
                DiasLibres = (int)h.DiasLibres,
                DiasBloque = (int)h.DiasBloque,
                NombreManager = empleado.Jefe?.Persona?.Nombres,
                FechaCorte = new DateTime(h.Periodo, fechaIngreso.Month, fechaIngreso.Day)
            }).ToList();

            // Filtrar por período si se especifica
            if (periodoFiltro.HasValue)
            {
                saldos = saldos.Where(s => s.Periodo == periodoFiltro.Value).ToList();
            }

            return saldos;
        }

        private List<SaldoVacacionesDto> AplicarOrdenamientoSaldos(List<SaldoVacacionesDto> saldos, string? sortBy, bool isDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return saldos.OrderBy(s => s.NombreEmpleado).ThenByDescending(s => s.Periodo).ToList();
            }

            IOrderedEnumerable<SaldoVacacionesDto>? query = null;

            switch (sortBy.ToLower())
            {
                case "empleado.persona.nombres":
                case "empleado":
                case "nombreempleado":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.NombreEmpleado)
                        : saldos.OrderBy(s => s.NombreEmpleado);
                    break;
                case "periodo":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.Periodo)
                        : saldos.OrderBy(s => s.Periodo);
                    break;
                case "diasvencidas":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.DiasVencidas)
                        : saldos.OrderBy(s => s.DiasVencidas);
                    break;
                case "diaspendientes":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.DiasPendientes)
                        : saldos.OrderBy(s => s.DiasPendientes);
                    break;
                case "diastruncas":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.DiasTruncas)
                        : saldos.OrderBy(s => s.DiasTruncas);
                    break;
                case "diaslibres":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.DiasLibres)
                        : saldos.OrderBy(s => s.DiasLibres);
                    break;
                case "diasbloque":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.DiasBloque)
                        : saldos.OrderBy(s => s.DiasBloque);
                    break;
                case "manager.persona.nombres":
                case "nombremanager":
                    query = isDescending 
                        ? saldos.OrderByDescending(s => s.NombreManager ?? "")
                        : saldos.OrderBy(s => s.NombreManager ?? "");
                    break;
                default:
                    query = saldos.OrderBy(s => s.NombreEmpleado);
                    break;
            }

            return (query ?? saldos.OrderBy(s => s.NombreEmpleado)).ThenBy(s => s.Periodo).ToList();
        }

        private object CalcularEstadisticasSaldos(List<SaldoVacacionesDto> saldos)
        {
            var empleadosUnicos = saldos.Select(s => s.EmpleadoId).Distinct().Count();

            return new
            {
                TotalEmpleados = empleadosUnicos,
                TotalDiasVencidas = saldos.Sum(s => s.DiasVencidas),
                TotalDiasPendientes = saldos.Sum(s => s.DiasPendientes),
                TotalDiasTruncas = saldos.Sum(s => s.DiasTruncas),
                TotalDiasLibres = saldos.Sum(s => s.DiasLibres),
                TotalDiasBloque = saldos.Sum(s => s.DiasBloque)
            };
        }
    }
}
