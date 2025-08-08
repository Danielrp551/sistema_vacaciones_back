using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.SolicitudVacaciones
{
    public class SolicitudVacacionesDto
    {
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        public string TipoVacaciones { get; set; } = string.Empty; // "libres" o "bloque"

        public int DiasSolicitados { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty; // "pendiente", "aprobado", "rechazado", "cancelado"

        public DateTime FechaSolicitud { get; set; }

        public int Periodo { get; set; } // 2020, 2021, 2022, etc.

        public int DiasFinde { get; set; } // Cantidad de días de fin de semana en el rango de fechas        
    }
}