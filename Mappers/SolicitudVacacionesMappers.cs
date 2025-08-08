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
            this CreateSolicitudRequestDto createSolicitudDto, string usuarioId)
        {
            return new SolicitudVacaciones
            {
                Id = Guid.NewGuid().ToString(), // Generar un ID único
                SolicitanteId = usuarioId,
                TipoVacaciones = createSolicitudDto.TipoVacaciones,
                DiasSolicitados = createSolicitudDto.DiasSolicitados,
                FechaInicio = createSolicitudDto.FechaInicio,
                FechaFin = createSolicitudDto.FechaFin,
                Periodo = createSolicitudDto.Periodo,
                Estado = "pendiente",
                FechaSolicitud = DateTime.UtcNow,
                Comentarios = string.Empty, // Inicializar comentarios vacíos (para el aprobador)
                Observaciones = createSolicitudDto.Observaciones ?? string.Empty // Observaciones del solicitante
            };
        }

        public static SolicitudVacacionesDto ToSolicitudVacacionesDto(this SolicitudVacaciones solicitud)
        {
            return new SolicitudVacacionesDto
            {
                Id = solicitud.Id ?? string.Empty, // Usar el ID real de la solicitud (GUID)
                UsuarioId = solicitud.SolicitanteId ?? string.Empty,
                TipoVacaciones = solicitud.TipoVacaciones ?? string.Empty,
                DiasSolicitados = solicitud.DiasSolicitados,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Estado = solicitud.Estado ?? "pendiente",
                FechaSolicitud = solicitud.FechaSolicitud,
                Periodo = solicitud.Periodo,
                DiasFinde = solicitud.DiasFinde
            };
        }

        /// <summary>
        /// Convierte una solicitud a DTO con detalles completos
        /// </summary>
        public static SolicitudVacacionesDetailDto ToSolicitudVacacionesDetailDto(
            this SolicitudVacaciones solicitud, 
            string currentUserId, 
            bool isAdmin = false)
        {
            var dto = new SolicitudVacacionesDetailDto
            {
                Id = solicitud.Id ?? string.Empty,
                UsuarioId = solicitud.SolicitanteId ?? string.Empty,
                NombreSolicitante = $"{solicitud.Solicitante?.Persona?.Nombres} {solicitud.Solicitante?.Persona?.ApellidoPaterno} {solicitud.Solicitante?.Persona?.ApellidoMaterno}".Trim(),
                EmailSolicitante = solicitud.Solicitante?.Email ?? string.Empty,
                AprobadorId = solicitud.AprobadorId,
                NombreAprobador = solicitud.Aprobador?.Persona != null 
                    ? $"{solicitud.Aprobador.Persona.Nombres} {solicitud.Aprobador.Persona.ApellidoPaterno} {solicitud.Aprobador.Persona.ApellidoMaterno}".Trim()
                    : null,
                JefeDirectoId = solicitud.Solicitante?.JefeId,
                NombreJefeDirecto = solicitud.Solicitante?.Jefe?.Persona != null 
                    ? $"{solicitud.Solicitante.Jefe.Persona.Nombres} {solicitud.Solicitante.Jefe.Persona.ApellidoPaterno} {solicitud.Solicitante.Jefe.Persona.ApellidoMaterno}".Trim()
                    : null,
                TipoVacaciones = solicitud.TipoVacaciones ?? string.Empty,
                DiasSolicitados = solicitud.DiasSolicitados,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Estado = solicitud.Estado ?? "pendiente",
                FechaSolicitud = solicitud.FechaSolicitud,
                Periodo = solicitud.Periodo,
                DiasFinde = solicitud.DiasFinde,
                FechaAprobacion = solicitud.FechaAprobacion,
                Comentarios = solicitud.Comentarios ?? string.Empty,
                Observaciones = solicitud.Observaciones ?? string.Empty
            };

            // Lógica de permisos
            dto.PuedeCancelar = CanCancelSolicitud(solicitud, currentUserId);
            dto.PuedeAprobar = CanApproveSolicitud(solicitud, currentUserId, isAdmin);

            return dto;
        }

        /// <summary>
        /// Determina si una solicitud puede ser cancelada
        /// </summary>
        private static bool CanCancelSolicitud(SolicitudVacaciones solicitud, string currentUserId)
        {
            // Solo el solicitante puede cancelar
            if (solicitud.SolicitanteId != currentUserId)
                return false;

            // Solo se puede cancelar si está pendiente
            if (solicitud.Estado != "pendiente")
                return false;

            // Se puede cancelar hasta el mismo día que comienzan las vacaciones
            // No se puede cancelar después de que ya comenzaron
            if (solicitud.FechaInicio < DateTime.Today)
                return false;

            return true;
        }

        /// <summary>
        /// Determina si una solicitud puede ser aprobada
        /// </summary>
        private static bool CanApproveSolicitud(SolicitudVacaciones solicitud, string currentUserId, bool isAdmin)
        {
            // No puede aprobar sus propias solicitudes
            if (solicitud.SolicitanteId == currentUserId)
                return false;

            // Solo se puede aprobar si está pendiente
            if (solicitud.Estado != "pendiente")
                return false;

            // Debe ser admin o jefe del solicitante
            if (!isAdmin && solicitud.Solicitante?.JefeId != currentUserId)
                return false;

            return true;
        }
    }
}