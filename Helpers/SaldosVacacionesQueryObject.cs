using System;

namespace sistema_vacaciones_back.Helpers
{
    public class SaldosVacacionesQueryObject
    {
        // Filtros
        public string? EmpleadoId { get; set; } = null;
        public int? Periodo { get; set; } = null;
        public bool? IncluirSubordinadosNivelN { get; set; } = false;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Orden
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;
    }
}
