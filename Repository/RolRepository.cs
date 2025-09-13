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
            try
            {
                // Buscar el rol existente
                var rolExistente = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == rolRequestDto.Id && !r.IsDeleted);

                if (rolExistente == null)
                {
                    return (false, "El rol no existe", null);
                }

                // Verificar si existe otro rol con el mismo nombre (excluyendo el actual)
                var existeOtroRol = await _context.Roles
                    .AnyAsync(r => r.Name != null && r.Name.ToLower() == rolRequestDto.Name.ToLower() 
                              && !r.IsDeleted && r.Id != rolRequestDto.Id);

                if (existeOtroRol)
                {
                    return (false, "Ya existe otro rol con ese nombre", null);
                }

                // Verificar que los permisos existan
                var permisosExisten = await _context.Permisos
                    .Where(p => rolRequestDto.Permisos.Contains(p.Id))
                    .CountAsync();

                if (permisosExisten != rolRequestDto.Permisos.Count)
                {
                    return (false, "Uno o más permisos no existen", null);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Actualizar el rol
                    rolExistente.Name = rolRequestDto.Name.Trim();
                    rolExistente.NormalizedName = rolRequestDto.Name.Trim().ToUpper();
                    rolExistente.Descripcion = rolRequestDto.Descripcion.Trim();
                    rolExistente.UpdatedBy = userName;
                    rolExistente.UpdatedOn = DateTime.UtcNow;

                    _context.Roles.Update(rolExistente);

                    // Eliminar permisos existentes
                    var permisosActuales = await _context.RolPermisos
                        .Where(rp => rp.RolId == rolRequestDto.Id)
                        .ToListAsync();

                    _context.RolPermisos.RemoveRange(permisosActuales);

                    // Asignar nuevos permisos
                    var nuevosRolPermisos = rolRequestDto.Permisos.Select(permisoId => new RolPermiso
                    {
                        RolId = rolExistente.Id,
                        PermisoId = permisoId
                    }).ToList();

                    _context.RolPermisos.AddRange(nuevosRolPermisos);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return (true, null, rolExistente);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return (false, "Error interno al actualizar el rol: " + ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, Rol? rol)> CrearRol(CreateRolRequestDto createRolDto, string userName)
        {
            try
            {
                // Verificar si el rol ya existe
                var existeRol = await _context.Roles
                    .AnyAsync(r => r.Name != null && r.Name.ToLower() == createRolDto.Name.ToLower() && !r.IsDeleted);

                if (existeRol)
                {
                    return (false, "Ya existe un rol con ese nombre", null);
                }

                // Verificar que los permisos existan
                var permisosExisten = await _context.Permisos
                    .Where(p => createRolDto.Permisos.Contains(p.Id))
                    .CountAsync();

                if (permisosExisten != createRolDto.Permisos.Count)
                {
                    return (false, "Uno o más permisos no existen", null);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Crear el rol
                    var nuevoRol = new Rol
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = createRolDto.Name.Trim(),
                        NormalizedName = createRolDto.Name.Trim().ToUpper(),
                        Descripcion = createRolDto.Descripcion.Trim(),
                        NumeroPersonas = 0,
                        Estado = "activo",
                        CreatedBy = userName,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.Roles.Add(nuevoRol);
                    await _context.SaveChangesAsync();

                    // Asignar permisos al rol
                    var rolPermisos = createRolDto.Permisos.Select(permisoId => new RolPermiso
                    {
                        RolId = nuevoRol.Id,
                        PermisoId = permisoId
                    }).ToList();

                    _context.RolPermisos.AddRange(rolPermisos);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return (true, null, nuevoRol);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return (false, "Error interno al crear el rol: " + ex.Message, null);
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

        public async Task<RolDto?> GetRolById(string rolId, string usuarioId)
        {
            try
            {
                var rol = await _context.Roles
                    .Where(r => r.Id == rolId && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (rol == null)
                {
                    return null;
                }

                // Obtener permisos para el rol
                var permisos = await _context.RolPermisos
                    .Where(rp => rp.RolId == rol.Id)
                    .Include(rp => rp.Permiso)
                    .Select(rp => rp.Permiso)
                    .ToListAsync();

                // Obtener número de personas asignadas a este rol (calculado dinámicamente)
                var numeroPersonas = await _context.UserRoles
                    .CountAsync(ur => ur.RoleId == rolId);

                var rolDto = rol.ToRolDto();
                rolDto.Permisos = permisos.ToPermisoDtoList();
                // Asegurar que el número de personas se calcule dinámicamente
                rolDto.NumeroPersonas = numeroPersonas;

                return rolDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(int, List<RolDto>)> GetRolPagination(RolesQueryObject queryObject, string usuarioId)
        {
            try
            {
                var query = _context.Roles
                    .Where(r => !r.IsDeleted)
                    .AsQueryable();

                // Filtro por nombre de rol
                if (!string.IsNullOrWhiteSpace(queryObject.Name))
                {
                    var searchTerm = queryObject.Name.ToLower().Trim();
                    query = query.Where(r => r.Name != null && r.Name.ToLower().Contains(searchTerm));
                }

                // Aplicar ordenamiento
                if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
                {
                    switch (queryObject.SortBy.ToLower())
                    {
                        case "name":
                        case "nombre":
                            query = queryObject.IsDescending 
                                ? query.OrderByDescending(r => r.Name)
                                : query.OrderBy(r => r.Name);
                            break;
                        case "descripcion":
                            query = queryObject.IsDescending 
                                ? query.OrderByDescending(r => r.Descripcion)
                                : query.OrderBy(r => r.Descripcion);
                            break;
                        case "fechacreacion":
                        case "createdon":
                            query = queryObject.IsDescending 
                                ? query.OrderByDescending(r => r.CreatedOn)
                                : query.OrderBy(r => r.CreatedOn);
                            break;
                        case "usuarios":
                        case "usuarioscount":
                        case "numeropersonas":
                            // Ordenar por el conteo dinámico de UserRoles en lugar del campo NumeroPersonas
                            query = queryObject.IsDescending 
                                ? query.OrderByDescending(r => _context.UserRoles.Count(ur => ur.RoleId == r.Id))
                                : query.OrderBy(r => _context.UserRoles.Count(ur => ur.RoleId == r.Id));
                            break;
                        default:
                            query = query.OrderBy(r => r.Name);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(r => r.Name);
                }

                // Contar total antes de paginación
                var totalCount = await query.CountAsync();

                // Aplicar paginación
                var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;
                var roles = await query
                    .Skip(skipNumber)
                    .Take(queryObject.PageSize)
                    .ToListAsync();

                // Obtener conteo de usuarios por rol de manera eficiente
                var roleIds = roles.Select(r => r.Id).ToList();
                var userRoleCounts = await _context.UserRoles
                    .Where(ur => roleIds.Contains(ur.RoleId))
                    .GroupBy(ur => ur.RoleId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                // Obtener permisos para cada rol
                var rolDtoList = new List<RolDto>();
                foreach (var rol in roles)
                {
                    var permisos = await _context.RolPermisos
                        .Where(rp => rp.RolId == rol.Id)
                        .Include(rp => rp.Permiso)
                        .Select(rp => rp.Permiso)
                        .ToListAsync();

                    var rolDto = rol.ToRolDto();
                    rolDto.Permisos = permisos.ToPermisoDtoList();
                    // Calcular dinámicamente el número de personas asignadas
                    rolDto.NumeroPersonas = userRoleCounts.GetValueOrDefault(rol.Id, 0);
                    rolDtoList.Add(rolDto);
                }

                return (totalCount, rolDtoList);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al obtener roles: " + e.Message);
                return (0, new List<RolDto>());
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> EliminarRol(string rolId, string userName)
        {
            try
            {
                // Buscar el rol existente
                var rolExistente = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == rolId && !r.IsDeleted);

                if (rolExistente == null)
                {
                    return (false, "El rol no existe");
                }

                // Verificar si hay usuarios asignados a este rol
                var usuariosConRol = await _context.UserRoles
                    .CountAsync(ur => ur.RoleId == rolId);

                if (usuariosConRol > 0)
                {
                    return (false, "No se puede eliminar el rol porque tiene usuarios asignados");
                }

                // Soft delete
                rolExistente.IsDeleted = true;
                rolExistente.Estado = "inactivo";
                rolExistente.UpdatedBy = userName;
                rolExistente.UpdatedOn = DateTime.UtcNow;

                _context.Roles.Update(rolExistente);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Error interno al eliminar el rol: " + ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, string? nuevoEstado)> CambiarEstadoRol(string rolId, string userName)
        {
            try
            {
                // Buscar el rol existente
                var rolExistente = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == rolId && !r.IsDeleted);

                if (rolExistente == null)
                {
                    return (false, "El rol no existe", null);
                }

                // Determinar el nuevo estado
                string nuevoEstado = rolExistente.Estado?.ToLower() == "activo" ? "inactivo" : "activo";

                // Actualizar el estado
                rolExistente.Estado = nuevoEstado;
                rolExistente.UpdatedBy = userName;
                rolExistente.UpdatedOn = DateTime.UtcNow;

                _context.Roles.Update(rolExistente);
                await _context.SaveChangesAsync();

                return (true, null, nuevoEstado);
            }
            catch (Exception ex)
            {
                return (false, "Error interno al cambiar estado del rol: " + ex.Message, null);
            }
        }
    }
}