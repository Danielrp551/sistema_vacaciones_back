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
    }
}