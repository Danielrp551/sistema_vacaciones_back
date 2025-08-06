using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace sistema_vacaciones_back.Models
{
    public class Rol : IdentityRole
    {
        public string Descripcion { get; set; }
        public int NumeroPersonas { get; set; }

        public string Estado { get; set; } // "activo", "inactivo"
        // Audit fields
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isDeleted { get; set; }
        public ICollection<UsuarioRol> UsuarioRoles { get; set; }
        public ICollection<RolPermiso> RolPermisos { get; set; }
    }
}