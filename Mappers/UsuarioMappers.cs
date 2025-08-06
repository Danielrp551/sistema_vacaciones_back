using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    public static class UsuarioMappers
    {
        public static UsuarioDto ToUsuarioDto(this Usuario usuario)
        {
            return new UsuarioDto
            {
                IdPersona = usuario.PersonaId,
                Id = usuario.Id,
                Dni = usuario.Persona.Dni,
                Nombre = usuario.Persona.Nombre,
                FechaIngreso = usuario.Persona.FechaIngreso,
                Extranjero = usuario.Persona.Extranjero,
                NombreJefe = usuario.Jefe?.Persona?.Nombre,
                NombreArea = "",
                Roles = usuario.UsuarioRoles?.Select(ur => ur.Rol?.Name)
                                   .Where(name => !string.IsNullOrEmpty(name))
                                   .ToList() 
                        ?? new List<string>()
            };
        }

        public static List<UsuarioDto> ToUsuarioDtoList(this List<Usuario> usuarios)
        {
            return usuarios.Select(u => ToUsuarioDto(u)).ToList();
        }
    }
}