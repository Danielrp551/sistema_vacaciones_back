using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    /// <summary>
    /// DTO para actualizar un permiso existente
    /// </summary>
    public class UpdatePermisoDto
    {
        /// <summary>
        /// Nombre del permiso (requerido)
        /// </summary>
        [Required(ErrorMessage = "El nombre del permiso es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\u00f1\u00d1\s\.\-_]+$", ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, guiones y guiones bajos")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso (requerida)
        /// </summary>
        [Required(ErrorMessage = "La descripción del permiso es requerida")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Módulo al que pertenece el permiso (requerido)
        /// </summary>
        [Required(ErrorMessage = "El módulo es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El módulo debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\u00f1\u00d1\s\.\-_]+$", ErrorMessage = "El módulo solo puede contener letras, espacios, puntos, guiones y guiones bajos")]
        public string Modulo { get; set; } = string.Empty;

        /// <summary>
        /// Código único del permiso para validaciones programáticas
        /// </summary>
        [Required(ErrorMessage = "El código del permiso es obligatorio")]
        [StringLength(150, ErrorMessage = "El código no puede exceder los 150 caracteres")]
        public string CodigoPermiso { get; set; } = string.Empty;

        /// <summary>
        /// Estado activo del permiso
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
