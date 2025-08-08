using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Persona
{
    public class PersonaDto
    {
        public string Dni { get; set; }

        public string Nombres { get; set; }

        public string ApellidoPaterno { get; set; }


        public string ApellidoMaterno { get; set; }


        public DateTime FechaIngreso { get; set; }

        public Boolean Extranjero { get; set; }        
    }
}