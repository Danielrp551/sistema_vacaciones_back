using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Models
{
    /// <summary>
    /// Tabla de auditoría para registrar todas las acciones realizadas en el sistema
    /// </summary>
    [Table("AuditoriaAcciones")]
    public class AuditoriaAcciones
    {
        /// <summary>
        /// Identificador único de la acción auditada
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // === INFORMACIÓN DE LA ACCIÓN ===
        
        /// <summary>
        /// Tipo de acción realizada (CREAR, EDITAR, ELIMINAR, etc.)
        /// </summary>
        [Required]
        public TipoAccionAuditoria TipoAccion { get; set; }

        /// <summary>
        /// Módulo o sección del sistema donde se realizó la acción
        /// </summary>
        [Required]
        public ModuloSistema Modulo { get; set; }

        /// <summary>
        /// Nombre de la tabla de base de datos afectada
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TablaAfectada { get; set; } = string.Empty;

        /// <summary>
        /// ID del registro que fue modificado/afectado
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string RegistroAfectadoId { get; set; } = string.Empty;

        // === INFORMACIÓN DEL USUARIO QUE EJECUTA LA ACCIÓN ===
        
        /// <summary>
        /// ID del usuario que ejecuta la acción
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string UsuarioEjecutorId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario que ejecuta la acción (para mostrar en UI)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string UsuarioEjecutorNombre { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario que ejecuta la acción
        /// </summary>
        [MaxLength(200)]
        public string? UsuarioEjecutorEmail { get; set; }

        // === INFORMACIÓN DEL USUARIO AFECTADO (si aplica) ===
        
        /// <summary>
        /// ID del usuario sobre el que se realiza la acción (opcional)
        /// </summary>
        [MaxLength(100)]
        public string? UsuarioAfectadoId { get; set; }

        /// <summary>
        /// Nombre completo del usuario afectado (para mostrar en UI)
        /// </summary>
        [MaxLength(200)]
        public string? UsuarioAfectadoNombre { get; set; }

        /// <summary>
        /// Email del usuario afectado
        /// </summary>
        [MaxLength(200)]
        public string? UsuarioAfectadoEmail { get; set; }

        // === MENSAJES PARA LA INTERFAZ DE USUARIO ===
        
        /// <summary>
        /// Mensaje corto para mostrar en listas (ej: "Contraseña reiniciada")
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string MensajeCorto { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de la acción para mostrar en vistas de detalle
        /// </summary>
        [MaxLength(1000)]
        public string? MensajeDetallado { get; set; }

        /// <summary>
        /// Plantilla de mensaje con variables para personalización
        /// Ej: "{usuarioEjecutor} reinició la contraseña de {usuarioAfectado}"
        /// </summary>
        [MaxLength(1000)]
        public string? MensajePlantilla { get; set; }

        // === INFORMACIÓN ADICIONAL ===
        
        /// <summary>
        /// Motivo o justificación proporcionada por el usuario
        /// </summary>
        [MaxLength(1000)]
        public string? Motivo { get; set; }

        /// <summary>
        /// Observaciones adicionales del sistema o del administrador
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // === DATOS TÉCNICOS PARA AUDITORÍA ===
        
        /// <summary>
        /// Valores anteriores del registro en formato JSON (para cambios)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ValoresAnteriores { get; set; }

        /// <summary>
        /// Valores nuevos del registro en formato JSON (para cambios)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ValoresNuevos { get; set; }

        /// <summary>
        /// Dirección IP desde donde se realizó la acción
        /// </summary>
        [Required]
        [MaxLength(45)] // IPv6 máximo
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// User Agent del navegador/aplicación
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // === METADATOS Y CLASIFICACIÓN ===
        
        /// <summary>
        /// Fecha y hora UTC cuando se realizó la acción
        /// </summary>
        [Required]
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nivel de severidad de la acción
        /// </summary>
        [Required]
        public SeveridadAuditoria Severidad { get; set; } = SeveridadAuditoria.INFO;

        /// <summary>
        /// Indica si esta acción debe ser visible en el historial público
        /// </summary>
        public bool EsVisible { get; set; } = true;

        /// <summary>
        /// Tags separados por comas para facilitar búsquedas y filtros
        /// Ej: "seguridad,password,admin"
        /// </summary>
        [MaxLength(200)]
        public string? Tags { get; set; }

        /// <summary>
        /// Identificador de sesión para agrupar acciones relacionadas
        /// </summary>
        [MaxLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// Tiempo que tomó ejecutar la acción (en milisegundos)
        /// </summary>
        public int? TiempoEjecucionMs { get; set; }

        /// <summary>
        /// Información adicional en formato JSON para casos especiales
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? MetadatosExtras { get; set; }
    }
}
