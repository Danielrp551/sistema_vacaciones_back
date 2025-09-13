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

        // ========== MÉTODOS PARA BULK IMPORT ==========

        /// <summary>
        /// Obtiene el ID de un departamento por su código
        /// </summary>
        /// <param name="codigo">Código del departamento</param>
        /// <returns>ID del departamento o null si no existe</returns>
        Task<string?> GetDepartamentoIdByCodigo(string codigo);

        /// <summary>
        /// Obtiene el ID de un usuario por su DNI
        /// </summary>
        /// <param name="dni">DNI del usuario</param>
        /// <returns>ID del usuario o null si no existe</returns>
        Task<string?> GetUsuarioIdByDni(string dni);

        /// <summary>
        /// Valida que múltiples emails no existan en el sistema
        /// </summary>
        /// <param name="emails">Lista de emails a validar</param>
        /// <returns>Lista de emails que ya existen</returns>
        Task<List<string>> GetExistingEmails(List<string> emails);

        /// <summary>
        /// Valida que múltiples DNIs no existan en el sistema
        /// </summary>
        /// <param name="dnis">Lista de DNIs a validar</param>
        /// <returns>Lista de DNIs que ya existen</returns>
        Task<List<string>> GetExistingDnis(List<string> dnis);

        /// <summary>
        /// Valida que múltiples códigos de departamento existan
        /// </summary>
        /// <param name="codigos">Lista de códigos de departamento</param>
        /// <returns>Diccionario con código como key e ID como value para los que existen</returns>
        Task<Dictionary<string, string>> GetDepartamentosByCodigos(List<string> codigos);

        /// <summary>
        /// Valida que múltiples DNIs de jefes existan
        /// </summary>
        /// <param name="dnis">Lista de DNIs de jefes</param>
        /// <returns>Diccionario con DNI como key e ID de usuario como value para los que existen</returns>
        Task<Dictionary<string, string>> GetJefesByDnis(List<string> dnis);

        /// <summary>
        /// Valida que múltiples roles existan en el sistema
        /// </summary>
        /// <param name="roles">Lista de nombres de roles</param>
        /// <returns>Lista de roles que existen</returns>
        Task<List<string>> GetValidRoles(List<string> roles);

        /// <summary>
        /// Crea múltiples usuarios en una transacción
        /// </summary>
        /// <param name="usuariosData">Lista de tuplas con Usuario, Persona, contraseña y roles</param>
        /// <returns>Lista de resultados con éxito/error y ID generado</returns>
        Task<List<(bool Success, List<string> Errors, string? UserId, string Email)>> CreateUsuariosBulkAsync(
            List<(Usuario usuario, Persona persona, string password, List<string> roles)> usuariosData);
    }
}