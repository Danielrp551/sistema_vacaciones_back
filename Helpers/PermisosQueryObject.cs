namespace sistema_vacaciones_back.Helpers
{
    /// <summary>
    /// Query object para filtrado y paginación de permisos
    /// </summary>
    public class PermisosQueryObject
    {
        /// <summary>
        /// Término de búsqueda que aplicará a nombre, descripción y módulo
        /// </summary>
        public string? SearchTerm { get; set; } = null;

        /// <summary>
        /// Filtrar por módulo específico
        /// </summary>
        public string? Modulo { get; set; } = null;

        /// <summary>
        /// Filtrar por estado activo/inactivo
        /// </summary>
        public bool? IsActive { get; set; } = null;

        /// <summary>
        /// Campo por el cual ordenar los resultados
        /// </summary>
        public string? SortBy { get; set; } = null;

        /// <summary>
        /// Dirección del ordenamiento (asc/desc)
        /// </summary>
        public bool IsDescending { get; set; } = false;

        /// <summary>
        /// Número de página (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Cantidad de elementos por página
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Asegurar que los valores de paginación sean válidos
        /// </summary>
        public void ValidatePagination()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 20;
            if (PageSize > 100) PageSize = 100; // Límite máximo
        }
    }
}
