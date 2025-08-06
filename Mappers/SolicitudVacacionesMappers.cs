using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.SolicitudVacaciones;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Mappers
{
    public static class SolicitudVacacionesMappers
    {
        public static SolicitudVacaciones ToSolicitudVacacionesFromCreateDto(
            this CreateSolicitudRequestDto createSolicitudDto)
        {
            return new SolicitudVacaciones
            {
                UsuarioId = createSolicitudDto.UsuarioId,
                TipoVacaciones = createSolicitudDto.TipoVacaciones,
                DiasSolicitados = createSolicitudDto.DiasSolicitados,
                FechaInicio = createSolicitudDto.FechaInicio,
                FechaFin = createSolicitudDto.FechaFin,
                Periodo = createSolicitudDto.Periodo,
                Estado = "pendiente",
                FechaSolicitud = DateTime.UtcNow,
            };
        }

        public static SolicitudVacacionesDto ToSolicitudVacacionesDto(this SolicitudVacaciones solicitud)
        {
            return new SolicitudVacacionesDto
            {
                Id = solicitud.Id,
                UsuarioId = solicitud.UsuarioId,
                TipoVacaciones = solicitud.TipoVacaciones,
                DiasSolicitados = solicitud.DiasSolicitados,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Estado = solicitud.Estado,
                FechaSolicitud = solicitud.FechaSolicitud,
                Periodo = solicitud.Periodo,
                DiasFinde = solicitud.DiasFinde,
            };
        }
    }
}