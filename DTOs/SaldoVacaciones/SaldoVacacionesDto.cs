using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.SaldoVacaciones
{
    /// <summary>
    /// DTO para representar los saldos de vacaciones de un empleado
    /// </summary>
    public class SaldoVacacionesDto
    {
        /// <summary>
        /// ID único del saldo (empleado + periodo)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// ID del empleado
        /// </summary>
        [Required]
        public string EmpleadoId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del empleado
        /// </summary>
        [Required]
        public string NombreEmpleado { get; set; } = string.Empty;

        /// <summary>
        /// Email del empleado
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Año del periodo vacacional
        /// </summary>
        [Required]
        [Range(2020, 2099)]
        public int Periodo { get; set; }

        /// <summary>
        /// Días vencidos (más de 2 años)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DiasVencidas { get; set; }

        /// <summary>
        /// Días pendientes (1-2 años)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DiasPendientes { get; set; }

        /// <summary>
        /// Días truncas (menos de 1 año)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DiasTruncas { get; set; }

        /// <summary>
        /// Días libres disponibles en este periodo
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DiasLibres { get; set; }

        /// <summary>
        /// Días de bloque disponibles en este periodo
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DiasBloque { get; set; }

        /// <summary>
        /// Nombre del supervisor/manager
        /// </summary>
        public string? NombreManager { get; set; }

        /// <summary>
        /// Fecha de corte para el cálculo
        /// </summary>
        public DateTime FechaCorte { get; set; }

        /// <summary>
        /// Total de días disponibles en este periodo
        /// </summary>
        public int TotalDias => DiasLibres + DiasBloque;

        /// <summary>
        /// Total de días históricos
        /// </summary>
        public int TotalHistorico => DiasVencidas + DiasPendientes + DiasTruncas;
    }
}