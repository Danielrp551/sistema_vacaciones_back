using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    public class UpdatePermisoRequestDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreRuta { get; set; }

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; }     
    }
}