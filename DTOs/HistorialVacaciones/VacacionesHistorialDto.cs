using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.HistorialVacaciones
{
    public class VacacionesHistorialDto
    {
        public int Periodo { get; set; }
        public string Categoria { get; set; }
        public double Dias { get; set; }        
    }
}