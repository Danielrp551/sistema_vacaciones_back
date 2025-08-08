using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class Vacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string  PersonaId { get; set; }

        [ForeignKey("PersonaId")]
        public Persona Persona { get; set; }

        [Required]
        public int DiasLibresDisponibles { get; set; } = 15;

        [Required]
        public int DiasBloqueDisponibles { get; set; } = 15;

        [Required]
        public string Estado { get; set; } // "vencidas", "pendientes", "truncas"        
    }
}