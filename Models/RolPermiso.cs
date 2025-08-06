using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class RolPermiso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RolId { get; set; }

        [ForeignKey("RolId")]
        public Rol Rol { get; set; }

        [Required]
        public int PermisoId { get; set; }

        [ForeignKey("PermisoId")]
        public Permiso Permiso { get; set; }
    }
}