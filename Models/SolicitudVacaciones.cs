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
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        public string TipoVacaciones { get; set; } // "libres" o "bloque"

        [Required]
        public int DiasSolicitados { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public string Estado { get; set; } // "pendiente", "aprobado", "rechazado", "cancelado"

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public int Periodo { get; set; } // 2020, 2021, 2022, etc.

        public int DiasFinde { get; set; } // Cantidad de días de fin de semana en el rango de fechas
    }
}