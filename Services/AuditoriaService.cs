using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.Text.Json;
using sistema_vacaciones_back.Data;
using sistema_vacaciones_back.DTOs.Auditoria;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Services
{
    /// <summary>
    /// Servicio para gestionar la auditoría del sistema
    /// </summary>
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<Usuario> _userManager;

        public AuditoriaService(ApplicationDBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== MÉTODOS DE REGISTRO =====

        public async Task<string> RegistrarAccionAsync(
            CrearAuditoriaDto dto, 
            string usuarioEjecutorId, 
            string ipAddress, 
            string? userAgent = null, 
            string? sessionId = null)
        {
            var usuarioEjecutor = await _userManager.Users
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == usuarioEjecutorId);
            
            if (usuarioEjecutor == null)
                throw new ArgumentException($"Usuario ejecutor con ID {usuarioEjecutorId} no encontrado");

            Usuario? usuarioAfectado = null;
            if (!string.IsNullOrEmpty(dto.UsuarioAfectadoId))
            {
                usuarioAfectado = await _userManager.Users
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Id == dto.UsuarioAfectadoId);
            }

            var nombreEjecutor = usuarioEjecutor.Persona != null 
                ? $"{usuarioEjecutor.Persona.Nombres} {usuarioEjecutor.Persona.ApellidoPaterno}".Trim()
                : usuarioEjecutor.UserName ?? "Usuario";

            var nombreAfectado = usuarioAfectado?.Persona != null 
                ? $"{usuarioAfectado.Persona.Nombres} {usuarioAfectado.Persona.ApellidoPaterno}".Trim()
                : usuarioAfectado?.UserName;

            var mensajes = GenerarMensajes(dto.TipoAccion, nombreEjecutor, nombreAfectado, dto.Motivo);

            var auditoria = new AuditoriaAcciones
            {
                TipoAccion = dto.TipoAccion,
                Modulo = dto.Modulo,
                TablaAfectada = dto.TablaAfectada,
                RegistroAfectadoId = dto.RegistroAfectadoId,
                
                UsuarioEjecutorId = usuarioEjecutor.Id,
                UsuarioEjecutorNombre = nombreEjecutor,
                UsuarioEjecutorEmail = usuarioEjecutor.Email,
                
                UsuarioAfectadoId = usuarioAfectado?.Id,
                UsuarioAfectadoNombre = nombreAfectado,
                UsuarioAfectadoEmail = usuarioAfectado?.Email,
                
                MensajeCorto = mensajes.mensajeCorto,
                MensajeDetallado = mensajes.mensajeDetallado,
                MensajePlantilla = mensajes.mensajePlantilla,
                
                Motivo = dto.Motivo,
                Observaciones = dto.Observaciones,
                ValoresAnteriores = dto.ValoresAnteriores,
                ValoresNuevos = dto.ValoresNuevos,
                
                IpAddress = ipAddress,
                UserAgent = userAgent,
                SessionId = sessionId,
                
                Severidad = dto.Severidad,
                Tags = dto.Tags,
                MetadatosExtras = dto.MetadatosExtras,
                
                FechaHora = DateTime.UtcNow,
                EsVisible = true
            };

            _context.AuditoriaAcciones.Add(auditoria);
            await _context.SaveChangesAsync();

            return auditoria.Id;
        }

        public async Task<string> RegistrarAccionSimpleAsync(
            TipoAccionAuditoria tipoAccion,
            ModuloSistema modulo,
            string tablaAfectada,
            string registroAfectadoId,
            string usuarioEjecutorId,
            string? usuarioAfectadoId = null,
            string? motivo = null,
            string ipAddress = "0.0.0.0",
            SeveridadAuditoria severidad = SeveridadAuditoria.INFO)
        {
            var dto = new CrearAuditoriaDto
            {
                TipoAccion = tipoAccion,
                Modulo = modulo,
                TablaAfectada = tablaAfectada,
                RegistroAfectadoId = registroAfectadoId,
                UsuarioAfectadoId = usuarioAfectadoId,
                Motivo = motivo,
                Severidad = severidad
            };

            return await RegistrarAccionAsync(dto, usuarioEjecutorId, ipAddress);
        }

        // ===== MÉTODOS DE CONSULTA =====

        public async Task<AuditoriaPaginadaDto> ObtenerRegistrosAsync(ConsultarAuditoriaDto filtros)
        {
            var query = _context.AuditoriaAcciones.AsQueryable();

            // Aplicar filtros
            if (filtros.Modulo.HasValue)
                query = query.Where(a => a.Modulo == filtros.Modulo.Value);

            if (filtros.TipoAccion.HasValue)
                query = query.Where(a => a.TipoAccion == filtros.TipoAccion.Value);

            if (!string.IsNullOrEmpty(filtros.UsuarioEjecutorId))
                query = query.Where(a => a.UsuarioEjecutorId == filtros.UsuarioEjecutorId);

            if (!string.IsNullOrEmpty(filtros.UsuarioAfectadoId))
                query = query.Where(a => a.UsuarioAfectadoId == filtros.UsuarioAfectadoId);

            if (!string.IsNullOrEmpty(filtros.TablaAfectada))
                query = query.Where(a => a.TablaAfectada == filtros.TablaAfectada);

            if (!string.IsNullOrEmpty(filtros.RegistroAfectadoId))
                query = query.Where(a => a.RegistroAfectadoId == filtros.RegistroAfectadoId);

            if (filtros.FechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= filtros.FechaDesde.Value);

            if (filtros.FechaHasta.HasValue)
                query = query.Where(a => a.FechaHora <= filtros.FechaHasta.Value);

            if (filtros.Severidad.HasValue)
                query = query.Where(a => a.Severidad == filtros.Severidad.Value);

            if (!string.IsNullOrEmpty(filtros.Tags))
                query = query.Where(a => a.Tags != null && a.Tags.Contains(filtros.Tags));

            if (filtros.SoloVisibles)
                query = query.Where(a => a.EsVisible);

            // Contar total antes de paginación
            var totalRegistros = await query.CountAsync();

            // Aplicar ordenamiento
            query = filtros.OrdenarPor.ToLower() switch
            {
                "fechahora" => filtros.OrdenDescendente ? query.OrderByDescending(a => a.FechaHora) : query.OrderBy(a => a.FechaHora),
                "tipoAccion" => filtros.OrdenDescendente ? query.OrderByDescending(a => a.TipoAccion) : query.OrderBy(a => a.TipoAccion),
                "modulo" => filtros.OrdenDescendente ? query.OrderByDescending(a => a.Modulo) : query.OrderBy(a => a.Modulo),
                "usuarioejecutor" => filtros.OrdenDescendente ? query.OrderByDescending(a => a.UsuarioEjecutorNombre) : query.OrderBy(a => a.UsuarioEjecutorNombre),
                _ => filtros.OrdenDescendente ? query.OrderByDescending(a => a.FechaHora) : query.OrderBy(a => a.FechaHora)
            };

            // Aplicar paginación
            var registros = await query
                .Skip((filtros.Pagina - 1) * filtros.TamanoPagina)
                .Take(filtros.TamanoPagina)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    TablaAfectada = a.TablaAfectada,
                    RegistroAfectadoId = a.RegistroAfectadoId,
                    UsuarioEjecutorId = a.UsuarioEjecutorId,
                    UsuarioEjecutorNombre = a.UsuarioEjecutorNombre,
                    UsuarioEjecutorEmail = a.UsuarioEjecutorEmail,
                    UsuarioAfectadoId = a.UsuarioAfectadoId,
                    UsuarioAfectadoNombre = a.UsuarioAfectadoNombre,
                    UsuarioAfectadoEmail = a.UsuarioAfectadoEmail,
                    MensajeCorto = a.MensajeCorto,
                    MensajeDetallado = a.MensajeDetallado,
                    MensajePlantilla = a.MensajePlantilla,
                    Motivo = a.Motivo,
                    Observaciones = a.Observaciones,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    FechaHora = a.FechaHora,
                    Severidad = a.Severidad,
                    EsVisible = a.EsVisible,
                    Tags = a.Tags,
                    SessionId = a.SessionId,
                    TiempoEjecucionMs = a.TiempoEjecucionMs
                })
                .ToListAsync();

            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / filtros.TamanoPagina);

            return new AuditoriaPaginadaDto
            {
                Registros = registros,
                TotalRegistros = totalRegistros,
                PaginaActual = filtros.Pagina,
                TotalPaginas = totalPaginas,
                TamanoPagina = filtros.TamanoPagina,
                TienePaginaAnterior = filtros.Pagina > 1,
                TienePaginaSiguiente = filtros.Pagina < totalPaginas
            };
        }

        public async Task<AuditoriaPaginadaDto> ObtenerHistorialPorModuloAsync(
            ModuloSistema modulo, 
            int pagina = 1, 
            int tamanoPagina = 20)
        {
            var filtros = new ConsultarAuditoriaDto
            {
                Modulo = modulo,
                Pagina = pagina,
                TamanoPagina = tamanoPagina
            };

            return await ObtenerRegistrosAsync(filtros);
        }

        public async Task<AuditoriaPaginadaDto> ObtenerHistorialPorUsuarioAsync(
            string usuarioId, 
            bool comoEjecutor = true, 
            int pagina = 1, 
            int tamanoPagina = 20)
        {
            var filtros = new ConsultarAuditoriaDto
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina
            };

            if (comoEjecutor)
                filtros.UsuarioEjecutorId = usuarioId;
            else
                filtros.UsuarioAfectadoId = usuarioId;

            return await ObtenerRegistrosAsync(filtros);
        }

        public async Task<List<AuditoriaDto>> ObtenerHistorialPorRegistroAsync(
            string tablaAfectada, 
            string registroAfectadoId)
        {
            var registros = await _context.AuditoriaAcciones
                .Where(a => a.TablaAfectada == tablaAfectada && a.RegistroAfectadoId == registroAfectadoId)
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    TablaAfectada = a.TablaAfectada,
                    RegistroAfectadoId = a.RegistroAfectadoId,
                    UsuarioEjecutorId = a.UsuarioEjecutorId,
                    UsuarioEjecutorNombre = a.UsuarioEjecutorNombre,
                    UsuarioEjecutorEmail = a.UsuarioEjecutorEmail,
                    UsuarioAfectadoId = a.UsuarioAfectadoId,
                    UsuarioAfectadoNombre = a.UsuarioAfectadoNombre,
                    UsuarioAfectadoEmail = a.UsuarioAfectadoEmail,
                    MensajeCorto = a.MensajeCorto,
                    MensajeDetallado = a.MensajeDetallado,
                    MensajePlantilla = a.MensajePlantilla,
                    Motivo = a.Motivo,
                    Observaciones = a.Observaciones,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    FechaHora = a.FechaHora,
                    Severidad = a.Severidad,
                    EsVisible = a.EsVisible,
                    Tags = a.Tags,
                    SessionId = a.SessionId,
                    TiempoEjecucionMs = a.TiempoEjecucionMs
                })
                .ToListAsync();

            return registros;
        }

        public async Task<AuditoriaDto?> ObtenerPorIdAsync(string auditoriaId)
        {
            var auditoria = await _context.AuditoriaAcciones
                .Where(a => a.Id == auditoriaId)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    TablaAfectada = a.TablaAfectada,
                    RegistroAfectadoId = a.RegistroAfectadoId,
                    UsuarioEjecutorId = a.UsuarioEjecutorId,
                    UsuarioEjecutorNombre = a.UsuarioEjecutorNombre,
                    UsuarioEjecutorEmail = a.UsuarioEjecutorEmail,
                    UsuarioAfectadoId = a.UsuarioAfectadoId,
                    UsuarioAfectadoNombre = a.UsuarioAfectadoNombre,
                    UsuarioAfectadoEmail = a.UsuarioAfectadoEmail,
                    MensajeCorto = a.MensajeCorto,
                    MensajeDetallado = a.MensajeDetallado,
                    MensajePlantilla = a.MensajePlantilla,
                    Motivo = a.Motivo,
                    Observaciones = a.Observaciones,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    FechaHora = a.FechaHora,
                    Severidad = a.Severidad,
                    EsVisible = a.EsVisible,
                    Tags = a.Tags,
                    SessionId = a.SessionId,
                    TiempoEjecucionMs = a.TiempoEjecucionMs
                })
                .FirstOrDefaultAsync();

            return auditoria;
        }

        // ===== MÉTODOS DE ESTADÍSTICAS =====

        public async Task<EstadisticasAuditoriaDto> ObtenerEstadisticasAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null)
        {
            var query = _context.AuditoriaAcciones.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHora <= fechaHasta.Value);

            var ahora = DateTime.UtcNow;
            var hoy = ahora.Date;
            var semanaAtras = hoy.AddDays(-7);
            var mesAtras = hoy.AddDays(-30);

            var estadisticas = new EstadisticasAuditoriaDto
            {
                TotalAcciones = await query.CountAsync(),
                AccionesHoy = await query.Where(a => a.FechaHora >= hoy).CountAsync(),
                AccionesSemana = await query.Where(a => a.FechaHora >= semanaAtras).CountAsync(),
                AccionesMes = await query.Where(a => a.FechaHora >= mesAtras).CountAsync()
            };

            // Acciones por tipo
            var accionesPorTipo = await query
                .GroupBy(a => a.TipoAccion)
                .Select(g => new { Tipo = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Tipo, x => x.Count);
            estadisticas.AccionesPorTipo = accionesPorTipo;

            // Acciones por módulo
            var accionesPorModulo = await query
                .GroupBy(a => a.Modulo)
                .Select(g => new { Modulo = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Modulo, x => x.Count);
            estadisticas.AccionesPorModulo = accionesPorModulo;

            // Acciones por severidad
            var accionesPorSeveridad = await query
                .GroupBy(a => a.Severidad)
                .Select(g => new { Severidad = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Severidad, x => x.Count);
            estadisticas.AccionesPorSeveridad = accionesPorSeveridad;

            // Últimas acciones críticas
            var accionesCriticas = await _context.AuditoriaAcciones
                .Where(a => a.Severidad == SeveridadAuditoria.CRITICAL || a.Severidad == SeveridadAuditoria.SECURITY)
                .OrderByDescending(a => a.FechaHora)
                .Take(5)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    UsuarioEjecutorNombre = a.UsuarioEjecutorNombre,
                    MensajeCorto = a.MensajeCorto,
                    FechaHora = a.FechaHora,
                    Severidad = a.Severidad
                })
                .ToListAsync();
            estadisticas.UltimasAccionesCriticas = accionesCriticas;

            return estadisticas;
        }

        public async Task<List<AuditoriaDto>> ObtenerAccionesCriticasRecientesAsync(int limite = 10)
        {
            var accionesCriticas = await _context.AuditoriaAcciones
                .Where(a => a.Severidad == SeveridadAuditoria.CRITICAL || 
                           a.Severidad == SeveridadAuditoria.SECURITY ||
                           a.Severidad == SeveridadAuditoria.ERROR)
                .OrderByDescending(a => a.FechaHora)
                .Take(limite)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    UsuarioEjecutorNombre = a.UsuarioEjecutorNombre,
                    UsuarioAfectadoNombre = a.UsuarioAfectadoNombre,
                    MensajeCorto = a.MensajeCorto,
                    MensajeDetallado = a.MensajeDetallado,
                    Motivo = a.Motivo,
                    IpAddress = a.IpAddress,
                    FechaHora = a.FechaHora,
                    Severidad = a.Severidad
                })
                .ToListAsync();

            return accionesCriticas;
        }

        // ===== MÉTODOS UTILITARIOS =====

        public (string mensajeCorto, string mensajeDetallado, string mensajePlantilla) GenerarMensajes(
            TipoAccionAuditoria tipoAccion,
            string usuarioEjecutor,
            string? usuarioAfectado = null,
            string? motivo = null)
        {
            var descripcion = GetEnumDescription(tipoAccion);
            var mensajeCorto = descripcion;

            var mensajeDetallado = $"{usuarioEjecutor} realizó la acción: {descripcion}";
            if (!string.IsNullOrEmpty(usuarioAfectado))
                mensajeDetallado += $" sobre {usuarioAfectado}";
            if (!string.IsNullOrEmpty(motivo))
                mensajeDetallado += $". Motivo: {motivo}";

            var mensajePlantilla = $"{{usuarioEjecutor}} {descripcion.ToLower()}";
            if (!string.IsNullOrEmpty(usuarioAfectado))
                mensajePlantilla += " {{usuarioAfectado}}";

            return (mensajeCorto, mensajeDetallado, mensajePlantilla);
        }

        public async Task<int> LimpiarRegistrosAntiguosAsync(int diasRetencion = 365)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-diasRetencion);
            
            var registrosAEliminar = await _context.AuditoriaAcciones
                .Where(a => a.FechaHora < fechaLimite && a.Severidad != SeveridadAuditoria.CRITICAL)
                .ToListAsync();

            _context.AuditoriaAcciones.RemoveRange(registrosAEliminar);
            await _context.SaveChangesAsync();

            return registrosAEliminar.Count;
        }

        // ===== MÉTODOS PRIVADOS =====

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }
    }
}
