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

        // ========== MÉTODOS ADMINISTRATIVOS ==========

        /// <summary>
        /// Obtiene usuarios paginados con filtros para administración
        /// </summary>
        /// <summary>
        /// Obtiene usuarios con filtros y paginación para administración
        /// </summary>
        Task<DTOs.Usuarios.UsuariosAdminResponseDto> GetUsuariosAdmin(Helpers.UsuariosAdminQueryObject queryObject);

        /// <summary>
        /// Obtiene un usuario por ID con todas sus relaciones para administración
        /// </summary>
        /// <summary>
        /// Obtiene un usuario por ID para administración con detalles completos
        /// </summary>
        Task<DTOs.Usuarios.UsuarioDetalleDto?> GetUsuarioAdminByIdAsync(string usuarioId);

        /// <summary>
        /// Crea un nuevo usuario con persona asociada
        /// </summary>
        Task<(bool Success, List<string> Errors, string? UserId)> CreateUsuarioAsync(
            Usuario usuario, Persona persona, string password, List<string> roles);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        Task<(bool Success, List<string> Errors)> UpdateUsuarioAsync(Usuario usuario, List<string> newRoles);

        /// <summary>
        /// Resetea la contraseña de un usuario
        /// </summary>
        Task<(bool Success, List<string> Errors, string? NuevaContrasena)> ResetPasswordAsync(
            string usuarioId, string? nuevaContrasena, string adminId);

        /// <summary>
        /// Obtiene usuarios simples para dropdowns (jefes)
        /// </summary>
        Task<List<DTOs.Usuarios.UsuarioSimpleDto>> GetUsuariosSimpleAsync(bool soloActivos = true);

        /// <summary>
        /// Obtiene departamentos simples para dropdowns
        /// </summary>
        Task<List<DTOs.Usuarios.DepartamentoSimpleDto>> GetDepartamentosSimpleAsync(bool soloActivos = true);

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        Task<List<string>> GetAllRolesAsync();

        /// <summary>
        /// Obtiene estadísticas generales de usuarios
        /// </summary>
        Task<DTOs.Usuarios.UsuariosEstadisticasDto> GetUsuariosEstadisticasAsync();

        /// <summary>
        /// Verifica si un email ya está en uso por otro usuario
        /// </summary>
        Task<bool> EmailExistsAsync(string email, string? excludeUserId = null);

        /// <summary>
        /// Verifica si un DNI ya está en uso por otra persona
        /// </summary>
        Task<bool> DniExistsAsync(string dni, string? excludeUserId = null);

        /// <summary>
        /// Obtiene el número de subordinados directos de un usuario
        /// </summary>
        Task<int> GetNumeroSubordinadosAsync(string usuarioId);
    }
}