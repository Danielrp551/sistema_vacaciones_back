using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Rol
{
    public class CreateRolRequestDto
    {
        [Required(ErrorMessage = "El campo Nombre es requerido.")]
        [MinLength(5, ErrorMessage = "El campo Nombre debe tener al menos 5 carácteres.")]
        [MaxLength(100, ErrorMessage = "El campo Nombre no debe sobrepasar los 100 carácteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo descrpición es requerido.")]
        [MinLength(5, ErrorMessage = "El campo Nombre debe tener al menos 5 carácteres.")]
        [MaxLength(500, ErrorMessage = "El campo Nombre no debe sobrepasar los 500 carácteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El rol debe tener por lo menos un permiso asociado.")]
        public List<string> Permisos { get; set; }          
    }
}