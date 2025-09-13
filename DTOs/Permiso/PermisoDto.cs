using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.DTOs.Permiso
{
    public class PermisoDto
    {
        public string Id { get; set; } = string.Empty; // Cambio: usar string en lugar de int

        public string NombreRuta { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;     
    }
}