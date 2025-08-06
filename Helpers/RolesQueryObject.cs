using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Helpers
{
    public class RolesQueryObject
    {
        // Filtros
        public string? Name { get; set; } = null;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;        

        // Orden
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;           
    }
}