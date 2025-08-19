using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO simple para departamentos en dropdowns y listas
    /// </summary>
    public class DepartamentoSimpleDto
    {
        /// <summary>
        /// ID único del departamento
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del departamento
        /// </summary>
        [Required]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Código del departamento (opcional)
        /// </summary>
        public string? Codigo { get; set; }

        /// <summary>
        /// Descripción del departamento
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Indica si el departamento está activo
        /// </summary>
        public bool EstaActivo { get; set; } = true;

        /// <summary>
        /// Indica si el departamento está activo (alias para compatibilidad)
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Nombre del jefe del departamento
        /// </summary>
        [StringLength(200)]
        public string? NombreJefe { get; set; }
    }
}
