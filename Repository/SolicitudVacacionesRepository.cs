using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Data;
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

        // Constantes para validación de cancelación
        private const string ESTADO_PENDIENTE = "pendiente";
        private const string ESTADO_CANCELADA = "cancelada";

        public SolicitudVacacionesRepository(ApplicationDBContext context, IUsuarioRepository usuarioRepository)
        {
            _context = context;
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Valida si una solicitud puede ser cancelada
        /// Reglas de negocio:
        /// 1. La solicitud debe estar en estado 'pendiente'
        /// 2. La fecha de inicio debe ser mayor que la fecha actual (no incluye hoy)
        /// </summary>
        /// <param name="solicitud">Solicitud a validar</param>
        /// <returns>Tupla con el resultado de la validación y mensaje de error si aplica</returns>
        private static (bool IsValid, string? ErrorMessage) ValidateCanCancelSolicitud(SolicitudVacaciones solicitud)
        {
            // Validar estado pendiente
            if (solicitud.Estado != ESTADO_PENDIENTE)
                return (false, "Solo se pueden cancelar solicitudes pendientes");

            // Validar fecha de inicio mayor que hoy
            var fechaHoy = DateTime.Today;
            var fechaInicioSolicitud = solicitud.FechaInicio.Date;
            
            if (fechaInicioSolicitud <= fechaHoy)
                return (false, $"Solo se pueden cancelar solicitudes cuya fecha de inicio sea posterior a hoy. Fecha inicio: {fechaInicioSolicitud:yyyy-MM-dd}, Fecha actual: {fechaHoy:yyyy-MM-dd}");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, SolicitudVacaciones? Solicitud)> CrearSolicitudVacaciones(SolicitudVacaciones solicitud, string usuarioId)
        {
            try
            {
                // 1️⃣ Validaciones básicas
                if (solicitud == null)
                    return (false, "La solicitud no puede ser nula.", null);

                if (string.IsNullOrEmpty(solicitud.Id))
                {
                    solicitud.Id = Guid.NewGuid().ToString(); // Asegurar que tenga ID
                }

                // Validación de usuario
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                    return (false, "El usuario no existe.", null);

                
                var restriccion = await _context.Restricciones.FirstOrDefaultAsync(x => x.IsDeleted != true);
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

            // Validacion de que no se puede pedir solo fechas que son fin de semana (ejm.solo sabado, o domingo, o ambos)
            if (nuevaFechaInicio.DayOfWeek == DayOfWeek.Saturday && nuevaFechaFin.DayOfWeek == DayOfWeek.Saturday)
            {
                return (false, "No se pueden solicitar vacaciones solo para sábados.", null);
            }
            if (nuevaFechaInicio.DayOfWeek == DayOfWeek.Sunday && nuevaFechaFin.DayOfWeek == DayOfWeek.Sunday)
            {
                return (false, "No se pueden solicitar vacaciones solo para domingos.", null);
            }
            if (nuevaFechaInicio.DayOfWeek == DayOfWeek.Saturday && nuevaFechaFin.DayOfWeek == DayOfWeek.Sunday)
            {
                return (false, "No se pueden solicitar vacaciones solo para sábados y domingos.", null);
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

            if (sabados + domingos == solicitud.DiasSolicitados)
            {
                return (false, "No se puede solicitar vacaciones solo para fines de semana.", null);
            }

            await _context.SolicitudesVacaciones.AddAsync(solicitud);
            await _context.SaveChangesAsync();
            return (true, null, solicitud);
            }
            catch (Exception ex)
            {
                // Log del error si tienes logging configurado
                throw new InvalidOperationException($"Error al crear solicitud de vacaciones: {ex.Message}", ex);
            }
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
                    s.SolicitanteId.ToString() == usuarioId &&
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
                    case 0:
                        totalLibresFindeRestar = 0;
                        break;
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

        public async Task<List<SolicitudVacaciones>> ObtenerSolicitudesAprobadasPendientes(string usuarioId)
        {
            
            return await _context.SolicitudesVacaciones
                .Where(s => s.SolicitanteId.ToString() == usuarioId && (s.Estado == "aprobado" || s.Estado == "pendiente"))
                .OrderBy(s => s.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<SolicitudVacaciones>> GetSolicitudesPagination(SolicitudesQueryObject queryObject, string usuarioId)
        {
            var solicitudes = _context.SolicitudesVacaciones
                .Include(s => s.Solicitante!)
                    .ThenInclude(u => u.Persona)
                .Include(s => s.Solicitante!)
                    .ThenInclude(u => u.Jefe!)
                        .ThenInclude(j => j.Persona)
                .Include(s => s.Aprobador!)
                    .ThenInclude(a => a.Persona)
                .AsQueryable();
            
            // FILTRO PRINCIPAL: Solo solicitudes del usuario actual
            solicitudes = solicitudes.Where(s => s.SolicitanteId == usuarioId);
            
            // Filtro por busqueda de texto - barra de busqueda
            if (!string.IsNullOrWhiteSpace(queryObject.SearchValue))
            {
                solicitudes = solicitudes.Where(s => s.Estado.Contains(queryObject.SearchValue));
            }

            if(!string.IsNullOrWhiteSpace(queryObject.Estado))
            {
                solicitudes = solicitudes.Where(s => s.Estado == queryObject.Estado);
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

            // Aplicar ordenamiento dinámico usando el método centralizado
            solicitudes = ApplyOrdering(solicitudes, queryObject.SortBy, queryObject.IsDescending);

            var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            return await solicitudes.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();            
        }

        /// <summary>
        /// Obtiene el detalle de una solicitud por ID con todas las relaciones
        /// </summary>
        public async Task<SolicitudVacaciones?> GetSolicitudByIdAsync(string solicitudId)
        {
            try
            {
                return await _context.SolicitudesVacaciones
                    .Include(s => s.Solicitante!)
                        .ThenInclude(u => u.Persona)
                    .Include(s => s.Solicitante!)
                        .ThenInclude(u => u.Jefe!)
                            .ThenInclude(j => j.Persona)
                    .Include(s => s.Aprobador!)
                        .ThenInclude(u => u.Persona)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener solicitud por ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancela una solicitud de vacaciones
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> CancelarSolicitudAsync(
            string solicitudId, 
            string usuarioId, 
            string? motivo)
        {
            try
            {
                var solicitud = await _context.SolicitudesVacaciones
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return (false, "La solicitud no existe");

                // Validar que sea el propietario
                if (solicitud.SolicitanteId != usuarioId)
                    return (false, "No tienes permisos para cancelar esta solicitud");

                // Validar que se pueda cancelar usando las reglas de negocio
                var validationResult = ValidateCanCancelSolicitud(solicitud);
                if (!validationResult.IsValid)
                    return (false, validationResult.ErrorMessage);

                // Actualizar estado a cancelado
                solicitud.Estado = ESTADO_CANCELADA;
                if (!string.IsNullOrEmpty(motivo))
                {
                    solicitud.Comentarios = $"Cancelado por usuario: {motivo}";
                }

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cancelar solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Aprueba o rechaza una solicitud de vacaciones
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AprobarRechazarSolicitudAsync(
            string solicitudId, 
            string aprobadorId, 
            string accion, 
            string? comentarios)
        {
            try
            {
                var solicitud = await _context.SolicitudesVacaciones
                    .Include(s => s.Solicitante)
                        .ThenInclude(u => u!.Jefe)
                            .ThenInclude(j => j!.Persona)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return (false, "La solicitud no existe");

                // Validar que no sea su propia solicitud
                if (solicitud.SolicitanteId == aprobadorId)
                    return (false, "No puedes aprobar tu propia solicitud");

                // Validar que esté pendiente
                if (solicitud.Estado != "pendiente")
                    return (false, "Solo se pueden aprobar/rechazar solicitudes pendientes");

                // Verificar permisos de aprobación específicos (jefe directo o admin)
                var canApprove = await CanUserApproveSpecificSolicitudAsync(aprobadorId, solicitudId);
                if (!canApprove)
                    return (false, "No tienes permisos para aprobar esta solicitud. Solo el jefe directo o administradores pueden aprobar solicitudes.");

                // Actualizar solicitud
                if (accion == "aprobar")
                {
                    solicitud.Estado = "aprobado";
                    solicitud.AprobadorId = aprobadorId;
                    solicitud.FechaAprobacion = DateTime.UtcNow;
                }
                else if (accion == "rechazar")
                {
                    solicitud.Estado = "rechazado";
                    solicitud.AprobadorId = aprobadorId;
                    solicitud.FechaAprobacion = DateTime.UtcNow;
                }
                else
                {
                    return (false, "Acción inválida. Debe ser 'aprobar' o 'rechazar'");
                }

                if (!string.IsNullOrEmpty(comentarios))
                {
                    solicitud.Comentarios = comentarios;
                }

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al procesar solicitud: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si un usuario puede aprobar solicitudes
        /// </summary>
        public async Task<bool> CanUserApproveSolicitudesAsync(string usuarioId)
        {
            try
            {
                var usuario = await _context.Users
                    .Where(u => u.Id == usuarioId)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return false;

                // Verificar si tiene rol de admin
                var userRoles = await _usuarioRepository.GetUserRolesAsync(usuario);
                
                return userRoles.Contains("Admin") || 
                       userRoles.Contains("Administrador") || 
                       userRoles.Contains("Jefe") ||
                       userRoles.Contains("Supervisor");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si un usuario puede aprobar una solicitud específica (jefe directo o admin)
        /// </summary>
        public async Task<bool> CanUserApproveSpecificSolicitudAsync(string aprobadorId, string solicitudId)
        {
            try
            {
                var solicitud = await _context.SolicitudesVacaciones
                    .Include(s => s.Solicitante)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                    return false;

                var aprobador = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == aprobadorId);

                if (aprobador == null)
                    return false;

                // Verificar si tiene rol de administrador (puede aprobar cualquier solicitud)
                var userRoles = await _usuarioRepository.GetUserRolesAsync(aprobador);
                if (userRoles.Contains("Admin") || userRoles.Contains("Administrador"))
                    return true;

                // Verificar si es jefe directo (nivel 1) del solicitante
                if (solicitud.Solicitante?.JefeId == aprobadorId)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene solicitudes del equipo para gestión por supervisores
        /// </summary>
        public async Task<(List<SolicitudVacaciones> Solicitudes, int TotalCount)> GetSolicitudesEquipo(SolicitudesQueryObject queryObject, string supervisorId)
        {
            try
            {
                var query = _context.SolicitudesVacaciones
                    .Include(s => s.Solicitante!)
                        .ThenInclude(u => u.Persona)
                    .Include(s => s.Solicitante!)
                        .ThenInclude(u => u.Jefe!)
                            .ThenInclude(j => j.Persona)
                    .Include(s => s.Aprobador!)
                        .ThenInclude(u => u.Persona)
                    .AsQueryable();

                // Filtrar por empleados del equipo
                if (queryObject.IncluirSubordinadosNivelN == true)
                {
                    // Obtener todos los subordinados de nivel N
                    var subordinadosIds = await GetSubordinadosRecursivos(supervisorId);
                    query = query.Where(s => subordinadosIds.Contains(s.SolicitanteId));
                }
                else
                {
                    // Solo empleados directos (nivel 1)
                    query = query.Where(s => s.Solicitante != null && s.Solicitante.JefeId == supervisorId);
                }

                // Aplicar filtros
                if (!string.IsNullOrEmpty(queryObject.Estado))
                {
                    query = query.Where(s => s.Estado.ToLower() == queryObject.Estado.ToLower());
                }

                if (!string.IsNullOrEmpty(queryObject.TipoVacaciones))
                {
                    query = query.Where(s => s.TipoVacaciones.ToLower() == queryObject.TipoVacaciones.ToLower());
                }

                if (queryObject.Periodo.HasValue)
                {
                    query = query.Where(s => s.Periodo == queryObject.Periodo.Value);
                }

                if (!string.IsNullOrEmpty(queryObject.EmpleadoId))
                {
                    query = query.Where(s => s.SolicitanteId == queryObject.EmpleadoId);
                }

                if (queryObject.FechaInicio.HasValue)
                {
                    query = query.Where(s => s.FechaInicio >= queryObject.FechaInicio.Value);
                }

                if (queryObject.FechaFin.HasValue)
                {
                    query = query.Where(s => s.FechaFin <= queryObject.FechaFin.Value);
                }

                // Filtros de rango para fecha de inicio de vacaciones
                if (queryObject.FechaInicioRango.HasValue)
                {
                    query = query.Where(s => s.FechaInicio >= queryObject.FechaInicioRango.Value);
                }

                if (queryObject.FechaFinRango.HasValue)
                {
                    query = query.Where(s => s.FechaInicio <= queryObject.FechaFinRango.Value);
                }

                // Aplicar ordenamiento dinámico
                query = ApplyOrdering(query, queryObject.SortBy, queryObject.IsDescending);

                // Obtener el total de registros sin paginación
                var totalCount = await query.CountAsync();

                // Aplicar paginación
                var solicitudes = await query
                    .Skip((queryObject.PageNumber - 1) * queryObject.PageSize)
                    .Take(queryObject.PageSize)
                    .ToListAsync();

                return (solicitudes, totalCount);
            }
            catch (Exception)
            {
                return (new List<SolicitudVacaciones>(), 0);
            }
        }

        /// <summary>
        /// Obtiene estadísticas de solicitudes del equipo
        /// </summary>
        public async Task<object> GetEstadisticasEquipo(string supervisorId, SolicitudesQueryObject queryObject)
        {
            try
            {
                var query = _context.SolicitudesVacaciones.AsQueryable();

                // Filtrar por empleados del equipo
                if (queryObject.IncluirSubordinadosNivelN == true)
                {
                    var subordinadosIds = await GetSubordinadosRecursivos(supervisorId);
                    query = query.Where(s => subordinadosIds.Contains(s.SolicitanteId));
                }
                else
                {
                    query = query.Where(s => s.Solicitante != null && s.Solicitante.JefeId == supervisorId);
                }

                // Aplicar los mismos filtros que en GetSolicitudesEquipo
                if (!string.IsNullOrEmpty(queryObject.Estado))
                {
                    query = query.Where(s => s.Estado.ToLower() == queryObject.Estado.ToLower());
                }

                if (!string.IsNullOrEmpty(queryObject.TipoVacaciones))
                {
                    query = query.Where(s => s.TipoVacaciones.ToLower() == queryObject.TipoVacaciones.ToLower());
                }

                if (queryObject.Periodo.HasValue)
                {
                    query = query.Where(s => s.Periodo == queryObject.Periodo.Value);
                }

                if (!string.IsNullOrEmpty(queryObject.EmpleadoId))
                {
                    query = query.Where(s => s.SolicitanteId == queryObject.EmpleadoId);
                }

                if (queryObject.FechaInicio.HasValue)
                {
                    query = query.Where(s => s.FechaInicio >= queryObject.FechaInicio.Value);
                }

                if (queryObject.FechaFin.HasValue)
                {
                    query = query.Where(s => s.FechaFin <= queryObject.FechaFin.Value);
                }

                // Filtros de rango para fecha de inicio de vacaciones
                if (queryObject.FechaInicioRango.HasValue)
                {
                    query = query.Where(s => s.FechaInicio >= queryObject.FechaInicioRango.Value);
                }

                if (queryObject.FechaFinRango.HasValue)
                {
                    query = query.Where(s => s.FechaInicio <= queryObject.FechaFinRango.Value);
                }

                var estadisticas = await query
                    .GroupBy(s => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Pendientes = g.Count(s => s.Estado.ToLower() == "pendiente"),
                        Aprobadas = g.Count(s => s.Estado.ToLower() == "aprobado"),
                        Rechazadas = g.Count(s => s.Estado.ToLower() == "rechazado"),
                        Canceladas = g.Count(s => s.Estado.ToLower() == "cancelado")
                    })
                    .FirstOrDefaultAsync();

                return estadisticas ?? new
                {
                    Total = 0,
                    Pendientes = 0,
                    Aprobadas = 0,
                    Rechazadas = 0,
                    Canceladas = 0
                };
            }
            catch (Exception)
            {
                return new
                {
                    Total = 0,
                    Pendientes = 0,
                    Aprobadas = 0,
                    Rechazadas = 0,
                    Canceladas = 0
                };
            }
        }

        /// <summary>
        /// Obtiene subordinados de manera recursiva
        /// </summary>
        private async Task<List<string>> GetSubordinadosRecursivos(string supervisorId)
        {
            var subordinados = new List<string>();
            var queue = new Queue<string>();
            queue.Enqueue(supervisorId);
            var visitados = new HashSet<string>();

            while (queue.Count > 0)
            {
                var currentSupervisorId = queue.Dequeue();
                
                if (visitados.Contains(currentSupervisorId))
                    continue;
                    
                visitados.Add(currentSupervisorId);

                var empleadosDirectos = await _context.Users
                    .Where(u => u.JefeId == currentSupervisorId)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var empleadoId in empleadosDirectos)
                {
                    if (!subordinados.Contains(empleadoId))
                    {
                        subordinados.Add(empleadoId);
                        queue.Enqueue(empleadoId); // Para buscar sus subordinados
                    }
                }
            }

            return subordinados;
        }

        /// <summary>
        /// Obtiene una solicitud por ID
        /// </summary>
        public async Task<SolicitudVacaciones?> GetByIdAsync(int solicitudId)
        {
            try
            {
                return await _context.SolicitudesVacaciones
                    .Include(s => s.Solicitante)
                        .ThenInclude(u => u!.Persona)
                    .Include(s => s.Solicitante)
                        .ThenInclude(u => u!.Jefe)
                            .ThenInclude(j => j!.Persona)
                    .Include(s => s.Aprobador)
                        .ThenInclude(a => a!.Persona)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Aplica ordenamiento dinámico a la consulta de solicitudes
        /// </summary>
        private IQueryable<SolicitudVacaciones> ApplyOrdering(IQueryable<SolicitudVacaciones> query, string? sortBy, bool isDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                // Ordenamiento por defecto: fecha de solicitud descendente
                return query.OrderByDescending(s => s.FechaSolicitud);
            }

            // Logging para debug
            Console.WriteLine($"[DEBUG] ApplyOrdering - sortBy: '{sortBy}', isDescending: {isDescending}");

            // Normalizar el campo de ordenamiento (case-insensitive)
            var sortField = sortBy.ToLowerInvariant();
            Console.WriteLine($"[DEBUG] ApplyOrdering - sortField normalizado: '{sortField}'");

            // Aplicar ordenamiento según el campo
            if (sortField == "solicitante.persona.nombres")
            {
                Console.WriteLine($"[DEBUG] Aplicando ordenamiento por Solicitante.Persona.Nombres, isDescending: {isDescending}");
                return isDescending
                    ? query.OrderByDescending(s => s.Solicitante!.Persona!.Nombres)
                    : query.OrderBy(s => s.Solicitante!.Persona!.Nombres);
            }
            
            return sortField switch
            {
                "id" => isDescending 
                    ? query.OrderByDescending(s => s.Id) 
                    : query.OrderBy(s => s.Id),
                
                "tipovacaciones" => isDescending
                    ? query.OrderByDescending(s => s.TipoVacaciones)
                    : query.OrderBy(s => s.TipoVacaciones),
                
                "fechainicio" => isDescending
                    ? query.OrderByDescending(s => s.FechaInicio)
                    : query.OrderBy(s => s.FechaInicio),
                
                "fechafin" => isDescending
                    ? query.OrderByDescending(s => s.FechaFin)
                    : query.OrderBy(s => s.FechaFin),
                
                "diassolicitados" => isDescending
                    ? query.OrderByDescending(s => s.DiasSolicitados)
                    : query.OrderBy(s => s.DiasSolicitados),
                
                "estado" => isDescending
                    ? query.OrderByDescending(s => s.Estado)
                    : query.OrderBy(s => s.Estado),
                
                "fechasolicitud" => isDescending
                    ? query.OrderByDescending(s => s.FechaSolicitud)
                    : query.OrderBy(s => s.FechaSolicitud),
                
                "periodo" => isDescending
                    ? query.OrderByDescending(s => s.Periodo)
                    : query.OrderBy(s => s.Periodo),
                
                "aprobador.persona.nombres" => isDescending
                    ? query.OrderByDescending(s => s.Aprobador!.Persona!.Nombres)
                    : query.OrderBy(s => s.Aprobador!.Persona!.Nombres),
                
                "fechaaprobacion" => isDescending
                    ? query.OrderByDescending(s => s.FechaAprobacion)
                    : query.OrderBy(s => s.FechaAprobacion),
                
                "jefe.persona.nombres" => isDescending
                    ? query.OrderByDescending(s => s.Solicitante!.Jefe!.Persona!.Nombres)
                    : query.OrderBy(s => s.Solicitante!.Jefe!.Persona!.Nombres),
                
                // Fallback a ordenamiento por defecto
                _ => query.OrderByDescending(s => s.FechaSolicitud)
            };
        }
    }
}