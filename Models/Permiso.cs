using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sistema_vacaciones_back.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreRuta { get; set; }

        [Required]
        public string Descripcion { get; set; }

        // Audit fields
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public ICollection<RolPermiso> RolPermisos { get; set; }
    }
}