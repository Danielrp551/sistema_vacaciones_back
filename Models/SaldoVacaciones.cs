using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_vacaciones_back.Models
{
    public class SaldoVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmpleadoId { get; set; } = string.Empty;

        [ForeignKey("EmpleadoId")]
        public Usuario? Empleado { get; set; }

        [Required]
        public int Periodo { get; set; }

        [Required]
        public int DiasVencidas { get; set; } = 0;

        [Required]
        public int DiasPendientes { get; set; } = 0;

        [Required]
        public int DiasTruncas { get; set; } = 0;

        [Required]
        public int DiasLibres { get; set; } = 0;

        [Required]
        public int DiasBloque { get; set; } = 0;

        [Required]
        public DateTime FechaCorte { get; set; }

        public DateTime? FechaProximoCorte { get; set; }

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
