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

        // Nuevos filtros para gestión de solicitudes
        public string? EmpleadoId { get; set; } = null;
        public bool? IncluirSubordinadosNivelN { get; set; } = false;
        public DateTime? FechaInicio { get; set; } = null;
        public DateTime? FechaFin { get; set; } = null;
        
        // Filtros de rango para fecha de inicio de vacaciones
        public DateTime? FechaInicioRango { get; set; } = null; // Desde cuándo buscar
        public DateTime? FechaFinRango { get; set; } = null;    // Hasta cuándo buscar

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;        

        // Orden
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;        
    }
}