using System.Globalization;
using System.Text;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Services
{
    /// <summary>
    /// Implementación del servicio principal de bulk import de usuarios.
    /// Orquesta todo el proceso de validación, conversión y creación masiva de usuarios.
    /// </summary>
    public class BulkImportService : IBulkImportService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IBulkImportValidator _validator;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<BulkImportService> _logger;

        public BulkImportService(
            IUsuarioRepository usuarioRepository,
            IBulkImportValidator validator,
            IAuditoriaService auditoriaService,
            ILogger<BulkImportService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _validator = validator;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Procesa una solicitud completa de bulk import de usuarios
        /// </summary>
        public async Task<BulkImportResultDto> ProcessBulkImportAsync(
            BulkImportRequestDto request, 
            string adminId, 
            string ipAddress, 
            string userAgent)
        {
            var resultado = new BulkImportResultDto
            {
                FechaInicio = DateTime.UtcNow,
                AdminId = adminId,
                Metadata = request.Metadata
            };

            try
            {
                _logger.LogInformation("Iniciando bulk import de {Count} usuarios por admin {AdminId}", 
                    request.Usuarios.Count, adminId);

                // ✅ 1. Validar la estructura general de la solicitud
                var erroresEstructura = _validator.ValidateRequestStructure(request);
                if (erroresEstructura.Any())
                {
                    resultado.Exitoso = false;
                    resultado.Mensaje = "Errores de estructura en la solicitud";
                    resultado.RegistrosFallidos.AddRange(erroresEstructura);
                    resultado.FechaFin = DateTime.UtcNow;
                    resultado.TiempoProcesamiento = resultado.FechaFin - resultado.FechaInicio;
                    return resultado;
                }

                // ✅ 2. Validar completamente la solicitud
                var erroresValidacion = await ValidateRequestAsync(request);
                if (erroresValidacion.Any())
                {
                    _logger.LogWarning("Bulk import falló validaciones. Total errores: {Count}", erroresValidacion.Count);
                    
                    resultado.Exitoso = false;
                    resultado.Mensaje = $"Se encontraron {erroresValidacion.Count} errores de validación";
                    resultado.RegistrosFallidos.AddRange(erroresValidacion);
                    resultado.FechaFin = DateTime.UtcNow;
                    resultado.TiempoProcesamiento = resultado.FechaFin - resultado.FechaInicio;
                    resultado.Estadisticas = GenerarEstadisticas(new List<BulkImportUsuarioResultDto>(), erroresValidacion, resultado.TiempoProcesamiento);
                    return resultado;
                }

                // ✅ 3. Procesar usuarios válidos
                resultado = await ProcesarUsuariosValidosAsync(request, adminId, ipAddress, userAgent, resultado);

                // ✅ 4. Registrar auditoría del proceso completo
                await RegistrarAuditoriaProcesoAsync(resultado, adminId, ipAddress, userAgent);

                _logger.LogInformation("Bulk import completado. Exitosos: {Exitosos}, Fallidos: {Fallidos}", 
                    resultado.UsuariosCreados.Count, resultado.RegistrosFallidos.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante bulk import por admin {AdminId}", adminId);
                
                resultado.Exitoso = false;
                resultado.Mensaje = "Error crítico durante el procesamiento: " + ex.Message;
                resultado.FechaFin = DateTime.UtcNow;
                resultado.TiempoProcesamiento = resultado.FechaFin - resultado.FechaInicio;

                // Registrar error en auditoría
                try
                {
                    await _auditoriaService.RegistrarAccionSimpleAsync(
                        TipoAccionAuditoria.BULK_IMPORT_ERROR,
                        ModuloSistema.GESTION_USUARIOS,
                        "BULK_IMPORT",
                        "ERROR",
                        adminId,
                        motivo: $"Error crítico en bulk import: {ex.Message}",
                        ipAddress: ipAddress,
                        severidad: SeveridadAuditoria.ERROR
                    );
                }
                catch (Exception auditEx)
                {
                    _logger.LogError(auditEx, "Error adicional al registrar auditoría de error");
                }

                return resultado;
            }
        }

        /// <summary>
        /// Valida completamente una solicitud de bulk import
        /// </summary>
        public async Task<List<BulkImportErrorDto>> ValidateRequestAsync(BulkImportRequestDto request)
        {
            var errores = new List<BulkImportErrorDto>();

            try
            {
                // ✅ 1. Validar estructura básica
                errores.AddRange(_validator.ValidateRequestStructure(request));
                if (errores.Any()) return errores;

                // ✅ 2. Validar cada usuario individualmente
                foreach (var usuario in request.Usuarios)
                {
                    var erroresUsuario = _validator.ValidateUsuarioFields(usuario);
                    errores.AddRange(erroresUsuario);
                }

                // ✅ 3. Validar duplicados internos
                var erroresDuplicados = _validator.ValidateDuplicadosInternos(request.Usuarios);
                errores.AddRange(erroresDuplicados);

                // ✅ 4. Validar jerarquías circulares
                var erroresJerarquia = _validator.ValidateJerarquiasCirculares(request.Usuarios);
                errores.AddRange(erroresJerarquia);

                // ✅ 5. Validar contra base de datos
                var erroresBD = await ValidarContraBaseDatosAsync(request.Usuarios);
                errores.AddRange(erroresBD);

                return errores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante validación de bulk import");
                errores.Add(_validator.CreateError(0, TipoErrorBulkImport.ERROR_VALIDACION_NEGOCIO, 
                    "Error interno durante validación: " + ex.Message));
                return errores;
            }
        }

        /// <summary>
        /// Valida un usuario individual contra las reglas de negocio
        /// </summary>
        public async Task<List<BulkImportErrorDto>> ValidateUsuarioAsync(
            BulkImportUsuarioDto usuario, 
            List<BulkImportUsuarioDto> otrosUsuarios)
        {
            var errores = new List<BulkImportErrorDto>();

            // Validar campos básicos
            errores.AddRange(_validator.ValidateUsuarioFields(usuario));

            // Validar duplicados dentro del lote
            var usuariosParaDuplicados = otrosUsuarios.ToList();
            usuariosParaDuplicados.Add(usuario);
            errores.AddRange(_validator.ValidateDuplicadosInternos(usuariosParaDuplicados));

            // Validar contra base de datos
            var erroresBD = await ValidarUsuarioContraBaseDatosAsync(usuario);
            errores.AddRange(erroresBD);

            return errores;
        }

        /// <summary>
        /// Resuelve el ID de departamento a partir del código
        /// </summary>
        public async Task<string?> ResolverDepartamentoAsync(string codigoDepartamento)
        {
            try
            {
                return await _usuarioRepository.GetDepartamentoIdByCodigo(codigoDepartamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver departamento {Codigo}", codigoDepartamento);
                return null;
            }
        }

        /// <summary>
        /// Resuelve el ID de usuario (jefe) a partir del DNI
        /// </summary>
        public async Task<string?> ResolverJefeAsync(string dniJefe)
        {
            try
            {
                return await _usuarioRepository.GetUsuarioIdByDni(dniJefe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver jefe con DNI {Dni}", dniJefe);
                return null;
            }
        }

        /// <summary>
        /// Valida que todos los roles especificados existan
        /// </summary>
        public async Task<List<string>> ValidarRolesAsync(List<string> roles)
        {
            try
            {
                var rolesValidos = await _usuarioRepository.GetValidRoles(roles);
                return roles.Except(rolesValidos, StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar roles");
                return roles; // Todos son inválidos si hay error
            }
        }

        /// <summary>
        /// Convierte un usuario de bulk import a entidades del sistema
        /// </summary>
        public (Usuario usuario, Persona persona) ConvertirAEntidades(
            BulkImportUsuarioDto bulkUsuario, 
            string departamentoId, 
            string? jefeId, 
            string adminId)
        {
            // Parsear fecha de ingreso
            var (fechaIngreso, _) = _validator.ValidateAndParseFecha(
                bulkUsuario.FechaIngreso, bulkUsuario.NumeroFila, "FechaIngreso");

            // Parsear extranjero
            var (extranjero, _) = _validator.ValidateAndParseBoolean(
                bulkUsuario.Extranjero, bulkUsuario.NumeroFila, "Extranjero");

            // Crear entidad Persona
            var persona = new Persona
            {
                Nombres = bulkUsuario.Nombres.Trim(),
                ApellidoPaterno = bulkUsuario.ApellidoPaterno.Trim(),
                ApellidoMaterno = bulkUsuario.ApellidoMaterno.Trim(),
                Dni = bulkUsuario.Dni.Trim(),
                Extranjero = extranjero ?? false,
                FechaIngreso = fechaIngreso ?? DateTime.Today,
                Empresa = bulkUsuario.Empresa.Trim()
            };

            // Crear entidad Usuario
            var usuario = new Usuario
            {
                Email = bulkUsuario.Email.Trim().ToLower(),
                DepartamentoId = departamentoId,
                JefeId = jefeId,
                CreatedBy = adminId,
                CreatedOn = DateTime.UtcNow
            };

            return (usuario, persona);
        }

        /// <summary>
        /// Genera estadísticas del procesamiento
        /// </summary>
        public BulkImportEstadisticasDto GenerarEstadisticas(
            List<BulkImportUsuarioResultDto> usuariosCreados,
            List<BulkImportErrorDto> errores,
            TimeSpan tiempoProcesamiento)
        {
            var totalProcesados = usuariosCreados.Count + errores.Count(e => e.EsCritico);
            var registrosFallidos = errores.Count(e => e.EsCritico);
            var registrosOmitidos = errores.Count(e => !e.EsCritico);

            return new BulkImportEstadisticasDto
            {
                TotalProcesados = totalProcesados,
                UsuariosCreados = usuariosCreados.Count,
                RegistrosFallidos = registrosFallidos,
                RegistrosOmitidos = registrosOmitidos,
                TiempoPromedioMilisegundos = totalProcesados > 0 
                    ? tiempoProcesamiento.TotalMilliseconds / totalProcesados 
                    : 0
            };
        }

        #region Métodos Privados

        /// <summary>
        /// Valida usuarios contra la base de datos
        /// </summary>
        private async Task<List<BulkImportErrorDto>> ValidarContraBaseDatosAsync(List<BulkImportUsuarioDto> usuarios)
        {
            var errores = new List<BulkImportErrorDto>();

            try
            {
                // ✅ Validar emails existentes
                var emails = usuarios.Where(u => !string.IsNullOrWhiteSpace(u.Email))
                                   .Select(u => u.Email).ToList();
                var emailsExistentes = await _usuarioRepository.GetExistingEmails(emails);
                
                foreach (var usuario in usuarios.Where(u => emailsExistentes.Contains(u.Email)))
                {
                    errores.Add(_validator.CreateError(usuario.NumeroFila, TipoErrorBulkImport.DUPLICADO_EMAIL,
                        $"El email '{usuario.Email}' ya existe en el sistema", "Email", usuario.Email,
                        "Use un email diferente para este usuario"));
                }

                // ✅ Validar DNIs existentes
                var dnis = usuarios.Where(u => !string.IsNullOrWhiteSpace(u.Dni))
                                 .Select(u => u.Dni).ToList();
                var dnisExistentes = await _usuarioRepository.GetExistingDnis(dnis);
                
                foreach (var usuario in usuarios.Where(u => dnisExistentes.Contains(u.Dni)))
                {
                    errores.Add(_validator.CreateError(usuario.NumeroFila, TipoErrorBulkImport.DUPLICADO_DNI,
                        $"El DNI '{usuario.Dni}' ya existe en el sistema", "Dni", usuario.Dni,
                        "Verifique el DNI o consulte con el administrador"));
                }

                // ✅ Validar departamentos existentes
                var codigosDepartamento = usuarios.Where(u => !string.IsNullOrWhiteSpace(u.CodigoDepartamento))
                                                .Select(u => u.CodigoDepartamento).Distinct().ToList();
                var departamentosValidos = await _usuarioRepository.GetDepartamentosByCodigos(codigosDepartamento);
                
                foreach (var usuario in usuarios)
                {
                    if (!departamentosValidos.ContainsKey(usuario.CodigoDepartamento))
                    {
                        errores.Add(_validator.CreateError(usuario.NumeroFila, TipoErrorBulkImport.DEPARTAMENTO_NO_ENCONTRADO,
                            $"El departamento con código '{usuario.CodigoDepartamento}' no existe", 
                            "CodigoDepartamento", usuario.CodigoDepartamento,
                            "Verifique el código del departamento"));
                    }
                }

                // ✅ Validar jefes existentes
                var dnisJefes = usuarios.Where(u => !string.IsNullOrWhiteSpace(u.DniJefe))
                                      .Select(u => u.DniJefe!).Distinct().ToList();
                var jefesValidos = await _usuarioRepository.GetJefesByDnis(dnisJefes);
                
                foreach (var usuario in usuarios)
                {
                    if (!string.IsNullOrWhiteSpace(usuario.DniJefe) && !jefesValidos.ContainsKey(usuario.DniJefe))
                    {
                        errores.Add(_validator.CreateError(usuario.NumeroFila, TipoErrorBulkImport.JEFE_NO_ENCONTRADO,
                            $"No se encontró un usuario con DNI '{usuario.DniJefe}' para asignar como jefe", 
                            "DniJefe", usuario.DniJefe,
                            "Verifique que el jefe esté registrado en el sistema"));
                    }
                }

                // ✅ Validar roles existentes
                var todosRoles = usuarios.Where(u => !string.IsNullOrWhiteSpace(u.Roles))
                                       .SelectMany(u => u.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                               .Select(r => r.Trim()))
                                       .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                
                if (todosRoles.Any())
                {
                    var rolesInvalidos = await ValidarRolesAsync(todosRoles);
                    
                    foreach (var usuario in usuarios.Where(u => !string.IsNullOrWhiteSpace(u.Roles)))
                    {
                        var (rolesUsuario, _) = _validator.ValidateAndParseRoles(usuario.Roles, usuario.NumeroFila);
                        var rolesInvalidosUsuario = rolesUsuario.Intersect(rolesInvalidos, StringComparer.OrdinalIgnoreCase);
                        
                        foreach (var rolInvalido in rolesInvalidosUsuario)
                        {
                            errores.Add(_validator.CreateError(usuario.NumeroFila, TipoErrorBulkImport.ROL_NO_VALIDO,
                                $"El rol '{rolInvalido}' no existe en el sistema", "Roles", usuario.Roles,
                                "Use roles válidos del sistema"));
                        }
                    }
                }

                return errores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar contra base de datos");
                errores.Add(_validator.CreateError(0, TipoErrorBulkImport.ERROR_BASE_DATOS,
                    "Error al validar datos contra la base de datos: " + ex.Message));
                return errores;
            }
        }

        /// <summary>
        /// Valida un usuario individual contra la base de datos
        /// </summary>
        private async Task<List<BulkImportErrorDto>> ValidarUsuarioContraBaseDatosAsync(BulkImportUsuarioDto usuario)
        {
            return await ValidarContraBaseDatosAsync(new List<BulkImportUsuarioDto> { usuario });
        }

        /// <summary>
        /// Procesa usuarios válidos y los crea en el sistema
        /// </summary>
        private async Task<BulkImportResultDto> ProcesarUsuariosValidosAsync(
            BulkImportRequestDto request,
            string adminId,
            string ipAddress,
            string userAgent,
            BulkImportResultDto resultado)
        {
            try
            {
                // ✅ Preparar datos para creación
                var usuariosParaCrear = new List<(Usuario usuario, Persona persona, string password, List<string> roles)>();
                var departamentosResueltos = await _usuarioRepository.GetDepartamentosByCodigos(
                    request.Usuarios.Select(u => u.CodigoDepartamento).Distinct().ToList());
                var jefesResueltos = await _usuarioRepository.GetJefesByDnis(
                    request.Usuarios.Where(u => !string.IsNullOrWhiteSpace(u.DniJefe))
                                   .Select(u => u.DniJefe!).Distinct().ToList());

                foreach (var usuarioBulk in request.Usuarios)
                {
                    try
                    {
                        var departamentoId = departamentosResueltos.GetValueOrDefault(usuarioBulk.CodigoDepartamento);
                        var jefeId = !string.IsNullOrWhiteSpace(usuarioBulk.DniJefe) 
                            ? jefesResueltos.GetValueOrDefault(usuarioBulk.DniJefe) 
                            : null;

                        if (string.IsNullOrEmpty(departamentoId))
                        {
                            resultado.RegistrosFallidos.Add(_validator.CreateError(usuarioBulk.NumeroFila, 
                                TipoErrorBulkImport.DEPARTAMENTO_NO_ENCONTRADO,
                                $"No se pudo resolver el departamento {usuarioBulk.CodigoDepartamento}"));
                            continue;
                        }

                        var (usuario, persona) = ConvertirAEntidades(usuarioBulk, departamentoId, jefeId, adminId);
                        
                        // Determinar contraseña
                        var password = !string.IsNullOrEmpty(usuarioBulk.ContrasenaTemporal) 
                            ? usuarioBulk.ContrasenaTemporal 
                            : GenerarPasswordTemporal();

                        // Determinar roles
                        var roles = new List<string>();
                        if (!string.IsNullOrEmpty(usuarioBulk.Roles))
                        {
                            var (rolesUsuario, _) = _validator.ValidateAndParseRoles(usuarioBulk.Roles, usuarioBulk.NumeroFila);
                            roles = rolesUsuario;
                        }
                        else
                        {
                            roles = request.Configuracion.RolesPorDefecto;
                        }

                        usuariosParaCrear.Add((usuario, persona, password, roles));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al preparar usuario en fila {Fila}", usuarioBulk.NumeroFila);
                        resultado.RegistrosFallidos.Add(_validator.CreateError(usuarioBulk.NumeroFila,
                            TipoErrorBulkImport.ERROR_CREACION_USUARIO,
                            $"Error al preparar usuario: {ex.Message}"));
                    }
                }

                // ✅ Crear usuarios en lote
                if (usuariosParaCrear.Any())
                {
                    var resultadosCreacion = await _usuarioRepository.CreateUsuariosBulkAsync(usuariosParaCrear);
                    
                    for (int i = 0; i < resultadosCreacion.Count; i++)
                    {
                        var (success, errors, userId, email) = resultadosCreacion[i];
                        var usuarioOriginal = request.Usuarios[i];

                        if (success && !string.IsNullOrEmpty(userId))
                        {
                            resultado.UsuariosCreados.Add(new BulkImportUsuarioResultDto
                            {
                                NumeroFila = usuarioOriginal.NumeroFila,
                                UsuarioId = userId,
                                Email = email,
                                NombreCompleto = $"{usuarioOriginal.Nombres} {usuarioOriginal.ApellidoPaterno} {usuarioOriginal.ApellidoMaterno}",
                                Dni = usuarioOriginal.Dni,
                                ContrasenaTemporal = usuariosParaCrear[i].password,
                                RolesAsignados = usuariosParaCrear[i].roles,
                                Departamento = usuarioOriginal.CodigoDepartamento,
                                RequiereCambioContrasena = true
                            });
                        }
                        else
                        {
                            foreach (var error in errors)
                            {
                                resultado.RegistrosFallidos.Add(_validator.CreateError(usuarioOriginal.NumeroFila,
                                    TipoErrorBulkImport.ERROR_CREACION_USUARIO, error, valor: email));
                            }
                        }
                    }
                }

                // ✅ Finalizar resultado
                resultado.FechaFin = DateTime.UtcNow;
                resultado.TiempoProcesamiento = resultado.FechaFin - resultado.FechaInicio;
                resultado.Exitoso = resultado.UsuariosCreados.Any();
                resultado.Mensaje = resultado.Exitoso 
                    ? $"Bulk import completado exitosamente. {resultado.UsuariosCreados.Count} usuarios creados"
                    : "Bulk import falló. No se pudo crear ningún usuario";
                resultado.Estadisticas = GenerarEstadisticas(resultado.UsuariosCreados, resultado.RegistrosFallidos, resultado.TiempoProcesamiento);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante procesamiento de usuarios válidos");
                throw;
            }
        }

        /// <summary>
        /// Registra auditoría del proceso completo
        /// </summary>
        private async Task RegistrarAuditoriaProcesoAsync(
            BulkImportResultDto resultado,
            string adminId,
            string ipAddress,
            string userAgent)
        {
            try
            {
                var tipoAccion = resultado.Exitoso ? TipoAccionAuditoria.BULK_IMPORT_SUCCESS : TipoAccionAuditoria.BULK_IMPORT_ERROR;
                var severidad = resultado.Exitoso ? SeveridadAuditoria.INFO : SeveridadAuditoria.WARNING;

                await _auditoriaService.RegistrarAccionSimpleAsync(
                    tipoAccion,
                    ModuloSistema.GESTION_USUARIOS,
                    "BULK_IMPORT",
                    resultado.UsuariosCreados.Count.ToString(),
                    adminId,
                    motivo: $"Bulk import procesado: {resultado.UsuariosCreados.Count} creados, {resultado.RegistrosFallidos.Count} fallidos",
                    ipAddress: ipAddress,
                    severidad: severidad
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar auditoría del proceso bulk import");
            }
        }

        /// <summary>
        /// Genera una contraseña temporal que cumple con las políticas de Identity
        /// </summary>
        private string GenerarPasswordTemporal()
        {
            const int longitud = 12;
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string simbolos = "!@#$%&*+=-_?";

            var random = new Random();
            var password = new StringBuilder();

            // Garantizar al menos 1 carácter de cada tipo requerido
            password.Append(mayusculas[random.Next(mayusculas.Length)]);
            password.Append(minusculas[random.Next(minusculas.Length)]);
            password.Append(numeros[random.Next(numeros.Length)]);
            password.Append(simbolos[random.Next(simbolos.Length)]);

            // Completar con caracteres aleatorios
            var todosCaracteres = mayusculas + minusculas + numeros + simbolos;
            for (int i = 4; i < longitud; i++)
            {
                password.Append(todosCaracteres[random.Next(todosCaracteres.Length)]);
            }

            // Mezclar para evitar patrones
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        #endregion
    }
}
