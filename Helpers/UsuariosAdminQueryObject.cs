using System;

namespace sistema_vacaciones_back.Helpers
{
    /// <summary>
    /// Objeto de consulta para filtrar y paginar usuarios en la vista administrativa
    /// </summary>
    public class UsuariosAdminQueryObject
    {
        /// <summary>
        /// Número de página (base 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Término de búsqueda (busca en nombre, email)
        /// </summary>
        public string? BusquedaGeneral { get; set; }

        /// <summary>
        /// Filtro por email específico
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Filtro por departamento
        /// </summary>
        public string? DepartamentoId { get; set; }

        /// <summary>
        /// Filtro por rol
        /// </summary>
        public string? Rol { get; set; }

        /// <summary>
        /// Filtro por estado (true = activo, false = inactivo, null = todos)
        /// </summary>
        public bool? EstaActivo { get; set; }

        /// <summary>
        /// Filtro por extranjero
        /// </summary>
        public bool? Extranjero { get; set; }

        /// <summary>
        /// Filtro por rango de fecha de ingreso - desde
        /// </summary>
        public DateTime? FechaIngresoDesde { get; set; }

        /// <summary>
        /// Filtro por rango de fecha de ingreso - hasta
        /// </summary>
        public DateTime? FechaIngresoHasta { get; set; }

        /// <summary>
        /// Campo por el cual ordenar
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Orden descendente
        /// </summary>
        public bool IsDescending { get; set; } = false;
    }
}
