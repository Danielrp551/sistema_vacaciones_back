using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    /// <summary>
    /// DTO para la creación de un nuevo permiso
    /// </summary>
    public class CreatePermisoDto
    {
        /// <summary>
        /// Nombre del permiso
        /// </summary>
        [Required(ErrorMessage = "El nombre del permiso es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del permiso
        /// </summary>
        [Required(ErrorMessage = "La descripción del permiso es obligatoria")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Módulo al que pertenece el permiso
        /// </summary>
        [Required(ErrorMessage = "El módulo del permiso es obligatorio")]
        [StringLength(100, ErrorMessage = "El módulo no puede exceder los 100 caracteres")]
        public string Modulo { get; set; } = string.Empty;

        /// <summary>
        /// Código único del permiso para validaciones programáticas
        /// </summary>
        [Required(ErrorMessage = "El código del permiso es obligatorio")]
        [StringLength(150, ErrorMessage = "El código no puede exceder los 150 caracteres")]
        public string CodigoPermiso { get; set; } = string.Empty;
    }
}
