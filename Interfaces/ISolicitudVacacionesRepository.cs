using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.SolicitudVacaciones;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Interfaces
{
    public interface ISolicitudVacacionesRepository
    {
        Task<List<SolicitudVacaciones>> ObtenerSolicitudesAprobadasPendientes(string usuarioId);

        Task<(bool Success, string? ErrorMessage, SolicitudVacaciones? Solicitud)> CrearSolicitudVacaciones(SolicitudVacaciones solicitud, string usuarioId);

        Task<List<SolicitudVacaciones>> GetSolicitudesPagination(SolicitudesQueryObject queryObject, string usuarioId);

        /// <summary>
        /// Obtiene el detalle de una solicitud por ID
        /// </summary>
        Task<SolicitudVacaciones?> GetSolicitudByIdAsync(string solicitudId);

        /// <summary>
        /// Cancela una solicitud de vacaciones
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> CancelarSolicitudAsync(string solicitudId, string usuarioId, string? motivo);

        /// <summary>
        /// Aprueba o rechaza una solicitud de vacaciones
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> AprobarRechazarSolicitudAsync(
            string solicitudId, 
            string aprobadorId, 
            string accion, 
            string? comentarios);

        /// <summary>
        /// Verifica si un usuario puede aprobar solicitudes (es admin o jefe)
        /// </summary>
        Task<bool> CanUserApproveSolicitudesAsync(string usuarioId);

        /// <summary>
        /// Verifica si un usuario puede aprobar una solicitud específica (jefe directo o admin)
        /// </summary>
        Task<bool> CanUserApproveSpecificSolicitudAsync(string aprobadorId, string solicitudId);

        /// <summary>
        /// Obtiene solicitudes del equipo para gestión por supervisores
        /// </summary>
        Task<(List<SolicitudVacaciones> Solicitudes, int TotalCount)> GetSolicitudesEquipo(SolicitudesQueryObject queryObject, string supervisorId);

        /// <summary>
        /// Obtiene estadísticas de solicitudes del equipo
        /// </summary>
        Task<object> GetEstadisticasEquipo(string supervisorId, SolicitudesQueryObject queryObject);

        /// <summary>
        /// Obtiene una solicitud por ID
        /// </summary>
        Task<SolicitudVacaciones?> GetByIdAsync(int solicitudId);
    }
}