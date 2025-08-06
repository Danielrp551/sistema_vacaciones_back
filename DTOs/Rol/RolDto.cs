using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Permiso;

namespace sistema_vacaciones_back.DTOs.Rol
{
    public class RolDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public int NumeroPersonas { get; set; }
        public string Estado { get; set; } // "activo", "inactivo"

        // Audit fields
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isDeleted { get; set; }
        public List<PermisoDto> Permisos { get; set; }      
    }
}