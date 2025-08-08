using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Persona;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    public static class PersonaMappers
    {
        public static PersonaDto ToPersonaDto(this Persona persona)
        {
            return new PersonaDto
            {
                Dni = persona.Dni,
                Nombres = persona.Nombres,
                ApellidoPaterno = persona.ApellidoPaterno,
                ApellidoMaterno = persona.ApellidoMaterno,
                FechaIngreso = persona.FechaIngreso,
                Extranjero = persona.Extranjero
            };
        }
    }
}