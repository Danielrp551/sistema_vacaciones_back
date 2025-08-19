using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.DTOs.Auditoria
{
    /// <summary>
    /// DTO para crear un nuevo registro de auditoría
    /// </summary>
    public class CrearAuditoriaDto
    {
        /// <summary>
        /// Tipo de acción realizada
        /// </summary>
        public TipoAccionAuditoria TipoAccion { get; set; }

        /// <summary>
        /// Módulo donde se realizó la acción
        /// </summary>
        public ModuloSistema Modulo { get; set; }

        /// <summary>
        /// Tabla de base de datos afectada
        /// </summary>
        public string TablaAfectada { get; set; } = string.Empty;

        /// <summary>
        /// ID del registro afectado
        /// </summary>
        public string RegistroAfectadoId { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario sobre el que se realiza la acción (opcional)
        /// </summary>
        public string? UsuarioAfectadoId { get; set; }

        /// <summary>
        /// Motivo de la acción (opcional)
        /// </summary>
        public string? Motivo { get; set; }

        /// <summary>
        /// Observaciones adicionales (opcional)
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Valores anteriores del registro en formato JSON (opcional)
        /// </summary>
        public string? ValoresAnteriores { get; set; }

        /// <summary>
        /// Valores nuevos del registro en formato JSON (opcional)
        /// </summary>
        public string? ValoresNuevos { get; set; }

        /// <summary>
        /// Nivel de severidad de la acción
        /// </summary>
        public SeveridadAuditoria Severidad { get; set; } = SeveridadAuditoria.INFO;

        /// <summary>
        /// Tags para clasificación y búsqueda (opcional)
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Información adicional en formato JSON (opcional)
        /// </summary>
        public string? MetadatosExtras { get; set; }
    }
}
