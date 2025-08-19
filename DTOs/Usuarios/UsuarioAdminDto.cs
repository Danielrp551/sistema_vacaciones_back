using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para la gestión administrativa de usuarios - Vista Lista
    /// </summary>
    public class UsuarioAdminDto
    {
        /// <summary>
        /// ID único del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo concatenado (Nombres + ApellidoPaterno + ApellidoMaterno)
        /// </summary>
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Documento Nacional de Identidad
        /// </summary>
        public string? Dni { get; set; }

        /// <summary>
        /// Lista de roles asignados al usuario
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Fecha de ingreso a la empresa
        /// </summary>
        [Required]
        public DateTime FechaIngreso { get; set; }

        /// <summary>
        /// Empresa donde trabaja
        /// </summary>
        public string? Empresa { get; set; }

        /// <summary>
        /// Nombre del departamento
        /// </summary>
        public string? Departamento { get; set; }

        /// <summary>
        /// Indica si es extranjero
        /// </summary>
        public bool Extranjero { get; set; }

        /// <summary>
        /// Nombre completo del jefe directo
        /// </summary>
        public string? Manager { get; set; }

        /// <summary>
        /// Número de subordinados directos
        /// </summary>
        public int NumeroSubordinados { get; set; }

        /// <summary>
        /// Estado del usuario (Activo/Inactivo)
        /// </summary>
        public string Estado { get; set; } = "Activo";

        /// <summary>
        /// ID del jefe directo (para uso interno)
        /// </summary>
        public string? JefeId { get; set; }

        /// <summary>
        /// ID del departamento (para uso interno)
        /// </summary>
        public string? DepartamentoId { get; set; }

        /// <summary>
        /// Fecha de creación del usuario
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }
    }
}
