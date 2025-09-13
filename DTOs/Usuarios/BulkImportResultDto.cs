using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para el resultado completo del bulk import de usuarios.
    /// Contiene estadísticas, registros procesados y errores detallados.
    /// </summary>
    public class BulkImportResultDto
    {
        /// <summary>
        /// Indica si el proceso de bulk import fue exitoso en general
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Mensaje general del resultado del proceso
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Estadísticas detalladas del procesamiento
        /// </summary>
        public BulkImportEstadisticasDto Estadisticas { get; set; } = new BulkImportEstadisticasDto();

        /// <summary>
        /// Lista de usuarios que se crearon exitosamente
        /// </summary>
        public List<BulkImportUsuarioResultDto> UsuariosCreados { get; set; } = new List<BulkImportUsuarioResultDto>();

        /// <summary>
        /// Lista de registros que fallaron con sus errores específicos
        /// </summary>
        public List<BulkImportErrorDto> RegistrosFallidos { get; set; } = new List<BulkImportErrorDto>();

        /// <summary>
        /// Tiempo total de procesamiento
        /// </summary>
        public TimeSpan TiempoProcesamiento { get; set; }

        /// <summary>
        /// Fecha y hora de inicio del procesamiento
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de finalización del procesamiento
        /// </summary>
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// ID del administrador que ejecutó el bulk import
        /// </summary>
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// Información del archivo procesado
        /// </summary>
        public BulkImportMetadataDto? Metadata { get; set; }
    }

    /// <summary>
    /// Estadísticas detalladas del procesamiento de bulk import
    /// </summary>
    public class BulkImportEstadisticasDto
    {
        /// <summary>
        /// Total de registros procesados
        /// </summary>
        public int TotalProcesados { get; set; }

        /// <summary>
        /// Total de usuarios creados exitosamente
        /// </summary>
        public int UsuariosCreados { get; set; }

        /// <summary>
        /// Total de registros que fallaron
        /// </summary>
        public int RegistrosFallidos { get; set; }

        /// <summary>
        /// Total de registros omitidos (por duplicados o validaciones)
        /// </summary>
        public int RegistrosOmitidos { get; set; }

        /// <summary>
        /// Porcentaje de éxito del procesamiento
        /// </summary>
        public decimal PorcentajeExito => TotalProcesados > 0 
            ? Math.Round((decimal)UsuariosCreados / TotalProcesados * 100, 2) 
            : 0;

        /// <summary>
        /// Tiempo promedio de procesamiento por registro (en milisegundos)
        /// </summary>
        public double TiempoPromedioMilisegundos { get; set; }
    }

    /// <summary>
    /// Información de un usuario creado exitosamente en el bulk import
    /// </summary>
    public class BulkImportUsuarioResultDto
    {
        /// <summary>
        /// Número de fila en el archivo original
        /// </summary>
        public int NumeroFila { get; set; }

        /// <summary>
        /// ID del usuario creado en el sistema
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario creado
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario creado
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// DNI del usuario creado
        /// </summary>
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña temporal generada (si aplica)
        /// </summary>
        public string? ContrasenaTemporal { get; set; }

        /// <summary>
        /// Roles asignados al usuario
        /// </summary>
        public List<string> RolesAsignados { get; set; } = new List<string>();

        /// <summary>
        /// Departamento asignado
        /// </summary>
        public string Departamento { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se forzó el cambio de contraseña en el primer login
        /// </summary>
        public bool RequiereCambioContrasena { get; set; } = true;
    }

    /// <summary>
    /// Información detallada de un error en el bulk import
    /// </summary>
    public class BulkImportErrorDto
    {
        /// <summary>
        /// Número de fila en el archivo original donde ocurrió el error
        /// </summary>
        public int NumeroFila { get; set; }

        /// <summary>
        /// Tipo de error ocurrido
        /// </summary>
        public TipoErrorBulkImport TipoError { get; set; }

        /// <summary>
        /// Descripción detallada del error
        /// </summary>
        public string DescripcionError { get; set; } = string.Empty;

        /// <summary>
        /// Campo específico que causó el error (si aplica)
        /// </summary>
        public string? Campo { get; set; }

        /// <summary>
        /// Valor que causó el error
        /// </summary>
        public string? ValorError { get; set; }

        /// <summary>
        /// Datos del registro que falló (para referencia)
        /// </summary>
        public BulkImportUsuarioDto? RegistroOriginal { get; set; }

        /// <summary>
        /// Sugerencia para corregir el error
        /// </summary>
        public string? Sugerencia { get; set; }

        /// <summary>
        /// Indica si este error es crítico y previene la creación del usuario
        /// </summary>
        public bool EsCritico { get; set; } = true;
    }

    /// <summary>
    /// Tipos de errores posibles en el bulk import
    /// </summary>
    public enum TipoErrorBulkImport
    {
        VALIDACION_CAMPO,
        DUPLICADO_EMAIL,
        DUPLICADO_DNI,
        DEPARTAMENTO_NO_ENCONTRADO,
        JEFE_NO_ENCONTRADO,
        ROL_NO_VALIDO,
        FORMATO_FECHA_INVALIDO,
        FORMATO_BOOLEAN_INVALIDO,
        ERROR_CREACION_USUARIO,
        ERROR_ASIGNACION_ROLES,
        ERROR_BASE_DATOS,
        CAMPO_REQUERIDO_FALTANTE,
        LONGITUD_CAMPO_EXCEDIDA,
        FORMATO_EMAIL_INVALIDO,
        FORMATO_DNI_INVALIDO,
        JERARQUIA_CIRCULAR,
        ERROR_VALIDACION_NEGOCIO
    }
}
