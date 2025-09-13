using sistema_vacaciones_back.DTOs.Usuarios;

namespace sistema_vacaciones_back.Interfaces
{
    /// <summary>
    /// Interfaz para el procesador de archivos de bulk import.
    /// Maneja la conversión y procesamiento de datos desde archivos Excel/CSV.
    /// </summary>
    public interface IBulkImportFileProcessor
    {
        /// <summary>
        /// Procesa un archivo Excel y extrae los datos de usuarios.
        /// </summary>
        /// <param name="fileStream">Stream del archivo Excel</param>
        /// <param name="fileName">Nombre del archivo original</param>
        /// <returns>Lista de usuarios parseados con metadatos</returns>
        Task<(List<BulkImportUsuarioDto> usuarios, BulkImportMetadataDto metadata)> ProcessExcelFileAsync(
            Stream fileStream, 
            string fileName);

        /// <summary>
        /// Procesa un archivo CSV y extrae los datos de usuarios.
        /// </summary>
        /// <param name="fileStream">Stream del archivo CSV</param>
        /// <param name="fileName">Nombre del archivo original</param>
        /// <returns>Lista de usuarios parseados con metadatos</returns>
        Task<(List<BulkImportUsuarioDto> usuarios, BulkImportMetadataDto metadata)> ProcessCsvFileAsync(
            Stream fileStream, 
            string fileName);

        /// <summary>
        /// Valida que un archivo tenga el formato esperado (columnas requeridas).
        /// </summary>
        /// <param name="fileStream">Stream del archivo</param>
        /// <param name="fileType">Tipo de archivo (Excel/CSV)</param>
        /// <returns>Lista de errores de formato (vacía si es válido)</returns>
        Task<List<string>> ValidateFileFormatAsync(Stream fileStream, string fileType);

        /// <summary>
        /// Genera una plantilla de ejemplo en formato Excel.
        /// </summary>
        /// <returns>Stream con la plantilla Excel</returns>
        MemoryStream GenerateExcelTemplate();

        /// <summary>
        /// Genera una plantilla de ejemplo en formato CSV.
        /// </summary>
        /// <returns>Stream con la plantilla CSV</returns>
        MemoryStream GenerateCsvTemplate();

        /// <summary>
        /// Obtiene las columnas esperadas en el archivo de bulk import.
        /// </summary>
        /// <returns>Lista ordenada de nombres de columnas</returns>
        List<string> GetExpectedColumns();

        /// <summary>
        /// Valida el tamaño y tipo de archivo.
        /// </summary>
        /// <param name="fileSize">Tamaño del archivo en bytes</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <returns>Error de validación (null si es válido)</returns>
        string? ValidateFileConstraints(long fileSize, string fileName);
    }
}
