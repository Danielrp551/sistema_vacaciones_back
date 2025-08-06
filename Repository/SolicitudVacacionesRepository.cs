using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SISTEMA_VACACIONES.Data;
using sistema_vacaciones_back.DTOs.HistorialVacaciones;
using sistema_vacaciones_back.DTOs.SolicitudVacaciones;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Repository
{
    public class SolicitudVacacionesRepository : ISolicitudVacacionesRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IUsuarioRepository _usuarioRepository;

        public SolicitudVacacionesRepository(ApplicationDBContext context, IUsuarioRepository usuarioRepository)
        {
            _context = context;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, SolicitudVacaciones? Solicitud)> CrearSolicitudVacaciones(SolicitudVacaciones solicitud, string usuarioId)
        {
            // Validaciones
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                return (false, "El usuario no existe.", null);

            
            var restriccion = await _context.Restricciones.FirstOrDefaultAsync(x => x.Activo == true);
            if(restriccion == null)
            {
                restriccion = new Restriccion();
                restriccion.FechaLimiteMes = 15;
            }
            DateTime hoy = DateTime.Today;
    
            bool esMismoMesAnio = 
                (solicitud.FechaInicio.Year == hoy.Year) &&
                (solicitud.FechaInicio.Month == hoy.Month);

            if (esMismoMesAnio && hoy.Day > restriccion.FechaLimiteMes)
            {
                return (false, 
                    $"No se pueden solicitar vacaciones para este mes después del día {restriccion.FechaLimiteMes}.", 
                    null
                );
            }

            DateTime nuevaFechaInicio = solicitud.FechaInicio.Date;
            DateTime nuevaFechaFin = solicitud.FechaFin.Date;

            bool existeSolapamiento = await ExisteSolapamiento(nuevaFechaInicio, nuevaFechaFin, usuarioId);
            if (existeSolapamiento)
            {
                return (false, "Existe solapamiento con otra solicitud en estado pendiente o aprobado.", null);
            }

            DateTime fechaIngreso = usuario.Persona.FechaIngreso;
            DateTime fechaActual = solicitud.FechaInicio.AddDays(1).AddTicks(-1); 

            var historial = GenerarPeriodosVacaciones(fechaIngreso, fechaActual);

            // 2️⃣ Obtener solicitudes de vacaciones aprobadas del usuario
            var solicitudes = await ObtenerSolicitudesAprobadasPendientes(usuarioId);

            // 3️⃣ Restar días gastados según el orden correcto
            AplicarDeducciones(historial, solicitudes);

            //Validad si puede gastar los días solicitados

            var periodoSolicitado = historial.FirstOrDefault(h => h.Periodo == solicitud.Periodo);

            if (periodoSolicitado == null)
                return (false, "No se encontró el período solicitado.", null);

            if (solicitud.TipoVacaciones == "libres" && solicitud.DiasSolicitados > periodoSolicitado.DiasLibres)
                return (false, "No tienes suficientes días libres en ese período.", null);
            if (solicitud.TipoVacaciones == "bloque" && solicitud.DiasSolicitados > periodoSolicitado.DiasBloque)
                return (false, "No tienes suficientes días de bloque en ese período.", null);

            int sabados = 0;
            int domingos = 0;

            DateTime fechaActualSolicitud = solicitud.FechaInicio.Date;
            DateTime fechaFin = solicitud.FechaFin.Date;

            while (fechaActualSolicitud <= fechaFin)
            {
                if (fechaActualSolicitud.DayOfWeek == DayOfWeek.Saturday)
                    sabados++;
                else if (fechaActualSolicitud.DayOfWeek == DayOfWeek.Sunday)
                    domingos++;

                fechaActualSolicitud = fechaActualSolicitud.AddDays(1);
            }

            solicitud.DiasFinde = sabados + domingos;

            await _context.SolicitudesVacaciones.AddAsync(solicitud);
            await _context.SaveChangesAsync();
            return (true, null, solicitud);
        }

        /// <summary>
        /// Valida si existe otra solicitud (pendiente o aprobada) con fechas que choquen con [fechaInicio, fechaFin].
        /// </summary>
        private async Task<bool> ExisteSolapamiento(DateTime fechaInicio, DateTime fechaFin, string usuarioId)
        {
            // La condición para solapamiento de rangos [A, B] y [C, D] es:
            // A <= D && B >= C
            return await _context.SolicitudesVacaciones
                .AnyAsync(s =>
                    s.UsuarioId == usuarioId &&
                    (s.Estado == "pendiente" || s.Estado == "aprobado") &&
                    s.FechaInicio.Date <= fechaFin &&
                    s.FechaFin.Date >= fechaInicio
                );
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

        public async Task<List<SolicitudVacaciones>> ObtenerSolicitudesAprobadasPendientes(string usuarioId)
        {
            return await _context.SolicitudesVacaciones
                .Where(s => s.UsuarioId == usuarioId && (s.Estado == "aprobado" || s.Estado == "pendiente"))
                .OrderBy(s => s.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<SolicitudVacaciones>> GetSolicitudesPagination(SolicitudesQueryObject queryObject, string usuarioId)
        {
            var solicitudes = _context.SolicitudesVacaciones.AsQueryable();
            // Filtro por busqueda de texto - barra de busqueda
            if (!string.IsNullOrWhiteSpace(queryObject.SearchValue))
            {
                solicitudes = solicitudes.Where(s => s.UsuarioId == usuarioId && s.Estado.Contains(queryObject.SearchValue));
            }

            if(!string.IsNullOrWhiteSpace(queryObject.Estado))
            {
                solicitudes = solicitudes.Where(s => s.UsuarioId == usuarioId && s.Estado == queryObject.Estado);
            }

            if(queryObject.Id != null)
            {
                solicitudes = solicitudes.Where(s => s.Id == queryObject.Id);
            }

            if(queryObject.Periodo != null)
            {
                solicitudes = solicitudes.Where(s => s.Periodo == queryObject.Periodo);
            }

            if(queryObject.DiasSolicitados != null)
            {
                solicitudes = solicitudes.Where(s => s.DiasSolicitados == queryObject.DiasSolicitados);
            }

            if(!string.IsNullOrWhiteSpace(queryObject.TipoVacaciones))
            {
                solicitudes = solicitudes.Where(s => s.TipoVacaciones == queryObject.TipoVacaciones);
            }

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                IOrderedQueryable<SolicitudVacaciones> orderedSolicitudes;

                if (queryObject.SortBy.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.Id)
                        : solicitudes.OrderBy(p => p.Id);
                }
                else if (queryObject.SortBy.Equals("periodo", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.Periodo)
                        : solicitudes.OrderBy(p => p.Periodo);
                }
                else if (queryObject.SortBy.Equals("diasSolicitados", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.DiasSolicitados)
                        : solicitudes.OrderBy(p => p.DiasSolicitados);
                }
                else if (queryObject.SortBy.Equals("tipoVacaciones", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.TipoVacaciones)
                        : solicitudes.OrderBy(p => p.TipoVacaciones);
                }
                else if (queryObject.SortBy.Equals("fechaInicio", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.FechaInicio)
                        : solicitudes.OrderBy(p => p.FechaInicio);
                }
                else if (queryObject.SortBy.Equals("fechaSolicitud", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.FechaSolicitud)
                        : solicitudes.OrderBy(p => p.FechaSolicitud);
                }
                else if (queryObject.SortBy.Equals("estado", StringComparison.OrdinalIgnoreCase))
                {
                    orderedSolicitudes = queryObject.IsDescending
                        ? solicitudes.OrderByDescending(p => p.Estado)
                        : solicitudes.OrderBy(p => p.Estado);
                }
                else
                {
                    // En caso de que SortBy tenga otro valor, se utiliza fecha de edición
                    orderedSolicitudes = solicitudes.OrderByDescending(p => p.FechaSolicitud);
                }

                // Ordenamiento secundario por FechaUltimaEdicion
                solicitudes = orderedSolicitudes.ThenByDescending(p => p.FechaSolicitud);
            }
            else
            {
                // Si no se especifica SortBy, ordenar solamente por FechaUltimaEdicion
                solicitudes = solicitudes.OrderByDescending(p => p.FechaSolicitud);
            }

            var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            var solicitudesList = await solicitudes.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();

            return await solicitudes.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();            
        }
    }
}