using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Data;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ApplicationDBContext _context;

        public UsuarioRepository(UserManager<Usuario> userManager, ApplicationDBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _userManager.Users.Include(u => u.Persona).FirstOrDefaultAsync(u => u.UserName == username.ToLower());
        }

        public async Task<IList<string>> GetUserRolesAsync(Usuario user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> AddUserToRoleAsync(Usuario user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task AddAsync(Usuario user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task UpdateAsync(Usuario user)
        {
            await _userManager.UpdateAsync(user);
        }

        public Task DeleteAsync(Usuario user)
        {
            return _userManager.DeleteAsync(user);
        }

        public Task<bool> SaveChangesAsync()
        {
            return Task.FromResult(true); // No se usa directamente un DbContext, ya que UserManager maneja los cambios
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _userManager.Users.Include(u => u.Persona).ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<string>> GetUserPermissionsAsync(Usuario usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            var permisos = await _context.RolPermisos
                .Where(rp => rp.Rol.Name != null && roles.Contains(rp.Rol.Name))
                .Select(rp => rp.Permiso.Nombre) 
                .Distinct()
                .ToListAsync();
            return permisos;
        }

        public async Task<(int, List<UsuarioDto>)> GetUsuarios(UsuariosQueryObject queryObject, string usuarioId)
        {
            var usuarios = _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Jefe)
                .Where(u => u.IsDeleted == false)
                .AsQueryable();
            // Filtro por busqueda de texto - barra de busqueda
            //if (!string.IsNullOrWhiteSpace(queryObject.Name))
            //{
            //    roles = roles.Where(s => s.Name.Contains(queryObject.Name));
            //}
            /*
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
            */

            int totalCount = await usuarios.CountAsync();

            var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            List<Usuario> usuariosList = await usuarios.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();

            var usuariosDtoList = usuariosList.ToUsuarioDtoList();

            return (totalCount, usuariosDtoList); 
        }

        /// <summary>
        /// Obtiene los empleados del equipo de un supervisor
        /// </summary>
        public async Task<List<Usuario>> GetEmpleadosEquipo(string supervisorId, bool incluirSubordinadosNivelN)
        {
            try
            {
                if (incluirSubordinadosNivelN)
                {
                    // Obtener subordinados de manera recursiva
                    var subordinados = new List<Usuario>();
                    var queue = new Queue<string>();
                    queue.Enqueue(supervisorId);
                    var visitados = new HashSet<string>();

                    while (queue.Count > 0)
                    {
                        var currentSupervisorId = queue.Dequeue();
                        
                        if (visitados.Contains(currentSupervisorId))
                            continue;
                            
                        visitados.Add(currentSupervisorId);

                        var empleadosDirectos = await _context.Users
                            .Include(u => u.Persona)
                            .Where(u => u.JefeId == currentSupervisorId && !u.IsDeleted)
                            .ToListAsync();

                        foreach (var empleado in empleadosDirectos)
                        {
                            if (!subordinados.Any(s => s.Id == empleado.Id))
                            {
                                subordinados.Add(empleado);
                                queue.Enqueue(empleado.Id); // Para buscar sus subordinados
                            }
                        }
                    }

                    return subordinados;
                }
                else
                {
                    // Solo empleados directos (nivel 1)
                    return await _context.Users
                        .Include(u => u.Persona)
                        .Where(u => u.JefeId == supervisorId && !u.IsDeleted)
                        .ToListAsync();
                }
            }
            catch (Exception)
            {
                return new List<Usuario>();
            }
        }
    }
}