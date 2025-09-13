using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.DTOs.Rol;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Interfaces
{
    public interface IRolRepository
    {
        Task<(int, List<RolDto>)> GetRolPagination(RolesQueryObject queryObject, string usuarioId);
        Task<(int, List<PermisoDto>)> GetPermisos(string usuarioId);
        Task<RolDto?> GetRolById(string rolId, string usuarioId);

        Task<(bool Success, string? ErrorMessage, Rol? rol)> CrearRol(CreateRolRequestDto CreateRolDto, string userName);

        Task<(bool Success, string? ErrorMessage, Permiso? permiso)> ActualizarPermiso(UpdatePermisoRequestDto permisoRequestDto, string userName);
        Task<(bool Success, string? ErrorMessage, Rol? rol)> ActualizarRol(UpdateRolRequestDto rolRequestDto, string userName);
        Task<(bool Success, string? ErrorMessage)> EliminarRol(string rolId, string userName);
        Task<(bool Success, string? ErrorMessage, string? nuevoEstado)> CambiarEstadoRol(string rolId, string userName); 
    }
}