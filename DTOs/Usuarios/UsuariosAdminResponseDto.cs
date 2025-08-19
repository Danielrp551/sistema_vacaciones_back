using System;
using System.Collections.Generic;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO de respuesta para la lista paginada de usuarios administrativos
    /// </summary>
    public class UsuariosAdminResponseDto
    {
        /// <summary>
        /// Lista de usuarios de la página actual
        /// </summary>
        public List<UsuarioAdminDto> Usuarios { get; set; } = new List<UsuarioAdminDto>();

        /// <summary>
        /// Número total de usuarios en la página actual
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Número total de usuarios que cumplen los filtros (para paginación)
        /// </summary>
        public int TotalCompleto { get; set; }

        /// <summary>
        /// Total de usuarios (alias para compatibilidad)
        /// </summary>
        public int TotalUsuarios => TotalCompleto;

        /// <summary>
        /// Página actual (base 1)
        /// </summary>
        public int PaginaActual { get; set; }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int TamanoPagina { get; set; }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPaginas { get; set; }

        /// <summary>
        /// Indica si hay página anterior
        /// </summary>
        public bool TienePaginaAnterior { get; set; }

        /// <summary>
        /// Indica si hay página siguiente
        /// </summary>
        public bool TienePaginaSiguiente { get; set; }

        /// <summary>
        /// Filtros aplicados en la consulta
        /// </summary>
        public UsuariosAdminFiltrosAplicados FiltrosAplicados { get; set; } = new UsuariosAdminFiltrosAplicados();

        /// <summary>
        /// Estadísticas generales
        /// </summary>
        public UsuariosEstadisticasDto Estadisticas { get; set; } = new UsuariosEstadisticasDto();
    }
}
