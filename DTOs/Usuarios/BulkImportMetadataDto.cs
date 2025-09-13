namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO con metadatos sobre el procesamiento de un archivo de bulk import.
    /// Contiene información estadística y de control del proceso de parsing.
    /// </summary>
    public class BulkImportMetadataDto
    {
        /// <summary>
        /// Nombre del archivo procesado.
        /// </summary>
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de archivo procesado (Excel, CSV).
        /// </summary>
        public string TipoArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        public long TamanoArchivo { get; set; }

        /// <summary>
        /// Fecha y hora de procesamiento.
        /// </summary>
        public DateTime FechaProcesamiento { get; set; }

        /// <summary>
        /// Total de filas leídas del archivo (excluyendo encabezados).
        /// </summary>
        public int TotalFilas { get; set; }

        /// <summary>
        /// Filas válidas con datos (no vacías).
        /// </summary>
        public int FilasConDatos { get; set; }

        /// <summary>
        /// Filas omitidas por estar vacías o incompletas.
        /// </summary>
        public int FilasOmitidas { get; set; }

        /// <summary>
        /// Lista de errores de parsing encontrados.
        /// </summary>
        public List<string> ErroresParsing { get; set; } = new();

        /// <summary>
        /// Lista de advertencias durante el procesamiento.
        /// </summary>
        public List<string> Advertencias { get; set; } = new();

        /// <summary>
        /// Tiempo total de procesamiento en milisegundos.
        /// </summary>
        public long TiempoProcesamiento { get; set; }

        /// <summary>
        /// Columnas encontradas en el archivo.
        /// </summary>
        public List<string> ColumnasEncontradas { get; set; } = new();

        /// <summary>
        /// Indica si el procesamiento fue exitoso (sin errores críticos).
        /// </summary>
        public bool EsExitoso => ErroresParsing.Count == 0;

        /// <summary>
        /// Resumen textual del procesamiento.
        /// </summary>
        public string Resumen => 
            $"Archivo: {NombreArchivo} ({TipoArchivo}) - " +
            $"Filas: {TotalFilas} ({FilasConDatos} con datos, {FilasOmitidas} omitidas) - " +
            $"Errores: {ErroresParsing.Count} - " +
            $"Tiempo: {TiempoProcesamiento}ms";
    }
}
