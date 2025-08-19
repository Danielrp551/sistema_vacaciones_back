using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        #region Métodos Administrativos de Usuarios

        /// <summary>
        /// Obtiene usuarios con filtros y paginación para administración
        /// </summary>
        public async Task<UsuariosAdminResponseDto> GetUsuariosAdmin(UsuariosAdminQueryObject queryObject)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Persona)
                    .Include(u => u.Departamento)
                    .Include(u => u.Jefe)
                        .ThenInclude(j => j != null ? j.Persona : null)
                    .AsQueryable();

                // ✅ Filtro por búsqueda general (nombre, email)
                if (!string.IsNullOrWhiteSpace(queryObject.BusquedaGeneral))
                {
                    var busqueda = queryObject.BusquedaGeneral.ToLower().Trim();
                    query = query.Where(u => 
                        (u.Email != null && u.Email.ToLower().Contains(busqueda)) ||
                        (u.Persona != null && u.Persona.Nombres != null && u.Persona.Nombres.ToLower().Contains(busqueda)) ||
                        (u.Persona != null && u.Persona.ApellidoPaterno != null && u.Persona.ApellidoPaterno.ToLower().Contains(busqueda)) ||
                        (u.Persona != null && u.Persona.ApellidoMaterno != null && u.Persona.ApellidoMaterno.ToLower().Contains(busqueda))
                    );
                }

                // ✅ Filtro por email específico
                if (!string.IsNullOrWhiteSpace(queryObject.Email))
                {
                    query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(queryObject.Email.ToLower()));
                }

                // ✅ Filtro por departamento
                if (!string.IsNullOrWhiteSpace(queryObject.DepartamentoId))
                {
                    query = query.Where(u => u.DepartamentoId == queryObject.DepartamentoId);
                }

                // ✅ Filtro por rol
                if (!string.IsNullOrWhiteSpace(queryObject.Rol))
                {
                    var usuariosConRol = await _userManager.GetUsersInRoleAsync(queryObject.Rol);
                    var usuarioIds = usuariosConRol.Select(u => u.Id).ToList();
                    query = query.Where(u => usuarioIds.Contains(u.Id));
                }

                // ✅ Filtro por estado activo
                if (queryObject.EstaActivo.HasValue)
                {
                    if (queryObject.EstaActivo.Value)
                    {
                        query = query.Where(u => !u.IsDeleted);
                    }
                    else
                    {
                        query = query.Where(u => u.IsDeleted);
                    }
                }

                // ✅ Filtro por extranjero
                if (queryObject.Extranjero.HasValue)
                {
                    query = query.Where(u => u.Persona != null && u.Persona.Extranjero == queryObject.Extranjero.Value);
                }

                // ✅ Filtro por fecha de ingreso
                if (queryObject.FechaIngresoDesde.HasValue)
                {
                    query = query.Where(u => u.Persona != null && u.Persona.FechaIngreso >= queryObject.FechaIngresoDesde.Value);
                }
                if (queryObject.FechaIngresoHasta.HasValue)
                {
                    query = query.Where(u => u.Persona != null && u.Persona.FechaIngreso <= queryObject.FechaIngresoHasta.Value);
                }

                // ✅ Aplicar ordenamiento
                query = AplicarOrdenamientoUsuariosAdmin(query, queryObject.SortBy, queryObject.IsDescending);

                // ✅ Contar total antes de paginación
                var totalUsuarios = await query.CountAsync();

                // ✅ Aplicar paginación
                var usuarios = await query
                    .Skip((queryObject.PageNumber - 1) * queryObject.PageSize)
                    .Take(queryObject.PageSize)
                    .ToListAsync();

                // ✅ Obtener roles para cada usuario (usando batch para eficiencia)
                var usuariosConRoles = new List<UsuarioAdminDto>();
                foreach (var usuario in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    var usuarioDto = UsuarioAdminMappers.ToUsuarioAdminDto(usuario, roles.ToList());
                    usuariosConRoles.Add(usuarioDto);
                }

                // ✅ Calcular estadísticas (para los filtros aplicados)
                var estadisticas = await CalcularEstadisticasUsuarios(query);

                // ✅ Generar respuesta con metadatos de paginación
                var filtrosAplicados = new UsuariosAdminFiltrosAplicados
                {
                    BusquedaGeneral = queryObject.BusquedaGeneral,
                    Email = queryObject.Email,
                    DepartamentoId = queryObject.DepartamentoId,
                    Rol = queryObject.Rol,
                    EstaActivo = queryObject.EstaActivo,
                    Extranjero = queryObject.Extranjero,
                    FechaIngresoDesde = queryObject.FechaIngresoDesde,
                    FechaIngresoHasta = queryObject.FechaIngresoHasta,
                    SortBy = queryObject.SortBy,
                    IsDescending = queryObject.IsDescending
                };

                return new UsuariosAdminResponseDto
                {
                    Usuarios = usuariosConRoles,
                    Total = usuariosConRoles.Count,
                    TotalCompleto = totalUsuarios,
                    PaginaActual = queryObject.PageNumber,
                    TamanoPagina = queryObject.PageSize,
                    TotalPaginas = (int)Math.Ceiling((double)totalUsuarios / queryObject.PageSize),
                    TienePaginaAnterior = queryObject.PageNumber > 1,
                    TienePaginaSiguiente = queryObject.PageNumber < (int)Math.Ceiling((double)totalUsuarios / queryObject.PageSize),
                    FiltrosAplicados = filtrosAplicados,
                    Estadisticas = estadisticas
                };
            }
            catch (Exception)
            {
                // Log error (inject ILogger si es necesario)
                return new UsuariosAdminResponseDto
                {
                    Usuarios = new List<UsuarioAdminDto>(),
                    Total = 0,
                    TotalCompleto = 0,
                    PaginaActual = 1,
                    TamanoPagina = queryObject.PageSize,
                    TotalPaginas = 0,
                    FiltrosAplicados = new UsuariosAdminFiltrosAplicados(),
                    Estadisticas = new UsuariosEstadisticasDto()
                };
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID para edición administrativa
        /// </summary>
        public async Task<UsuarioDetalleDto?> GetUsuarioAdminByIdAsync(string usuarioId)
        {
            try
            {
                var usuario = await _context.Users
                    .Include(u => u.Persona)
                    .Include(u => u.Departamento)
                    .Include(u => u.Jefe)
                        .ThenInclude(j => j != null ? j.Persona : null)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId && !u.IsDeleted);

                if (usuario == null)
                    return null;

                var roles = await _userManager.GetRolesAsync(usuario);
                return UsuarioAdminMappers.ToUsuarioDetalleDto(usuario, roles.ToList());
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario con persona y asigna roles
        /// </summary>
        public async Task<(bool Success, List<string> Errors, string? UserId)> CreateUsuarioAsync(
            Usuario usuario, Persona persona, string contrasenaTemporal, List<string> roles)
        {
            var errores = new List<string>();

            try
            {
                // ✅ Validar duplicados de email y DNI
                if (await EmailExistsAsync(usuario.Email!, null))
                {
                    errores.Add("El email ya está registrado en el sistema");
                }

                if (!persona.Extranjero && await DniExistsAsync(persona.Dni!, null))
                {
                    errores.Add("El DNI ya está registrado en el sistema");
                }

                if (errores.Any())
                {
                    return (false, errores, null);
                }

                // ✅ Generar contraseña temporal si no se proporcionó
                var password = string.IsNullOrEmpty(contrasenaTemporal) ? GenerateRandomPassword() : contrasenaTemporal;

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // ✅ 1. Crear la persona primero
                    persona.Id = Guid.NewGuid().ToString();
                    // persona.CreatedOn = DateTime.UtcNow; // Persona no tiene propiedades de auditoría
                    // persona.CreatedBy = "Sistema"; // Persona no tiene propiedades de auditoría
                    
                    _context.Personas.Add(persona);
                    await _context.SaveChangesAsync();

                    // ✅ 2. Configurar el usuario
                    usuario.Id = Guid.NewGuid().ToString();
                    usuario.UserName = usuario.Email;
                    usuario.NormalizedUserName = usuario.Email!.ToUpper();
                    usuario.NormalizedEmail = usuario.Email!.ToUpper();
                    usuario.EmailConfirmed = true;
                    usuario.PersonaId = persona.Id;
                    usuario.IsDeleted = false;
                    usuario.CreatedOn = DateTime.UtcNow;
                    usuario.CreatedBy = "Sistema";

                    // ✅ 3. Crear el usuario con contraseña
                    var resultado = await _userManager.CreateAsync(usuario, password);

                    if (!resultado.Succeeded)
                    {
                        foreach (var error in resultado.Errors)
                        {
                            errores.Add(error.Description);
                        }
                        await transaction.RollbackAsync();
                        return (false, errores, null);
                    }

                    // ✅ 4. Asignar roles
                    if (roles.Any())
                    {
                        var resultadoRoles = await _userManager.AddToRolesAsync(usuario, roles);
                        if (!resultadoRoles.Succeeded)
                        {
                            foreach (var error in resultadoRoles.Errors)
                            {
                                errores.Add($"Error al asignar rol: {error.Description}");
                            }
                            await transaction.RollbackAsync();
                            return (false, errores, null);
                        }
                    }

                    await transaction.CommitAsync();
                    return (true, errores, usuario.Id);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                errores.Add("Error interno al crear el usuario: " + ex.Message);
                return (false, errores, null);
            }
        }

        /// <summary>
        /// Actualiza un usuario existente y sus roles
        /// </summary>
        public async Task<(bool Success, List<string> Errors)> UpdateUsuarioAsync(Usuario usuarioActualizado, List<string> nuevosRoles)
        {
            var errores = new List<string>();

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ✅ 1. Actualizar el usuario
                    var resultado = await _userManager.UpdateAsync(usuarioActualizado);
                    if (!resultado.Succeeded)
                    {
                        foreach (var error in resultado.Errors)
                        {
                            errores.Add(error.Description);
                        }
                        await transaction.RollbackAsync();
                        return (false, errores);
                    }

                    // ✅ 2. Actualizar roles si se proporcionaron
                    if (nuevosRoles.Any())
                    {
                        var rolesActuales = await _userManager.GetRolesAsync(usuarioActualizado);
                        
                        // Remover roles actuales
                        if (rolesActuales.Any())
                        {
                            var removerRoles = await _userManager.RemoveFromRolesAsync(usuarioActualizado, rolesActuales);
                            if (!removerRoles.Succeeded)
                            {
                                errores.Add("Error al remover roles actuales");
                                await transaction.RollbackAsync();
                                return (false, errores);
                            }
                        }

                        // Agregar nuevos roles
                        var agregarRoles = await _userManager.AddToRolesAsync(usuarioActualizado, nuevosRoles);
                        if (!agregarRoles.Succeeded)
                        {
                            foreach (var error in agregarRoles.Errors)
                            {
                                errores.Add($"Error al asignar rol: {error.Description}");
                            }
                            await transaction.RollbackAsync();
                            return (false, errores);
                        }
                    }

                    await transaction.CommitAsync();
                    return (true, errores);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                errores.Add("Error interno al actualizar el usuario: " + ex.Message);
                return (false, errores);
            }
        }

        /// <summary>
        /// Reinicia la contraseña de un usuario y opcionalmente fuerza el cambio
        /// </summary>
        public async Task<(bool Success, List<string> Errors, string? NuevaContrasena)> ResetPasswordAsync(
            string usuarioId, string? nuevaContrasena, string adminId)
        {
            var errores = new List<string>();

            try
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                {
                    errores.Add("Usuario no encontrado");
                    return (false, errores, null);
                }

                // ✅ Generar contraseña aleatoria si no se proporcionó
                var password = string.IsNullOrEmpty(nuevaContrasena) ? GenerateRandomPassword() : nuevaContrasena;

                // ✅ Remover la contraseña actual y establecer la nueva
                var removeResult = await _userManager.RemovePasswordAsync(usuario);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        errores.Add(error.Description);
                    }
                    return (false, errores, null);
                }

                var addResult = await _userManager.AddPasswordAsync(usuario, password);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        errores.Add(error.Description);
                    }
                    return (false, errores, null);
                }

                // ✅ Nota: Funcionalidad de forzar cambio de contraseña pendiente de implementar en modelo
                // usuario.ForzarCambioContrasena = true;
                usuario.UpdatedOn = DateTime.UtcNow;
                usuario.UpdatedBy = adminId;

                var updateResult = await _userManager.UpdateAsync(usuario);
                if (!updateResult.Succeeded)
                {
                    errores.Add("Error al actualizar la configuración del usuario");
                    return (false, errores, null);
                }

                return (true, errores, password);
            }
            catch (Exception ex)
            {
                errores.Add("Error interno al resetear la contraseña: " + ex.Message);
                return (false, errores, null);
            }
        }

        /// <summary>
        /// Obtiene usuarios simples para dropdowns
        /// </summary>
        public async Task<List<UsuarioSimpleDto>> GetUsuariosSimpleAsync(bool soloActivos = true)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Persona)
                    .Where(u => !u.IsDeleted);

                if (soloActivos)
                {
                    query = query.Where(u => !u.IsDeleted);
                }

                var usuarios = await query
                    .OrderBy(u => u.Persona!.Nombres)
                    .Select(u => new UsuarioSimpleDto
                    {
                        Id = u.Id,
                        NombreCompleto = u.Persona!.Nombres + " " + u.Persona.ApellidoPaterno + " " + (u.Persona.ApellidoMaterno ?? ""),
                        Email = u.Email ?? ""
                    })
                    .ToListAsync();

                return usuarios;
            }
            catch (Exception)
            {
                return new List<UsuarioSimpleDto>();
            }
        }

        /// <summary>
        /// Obtiene departamentos simples para dropdowns
        /// </summary>
        public async Task<List<DepartamentoSimpleDto>> GetDepartamentosSimpleAsync(bool soloActivos = true)
        {
            try
            {
                var query = _context.Departamentos.AsQueryable();

                if (soloActivos)
                {
                    query = query.Where(d => !d.IsDeleted);
                }

                // ✅ Traer los datos completos de la base de datos
                var departamentosFromDb = await query
                    .OrderBy(d => d.Nombre)
                    .ToListAsync();

                // ✅ Mapear a DTOs en memoria (evita problemas de traducción EF)
                var departamentos = departamentosFromDb.Select(d => 
                {
                    var departamentoDto = new DTOs.Usuarios.DepartamentoSimpleDto();
                    departamentoDto.Id = d.Id;
                    departamentoDto.Nombre = d.Nombre;
                    departamentoDto.Codigo = d.Codigo;
                    departamentoDto.EstaActivo = !d.IsDeleted;
                    return departamentoDto;
                }).ToList();

                return departamentos;
            }
            catch (Exception)
            {
                return new List<DepartamentoSimpleDto>();
            }
        }

        /// <summary>
        /// Obtiene todos los roles disponibles
        /// </summary>
        public async Task<List<string>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name!)
                    .ToListAsync();

                return roles;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales de usuarios
        /// </summary>
        public async Task<UsuariosEstadisticasDto> GetUsuariosEstadisticasAsync()
        {
            try
            {
                var totalUsuarios = await _context.Users.CountAsync(u => !u.IsDeleted);
                var usuariosActivos = await _context.Users.CountAsync(u => !u.IsDeleted);
                var usuariosInactivos = totalUsuarios - usuariosActivos;

                // var usuariosConForzarCambio = await _context.Users
                //     .CountAsync(u => !u.IsDeleted && u.ForzarCambioContrasena);
                var usuariosConForzarCambio = 0; // Temporal hasta implementar propiedad

                var extranjeros = await _context.Users
                    .CountAsync(u => !u.IsDeleted && u.Persona != null && u.Persona.Extranjero);

                return new UsuariosEstadisticasDto
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = usuariosActivos,
                    UsuariosInactivos = usuariosInactivos,
                    UsuariosForzarCambio = usuariosConForzarCambio,
                    UsuariosExtranjeros = extranjeros
                };
            }
            catch (Exception)
            {
                return new UsuariosEstadisticasDto();
            }
        }

        /// <summary>
        /// Verifica si un email ya existe (excluyendo un usuario específico)
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null)
        {
            try
            {
                var query = _context.Users.Where(u => !u.IsDeleted && u.Email!.ToLower() == email.ToLower());
                
                if (!string.IsNullOrEmpty(excludeUserId))
                {
                    query = query.Where(u => u.Id != excludeUserId);
                }

                return await query.AnyAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si un DNI ya existe (excluyendo un usuario específico)
        /// </summary>
        public async Task<bool> DniExistsAsync(string dni, string? excludeUserId = null)
        {
            try
            {
                var query = _context.Users
                    .Where(u => !u.IsDeleted && u.Persona != null && u.Persona.Dni == dni);
                
                if (!string.IsNullOrEmpty(excludeUserId))
                {
                    query = query.Where(u => u.Id != excludeUserId);
                }

                return await query.AnyAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el número de subordinados directos de un usuario
        /// </summary>
        public async Task<int> GetNumeroSubordinadosAsync(string usuarioId)
        {
            try
            {
                return await _context.Users
                    .CountAsync(u => !u.IsDeleted && u.JefeId == usuarioId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Métodos Privados de Apoyo

        /// <summary>
        /// Genera una contraseña aleatoria de 12 caracteres con alta complejidad
        /// </summary>
        private string GenerateRandomPassword()
        {
            const int longitud = 12;
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string simbolos = "!@#$%&*+=-_?";

            var random = new Random();
            var password = new StringBuilder();

            // ✅ Garantizar al menos 1 carácter de cada tipo
            password.Append(mayusculas[random.Next(mayusculas.Length)]);
            password.Append(minusculas[random.Next(minusculas.Length)]);
            password.Append(numeros[random.Next(numeros.Length)]);
            password.Append(simbolos[random.Next(simbolos.Length)]);

            // ✅ Llenar el resto de caracteres aleatoriamente
            var todosCaracteres = mayusculas + minusculas + numeros + simbolos;
            for (int i = 4; i < longitud; i++)
            {
                password.Append(todosCaracteres[random.Next(todosCaracteres.Length)]);
            }

            // ✅ Mezclar los caracteres para evitar patrones predecibles
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        /// <summary>
        /// Aplica ordenamiento a la consulta de usuarios administrativos
        /// </summary>
        private IQueryable<Usuario> AplicarOrdenamientoUsuariosAdmin(IQueryable<Usuario> query, string? sortBy, bool isDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(u => u.Persona!.Nombres)
                           .ThenBy(u => u.Persona!.ApellidoPaterno);
            }

            switch (sortBy.ToLower())
            {
                case "email":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email);

                case "nombrecompleto":
                case "nombres":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Persona!.Nombres)
                               .ThenByDescending(u => u.Persona!.ApellidoPaterno)
                        : query.OrderBy(u => u.Persona!.Nombres)
                               .ThenBy(u => u.Persona!.ApellidoPaterno);

                case "dni":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Persona!.Dni)
                        : query.OrderBy(u => u.Persona!.Dni);

                case "fechaingreso":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Persona!.FechaIngreso)
                        : query.OrderBy(u => u.Persona!.FechaIngreso);

                case "empresa":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Persona!.Empresa)
                        : query.OrderBy(u => u.Persona!.Empresa);

                case "departamento":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Departamento!.Nombre)
                        : query.OrderBy(u => u.Departamento!.Nombre);

                case "extranjero":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Persona!.Extranjero)
                        : query.OrderBy(u => u.Persona!.Extranjero);

                case "estado":
                case "estaactivo":
                    return isDescending 
                        ? query.OrderByDescending(u => !u.IsDeleted)
                        : query.OrderBy(u => !u.IsDeleted);

                case "manager":
                case "jefe":
                    return isDescending 
                        ? query.OrderByDescending(u => u.Jefe!.Persona!.Nombres)
                        : query.OrderBy(u => u.Jefe!.Persona!.Nombres);

                default:
                    return query.OrderBy(u => u.Persona!.Nombres)
                               .ThenBy(u => u.Persona!.ApellidoPaterno);
            }
        }

        /// <summary>
        /// Calcula estadísticas para una consulta específica de usuarios
        /// </summary>
        private async Task<UsuariosEstadisticasDto> CalcularEstadisticasUsuarios(IQueryable<Usuario> query)
        {
            try
            {
                var total = await query.CountAsync();
                var activos = await query.CountAsync(u => !u.IsDeleted);
                var inactivos = total - activos;
                // Nota: No hay propiedad ForzarCambioContrasena en Usuario - necesita implementarse
                var forzarCambio = 0; // Temporalmente 0 hasta implementar la funcionalidad
                var extranjeros = await query.CountAsync(u => u.Persona != null && u.Persona.Extranjero);

                return new UsuariosEstadisticasDto
                {
                    TotalUsuarios = total,
                    UsuariosActivos = activos,
                    UsuariosInactivos = inactivos,
                    UsuariosForzarCambio = forzarCambio,
                    UsuariosExtranjeros = extranjeros
                };
            }
            catch (Exception)
            {
                return new UsuariosEstadisticasDto();
            }
        }

        #endregion
    }
}