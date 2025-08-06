using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.HistorialVacaciones;

namespace sistema_vacaciones_back.Interfaces
{
    public interface IHistorialVacacionesRepository
    {
        Task<List<VacacionesHistorialDto>> ObtenerHistorialVacaciones(int personaId, DateTime fechaIngreso, DateTime fechaActual);
    }
}