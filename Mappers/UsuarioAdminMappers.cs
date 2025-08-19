using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    /// <summary>
    /// Mappers para operaciones administrativas de usuarios
    /// </summary>
    public static class UsuarioAdminMappers
    {
        /// <summary>
        /// Convierte un Usuario a UsuarioAdminDto para la lista administrativa
        /// </summary>
        /// <param name="usuario">Usuario a convertir</param>
        /// <param name="roles">Lista de roles del usuario</param>
        /// <param name="numeroSubordinados">Número de subordinados directos</param>
        /// <returns>UsuarioAdminDto</returns>
        public static UsuarioAdminDto ToUsuarioAdminDto(this Usuario usuario, 
            List<string>? roles = null, 
            int numeroSubordinados = 0)
        {
            return new UsuarioAdminDto
            {
                Id = usuario.Id,
                Email = usuario.Email ?? "",
                NombreCompleto = GetNombreCompleto(usuario.Persona),
                Dni = usuario.Persona?.Dni,
                Roles = roles ?? new List<string>(),
                FechaIngreso = usuario.Persona?.FechaIngreso ?? DateTime.MinValue,
                Empresa = usuario.Persona?.Empresa ?? "",
                Departamento = usuario.Departamento?.Nombre,
                Extranjero = usuario.Persona?.Extranjero ?? false,
                Manager = GetNombreCompleto(usuario.Jefe?.Persona),
                NumeroSubordinados = numeroSubordinados,
                Estado = usuario.IsDeleted ? "Inactivo" : "Activo",
                JefeId = usuario.JefeId,
                DepartamentoId = usuario.DepartamentoId,
                FechaCreacion = usuario.CreatedOn,
                FechaActualizacion = usuario.UpdatedOn
            };
        }

        /// <summary>
        /// Convierte un Usuario a UsuarioDetalleDto para vista detallada
        /// </summary>
        /// <param name="usuario">Usuario a convertir</param>
        /// <param name="roles">Lista de roles del usuario</param>
        /// <param name="subordinados">Lista de subordinados directos</param>
        /// <param name="numeroSubordinados">Número total de subordinados</param>
        /// <returns>UsuarioDetalleDto</returns>
        public static UsuarioDetalleDto ToUsuarioDetalleDto(this Usuario usuario,
            List<string>? roles = null,
            List<UsuarioSimpleDto>? subordinados = null,
            int numeroSubordinados = 0)
        {
            var fechaIngreso = usuario.Persona?.FechaIngreso ?? DateTime.MinValue;
            var tiempoEnEmpresa = CalcularTiempoEnEmpresa(fechaIngreso);

            return new UsuarioDetalleDto
            {
                Id = usuario.Id,
                
                // Información personal
                Nombres = usuario.Persona?.Nombres ?? "",
                ApellidoPaterno = usuario.Persona?.ApellidoPaterno ?? "",
                ApellidoMaterno = usuario.Persona?.ApellidoMaterno ?? "",
                NombreCompleto = GetNombreCompleto(usuario.Persona),
                Dni = usuario.Persona?.Dni ?? "",
                Extranjero = usuario.Persona?.Extranjero ?? false,
                NumeroCelular = usuario.PhoneNumber,

                // Información de cuenta
                Email = usuario.Email ?? "",
                UserName = usuario.UserName ?? "",
                EmailConfirmed = usuario.EmailConfirmed,
                DebeChangePassword = false, // TODO: Implementar lógica de cambio forzado

                // Información laboral
                FechaIngreso = fechaIngreso,
                Empresa = usuario.Persona?.Empresa,
                DepartamentoId = usuario.DepartamentoId,
                Departamento = usuario.Departamento?.Nombre,
                CodigoDepartamento = usuario.Departamento?.Codigo,
                JefeId = usuario.JefeId,
                Manager = GetNombreCompleto(usuario.Jefe?.Persona),
                EmailManager = usuario.Jefe?.Email,
                NumeroSubordinados = numeroSubordinados,

                // Configuración y roles
                Roles = roles ?? new List<string>(),
                Estado = usuario.IsDeleted ? "Inactivo" : "Activo",
                EstaActivo = !usuario.IsDeleted,

                // Auditoría
                CreadoPor = usuario.CreatedBy,
                FechaCreacion = usuario.CreatedOn,
                ActualizadoPor = usuario.UpdatedBy,
                FechaActualizacion = usuario.UpdatedOn,

                // Estadísticas
                TiempoEnEmpresa = tiempoEnEmpresa,
                SubordinadosDirectos = subordinados ?? new List<UsuarioSimpleDto>()
            };
        }

        /// <summary>
        /// Convierte un Usuario a UsuarioSimpleDto para dropdowns
        /// </summary>
        /// <param name="usuario">Usuario a convertir</param>
        /// <returns>UsuarioSimpleDto</returns>
        public static UsuarioSimpleDto ToUsuarioSimpleDto(this Usuario usuario)
        {
            return new UsuarioSimpleDto
            {
                Id = usuario.Id,
                NombreCompleto = GetNombreCompleto(usuario.Persona),
                Email = usuario.Email ?? "",
                Departamento = usuario.Departamento?.Nombre,
                EstaActivo = !usuario.IsDeleted,
                Cargo = null // TODO: Agregar campo cargo si es necesario
            };
        }

        /// <summary>
        /// Convierte un Departamento a DepartamentoSimpleDto
        /// </summary>
        /// <param name="departamento">Departamento a convertir</param>
        /// <returns>DepartamentoSimpleDto</returns>
        public static DepartamentoSimpleDto ToDepartamentoSimpleDto(this Departamento departamento)
        {
            return new DepartamentoSimpleDto
            {
                Id = departamento.Id,
                Nombre = departamento.Nombre,
                Codigo = departamento.Codigo,
                Descripcion = departamento.Descripcion,
                EstaActivo = !departamento.IsDeleted,
                Activo = departamento.Activo,
                NombreJefe = GetNombreCompleto(departamento.JefeDepartamento?.Persona)
            };
        }

        /// <summary>
        /// Convierte un IdentityRole a RolSimpleDto
        /// </summary>
        /// <param name="role">Rol a convertir</param>
        /// <returns>RolSimpleDto</returns>
        public static RolSimpleDto ToRolSimpleDto(this IdentityRole role)
        {
            return new RolSimpleDto
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Descripcion = null, // TODO: Agregar descripción si es necesario
                Activo = true
            };
        }

        /// <summary>
        /// Convierte CreateUsuarioDto a Usuario y Persona
        /// </summary>
        /// <param name="createDto">DTO de creación</param>
        /// <param name="creadoPor">Usuario que crea el registro</param>
        /// <returns>Tupla con Usuario y Persona</returns>
        public static (Usuario usuario, Persona persona) FromCreateUsuarioDto(CreateUsuarioDto createDto, string creadoPor)
        {
            var personaId = Guid.NewGuid().ToString();
            var usuarioId = Guid.NewGuid().ToString();

            var persona = new Persona
            {
                Id = personaId,
                Dni = createDto.Dni,
                Nombres = createDto.Nombres,
                ApellidoPaterno = createDto.ApellidoPaterno,
                ApellidoMaterno = createDto.ApellidoMaterno,
                FechaIngreso = createDto.FechaIngreso,
                Extranjero = createDto.Extranjero,
                Empresa = createDto.Empresa
            };

            var usuario = new Usuario
            {
                Id = usuarioId,
                UserName = createDto.Email,
                Email = createDto.Email,
                PhoneNumber = createDto.NumeroCelular,
                EmailConfirmed = false,
                PersonaId = personaId,
                Persona = persona,
                JefeId = createDto.JefeId,
                DepartamentoId = createDto.DepartamentoId,
                IsDeleted = !createDto.EstadoInicial,
                CreatedBy = creadoPor,
                CreatedOn = DateTime.UtcNow
            };

            return (usuario, persona);
        }

        /// <summary>
        /// Aplica cambios de UpdateUsuarioDto a Usuario y Persona existentes
        /// </summary>
        /// <param name="usuario">Usuario existente</param>
        /// <param name="updateDto">DTO con cambios</param>
        /// <param name="actualizadoPor">Usuario que actualiza</param>
        public static void ApplyUpdateUsuarioDto(this Usuario usuario, UpdateUsuarioDto updateDto, string actualizadoPor)
        {
            // Actualizar información de cuenta
            usuario.Email = updateDto.Email;
            usuario.UserName = updateDto.Email;
            usuario.PhoneNumber = updateDto.NumeroCelular;
            usuario.NormalizedEmail = updateDto.Email.ToUpperInvariant();
            usuario.NormalizedUserName = updateDto.Email.ToUpperInvariant();

            // Actualizar información laboral
            usuario.DepartamentoId = updateDto.DepartamentoId;
            usuario.JefeId = updateDto.JefeId;
            usuario.IsDeleted = !updateDto.EstaActivo;

            // Auditoría
            usuario.UpdatedBy = actualizadoPor;
            usuario.UpdatedOn = DateTime.UtcNow;

            // Actualizar información de persona si existe
            if (usuario.Persona != null)
            {
                usuario.Persona.Nombres = updateDto.Nombres;
                usuario.Persona.ApellidoPaterno = updateDto.ApellidoPaterno;
                usuario.Persona.ApellidoMaterno = updateDto.ApellidoMaterno;
                usuario.Persona.Empresa = updateDto.Empresa;
            }
        }

        #region Métodos privados de ayuda

        /// <summary>
        /// Obtiene el nombre completo concatenado de una persona
        /// </summary>
        /// <param name="persona">Persona</param>
        /// <returns>Nombre completo</returns>
        private static string GetNombreCompleto(Persona? persona)
        {
            if (persona == null) return "";
            
            var nombres = new[] { persona.Nombres, persona.ApellidoPaterno, persona.ApellidoMaterno }
                .Where(n => !string.IsNullOrWhiteSpace(n));
                
            return string.Join(" ", nombres);
        }

        /// <summary>
        /// Calcula el tiempo transcurrido desde la fecha de ingreso
        /// </summary>
        /// <param name="fechaIngreso">Fecha de ingreso</param>
        /// <returns>Texto descriptivo del tiempo</returns>
        private static string CalcularTiempoEnEmpresa(DateTime fechaIngreso)
        {
            if (fechaIngreso == DateTime.MinValue) return "No definido";

            var tiempoTranscurrido = DateTime.Now - fechaIngreso;
            var years = tiempoTranscurrido.Days / 365;
            var months = (tiempoTranscurrido.Days % 365) / 30;

            if (years > 0)
            {
                return months > 0 
                    ? $"{years} año{(years > 1 ? "s" : "")} y {months} mes{(months > 1 ? "es" : "")}"
                    : $"{years} año{(years > 1 ? "s" : "")}";
            }
            else if (months > 0)
            {
                return $"{months} mes{(months > 1 ? "es" : "")}";
            }
            else
            {
                var dias = tiempoTranscurrido.Days;
                return dias > 0 ? $"{dias} día{(dias > 1 ? "s" : "")}" : "Menos de un día";
            }
        }

        #endregion
    }
}
