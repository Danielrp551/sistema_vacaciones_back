using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class SolicitudVacaciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // No auto-generate by DB
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Generar por defecto

        [Required]
        [MaxLength(450)] // Longitud estándar para IDs de usuario en ASP.NET Identity
        public string SolicitanteId { get; set; } = string.Empty;

        [ForeignKey("SolicitanteId")]
        public Usuario? Solicitante { get; set; }

        [MaxLength(450)]
        public string? AprobadorId { get; set; }
        
        [ForeignKey("AprobadorId")]
        public Usuario? Aprobador { get; set; }

        [Required]
        [MaxLength(10)] // "libres" o "bloque"
        public string TipoVacaciones { get; set; } = string.Empty;

        [Required]
        [Range(1, 365, ErrorMessage = "Los días solicitados deben estar entre 1 y 365")]
        public int DiasSolicitados { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        [MaxLength(20)] // "pendiente", "aprobado", "rechazado", "cancelado"
        public string Estado { get; set; } = "pendiente";

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(2020, 2100, ErrorMessage = "El período debe ser un año válido")]
        public int Periodo { get; set; }

        [Range(0, 100, ErrorMessage = "Los días de fin de semana deben estar entre 0 y 100")]
        public int DiasFinde { get; set; } = 0;

        public DateTime? FechaAprobacion { get; set; }

        [MaxLength(1000)]
        public string Comentarios { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Observaciones { get; set; } = string.Empty;
    }
}