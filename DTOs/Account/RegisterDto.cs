using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.DTOs.Account
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [MaxLength(15, ErrorMessage = "El DNI no puede tener más de 15 caracteres.")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El apellido paterno no puede tener más de 100 caracteres.")]
        public string ApellidoPaterno { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El apellido materno no puede tener más de 100 caracteres.")]
        public string ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        public DateTime FechaIngreso { get; set; }
        
        public bool Extranjero { get; set; }
    }
}