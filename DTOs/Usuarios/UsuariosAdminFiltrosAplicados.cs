using System;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// Información sobre los filtros aplicados en la consulta de usuarios administrativos
    /// </summary>
    public class UsuariosAdminFiltrosAplicados
    {
        /// <summary>
        /// Término de búsqueda aplicado
        /// </summary>
        public string? BusquedaGeneral { get; set; }

        /// <summary>
        /// Filtro de email aplicado
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Filtro de departamento aplicado (ID)
        /// </summary>
        public string? DepartamentoId { get; set; }

        /// <summary>
        /// Nombre del departamento filtrado
        /// </summary>
        public string? NombreDepartamento { get; set; }

        /// <summary>
        /// Filtro de rol aplicado
        /// </summary>
        public string? Rol { get; set; }

        /// <summary>
        /// Filtro de estado aplicado
        /// </summary>
        public bool? EstaActivo { get; set; }

        /// <summary>
        /// Filtro de extranjero aplicado
        /// </summary>
        public bool? Extranjero { get; set; }

        /// <summary>
        /// Fecha de ingreso desde para filtrado
        /// </summary>
        public DateTime? FechaIngresoDesde { get; set; }

        /// <summary>
        /// Fecha de ingreso hasta para filtrado
        /// </summary>
        public DateTime? FechaIngresoHasta { get; set; }

        /// <summary>
        /// Campo de ordenamiento (nombres de propiedad)
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Dirección del ordenamiento (verdadero = descendente)
        /// </summary>
        public bool IsDescending { get; set; }

        /// <summary>
        /// Campo de ordenamiento (alias para compatibilidad)
        /// </summary>
        public string? OrdenadoPor => SortBy;

        /// <summary>
        /// Dirección del ordenamiento (alias para compatibilidad)
        /// </summary>
        public bool OrdenDescendente => IsDescending;
    }
}
