using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using sistema_vacaciones_back.DTOs.Usuarios;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Services
{
    /// <summary>
    /// Implementación del procesador de archivos para bulk import.
    /// Maneja la lectura y conversión de archivos Excel y CSV a objetos BulkImportUsuarioDto.
    /// </summary>
    public class BulkImportFileProcessor : IBulkImportFileProcessor
    {
        private readonly ILogger<BulkImportFileProcessor> _logger;

        public BulkImportFileProcessor(ILogger<BulkImportFileProcessor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Procesa un archivo Excel (.xlsx) y extrae los usuarios.
        /// </summary>
        public Task<(List<BulkImportUsuarioDto> usuarios, BulkImportMetadataDto metadata)> ProcessExcelFileAsync(Stream fileStream, string fileName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var metadata = new BulkImportMetadataDto
            {
                NombreArchivo = fileName,
                TipoArchivo = "Excel",
                FechaProcesamiento = DateTime.UtcNow,
                TamanoArchivo = fileStream.Length
            };

            try
            {
                _logger.LogInformation("Iniciando procesamiento de archivo Excel: {FileName}", fileName);

                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    metadata.ErroresParsing.Add("El archivo Excel no contiene hojas de cálculo");
                    stopwatch.Stop();
                    metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;
                    return Task.FromResult((new List<BulkImportUsuarioDto>(), metadata));
                }

                // Validar encabezados
                var encabezadosValidos = ValidarEncabezadosExcel(worksheet);
                if (!encabezadosValidos.esValido)
                {
                    metadata.ErroresParsing.AddRange(encabezadosValidos.errores);
                    stopwatch.Stop();
                    metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;
                    return Task.FromResult((new List<BulkImportUsuarioDto>(), metadata));
                }

                // Obtener columnas encontradas
                metadata.ColumnasEncontradas = GetColumnsFromExcel(worksheet);

                // Procesar filas de datos
                var usuarios = new List<BulkImportUsuarioDto>();
                var filaActual = 2; // Empezar desde la fila 2 (después de encabezados)

                while (!worksheet.Cell(filaActual, 1).IsEmpty())
                {
                    try
                    {
                        var usuario = ExtractUsuarioFromExcelRow(worksheet, filaActual);
                        if (usuario != null)
                        {
                            usuarios.Add(usuario);
                            metadata.FilasConDatos++;
                        }
                        else
                        {
                            metadata.FilasOmitidas++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al procesar fila {Fila} del archivo {FileName}", filaActual, fileName);
                        metadata.ErroresParsing.Add($"Error en fila {filaActual}: {ex.Message}");
                        metadata.FilasOmitidas++;
                    }

                    metadata.TotalFilas++;
                    filaActual++;

                    // Límite de seguridad para evitar archivos infinitos
                    if (filaActual > 10000)
                    {
                        metadata.Advertencias.Add("El archivo excede el límite máximo de 10,000 filas");
                        break;
                    }
                }

                stopwatch.Stop();
                metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Procesamiento de Excel completado. Archivo: {FileName}, Usuarios: {Count}", 
                    fileName, usuarios.Count);

                return Task.FromResult((usuarios, metadata));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar archivo Excel: {FileName}", fileName);
                metadata.ErroresParsing.Add($"Error crítico al procesar archivo: {ex.Message}");
                stopwatch.Stop();
                metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;
                return Task.FromResult((new List<BulkImportUsuarioDto>(), metadata));
            }
        }

        /// <summary>
        /// Procesa un archivo CSV y extrae los usuarios.
        /// </summary>
        public async Task<(List<BulkImportUsuarioDto> usuarios, BulkImportMetadataDto metadata)> ProcessCsvFileAsync(Stream fileStream, string fileName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var metadata = new BulkImportMetadataDto
            {
                NombreArchivo = fileName,
                TipoArchivo = "CSV",
                FechaProcesamiento = DateTime.UtcNow,
                TamanoArchivo = fileStream.Length
            };

            try
            {
                _logger.LogInformation("Iniciando procesamiento de archivo CSV: {FileName}", fileName);

                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null // Ignorar campos faltantes
                });

                // Leer encabezados
                await csv.ReadAsync();
                csv.ReadHeader();
                
                // Validar encabezados CSV
                var encabezadosValidos = ValidarEncabezadosCsv(csv.HeaderRecord);
                if (!encabezadosValidos.esValido)
                {
                    metadata.ErroresParsing.AddRange(encabezadosValidos.errores);
                    stopwatch.Stop();
                    metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;
                    return (new List<BulkImportUsuarioDto>(), metadata);
                }

                // Obtener columnas encontradas
                metadata.ColumnasEncontradas = csv.HeaderRecord?.ToList() ?? new List<string>();

                // Procesar registros
                var usuarios = new List<BulkImportUsuarioDto>();
                var filaActual = 2; // Fila 1 son encabezados

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var usuario = ExtractUsuarioFromCsvRecord(csv, filaActual);
                        if (usuario != null)
                        {
                            usuarios.Add(usuario);
                            metadata.FilasConDatos++;
                        }
                        else
                        {
                            metadata.FilasOmitidas++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al procesar fila {Fila} del archivo CSV {FileName}", filaActual, fileName);
                        metadata.ErroresParsing.Add($"Error en fila {filaActual}: {ex.Message}");
                        metadata.FilasOmitidas++;
                    }

                    metadata.TotalFilas++;
                    filaActual++;

                    // Límite de seguridad
                    if (filaActual > 10000)
                    {
                        metadata.Advertencias.Add("El archivo excede el límite máximo de 10,000 filas");
                        break;
                    }
                }

                stopwatch.Stop();
                metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Procesamiento de CSV completado. Archivo: {FileName}, Usuarios: {Count}", 
                    fileName, usuarios.Count);

                return (usuarios, metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar archivo CSV: {FileName}", fileName);
                metadata.ErroresParsing.Add($"Error crítico al procesar archivo: {ex.Message}");
                stopwatch.Stop();
                metadata.TiempoProcesamiento = stopwatch.ElapsedMilliseconds;
                return (new List<BulkImportUsuarioDto>(), metadata);
            }
        }

        /// <summary>
        /// Valida que un archivo tenga el formato esperado (columnas requeridas).
        /// </summary>
        public async Task<List<string>> ValidateFileFormatAsync(Stream fileStream, string fileType)
        {
            var errores = new List<string>();
            
            try
            {
                if (fileType.ToLowerInvariant().Contains("excel") || fileType.ToLowerInvariant().Contains("xlsx"))
                {
                    using var workbook = new XLWorkbook(fileStream);
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    
                    if (worksheet == null)
                    {
                        errores.Add("El archivo Excel no contiene hojas de cálculo");
                        return errores;
                    }

                    var validacion = ValidarEncabezadosExcel(worksheet);
                    if (!validacion.esValido)
                    {
                        errores.AddRange(validacion.errores);
                    }
                }
                else if (fileType.ToLowerInvariant().Contains("csv"))
                {
                    using var reader = new StreamReader(fileStream, Encoding.UTF8);
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        Delimiter = ",",
                        TrimOptions = TrimOptions.Trim
                    });

                    await csv.ReadAsync();
                    csv.ReadHeader();
                    
                    var validacion = ValidarEncabezadosCsv(csv.HeaderRecord);
                    if (!validacion.esValido)
                    {
                        errores.AddRange(validacion.errores);
                    }
                }
                else
                {
                    errores.Add($"Tipo de archivo no soportado: {fileType}");
                }
            }
            catch (Exception ex)
            {
                errores.Add($"Error al validar formato del archivo: {ex.Message}");
            }

            return errores;
        }

        /// <summary>
        /// Genera una plantilla Excel con la estructura correcta para bulk import.
        /// </summary>
        public MemoryStream GenerateExcelTemplate()
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Plantilla Usuarios");

                // Definir encabezados según especificaciones
                var encabezados = GetExpectedColumns();

                // Escribir encabezados
                for (int i = 0; i < encabezados.Count; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = encabezados[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // Agregar filas de ejemplo
                worksheet.Cell(2, 1).Value = "Juan Carlos";
                worksheet.Cell(2, 2).Value = "Pérez";
                worksheet.Cell(2, 3).Value = "García";
                worksheet.Cell(2, 4).Value = "12345678";
                worksheet.Cell(2, 5).Value = "juan.perez@empresa.com";
                worksheet.Cell(2, 6).Value = "IT";
                worksheet.Cell(2, 7).Value = "87654321";
                worksheet.Cell(2, 8).Value = "01/01/2024";
                worksheet.Cell(2, 9).Value = "NO";
                worksheet.Cell(2, 10).Value = "Empresa Principal";
                worksheet.Cell(2, 11).Value = "Empleado";

                worksheet.Cell(3, 1).Value = "María Elena";
                worksheet.Cell(3, 2).Value = "López";
                worksheet.Cell(3, 3).Value = "Martínez";
                worksheet.Cell(3, 4).Value = "87654321";
                worksheet.Cell(3, 5).Value = "maria.lopez@empresa.com";
                worksheet.Cell(3, 6).Value = "RH";
                worksheet.Cell(3, 7).Value = "";
                worksheet.Cell(3, 8).Value = "15/02/2024";
                worksheet.Cell(3, 9).Value = "NO";
                worksheet.Cell(3, 10).Value = "Empresa Principal";
                worksheet.Cell(3, 11).Value = "Jefe";

                // Ajustar ancho de columnas
                worksheet.ColumnsUsed().AdjustToContents();

                // Agregar comentarios explicativos
                worksheet.Cell(1, 13).Value = "INSTRUCCIONES:";
                worksheet.Cell(2, 13).Value = "1. Nombres: Obligatorio";
                worksheet.Cell(3, 13).Value = "2. Apellidos: Ambos obligatorios";
                worksheet.Cell(4, 13).Value = "3. DNI: 8 dígitos únicos";
                worksheet.Cell(5, 13).Value = "4. Email: Formato válido único";
                worksheet.Cell(6, 13).Value = "5. Código Depto: Debe existir";
                worksheet.Cell(7, 13).Value = "6. DNI Jefe: Opcional, debe existir";
                worksheet.Cell(8, 13).Value = "7. Fecha: DD/MM/YYYY";
                worksheet.Cell(9, 13).Value = "8. Extranjero: SI/NO";
                worksheet.Cell(10, 13).Value = "9. Empresa: Obligatorio";
                worksheet.Cell(11, 13).Value = "10. Roles: Separados por comas";

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar plantilla Excel");
                throw new InvalidOperationException("No se pudo generar la plantilla Excel", ex);
            }
        }

        /// <summary>
        /// Genera una plantilla CSV con la estructura correcta para bulk import.
        /// </summary>
        public MemoryStream GenerateCsvTemplate()
        {
            try
            {
                var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                // Escribir encabezados
                var encabezados = GetExpectedColumns();
                foreach (var encabezado in encabezados)
                {
                    csv.WriteField(encabezado);
                }
                csv.NextRecord();

                // Agregar filas de ejemplo
                var ejemplos = new[]
                {
                    new[] { "Juan Carlos", "Pérez", "García", "12345678", "juan.perez@empresa.com", "IT", "87654321", "01/01/2024", "NO", "Empresa Principal", "Empleado" },
                    new[] { "María Elena", "López", "Martínez", "87654321", "maria.lopez@empresa.com", "RH", "", "15/02/2024", "NO", "Empresa Principal", "Jefe" }
                };

                foreach (var ejemplo in ejemplos)
                {
                    foreach (var campo in ejemplo)
                    {
                        csv.WriteField(campo);
                    }
                    csv.NextRecord();
                }

                writer.Flush();
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar plantilla CSV");
                throw new InvalidOperationException("No se pudo generar la plantilla CSV", ex);
            }
        }

        /// <summary>
        /// Obtiene las columnas esperadas en el archivo de bulk import.
        /// </summary>
        public List<string> GetExpectedColumns()
        {
            return new List<string>
            {
                "Nombres",
                "Apellido Paterno", 
                "Apellido Materno",
                "DNI",
                "Email",
                "Código Departamento",
                "DNI Jefe",
                "Fecha Ingreso",
                "Extranjero",
                "Empresa",
                "Roles"
            };
        }

        /// <summary>
        /// Valida el tamaño y tipo de archivo.
        /// </summary>
        public string? ValidateFileConstraints(long fileSize, string fileName)
        {
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB

            if (fileSize > maxFileSize)
            {
                return $"El archivo excede el tamaño máximo permitido de {maxFileSize / (1024 * 1024)} MB";
            }

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".csv")
            {
                return "Solo se permiten archivos Excel (.xlsx) o CSV (.csv)";
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "El nombre del archivo no puede estar vacío";
            }

            return null; // Válido
        }

        #region Métodos Privados

        /// <summary>
        /// Obtiene las columnas del worksheet de Excel.
        /// </summary>
        private List<string> GetColumnsFromExcel(IXLWorksheet worksheet)
        {
            var columnas = new List<string>();
            var columnaActual = 1;

            while (!worksheet.Cell(1, columnaActual).IsEmpty())
            {
                columnas.Add(worksheet.Cell(1, columnaActual).GetString().Trim());
                columnaActual++;
            }

            return columnas;
        }

        /// <summary>
        /// Valida los encabezados del archivo Excel.
        /// </summary>
        private (bool esValido, List<string> errores) ValidarEncabezadosExcel(IXLWorksheet worksheet)
        {
            var errores = new List<string>();
            var encabezadosEsperados = GetExpectedColumns();

            for (int i = 0; i < encabezadosEsperados.Count; i++)
            {
                var cellValue = worksheet.Cell(1, i + 1).GetString().Trim();
                if (!string.Equals(cellValue, encabezadosEsperados[i], StringComparison.OrdinalIgnoreCase))
                {
                    errores.Add($"Encabezado incorrecto en columna {i + 1}. Esperado: '{encabezadosEsperados[i]}', Encontrado: '{cellValue}'");
                }
            }

            return (errores.Count == 0, errores);
        }

        /// <summary>
        /// Valida los encabezados del archivo CSV.
        /// </summary>
        private (bool esValido, List<string> errores) ValidarEncabezadosCsv(string[]? headers)
        {
            var errores = new List<string>();
            
            if (headers == null || headers.Length == 0)
            {
                errores.Add("El archivo CSV no contiene encabezados");
                return (false, errores);
            }

            var encabezadosEsperados = GetExpectedColumns();

            for (int i = 0; i < encabezadosEsperados.Count && i < headers.Length; i++)
            {
                var header = headers[i]?.Trim() ?? "";
                if (!string.Equals(header, encabezadosEsperados[i], StringComparison.OrdinalIgnoreCase))
                {
                    errores.Add($"Encabezado incorrecto en columna {i + 1}. Esperado: '{encabezadosEsperados[i]}', Encontrado: '{header}'");
                }
            }

            if (headers.Length < encabezadosEsperados.Count)
            {
                errores.Add($"El archivo CSV tiene menos columnas de las esperadas. Esperadas: {encabezadosEsperados.Count}, Encontradas: {headers.Length}");
            }

            return (errores.Count == 0, errores);
        }

        /// <summary>
        /// Extrae un usuario desde una fila de Excel.
        /// </summary>
        private BulkImportUsuarioDto? ExtractUsuarioFromExcelRow(IXLWorksheet worksheet, int fila)
        {
            var usuario = new BulkImportUsuarioDto
            {
                NumeroFila = fila,
                Nombres = worksheet.Cell(fila, 1).GetString().Trim(),
                ApellidoPaterno = worksheet.Cell(fila, 2).GetString().Trim(),
                ApellidoMaterno = worksheet.Cell(fila, 3).GetString().Trim(),
                Dni = worksheet.Cell(fila, 4).GetString().Trim(),
                Email = worksheet.Cell(fila, 5).GetString().Trim(),
                CodigoDepartamento = worksheet.Cell(fila, 6).GetString().Trim(),
                DniJefe = worksheet.Cell(fila, 7).GetString().Trim(),
                FechaIngreso = worksheet.Cell(fila, 8).GetString().Trim(),
                Extranjero = worksheet.Cell(fila, 9).GetString().Trim(),
                Empresa = worksheet.Cell(fila, 10).GetString().Trim(),
                Roles = worksheet.Cell(fila, 11).GetString().Trim()
            };

            // Verificar que al menos tenga datos básicos
            if (string.IsNullOrWhiteSpace(usuario.Nombres) && 
                string.IsNullOrWhiteSpace(usuario.ApellidoPaterno) && 
                string.IsNullOrWhiteSpace(usuario.Dni))
            {
                return null; // Fila vacía
            }

            return usuario;
        }

        /// <summary>
        /// Extrae un usuario desde un registro CSV.
        /// </summary>
        private BulkImportUsuarioDto? ExtractUsuarioFromCsvRecord(CsvReader csv, int fila)
        {
            var usuario = new BulkImportUsuarioDto
            {
                NumeroFila = fila,
                Nombres = csv.GetField("Nombres")?.Trim() ?? "",
                ApellidoPaterno = csv.GetField("Apellido Paterno")?.Trim() ?? "",
                ApellidoMaterno = csv.GetField("Apellido Materno")?.Trim() ?? "",
                Dni = csv.GetField("DNI")?.Trim() ?? "",
                Email = csv.GetField("Email")?.Trim() ?? "",
                CodigoDepartamento = csv.GetField("Código Departamento")?.Trim() ?? "",
                DniJefe = csv.GetField("DNI Jefe")?.Trim() ?? "",
                FechaIngreso = csv.GetField("Fecha Ingreso")?.Trim() ?? "",
                Extranjero = csv.GetField("Extranjero")?.Trim() ?? "",
                Empresa = csv.GetField("Empresa")?.Trim() ?? "",
                Roles = csv.GetField("Roles")?.Trim() ?? ""
            };

            // Verificar que al menos tenga datos básicos
            if (string.IsNullOrWhiteSpace(usuario.Nombres) && 
                string.IsNullOrWhiteSpace(usuario.ApellidoPaterno) && 
                string.IsNullOrWhiteSpace(usuario.Dni))
            {
                return null; // Fila vacía
            }

            return usuario;
        }

        #endregion
    }
}
