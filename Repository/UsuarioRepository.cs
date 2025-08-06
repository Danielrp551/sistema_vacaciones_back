using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SISTEMA_VACACIONES.Data;
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

        public async Task<Usuario> GetByUsernameAsync(string username)
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

        public async Task<bool> SaveChangesAsync()
        {
            return true; // No se usa directamente un DbContext, ya que UserManager maneja los cambios
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _userManager.Users.Include(u => u.Persona).ToListAsync();
        }

        public async Task<Usuario> GetByIdAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<string>> GetUserRoutesAsync(Usuario usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            // 📌 Obtener las rutas asociadas a esos roles
            var rutas = await _context.RolPermisos
                .Where(rp => roles.Contains(rp.Rol.Name))
                .Select(rp => rp.Permiso.NombreRuta) // 📌 'NombreRuta' almacena la ruta
                .Distinct()
                .ToListAsync();

            return rutas;
        }

        public async Task<(int, List<UsuarioDto>)> GetUsuarios(UsuariosQueryObject queryObject, string usuarioId)
        {
            var usuarios = _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Jefe)
                .Where(u => u.isDeleted == false)
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
    }
}