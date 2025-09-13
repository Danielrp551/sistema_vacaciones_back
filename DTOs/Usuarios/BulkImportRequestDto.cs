using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para la solicitud completa de bulk import de usuarios.
    /// Contiene la lista de usuarios y configuraciones adicionales.
    /// </summary>
    public class BulkImportRequestDto
    {
        /// <summary>
        /// Lista de usuarios a importar (parseados desde Excel/CSV)
        /// </summary>
        [Required(ErrorMessage = "La lista de usuarios es obligatoria")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un usuario")]
        public List<BulkImportUsuarioDto> Usuarios { get; set; } = new List<BulkImportUsuarioDto>();

        /// <summary>
        /// Configuraciones adicionales para el bulk import
        /// </summary>
        [Required(ErrorMessage = "La configuración es obligatoria")]
        public BulkImportConfiguracionDto Configuracion { get; set; } = new BulkImportConfiguracionDto();

        /// <summary>
        /// Metadatos del archivo original para auditoría
        /// </summary>
        public BulkImportMetadataDto? Metadata { get; set; }
    }

    /// <summary>
    /// Configuraciones para el proceso de bulk import
    /// </summary>
    public class BulkImportConfiguracionDto
    {
        /// <summary>
        /// Departamento por defecto si no se puede resolver el código
        /// </summary>
        [StringLength(10, ErrorMessage = "El departamento por defecto no puede exceder 10 caracteres")]
        public string? DepartamentoPorDefecto { get; set; }

        /// <summary>
        /// Roles por defecto para usuarios que no tengan roles especificados
        /// </summary>
        public List<string> RolesPorDefecto { get; set; } = new List<string> { "Empleado" };

        /// <summary>
        /// Indica si se deben generar contraseñas automáticamente para usuarios sin contraseña
        /// </summary>
        public bool GenerarPasswordsAutomaticamente { get; set; } = true;

        /// <summary>
        /// Indica si se debe continuar con el procesamiento aunque algunos registros tengan errores
        /// </summary>
        public bool ContinuarConErrores { get; set; } = false;

        /// <summary>
        /// Empresa por defecto si no se especifica en el registro
        /// </summary>
        [StringLength(100, ErrorMessage = "La empresa por defecto no puede exceder 100 caracteres")]
        public string? EmpresaPorDefecto { get; set; }

        /// <summary>
        /// Indica si se debe validar que los jefes existan antes de procesar
        /// </summary>
        public bool ValidarJefesExistentes { get; set; } = true;

        /// <summary>
        /// Indica si se debe enviar notificación por email a los usuarios creados
        /// </summary>
        public bool EnviarNotificacionEmail { get; set; } = false;
    }
}
