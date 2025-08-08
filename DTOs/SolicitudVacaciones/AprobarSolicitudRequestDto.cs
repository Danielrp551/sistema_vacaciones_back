using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.SolicitudVacaciones
{
    /// <summary>
    /// DTO para aprobar o rechazar una solicitud de vacaciones
    /// </summary>
    public class AprobarSolicitudRequestDto
    {
        [Required(ErrorMessage = "La acción es requerida")]
        [RegularExpression("^(aprobar|rechazar)$", ErrorMessage = "La acción debe ser 'aprobar' o 'rechazar'")]
        public string Accion { get; set; } = string.Empty; // "aprobar" o "rechazar"

        [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder 1000 caracteres")]
        public string? Comentarios { get; set; }
    }
}
