using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    public static class PermisoMappers
    {
        public static PermisoDto ToPermisoDto(this Permiso permiso)
        {
            return new PermisoDto
            {
                Id = permiso.Id,
                NombreRuta = permiso.NombreRuta,
                Descripcion = permiso.Descripcion,
            };
        }

        public static List<PermisoDto> ToPermisoDtoList(this List<Permiso> permisos)
        {
            return permisos.Select(p => ToPermisoDto(p)).ToList();
        }
    }
}