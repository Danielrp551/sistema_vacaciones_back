using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.DTOs.Auditoria
{
    /// <summary>
    /// DTO para consultar registros de auditoría con filtros
    /// </summary>
    public class ConsultarAuditoriaDto
    {
        /// <summary>
        /// Módulo específico a consultar (opcional)
        /// </summary>
        public ModuloSistema? Modulo { get; set; }

        /// <summary>
        /// Tipo de acción específica (opcional)
        /// </summary>
        public TipoAccionAuditoria? TipoAccion { get; set; }

        /// <summary>
        /// ID del usuario ejecutor (opcional)
        /// </summary>
        public string? UsuarioEjecutorId { get; set; }

        /// <summary>
        /// ID del usuario afectado (opcional)
        /// </summary>
        public string? UsuarioAfectadoId { get; set; }

        /// <summary>
        /// Tabla específica (opcional)
        /// </summary>
        public string? TablaAfectada { get; set; }

        /// <summary>
        /// ID del registro específico (opcional)
        /// </summary>
        public string? RegistroAfectadoId { get; set; }

        /// <summary>
        /// Fecha desde (opcional)
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Fecha hasta (opcional)
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>
        /// Severidad específica (opcional)
        /// </summary>
        public SeveridadAuditoria? Severidad { get; set; }

        /// <summary>
        /// Tags para búsqueda (opcional)
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Solo registros visibles
        /// </summary>
        public bool SoloVisibles { get; set; } = true;

        /// <summary>
        /// Número de página (base 1)
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int TamanoPagina { get; set; } = 20;

        /// <summary>
        /// Campo por el cual ordenar
        /// </summary>
        public string OrdenarPor { get; set; } = "FechaHora";

        /// <summary>
        /// Orden descendente
        /// </summary>
        public bool OrdenDescendente { get; set; } = true;
    }
}
