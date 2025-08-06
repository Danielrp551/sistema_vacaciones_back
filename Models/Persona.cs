using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    [Table("Persona")]
    public class Persona
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Dni { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        public Boolean Extranjero { get; set; }

        public Usuario Usuario { get; set; }

    }
}