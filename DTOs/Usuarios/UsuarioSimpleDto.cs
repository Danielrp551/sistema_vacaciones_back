using System;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO simplificado de usuario para dropdowns y autocomplete
    /// </summary>
    public class UsuarioSimpleDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del departamento
        /// </summary>
        public string? Departamento { get; set; }

        /// <summary>
        /// Indica si está activo
        /// </summary>
        public bool EstaActivo { get; set; } = true;

        /// <summary>
        /// Cargo o posición (si está disponible)
        /// </summary>
        public string? Cargo { get; set; }

        /// <summary>
        /// Texto para mostrar en dropdown (NombreCompleto - Email - Departamento)
        /// </summary>
        public string TextoMostrar => $"{NombreCompleto} - {Email}" + 
                                    (string.IsNullOrEmpty(Departamento) ? "" : $" - {Departamento}");
    }
}
