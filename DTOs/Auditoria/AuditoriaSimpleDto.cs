namespace sistema_vacaciones_back.DTOs.Auditoria
{
    /// <summary>
    /// DTO simplificado para mostrar auditoría en el frontend
    /// </summary>
    public class AuditoriaSimpleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string UsuarioEjecutor { get; set; } = string.Empty;
        public string? UsuarioAfectado { get; set; }
        public string MensajeCorto { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public DateTime FechaHora { get; set; }
        public string Severidad { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// DTO de respuesta paginada simplificada para el frontend
    /// </summary>
    public class AuditoriaSimplePaginadaDto
    {
        public List<AuditoriaSimpleDto> Registros { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }
}
