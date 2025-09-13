using System.Globalization;
using System.Text.RegularExpressions;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Services
{
    /// <summary>
    /// Implementación del validador especializado para bulk import de usuarios.
    /// Maneja todas las validaciones de formato, estructura y reglas de negocio.
    /// </summary>
    public class BulkImportValidator : IBulkImportValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DniRegex = new Regex(
            @"^\d{8}$", 
            RegexOptions.Compiled);

        // Valores aceptados para el campo booleano "Extranjero"
        private static readonly HashSet<string> ValoresVerdaderos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sí", "si", "yes", "y", "1", "true", "verdadero", "v"
        };

        private static readonly HashSet<string> ValoresFalsos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "no", "n", "0", "false", "falso", "f"
        };

        /// <summary>
        /// Valida la estructura básica de la solicitud de bulk import
        /// </summary>
        public List<BulkImportErrorDto> ValidateRequestStructure(BulkImportRequestDto request)
        {
            var errores = new List<BulkImportErrorDto>();

            // Validar que la solicitud no sea nula
            if (request == null)
            {
                errores.Add(CreateError(0, TipoErrorBulkImport.ERROR_VALIDACION_NEGOCIO, 
                    "La solicitud de bulk import no puede ser nula"));
                return errores;
            }

            // Validar que haya usuarios
            if (request.Usuarios == null || !request.Usuarios.Any())
            {
                errores.Add(CreateError(0, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "La lista de usuarios es obligatoria y debe contener al menos un usuario"));
                return errores;
            }

            // Validar límite máximo de usuarios por lote
            const int maxUsuariosPorLote = 500;
            if (request.Usuarios.Count > maxUsuariosPorLote)
            {
                errores.Add(CreateError(0, TipoErrorBulkImport.ERROR_VALIDACION_NEGOCIO, 
                    $"No se pueden procesar más de {maxUsuariosPorLote} usuarios por lote. Total recibido: {request.Usuarios.Count}",
                    sugerencia: $"Divida el archivo en lotes más pequeños de máximo {maxUsuariosPorLote} usuarios"));
            }

            // Validar configuración
            if (request.Configuracion == null)
            {
                errores.Add(CreateError(0, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "La configuración de bulk import es obligatoria"));
            }

            return errores;
        }

        /// <summary>
        /// Valida los campos obligatorios y formato de un usuario individual
        /// </summary>
        public List<BulkImportErrorDto> ValidateUsuarioFields(BulkImportUsuarioDto usuario)
        {
            var errores = new List<BulkImportErrorDto>();

            if (usuario == null)
            {
                errores.Add(CreateError(0, TipoErrorBulkImport.ERROR_VALIDACION_NEGOCIO, 
                    "El usuario no puede ser nulo"));
                return errores;
            }

            // ✅ Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(usuario.Nombres))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Nombres es obligatorio", "Nombres", usuario.Nombres));
            }

            if (string.IsNullOrWhiteSpace(usuario.ApellidoPaterno))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Apellido Paterno es obligatorio", "ApellidoPaterno", usuario.ApellidoPaterno));
            }

            if (string.IsNullOrWhiteSpace(usuario.ApellidoMaterno))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Apellido Materno es obligatorio", "ApellidoMaterno", usuario.ApellidoMaterno));
            }

            if (string.IsNullOrWhiteSpace(usuario.Dni))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo DNI es obligatorio", "Dni", usuario.Dni));
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Email es obligatorio", "Email", usuario.Email));
            }

            if (string.IsNullOrWhiteSpace(usuario.CodigoDepartamento))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Departamento es obligatorio", "CodigoDepartamento", usuario.CodigoDepartamento));
            }

            if (string.IsNullOrWhiteSpace(usuario.Extranjero))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Extranjero es obligatorio", "Extranjero", usuario.Extranjero));
            }

            if (string.IsNullOrWhiteSpace(usuario.FechaIngreso))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Fecha Ingreso es obligatorio", "FechaIngreso", usuario.FechaIngreso));
            }

            if (string.IsNullOrWhiteSpace(usuario.Empresa))
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Empresa es obligatorio", "Empresa", usuario.Empresa));
            }

            // ✅ Validar longitudes máximas
            if (!string.IsNullOrEmpty(usuario.Nombres) && usuario.Nombres.Length > 100)
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.LONGITUD_CAMPO_EXCEDIDA, 
                    "Los nombres no pueden exceder 100 caracteres", "Nombres", usuario.Nombres));
            }

            if (!string.IsNullOrEmpty(usuario.ApellidoPaterno) && usuario.ApellidoPaterno.Length > 50)
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.LONGITUD_CAMPO_EXCEDIDA, 
                    "El apellido paterno no puede exceder 50 caracteres", "ApellidoPaterno", usuario.ApellidoPaterno));
            }

            if (!string.IsNullOrEmpty(usuario.ApellidoMaterno) && usuario.ApellidoMaterno.Length > 50)
            {
                errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.LONGITUD_CAMPO_EXCEDIDA, 
                    "El apellido materno no puede exceder 50 caracteres", "ApellidoMaterno", usuario.ApellidoMaterno));
            }

            // ✅ Validar formatos específicos
            var errorEmail = ValidateEmail(usuario.Email, usuario.NumeroFila);
            if (errorEmail != null) errores.Add(errorEmail);

            var errorDni = ValidateDni(usuario.Dni, usuario.NumeroFila, "DNI");
            if (errorDni != null) errores.Add(errorDni);

            if (!string.IsNullOrWhiteSpace(usuario.DniJefe))
            {
                var errorDniJefe = ValidateDni(usuario.DniJefe, usuario.NumeroFila, "DNI del Jefe");
                if (errorDniJefe != null) errores.Add(errorDniJefe);
            }

            // ✅ Validar fecha de ingreso
            var (fechaIngreso, errorFecha) = ValidateAndParseFecha(usuario.FechaIngreso, usuario.NumeroFila, "FechaIngreso");
            if (errorFecha != null) errores.Add(errorFecha);

            // ✅ Validar campo extranjero
            var (extranjero, errorExtranjero) = ValidateAndParseBoolean(usuario.Extranjero, usuario.NumeroFila, "Extranjero");
            if (errorExtranjero != null) errores.Add(errorExtranjero);

            // ✅ Validar roles si se proporcionaron
            if (!string.IsNullOrEmpty(usuario.Roles))
            {
                var (roles, errorRoles) = ValidateAndParseRoles(usuario.Roles, usuario.NumeroFila);
                if (errorRoles != null) errores.Add(errorRoles);
            }

            return errores;
        }

        /// <summary>
        /// Valida duplicados de email y DNI dentro del mismo lote de usuarios
        /// </summary>
        public List<BulkImportErrorDto> ValidateDuplicadosInternos(List<BulkImportUsuarioDto> usuarios)
        {
            var errores = new List<BulkImportErrorDto>();

            // ✅ Validar emails duplicados dentro del lote
            var emailsAgrupados = usuarios
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .GroupBy(u => u.Email.ToLower())
                .Where(g => g.Count() > 1);

            foreach (var grupo in emailsAgrupados)
            {
                var usuariosDuplicados = grupo.ToList();
                foreach (var usuario in usuariosDuplicados)
                {
                    errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.DUPLICADO_EMAIL, 
                        $"El email '{usuario.Email}' aparece duplicado en las filas: {string.Join(", ", usuariosDuplicados.Select(u => u.NumeroFila))}",
                        "Email", usuario.Email,
                        "Asegúrese de que cada email sea único en el archivo"));
                }
            }

            // ✅ Validar DNIs duplicados dentro del lote
            var dnisAgrupados = usuarios
                .Where(u => !string.IsNullOrWhiteSpace(u.Dni))
                .GroupBy(u => u.Dni)
                .Where(g => g.Count() > 1);

            foreach (var grupo in dnisAgrupados)
            {
                var usuariosDuplicados = grupo.ToList();
                foreach (var usuario in usuariosDuplicados)
                {
                    errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.DUPLICADO_DNI, 
                        $"El DNI '{usuario.Dni}' aparece duplicado en las filas: {string.Join(", ", usuariosDuplicados.Select(u => u.NumeroFila))}",
                        "Dni", usuario.Dni,
                        "Asegúrese de que cada DNI sea único en el archivo"));
                }
            }

            return errores;
        }

        /// <summary>
        /// Valida el formato de fecha y la convierte a DateTime
        /// </summary>
        public (DateTime? fecha, BulkImportErrorDto? error) ValidateAndParseFecha(
            string fechaString, int numeroFila, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(fechaString))
            {
                return (null, CreateError(numeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    $"El campo {nombreCampo} es obligatorio", nombreCampo, fechaString));
            }

            // Intentar múltiples formatos de fecha
            var formatosFecha = new[]
            {
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "MM/dd/yyyy",
                "M/d/yyyy"
            };

            foreach (var formato in formatosFecha)
            {
                if (DateTime.TryParseExact(fechaString.Trim(), formato, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out DateTime fecha))
                {
                    // Validar que la fecha sea razonable (no muy antigua ni futura)
                    var fechaMinima = new DateTime(1950, 1, 1);
                    var fechaMaxima = DateTime.Today.AddYears(1);

                    if (fecha < fechaMinima || fecha > fechaMaxima)
                    {
                        return (null, CreateError(numeroFila, TipoErrorBulkImport.FORMATO_FECHA_INVALIDO, 
                            $"La fecha en {nombreCampo} debe estar entre {fechaMinima:dd/MM/yyyy} y {fechaMaxima:dd/MM/yyyy}",
                            nombreCampo, fechaString,
                            "Verifique que la fecha sea válida y esté en un rango razonable"));
                    }

                    return (fecha, null);
                }
            }

            return (null, CreateError(numeroFila, TipoErrorBulkImport.FORMATO_FECHA_INVALIDO, 
                $"El formato de fecha en {nombreCampo} no es válido. Formatos aceptados: dd/MM/yyyy, dd-MM-yyyy, yyyy-MM-dd",
                nombreCampo, fechaString,
                "Use el formato dd/MM/yyyy (ejemplo: 15/03/2023)"));
        }

        /// <summary>
        /// Valida el formato de boolean (Extranjero) y lo convierte
        /// </summary>
        public (bool? valor, BulkImportErrorDto? error) ValidateAndParseBoolean(
            string booleanString, int numeroFila, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(booleanString))
            {
                return (null, CreateError(numeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    $"El campo {nombreCampo} es obligatorio", nombreCampo, booleanString));
            }

            var valor = booleanString.Trim();

            if (ValoresVerdaderos.Contains(valor))
            {
                return (true, null);
            }

            if (ValoresFalsos.Contains(valor))
            {
                return (false, null);
            }

            return (null, CreateError(numeroFila, TipoErrorBulkImport.FORMATO_BOOLEAN_INVALIDO, 
                $"El valor '{valor}' en {nombreCampo} no es válido. Use: Sí/No, 1/0, True/False",
                nombreCampo, valor,
                "Use 'Sí' o 'No' para indicar si es extranjero"));
        }

        /// <summary>
        /// Valida el formato de email
        /// </summary>
        public BulkImportErrorDto? ValidateEmail(string email, int numeroFila)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return CreateError(numeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    "El campo Email es obligatorio", "Email", email);
            }

            if (email.Length > 256)
            {
                return CreateError(numeroFila, TipoErrorBulkImport.LONGITUD_CAMPO_EXCEDIDA, 
                    "El email no puede exceder 256 caracteres", "Email", email);
            }

            if (!EmailRegex.IsMatch(email))
            {
                return CreateError(numeroFila, TipoErrorBulkImport.FORMATO_EMAIL_INVALIDO, 
                    "El formato del email no es válido", "Email", email,
                    "Use el formato: usuario@dominio.com");
            }

            return null;
        }

        /// <summary>
        /// Valida el formato de DNI (8 dígitos)
        /// </summary>
        public BulkImportErrorDto? ValidateDni(string dni, int numeroFila, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return CreateError(numeroFila, TipoErrorBulkImport.CAMPO_REQUERIDO_FALTANTE, 
                    $"El campo {nombreCampo} es obligatorio", nombreCampo, dni);
            }

            if (!DniRegex.IsMatch(dni.Trim()))
            {
                return CreateError(numeroFila, TipoErrorBulkImport.FORMATO_DNI_INVALIDO, 
                    $"El {nombreCampo} debe tener exactamente 8 dígitos", nombreCampo, dni,
                    "Ingrese solo los 8 dígitos sin espacios ni caracteres especiales");
            }

            return null;
        }

        /// <summary>
        /// Valida y parsea la lista de roles separados por comas
        /// </summary>
        public (List<string> roles, BulkImportErrorDto? error) ValidateAndParseRoles(
            string? rolesString, int numeroFila)
        {
            if (string.IsNullOrWhiteSpace(rolesString))
            {
                return (new List<string>(), null);
            }

            try
            {
                var roles = rolesString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Validar que no haya roles vacíos
                if (!roles.Any())
                {
                    return (new List<string>(), CreateError(numeroFila, TipoErrorBulkImport.ROL_NO_VALIDO, 
                        "La lista de roles contiene solo valores vacíos", "Roles", rolesString,
                        "Separe los roles con comas (ejemplo: Empleado,Supervisor)"));
                }

                // Validar longitud de cada rol
                foreach (var rol in roles)
                {
                    if (rol.Length > 50)
                    {
                        return (new List<string>(), CreateError(numeroFila, TipoErrorBulkImport.LONGITUD_CAMPO_EXCEDIDA, 
                            $"El rol '{rol}' excede 50 caracteres", "Roles", rolesString));
                    }
                }

                return (roles, null);
            }
            catch (Exception)
            {
                return (new List<string>(), CreateError(numeroFila, TipoErrorBulkImport.ROL_NO_VALIDO, 
                    "Error al procesar la lista de roles", "Roles", rolesString,
                    "Separe los roles con comas (ejemplo: Empleado,Supervisor)"));
            }
        }

        /// <summary>
        /// Valida que no haya jerarquías circulares en las relaciones jefe-subordinado
        /// </summary>
        public List<BulkImportErrorDto> ValidateJerarquiasCirculares(List<BulkImportUsuarioDto> usuarios)
        {
            var errores = new List<BulkImportErrorDto>();

            // Crear un mapa de DNI -> Usuario para búsquedas eficientes
            var usuariosPorDni = usuarios
                .Where(u => !string.IsNullOrWhiteSpace(u.Dni))
                .ToDictionary(u => u.Dni, u => u);

            foreach (var usuario in usuarios)
            {
                if (string.IsNullOrWhiteSpace(usuario.Dni) || string.IsNullOrWhiteSpace(usuario.DniJefe))
                    continue;

                // Validar que no se asigne a sí mismo como jefe
                if (usuario.Dni == usuario.DniJefe)
                {
                    errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.JERARQUIA_CIRCULAR, 
                        "Un usuario no puede ser jefe de sí mismo",
                        "DniJefe", usuario.DniJefe,
                        "Asigne un jefe diferente para este usuario"));
                    continue;
                }

                // Detectar ciclos en la jerarquía
                var visitados = new HashSet<string>();
                var actual = usuario.DniJefe;
                var pasos = 0;
                const int maxPasos = 10; // Evitar bucles infinitos

                while (!string.IsNullOrEmpty(actual) && pasos < maxPasos)
                {
                    if (visitados.Contains(actual))
                    {
                        errores.Add(CreateError(usuario.NumeroFila, TipoErrorBulkImport.JERARQUIA_CIRCULAR, 
                            $"Se detectó una jerarquía circular que involucra al usuario con DNI {usuario.Dni}",
                            "DniJefe", usuario.DniJefe,
                            "Revise la estructura jerárquica para evitar ciclos"));
                        break;
                    }

                    visitados.Add(actual);

                    if (usuariosPorDni.TryGetValue(actual, out var jefeUsuario))
                    {
                        actual = jefeUsuario.DniJefe;
                    }
                    else
                    {
                        // Jefe no está en el lote actual, salir del bucle
                        break;
                    }

                    pasos++;
                }
            }

            return errores;
        }

        /// <summary>
        /// Crea un error de bulk import con información estándar
        /// </summary>
        public BulkImportErrorDto CreateError(
            int numeroFila,
            TipoErrorBulkImport tipoError,
            string descripcion,
            string? campo = null,
            string? valor = null,
            string? sugerencia = null,
            bool esCritico = true)
        {
            return new BulkImportErrorDto
            {
                NumeroFila = numeroFila,
                TipoError = tipoError,
                DescripcionError = descripcion,
                Campo = campo,
                ValorError = valor,
                Sugerencia = sugerencia,
                EsCritico = esCritico
            };
        }
    }
}
