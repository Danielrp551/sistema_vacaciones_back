using sistema_vacaciones_back.DTOs.Usuarios;

namespace sistema_vacaciones_back.Interfaces
{
    /// <summary>
    /// Interfaz para el validador especializado de bulk import de usuarios.
    /// Separa la lógica de validación para mejor testabilidad y reutilización.
    /// </summary>
    public interface IBulkImportValidator
    {
        /// <summary>
        /// Valida la estructura básica de la solicitud de bulk import.
        /// </summary>
        /// <param name="request">Solicitud a validar</param>
        /// <returns>Lista de errores de estructura</returns>
        List<BulkImportErrorDto> ValidateRequestStructure(BulkImportRequestDto request);

        /// <summary>
        /// Valida los campos obligatorios y formato de un usuario individual.
        /// </summary>
        /// <param name="usuario">Usuario a validar</param>
        /// <returns>Lista de errores de formato y campos obligatorios</returns>
        List<BulkImportErrorDto> ValidateUsuarioFields(BulkImportUsuarioDto usuario);

        /// <summary>
        /// Valida duplicados de email y DNI dentro del mismo lote de usuarios.
        /// </summary>
        /// <param name="usuarios">Lista completa de usuarios del lote</param>
        /// <returns>Lista de errores de duplicados internos</returns>
        List<BulkImportErrorDto> ValidateDuplicadosInternos(List<BulkImportUsuarioDto> usuarios);

        /// <summary>
        /// Valida el formato de fecha y la convierte a DateTime.
        /// </summary>
        /// <param name="fechaString">Fecha en formato string</param>
        /// <param name="numeroFila">Número de fila para reportes de error</param>
        /// <param name="nombreCampo">Nombre del campo para reportes de error</param>
        /// <returns>Tupla con fecha convertida y error (si aplica)</returns>
        (DateTime? fecha, BulkImportErrorDto? error) ValidateAndParseFecha(
            string fechaString, 
            int numeroFila, 
            string nombreCampo);

        /// <summary>
        /// Valida el formato de boolean (Extranjero) y lo convierte.
        /// </summary>
        /// <param name="booleanString">Valor booleano en formato string</param>
        /// <param name="numeroFila">Número de fila para reportes de error</param>
        /// <param name="nombreCampo">Nombre del campo para reportes de error</param>
        /// <returns>Tupla con valor convertido y error (si aplica)</returns>
        (bool? valor, BulkImportErrorDto? error) ValidateAndParseBoolean(
            string booleanString, 
            int numeroFila, 
            string nombreCampo);

        /// <summary>
        /// Valida el formato de email.
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <param name="numeroFila">Número de fila para reportes de error</param>
        /// <returns>Error de validación (null si es válido)</returns>
        BulkImportErrorDto? ValidateEmail(string email, int numeroFila);

        /// <summary>
        /// Valida el formato de DNI (8 dígitos).
        /// </summary>
        /// <param name="dni">DNI a validar</param>
        /// <param name="numeroFila">Número de fila para reportes de error</param>
        /// <param name="nombreCampo">Nombre del campo (DNI o DNI del jefe)</param>
        /// <returns>Error de validación (null si es válido)</returns>
        BulkImportErrorDto? ValidateDni(string dni, int numeroFila, string nombreCampo);

        /// <summary>
        /// Valida y parsea la lista de roles separados por comas.
        /// </summary>
        /// <param name="rolesString">Roles separados por comas</param>
        /// <param name="numeroFila">Número de fila para reportes de error</param>
        /// <returns>Tupla con lista de roles y error (si aplica)</returns>
        (List<string> roles, BulkImportErrorDto? error) ValidateAndParseRoles(
            string? rolesString, 
            int numeroFila);

        /// <summary>
        /// Valida que no haya jerarquías circulares en las relaciones jefe-subordinado.
        /// </summary>
        /// <param name="usuarios">Lista de usuarios con sus jefes</param>
        /// <returns>Lista de errores de jerarquías circulares</returns>
        List<BulkImportErrorDto> ValidateJerarquiasCirculares(List<BulkImportUsuarioDto> usuarios);

        /// <summary>
        /// Crea un error de bulk import con información estándar.
        /// </summary>
        /// <param name="numeroFila">Número de fila</param>
        /// <param name="tipoError">Tipo de error</param>
        /// <param name="descripcion">Descripción del error</param>
        /// <param name="campo">Campo que causó el error</param>
        /// <param name="valor">Valor que causó el error</param>
        /// <param name="sugerencia">Sugerencia para corregir</param>
        /// <param name="esCritico">Si es error crítico</param>
        /// <returns>Error de bulk import creado</returns>
        BulkImportErrorDto CreateError(
            int numeroFila,
            TipoErrorBulkImport tipoError,
            string descripcion,
            string? campo = null,
            string? valor = null,
            string? sugerencia = null,
            bool esCritico = true);
    }
}
