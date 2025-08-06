using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class AprobacionVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SolicitudId { get; set; }

        [ForeignKey("SolicitudId")]
        public SolicitudVacaciones Solicitud { get; set; }

        [Required]
        public string AprobadorId { get; set; }

        [ForeignKey("AprobadorId")]
        public Usuario Aprobador { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        [Required]
        public string Estado { get; set; } // "aprobado", "rechazado"

        public string Comentario { get; set; }
    }
}