using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace sistema_vacaciones_back.Models
{
    public class Usuario : IdentityUser
    {
        public string  CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        public string PersonaId { get; set; }

        [ForeignKey("PersonaId")]
        public Persona Persona { get; set; }

        public string? JefeId { get; set; }

        [ForeignKey("JefeId")]
        public Usuario? Jefe { get; set; }

        public ICollection<Usuario>? Subordinados { get; set; }
    }
}