namespace sistema_vacaciones_back.DTOs.Permiso
{
    /// <summary>
    /// DTO completo de permiso para gestión administrativa
    /// </summary>
    public class PermisoAdminDto
    {
        /// <summary>
        /// ID único del permiso
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del permiso
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del permiso
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Módulo al que pertenece el permiso
        /// </summary>
        public string Modulo { get; set; } = string.Empty;

        /// <summary>
        /// Código único del permiso
        /// </summary>
        public string CodigoPermiso { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de roles que tienen asignado este permiso
        /// </summary>
        public int NumeroRoles { get; set; } = 0;

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Usuario que creó el permiso
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedOn { get; set; }

        /// <summary>
        /// Usuario que actualizó por última vez
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indica si el permiso está activo
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
