using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class Restriccion
    {
    [Key]
    public int Id { get; set; }

    [Required]
    public string AdminId { get; set; }

    [ForeignKey("AdminId")]
    public Usuario Admin { get; set; }

    [Required]
    public int FechaLimiteMes { get; set; } // Día máximo del mes para solicitar vacaciones

    [Required]
    public bool Activo { get; set; } = true; // Si la restricción está activa

    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;        
    }
}