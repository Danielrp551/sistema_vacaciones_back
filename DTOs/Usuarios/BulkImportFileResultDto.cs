namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO que contiene el resultado del procesamiento de un archivo de bulk import.
    /// Incluye usuarios extraídos, estadísticas y errores encontrados durante el parsing.
    /// </summary>
    public class BulkImportFileResultDto
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
        /// Fecha y hora cuando se procesó el archivo.
        /// </summary>
        public DateTime FechaProcesamiento { get; set; }

        /// <summary>
        /// Lista de usuarios extraídos del archivo.
        /// </summary>
        public List<BulkImportUsuarioDto> Usuarios { get; set; } = new();

        /// <summary>
        /// Total de filas procesadas (excluyendo encabezados).
        /// </summary>
        public int TotalFilasProcesadas { get; set; }

        /// <summary>
        /// Cantidad de usuarios exitosamente extraídos.
        /// </summary>
        public int UsuariosExtraidos { get; set; }

        /// <summary>
        /// Lista de errores encontrados durante el procesamiento del archivo.
        /// </summary>
        public List<string> Errores { get; set; } = new();

        /// <summary>
        /// Indica si el procesamiento fue exitoso (sin errores críticos).
        /// </summary>
        public bool EsExitoso => Errores.Count == 0;

        /// <summary>
        /// Estadísticas resumidas del procesamiento.
        /// </summary>
        public string ResumenProcesamiento => 
            $"Archivo: {NombreArchivo} | Tipo: {TipoArchivo} | " +
            $"Filas procesadas: {TotalFilasProcesadas} | " +
            $"Usuarios extraídos: {UsuariosExtraidos} | " +
            $"Errores: {Errores.Count}";
    }
}
