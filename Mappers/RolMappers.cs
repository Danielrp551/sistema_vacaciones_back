using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.DTOs.Rol;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    public static class RolMappers
    {
        public static RolDto ToRolDto(this Rol rol)
        {
            return new RolDto
            {
                Id = rol.Id,
                Name = rol.Name,
                Descripcion = rol.Descripcion,
                NumeroPersonas = 0, // Se calculará dinámicamente en el repositorio
                Estado = rol.Estado,
                // Audit fields
                CreatedBy = rol.CreatedBy?.ToString() ?? string.Empty,
                CreatedOn = rol.CreatedOn,
                UpdatedBy = rol.UpdatedBy?.ToString() ?? string.Empty,
                UpdatedOn = rol.UpdatedOn ?? DateTime.MinValue,
                isDeleted = rol.IsDeleted,
                Permisos = new List<PermisoDto>() // Inicializar con lista vacía en lugar de null
            };
        }

        public static List<RolDto> ToRolDtoList(this List<Rol> roles)
        {
            return roles.Select(r => ToRolDto(r)).ToList();
        }

        public static Rol ToRolFromCreateDto( this CreateRolRequestDto RolDto)
        {
            return new Rol {
                Name = RolDto.Name,
                Descripcion = RolDto.Descripcion,
                NumeroPersonas = 0,
                Estado = "Activo",
                // Audit fields
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsDeleted = false
            };
        }
    }
}