using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    public class PermisoDto
    {
        public int Id { get; set; }

        public string NombreRuta { get; set; }

        public string Descripcion { get; set; }     
    }
}