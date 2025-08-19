using sistema_vacaciones_back.DTOs.Auditoria;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de auditoría del sistema
    /// </summary>
    public interface IAuditoriaService
    {
        // ===== MÉTODOS DE REGISTRO =====

        /// <summary>
        /// Registra una nueva acción de auditoría en el sistema
        /// </summary>
        /// <param name="dto">Datos de la acción a registrar</param>
        /// <param name="usuarioEjecutorId">ID del usuario que ejecuta la acción</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">User Agent del navegador</param>
        /// <param name="sessionId">ID de sesión (opcional)</param>
        /// <returns>ID del registro de auditoría creado</returns>
        Task<string> RegistrarAccionAsync(
            CrearAuditoriaDto dto, 
            string usuarioEjecutorId, 
            string ipAddress, 
            string? userAgent = null, 
            string? sessionId = null);

        /// <summary>
        /// Registra una acción de auditoría de forma simplificada
        /// </summary>
        /// <param name="tipoAccion">Tipo de acción</param>
        /// <param name="modulo">Módulo del sistema</param>
        /// <param name="tablaAfectada">Tabla afectada</param>
        /// <param name="registroAfectadoId">ID del registro afectado</param>
        /// <param name="usuarioEjecutorId">ID del usuario ejecutor</param>
        /// <param name="usuarioAfectadoId">ID del usuario afectado (opcional)</param>
        /// <param name="motivo">Motivo de la acción (opcional)</param>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="severidad">Severidad de la acción</param>
        /// <returns>ID del registro creado</returns>
        Task<string> RegistrarAccionSimpleAsync(
            TipoAccionAuditoria tipoAccion,
            ModuloSistema modulo,
            string tablaAfectada,
            string registroAfectadoId,
            string usuarioEjecutorId,
            string? usuarioAfectadoId = null,
            string? motivo = null,
            string ipAddress = "0.0.0.0",
            SeveridadAuditoria severidad = SeveridadAuditoria.INFO);

        // ===== MÉTODOS DE CONSULTA =====

        /// <summary>
        /// Obtiene registros de auditoría con filtros y paginación
        /// </summary>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns>Resultado paginado de registros de auditoría</returns>
        Task<AuditoriaPaginadaDto> ObtenerRegistrosAsync(ConsultarAuditoriaDto filtros);

        /// <summary>
        /// Obtiene el historial de auditoría para un módulo específico
        /// </summary>
        /// <param name="modulo">Módulo del sistema</param>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Registros de auditoría del módulo</returns>
        Task<AuditoriaPaginadaDto> ObtenerHistorialPorModuloAsync(
            ModuloSistema modulo, 
            int pagina = 1, 
            int tamanoPagina = 20);

        /// <summary>
        /// Obtiene el historial de auditoría para un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="comoEjecutor">Si true, busca acciones ejecutadas por el usuario. Si false, acciones realizadas sobre el usuario</param>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Registros de auditoría del usuario</returns>
        Task<AuditoriaPaginadaDto> ObtenerHistorialPorUsuarioAsync(
            string usuarioId, 
            bool comoEjecutor = true, 
            int pagina = 1, 
            int tamanoPagina = 20);

        /// <summary>
        /// Obtiene el historial de auditoría para un registro específico
        /// </summary>
        /// <param name="tablaAfectada">Tabla del registro</param>
        /// <param name="registroAfectadoId">ID del registro</param>
        /// <returns>Lista de acciones realizadas sobre el registro</returns>
        Task<List<AuditoriaDto>> ObtenerHistorialPorRegistroAsync(
            string tablaAfectada, 
            string registroAfectadoId);

        /// <summary>
        /// Obtiene un registro de auditoría específico por su ID
        /// </summary>
        /// <param name="auditoriaId">ID del registro de auditoría</param>
        /// <returns>Registro de auditoría o null si no existe</returns>
        Task<AuditoriaDto?> ObtenerPorIdAsync(string auditoriaId);

        // ===== MÉTODOS DE ESTADÍSTICAS =====

        /// <summary>
        /// Obtiene estadísticas generales de auditoría
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <returns>Estadísticas de auditoría</returns>
        Task<EstadisticasAuditoriaDto> ObtenerEstadisticasAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene las últimas acciones críticas del sistema
        /// </summary>
        /// <param name="limite">Número máximo de registros a retornar</param>
        /// <returns>Lista de acciones críticas recientes</returns>
        Task<List<AuditoriaDto>> ObtenerAccionesCriticasRecientesAsync(int limite = 10);

        // ===== MÉTODOS UTILITARIOS =====

        /// <summary>
        /// Genera mensajes descriptivos para mostrar en la UI
        /// </summary>
        /// <param name="tipoAccion">Tipo de acción</param>
        /// <param name="usuarioEjecutor">Nombre del usuario ejecutor</param>
        /// <param name="usuarioAfectado">Nombre del usuario afectado (opcional)</param>
        /// <param name="motivo">Motivo de la acción (opcional)</param>
        /// <returns>Tupla con mensaje corto, detallado y plantilla</returns>
        (string mensajeCorto, string mensajeDetallado, string mensajePlantilla) GenerarMensajes(
            TipoAccionAuditoria tipoAccion,
            string usuarioEjecutor,
            string? usuarioAfectado = null,
            string? motivo = null);

        /// <summary>
        /// Limpia registros de auditoría antiguos según política de retención
        /// </summary>
        /// <param name="diasRetencion">Días de retención (por defecto 365)</param>
        /// <returns>Número de registros eliminados</returns>
        Task<int> LimpiarRegistrosAntiguosAsync(int diasRetencion = 365);
    }
}
