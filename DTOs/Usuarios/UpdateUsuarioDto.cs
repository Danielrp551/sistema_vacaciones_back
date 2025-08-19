using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para actualizar información de un usuario existente
    /// </summary>
    public class UpdateUsuarioDto
    {
        // ========== INFORMACIÓN PERSONAL ==========
        
        /// <summary>
        /// Nombres del usuario
        /// </summary>
        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [MaxLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
        public string Nombres { get; set; } = string.Empty;

        /// <summary>
        /// Apellido paterno del usuario
        /// </summary>
        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MaxLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>
        /// Apellido materno del usuario
        /// </summary>
        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [MaxLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>
        /// Documento Nacional de Identidad
        /// </summary>
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos")]
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Indica si es extranjero
        /// </summary>
        public bool Extranjero { get; set; } = false;

        /// <summary>
        /// Fecha de ingreso a la empresa
        /// </summary>
        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        public DateTime FechaIngreso { get; set; }

        /// <summary>
        /// Número de celular (opcional)
        /// </summary>
        [RegularExpression(@"^$|^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "El formato del número de celular no es válido")]
        [MaxLength(20, ErrorMessage = "El número de celular no puede exceder 20 caracteres")]
        public string? NumeroCelular { get; set; }

        // ========== INFORMACIÓN DE CUENTA ==========

        /// <summary>
        /// Email del usuario
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [MaxLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se debe forzar el cambio de contraseña en el próximo login
        /// </summary>
        public bool ForzarCambioContrasena { get; set; }

        // ========== INFORMACIÓN LABORAL ==========

        /// <summary>
        /// Empresa donde trabaja
        /// </summary>
        [Required(ErrorMessage = "La empresa es obligatoria")]
        [MaxLength(200, ErrorMessage = "El nombre de la empresa no puede exceder 200 caracteres")]
        public string Empresa { get; set; } = string.Empty;

        /// <summary>
        /// ID del departamento al que pertenece
        /// </summary>
        [Required(ErrorMessage = "El departamento es obligatorio")]
        public string DepartamentoId { get; set; } = string.Empty;

        /// <summary>
        /// ID del jefe directo (opcional)
        /// </summary>
        public string? JefeId { get; set; }

        // ========== CONFIGURACIÓN ==========

        /// <summary>
        /// Lista de roles a asignar al usuario
        /// </summary>
        [Required(ErrorMessage = "Debe asignar al menos un rol")]
        [MinLength(1, ErrorMessage = "Debe asignar al menos un rol")]
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Estado del usuario (true = activo, false = inactivo)
        /// </summary>
        public bool EstaActivo { get; set; } = true;
    }
}
