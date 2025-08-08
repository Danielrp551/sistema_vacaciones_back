using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Interfaces
{
    public interface  IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(string id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task AddAsync(Usuario user);
        Task UpdateAsync(Usuario user);
        Task DeleteAsync(Usuario user);
        Task<bool> SaveChangesAsync();
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<IList<string>> GetUserRolesAsync(Usuario user);
        Task<List<string>> GetUserPermissionsAsync(Usuario usuario);
        Task<bool> AddUserToRoleAsync(Usuario user, string role);     

        Task<(int, List<UsuarioDto>)> GetUsuarios(UsuariosQueryObject queryObject, string usuarioId);

        /// <summary>
        /// Obtiene los empleados del equipo de un supervisor
        /// </summary>
        Task<List<Usuario>> GetEmpleadosEquipo(string supervisorId, bool incluirSubordinadosNivelN);
    }
}