using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.SolicitudVacaciones
{
    /// <summary>
    /// DTO para cancelar una solicitud de vacaciones
    /// </summary>
    public class CancelarSolicitudRequestDto
    {
        [MaxLength(500, ErrorMessage = "El motivo de cancelación no puede exceder 500 caracteres")]
        public string? MotivoCancelacion { get; set; }
    }
}
