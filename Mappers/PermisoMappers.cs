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
                Id = permiso.Id, // Cambio: usar directamente el Id en lugar de GetHashCode()
                NombreRuta = permiso.Nombre,
                Descripcion = permiso.Descripcion,
            };
        }

        public static List<PermisoDto> ToPermisoDtoList(this List<Permiso> permisos)
        {
            return permisos.Select(p => ToPermisoDto(p)).ToList();
        }

        // ========== NUEVOS MAPPERS PARA GESTIÓN ADMINISTRATIVA ==========

        /// <summary>
        /// Convierte un CreatePermisoDto a entidad Permiso
        /// </summary>
        /// <param name="createDto">DTO de creación</param>
        /// <param name="createdBy">Usuario que crea el permiso</param>
        /// <returns>Entidad Permiso</returns>
        public static Permiso ToPermisoFromCreateDto(this CreatePermisoDto createDto, string createdBy)
        {
            return new Permiso
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = createDto.Nombre.Trim(),
                Descripcion = createDto.Descripcion.Trim(),
                Modulo = createDto.Modulo.Trim(),
                CodigoPermiso = createDto.CodigoPermiso.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        /// <summary>
        /// Convierte una entidad Permiso a PermisoAdminDto
        /// </summary>
        /// <param name="permiso">Entidad Permiso</param>
        /// <param name="numeroRoles">Número de roles que tienen este permiso</param>
        /// <returns>DTO administrativo</returns>
        public static PermisoAdminDto ToPermisoAdminDto(this Permiso permiso, int numeroRoles = 0)
        {
            return new PermisoAdminDto
            {
                Id = permiso.Id,
                Nombre = permiso.Nombre,
                Descripcion = permiso.Descripcion,
                Modulo = permiso.Modulo,
                CodigoPermiso = permiso.CodigoPermiso,
                NumeroRoles = numeroRoles,
                CreatedOn = permiso.CreatedOn,
                CreatedBy = permiso.CreatedBy ?? "",
                UpdatedOn = permiso.UpdatedOn,
                UpdatedBy = permiso.UpdatedBy,
                IsActive = permiso.IsActive
            };
        }

        /// <summary>
        /// Actualiza una entidad Permiso desde un UpdatePermisoDto
        /// </summary>
        /// <param name="permiso">Entidad Permiso a actualizar</param>
        /// <param name="updateDto">DTO con los datos de actualización</param>
        /// <param name="updatedBy">Usuario que actualiza</param>
        /// <returns>Entidad Permiso actualizada</returns>
        public static Permiso UpdateFromDto(this Permiso permiso, UpdatePermisoDto updateDto, string updatedBy)
        {
            permiso.Nombre = updateDto.Nombre.Trim();
            permiso.Descripcion = updateDto.Descripcion.Trim();
            permiso.Modulo = updateDto.Modulo.Trim();
            permiso.CodigoPermiso = updateDto.CodigoPermiso.Trim();
            permiso.IsActive = updateDto.IsActive;
            permiso.UpdatedOn = DateTime.UtcNow;
            permiso.UpdatedBy = updatedBy;

            return permiso;
        }

        /// <summary>
        /// Convierte una lista de entidades Permiso a lista de PermisoAdminDto
        /// </summary>
        /// <param name="permisos">Lista de entidades Permiso</param>
        /// <returns>Lista de DTOs administrativos</returns>
        public static List<PermisoAdminDto> ToPermisoAdminDtoList(this List<Permiso> permisos)
        {
            return permisos.Select(p => p.ToPermisoAdminDto()).ToList();
        }

        /// <summary>
        /// Genera un código único para el permiso basado en módulo y nombre
        /// </summary>
        /// <param name="modulo">Módulo del permiso</param>
        /// <param name="nombre">Nombre del permiso</param>
        /// <returns>Código único generado</returns>
        private static string GenerateCodigoPermiso(string modulo, string nombre)
        {
            // Limpiar y normalizar strings
            var moduloLimpio = LimpiarTextoParaCodigo(modulo);
            var nombreLimpio = LimpiarTextoParaCodigo(nombre);

            // Generar código: MODULO_NOMBRE_TIMESTAMP
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            return $"{moduloLimpio}_{nombreLimpio}_{timestamp}".ToUpperInvariant();
        }

        /// <summary>
        /// Limpia un texto para ser usado en códigos (solo letras, números y guiones bajos)
        /// </summary>
        /// <param name="texto">Texto a limpiar</param>
        /// <returns>Texto limpio</returns>
        private static string LimpiarTextoParaCodigo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "UNKNOWN";

            // Remover acentos y caracteres especiales
            var textoLimpio = texto.Trim()
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace(".", "_")
                .Replace(",", "_")
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("ñ", "n").Replace("Ñ", "N");

            // Mantener solo letras, números y guiones bajos
            var caracteresValidos = textoLimpio.Where(c => 
                char.IsLetterOrDigit(c) || c == '_').ToArray();

            var resultado = new string(caracteresValidos);

            // Asegurar que no esté vacío y no empiece con número
            if (string.IsNullOrEmpty(resultado))
                return "UNKNOWN";

            if (char.IsDigit(resultado[0]))
                resultado = "P_" + resultado;

            // Limitar longitud
            if (resultado.Length > 20)
                resultado = resultado.Substring(0, 20);

            return resultado;
        }

        /// <summary>
        /// Valida si un CreatePermisoDto tiene datos válidos
        /// </summary>
        /// <param name="createDto">DTO a validar</param>
        /// <returns>True si es válido</returns>
        public static bool IsValid(this CreatePermisoDto createDto)
        {
            return !string.IsNullOrWhiteSpace(createDto.Nombre) &&
                   !string.IsNullOrWhiteSpace(createDto.Descripcion) &&
                   !string.IsNullOrWhiteSpace(createDto.Modulo) &&
                   createDto.Nombre.Length >= 3 &&
                   createDto.Descripcion.Length >= 10 &&
                   createDto.Modulo.Length >= 3;
        }

        /// <summary>
        /// Valida si un UpdatePermisoDto tiene datos válidos
        /// </summary>
        /// <param name="updateDto">DTO a validar</param>
        /// <returns>True si es válido</returns>
        public static bool IsValid(this UpdatePermisoDto updateDto)
        {
            return !string.IsNullOrWhiteSpace(updateDto.Nombre) &&
                   !string.IsNullOrWhiteSpace(updateDto.Descripcion) &&
                   !string.IsNullOrWhiteSpace(updateDto.Modulo) &&
                   updateDto.Nombre.Length >= 3 &&
                   updateDto.Descripcion.Length >= 10 &&
                   updateDto.Modulo.Length >= 3;
        }
    }
}