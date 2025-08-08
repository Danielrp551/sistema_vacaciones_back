using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class Persona
    {
        public string  Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Dni { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApellidoMaterno { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        public Boolean Extranjero { get; set; }

    }
}