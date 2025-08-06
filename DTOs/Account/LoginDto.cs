using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.DTOs.Account
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; }        
    }
}