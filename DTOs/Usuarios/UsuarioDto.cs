using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    public class UsuarioDto
    {
        public int IdPersona { get; set; }

        public string Id { get; set; }

        public string Dni { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaIngreso { get; set; }

        public Boolean Extranjero { get; set; }

        public string NombreJefe { get; set; }

        public string NombreArea { get; set; }

        public List<string> Roles { get; set; }
    }
}