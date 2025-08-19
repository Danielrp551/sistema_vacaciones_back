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
        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [MaxLength(450)]
        public string PersonaId { get; set; } = string.Empty;

        [ForeignKey("PersonaId")]
        public Persona? Persona { get; set; }

        public string? JefeId { get; set; }

        [ForeignKey("JefeId")]
        public Usuario? Jefe { get; set; }

        public ICollection<Usuario>? Subordinados { get; set; }

        // Relación con Departamento
        [MaxLength(450)]
        public string? DepartamentoId { get; set; }

        [ForeignKey("DepartamentoId")]
        public Departamento? Departamento { get; set; }
    }
}