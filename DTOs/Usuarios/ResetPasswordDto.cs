using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para resetear la contraseña de un usuario
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// ID del usuario al que se le reseteará la contraseña
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Motivo del reseteo (para auditoría)
        /// </summary>
        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string? Motivo { get; set; }
    }

    /// <summary>
    /// Respuesta del reseteo de contraseña
    /// </summary>
    public class ResetPasswordResponseDto
    {
        /// <summary>
        /// Indica si el reseteo fue exitoso
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Mensaje del resultado
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña temporal generada (solo para mostrar al admin)
        /// </summary>
        public string? NuevaContrasenaTemporal { get; set; }

        /// <summary>
        /// Email del usuario al que se le reseteo la contraseña
        /// </summary>
        public string? EmailUsuario { get; set; }
    }
}
