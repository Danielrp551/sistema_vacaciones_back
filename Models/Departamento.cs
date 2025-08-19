using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_vacaciones_back.Models
{
    /// <summary>
    /// Representa un departamento o área de la organización
    /// </summary>
    public class Departamento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? Codigo { get; set; }

        /// <summary>
        /// ID del jefe del departamento
        /// </summary>
        [MaxLength(450)]
        public string? JefeDepartamentoId { get; set; }

        /// <summary>
        /// Navegación al jefe del departamento
        /// </summary>
        [ForeignKey("JefeDepartamentoId")]
        public Usuario? JefeDepartamento { get; set; }

        /// <summary>
        /// Usuarios asignados al departamento
        /// </summary>
        public ICollection<Usuario>? Empleados { get; set; }

        [Required]
        public bool Activo { get; set; } = true;

        // Campos de auditoría
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
    }
}
