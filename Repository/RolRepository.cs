using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Data;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.DTOs.Rol;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Repository
{
    public class RolRepository : IRolRepository
    {
        private readonly ApplicationDBContext _context;

        public RolRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage, Permiso? permiso)> ActualizarPermiso(UpdatePermisoRequestDto permisoRequestDto, string userName)
        {
            return (false, "Implementación pendiente", null);
        }

        public async Task<(bool Success, string? ErrorMessage, Rol? rol)> ActualizarRol(UpdateRolRequestDto rolRequestDto, string userName)
        {
            return (false, "Implementación pendiente", null);
        }

        public async Task<(bool Success, string? ErrorMessage, Rol? rol)> CrearRol(CreateRolRequestDto createRolDto, string userName)
        {
            return (false, "Implementación pendiente", null);
        }
        

        public async Task<(int, List<PermisoDto>)> GetPermisos(string usuarioId)
        {
            var permisos = _context.Permisos
                .AsQueryable();
                
            int totalCount = await permisos.CountAsync();

            //var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            //List<Rol> rolesList = await roles.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();
            List<Permiso> permisosList = await permisos.ToListAsync();
            var permisoDtoList = permisosList.ToPermisoDtoList();

            return (totalCount, permisoDtoList);    
            
        }

        public async Task<(int, List<RolDto>)> GetRolPagination(RolesQueryObject queryObject, string usuarioId)
        {
            return (0, null); // Implementación pendiente             
        }
    }
}