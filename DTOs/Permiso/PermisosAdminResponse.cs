using sistema_vacaciones_back.DTOs.Permiso;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    /// <summary>
    /// Response paginado para la gestión administrativa de permisos
    /// </summary>
    public class PermisosAdminResponse
    {
        /// <summary>
        /// Lista de permisos de la página actual
        /// </summary>
        public List<PermisoAdminDto> Permisos { get; set; } = new List<PermisoAdminDto>();

        /// <summary>
        /// Número total de registros que coinciden con los filtros
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Página actual (1-based)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Cantidad de elementos por página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Indica si hay página anterior
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Indica si hay página siguiente
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Lista de módulos únicos disponibles (para filtros)
        /// </summary>
        public List<string> ModulosDisponibles { get; set; } = new List<string>();

        /// <summary>
        /// Estadísticas rápidas
        /// </summary>
        public PermisosEstadisticas Estadisticas { get; set; } = new PermisosEstadisticas();
    }

    /// <summary>
    /// Estadísticas de permisos para el dashboard
    /// </summary>
    public class PermisosEstadisticas
    {
        /// <summary>
        /// Total de permisos activos
        /// </summary>
        public int TotalPermisosActivos { get; set; }

        /// <summary>
        /// Total de permisos inactivos
        /// </summary>
        public int TotalPermisosInactivos { get; set; }

        /// <summary>
        /// Total de módulos diferentes
        /// </summary>
        public int TotalModulos { get; set; }

        /// <summary>
        /// Promedio de permisos por módulo
        /// </summary>
        public decimal PromedioPermisosPorModulo { get; set; }
    }
}
