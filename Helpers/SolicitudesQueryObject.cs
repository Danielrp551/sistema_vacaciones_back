using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Helpers
{
    public class SolicitudesQueryObject
    {
        // Filtros
        public string? SearchValue { get; set; } = null;
        public string? Estado { get; set; } = null;  

        public int? DiasSolicitados { get; set; } = null;
        public int? Id { get; set; } = null;
        public int? Periodo { get; set; } = null;
        public string? TipoVacaciones { get; set; } = null;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;        

        // Orden
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;        
    }
}