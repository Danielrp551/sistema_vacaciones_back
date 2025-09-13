using sistema_vacaciones_back.DTOs.Usuarios;

namespace sistema_vacaciones_back.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de bulk import de usuarios.
    /// Define los métodos para procesar importaciones masivas de usuarios desde archivos Excel/CSV.
    /// </summary>
    public interface IBulkImportService
    {
        /// <summary>
        /// Procesa una solicitud de bulk import de usuarios.
        /// Valida, crea usuarios y registra auditoría del proceso completo.
        /// </summary>
        /// <param name="request">Solicitud con usuarios y configuraciones</param>
        /// <param name="adminId">ID del administrador que ejecuta el import</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">User agent del navegador</param>
        /// <returns>Resultado detallado del procesamiento</returns>
        Task<BulkImportResultDto> ProcessBulkImportAsync(
            BulkImportRequestDto request, 
            string adminId, 
            string ipAddress, 
            string userAgent);

        /// <summary>
        /// Valida la estructura y datos de una solicitud de bulk import.
        /// Realiza validaciones de formato, duplicados, referencias y reglas de negocio.
        /// </summary>
        /// <param name="request">Solicitud a validar</param>
        /// <returns>Lista de errores encontrados (vacía si es válida)</returns>
        Task<List<BulkImportErrorDto>> ValidateRequestAsync(BulkImportRequestDto request);

        /// <summary>
        /// Valida un usuario individual contra las reglas de negocio y existencia en BD.
        /// </summary>
        /// <param name="usuario">Usuario a validar</param>
        /// <param name="otrosUsuarios">Otros usuarios en el mismo lote para validar duplicados</param>
        /// <returns>Lista de errores para este usuario específico</returns>
        Task<List<BulkImportErrorDto>> ValidateUsuarioAsync(
            BulkImportUsuarioDto usuario, 
            List<BulkImportUsuarioDto> otrosUsuarios);

        /// <summary>
        /// Resuelve el ID de departamento a partir del código proporcionado.
        /// </summary>
        /// <param name="codigoDepartamento">Código del departamento</param>
        /// <returns>ID del departamento o null si no existe</returns>
        Task<string?> ResolverDepartamentoAsync(string codigoDepartamento);

        /// <summary>
        /// Resuelve el ID de usuario (jefe) a partir del DNI proporcionado.
        /// </summary>
        /// <param name="dniJefe">DNI del jefe</param>
        /// <returns>ID del usuario jefe o null si no existe</returns>
        Task<string?> ResolverJefeAsync(string dniJefe);

        /// <summary>
        /// Valida que todos los roles especificados existan en el sistema.
        /// </summary>
        /// <param name="roles">Lista de nombres de roles a validar</param>
        /// <returns>Lista de roles que no existen</returns>
        Task<List<string>> ValidarRolesAsync(List<string> roles);

        /// <summary>
        /// Convierte un usuario de bulk import a entidades Usuario y Persona del sistema.
        /// </summary>
        /// <param name="bulkUsuario">Usuario de bulk import</param>
        /// <param name="departamentoId">ID del departamento resuelto</param>
        /// <param name="jefeId">ID del jefe resuelto</param>
        /// <param name="adminId">ID del administrador que crea</param>
        /// <returns>Tupla con Usuario y Persona creados</returns>
        (Models.Usuario usuario, Models.Persona persona) ConvertirAEntidades(
            BulkImportUsuarioDto bulkUsuario, 
            string departamentoId, 
            string? jefeId, 
            string adminId);

        /// <summary>
        /// Genera estadísticas del procesamiento de bulk import.
        /// </summary>
        /// <param name="usuariosCreados">Lista de usuarios creados exitosamente</param>
        /// <param name="errores">Lista de errores ocurridos</param>
        /// <param name="tiempoProcesamiento">Tiempo total de procesamiento</param>
        /// <returns>Estadísticas calculadas</returns>
        BulkImportEstadisticasDto GenerarEstadisticas(
            List<BulkImportUsuarioResultDto> usuariosCreados,
            List<BulkImportErrorDto> errores,
            TimeSpan tiempoProcesamiento);
    }
}
