using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.SolicitudVacaciones
{
    /// <summary>
    /// DTO para obtener el detalle completo de una solicitud de vacaciones
    /// </summary>
    public class SolicitudVacacionesDetailDto
    {
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        
        public string NombreSolicitante { get; set; } = string.Empty;
        
        public string EmailSolicitante { get; set; } = string.Empty;

        public string? AprobadorId { get; set; }
        
        public string? NombreAprobador { get; set; }

        public string? JefeDirectoId { get; set; }
        
        public string? NombreJefeDirecto { get; set; }

        public string TipoVacaciones { get; set; } = string.Empty; // "libres" o "bloque"

        public int DiasSolicitados { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty; // "pendiente", "aprobado", "rechazado", "cancelado"

        public DateTime FechaSolicitud { get; set; }

        public int Periodo { get; set; }

        public int DiasFinde { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public string Comentarios { get; set; } = string.Empty;

        public string Observaciones { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la solicitud puede ser cancelada por el usuario
        /// </summary>
        public bool PuedeCancelar { get; set; }

        /// <summary>
        /// Indica si la solicitud puede ser aprobada (solo para administradores/jefes)
        /// </summary>
        public bool PuedeAprobar { get; set; }
    }
}
