using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.DTOs.Auditoria
{
    /// <summary>
    /// DTO de respuesta para un registro de auditoría
    /// </summary>
    public class AuditoriaDto
    {
        public string Id { get; set; } = string.Empty;
        public TipoAccionAuditoria TipoAccion { get; set; }
        public ModuloSistema Modulo { get; set; }
        public string TablaAfectada { get; set; } = string.Empty;
        public string RegistroAfectadoId { get; set; } = string.Empty;
        
        public string UsuarioEjecutorId { get; set; } = string.Empty;
        public string UsuarioEjecutorNombre { get; set; } = string.Empty;
        public string? UsuarioEjecutorEmail { get; set; }
        
        public string? UsuarioAfectadoId { get; set; }
        public string? UsuarioAfectadoNombre { get; set; }
        public string? UsuarioAfectadoEmail { get; set; }
        
        public string MensajeCorto { get; set; } = string.Empty;
        public string? MensajeDetallado { get; set; }
        public string? MensajePlantilla { get; set; }
        
        public string? Motivo { get; set; }
        public string? Observaciones { get; set; }
        
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        
        public DateTime FechaHora { get; set; }
        public SeveridadAuditoria Severidad { get; set; }
        public bool EsVisible { get; set; }
        public string? Tags { get; set; }
        public string? SessionId { get; set; }
        public int? TiempoEjecucionMs { get; set; }
    }

    /// <summary>
    /// DTO de respuesta paginada para consultas de auditoría
    /// </summary>
    public class AuditoriaPaginadaDto
    {
        public List<AuditoriaDto> Registros { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanoPagina { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para estadísticas de auditoría
    /// </summary>
    public class EstadisticasAuditoriaDto
    {
        public int TotalAcciones { get; set; }
        public int AccionesHoy { get; set; }
        public int AccionesSemana { get; set; }
        public int AccionesMes { get; set; }
        public Dictionary<string, int> AccionesPorTipo { get; set; } = new();
        public Dictionary<string, int> AccionesPorModulo { get; set; } = new();
        public Dictionary<string, int> AccionesPorSeveridad { get; set; } = new();
        public List<AuditoriaDto> UltimasAccionesCriticas { get; set; } = new();
    }
}
