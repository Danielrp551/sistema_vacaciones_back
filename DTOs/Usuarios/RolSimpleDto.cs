using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO simple para roles en dropdowns y listados básicos
    /// </summary>
    public class RolSimpleDto
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del rol
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Indica si el rol está activo
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}
