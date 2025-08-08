using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.SolicitudVacaciones
{
    public class CreateSolicitudRequestDto
    {
        [Required(ErrorMessage = "El campo TipoVacaciones es requerido")]
        public required string TipoVacaciones { get; set; } // "libres" o "bloque"

        [Required(ErrorMessage = "El campo DiasSolicitados es requerido")]
        public int DiasSolicitados { get; set; }

        [Required(ErrorMessage = "El campo FechaInicio es requerido")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "El campo FechaFin es requerido")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El campo Periodo es requerido")]
        public int Periodo { get; set; } // 2020, 2021, 2022, etc.
        
        [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string Observaciones { get; set; } = string.Empty;
    }
}