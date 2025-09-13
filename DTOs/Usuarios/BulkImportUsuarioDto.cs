using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO que representa un usuario individual en el bulk import.
    /// Corresponde a una fila de la plantilla Excel/CSV.
    /// </summary>
    public class BulkImportUsuarioDto
    {
        /// <summary>
        /// Nombres del usuario (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
        public string Nombres { get; set; } = string.Empty;

        /// <summary>
        /// Apellido paterno del usuario (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder 50 caracteres")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>
        /// Apellido materno del usuario (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido materno no puede exceder 50 caracteres")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>
        /// DNI del usuario (obligatorio, 8 dígitos)
        /// </summary>
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos")]
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario (obligatorio, único)
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Código del departamento (obligatorio, debe existir en el sistema)
        /// </summary>
        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(10, ErrorMessage = "El código del departamento no puede exceder 10 caracteres")]
        public string CodigoDepartamento { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario es extranjero (obligatorio)
        /// Valores aceptados: "Sí", "Si", "Yes", "Y", "1", "True" para true
        /// Valores aceptados: "No", "N", "0", "False" para false
        /// </summary>
        [Required(ErrorMessage = "El campo Extranjero es obligatorio")]
        public string Extranjero { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de ingreso del usuario (obligatorio)
        /// Formato esperado: DD/MM/YYYY
        /// </summary>
        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        public string FechaIngreso { get; set; } = string.Empty;

        /// <summary>
        /// Empresa del usuario (obligatorio, texto libre)
        /// </summary>
        [Required(ErrorMessage = "La empresa es obligatoria")]
        [StringLength(100, ErrorMessage = "La empresa no puede exceder 100 caracteres")]
        public string Empresa { get; set; } = string.Empty;

        /// <summary>
        /// DNI del jefe/supervisor (opcional)
        /// </summary>
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI del jefe debe tener exactamente 8 dígitos")]
        public string? DniJefe { get; set; }

        /// <summary>
        /// Número de celular del usuario (opcional)
        /// </summary>
        [StringLength(15, ErrorMessage = "El celular no puede exceder 15 caracteres")]
        public string? Celular { get; set; }

        /// <summary>
        /// Roles del usuario separados por comas (opcional)
        /// Ejemplo: "Empleado,Supervisor"
        /// Si no se especifica, se asignará el rol por defecto
        /// </summary>
        [StringLength(200, ErrorMessage = "Los roles no pueden exceder 200 caracteres")]
        public string? Roles { get; set; }

        /// <summary>
        /// Contraseña temporal del usuario (opcional)
        /// Si no se especifica, se generará automáticamente
        /// </summary>
        [StringLength(50, ErrorMessage = "La contraseña temporal no puede exceder 50 caracteres")]
        public string? ContrasenaTemporal { get; set; }

        // Campos calculados/validados internamente (no vienen del Excel)
        
        /// <summary>
        /// Número de fila en el archivo Excel/CSV para reportes de error
        /// </summary>
        public int NumeroFila { get; set; }

        /// <summary>
        /// Indica si este registro es válido después de las validaciones
        /// </summary>
        public bool EsValido { get; set; } = true;

        /// <summary>
        /// Lista de errores de validación para este registro
        /// </summary>
        public List<string> Errores { get; set; } = new List<string>();
    }
}
