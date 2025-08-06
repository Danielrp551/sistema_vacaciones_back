using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.HistorialVacaciones
{
    public class HistorialVacacionesDto
    {
        public int Periodo { get; set; }
        public double Vencidas { get; set; }
        public double Pendientes { get; set; }
        public double Truncas { get; set; }
        public double DiasLibres { get; set; }
        public double DiasBloque { get; set; }        
    }
}