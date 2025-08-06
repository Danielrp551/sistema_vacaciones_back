using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SISTEMA_VACACIONES.Data;
using sistema_vacaciones_back.DTOs.HistorialVacaciones;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Repository
{
    public class HistorialVacacionesRepository : IHistorialVacacionesRepository
    {
        private readonly ApplicationDBContext _context;

        public HistorialVacacionesRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<VacacionesHistorialDto>> ObtenerHistorialVacaciones(int personaId, DateTime fechaIngreso, DateTime fechaActual)
        {
            // ✅ Se cambia UsuarioId por PersonaId
            var diasTomados = await _context.Vacaciones 
                .Where(v => v.PersonaId == personaId)
                .SumAsync(v => v.DiasLibresDisponibles + v.DiasBloqueDisponibles); // Sumamos ambos tipos de días

            int diasTrabajados = (fechaActual - fechaIngreso).Days;
            double totalAcumulado = (30.0 * diasTrabajados) / 365; // ✅ Se usa 30.0 para precisión en decimales

            return CalcularVacaciones(totalAcumulado, diasTomados, fechaIngreso, fechaActual);
        }

        private List<VacacionesHistorialDto> CalcularVacaciones(double totalAcumulado, int diasTomados, DateTime fechaIngreso, DateTime fechaActual)
        {
            List<VacacionesHistorialDto> historial = new List<VacacionesHistorialDto>();

            for (int i = 0; i < 3; i++) // Últimos 3 años
            {
                int año = fechaActual.Year - i;
                DateTime inicioPeriodo = new DateTime(año, fechaIngreso.Month, fechaIngreso.Day);

                if (inicioPeriodo > fechaActual) continue; // Si el periodo aún no inicia, lo ignoramos

                int diasDesdeInicio = (fechaActual - inicioPeriodo).Days;
                double diasGenerados = (diasDesdeInicio >= 365) ? 30.0 : (30.0 * diasDesdeInicio) / 365;

                string categoria = (fechaActual >= inicioPeriodo.AddYears(2)) ? "Vencidas" :
                                   (fechaActual >= inicioPeriodo.AddYears(1)) ? "Pendientes" : "Truncas";

                historial.Add(new VacacionesHistorialDto
                {
                    Periodo = año,
                    Categoria = categoria,
                    Dias = diasGenerados
                });
            }

            // ✅ Se castea `diasTomados` a `double` para evitar error de conversión
            foreach (var item in historial.OrderBy(h => h.Categoria))
            {
                if (diasTomados > 0)
                {
                    double resta = Math.Min(item.Dias, (double)diasTomados);
                    item.Dias -= resta;
                    diasTomados -= (int)resta;
                }
            }

            return historial;
        }
    }
}