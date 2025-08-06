using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SISTEMA_VACACIONES.Data;
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
            Permiso permiso = await _context.Permisos.FirstOrDefaultAsync(p => p.Id == permisoRequestDto.Id);
            if (permiso == null)
            {
                return (false, "Permiso no encontrado", null);
            }

            permiso.Descripcion = permisoRequestDto.Descripcion;
            permiso.UpdatedBy = userName;
            permiso.UpdatedOn = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return (true, null, permiso);
            }
            catch (Exception e)
            {
                return (false, e.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, Rol? rol)> ActualizarRol(UpdateRolRequestDto rolRequestDto, string userName)
        {
            Rol rol = await _context.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.Id == rolRequestDto.Id);
            if (rol == null)
            {
                return (false, "Rol no encontrado", null);
            }

            rol.Name = rolRequestDto.Name;
            rol.Descripcion = rolRequestDto.Descripcion;
            rol.UpdatedBy = userName;
            rol.UpdatedOn = DateTime.UtcNow;

            // Actualizar los permisos del rol
            if(rol.RolPermisos != null && rol.RolPermisos.Any())
            {
                _context.RolPermisos.RemoveRange(rol.RolPermisos);
            }

            rol.RolPermisos = new List<RolPermiso>();
            foreach (var permisoId in rolRequestDto.Permisos)
            {
                rol.RolPermisos.Add(new RolPermiso
                {
                    PermisoId = permisoId,
                    Rol = rol
                });
            }

            try{
                await _context.SaveChangesAsync();
                return (true, null, rol);
            }
            catch(Exception e)
            {
                return (false, e.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, Rol? rol)> CrearRol(CreateRolRequestDto createRolDto, string userName)
        {
            Rol rol = createRolDto.ToRolFromCreateDto();
            rol.CreatedBy = userName;
            rol.UpdatedBy = userName;
            
            //Asociar los permisos al rol
            rol.RolPermisos = new List<RolPermiso>();
            foreach(var permisoId in createRolDto.Permisos)
            {
                rol.RolPermisos.Add(new RolPermiso{
                    PermisoId = permisoId,
                    Rol = rol
                });
            }

            _context.Roles.Add(rol);

            try{
                await _context.SaveChangesAsync();
                return (true, null, rol);
            }
            catch(Exception e)
            {
                return (false, e.Message, null);
            }
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
            var roles = _context.Roles
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .AsQueryable();
            // Filtro por busqueda de texto - barra de busqueda
            if (!string.IsNullOrWhiteSpace(queryObject.Name))
            {
                roles = roles.Where(s => s.Name.Contains(queryObject.Name));
            }

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                IOrderedQueryable<Rol> orderedRoles;

                if (queryObject.SortBy.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    orderedRoles = queryObject.IsDescending
                        ? roles.OrderByDescending(p => p.Id)
                        : roles.OrderBy(p => p.Id);
                }
                else
                {
                    // En caso de que SortBy tenga otro valor, se utiliza fecha de edición
                    orderedRoles = roles.OrderByDescending(p => p.UpdatedOn);
                }

                // Ordenamiento secundario por FechaUltimaEdicion
                roles = orderedRoles.ThenByDescending(p => p.UpdatedOn);
            }
            else
            {
                // Si no se especifica SortBy, ordenar solamente por FechaUltimaEdicion
                roles = roles.OrderByDescending(p => p.UpdatedOn);
            }

            int totalCount = await roles.CountAsync();

            var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            List<Rol> rolesList = await roles.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();

            var rolDtoList = rolesList.ToRolDtoList();

            return (totalCount, rolDtoList);                  
        }
    }
}