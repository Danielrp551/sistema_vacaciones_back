using System.ComponentModel.DataAnnotations;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para estadísticas de usuarios
    /// </summary>
    public class UsuariosEstadisticasDto
    {
        /// <summary>
        /// Total de usuarios
        /// </summary>
        public int TotalUsuarios { get; set; }

        /// <summary>
        /// Usuarios activos (no eliminados)
        /// </summary>
        public int UsuariosActivos { get; set; }

        /// <summary>
        /// Usuarios inactivos (eliminados)
        /// </summary>
        public int UsuariosInactivos { get; set; }

        /// <summary>
        /// Usuarios que deben cambiar contraseña
        /// </summary>
        public int UsuariosForzarCambio { get; set; }

        /// <summary>
        /// Usuarios pendientes de cambio de contraseña (alias para compatibilidad)
        /// </summary>
        public int UsuariosPendientesCambioContrasena => UsuariosForzarCambio;

        /// <summary>
        /// Usuarios extranjeros
        /// </summary>
        public int UsuariosExtranjeros { get; set; }

        /// <summary>
        /// Usuarios con jefe asignado
        /// </summary>
        public int UsuariosConJefe { get; set; }

        /// <summary>
        /// Porcentaje de usuarios activos
        /// </summary>
        public decimal PorcentajeActivos => TotalUsuarios > 0 
            ? Math.Round((decimal)UsuariosActivos / TotalUsuarios * 100, 2)
            : 0;

        /// <summary>
        /// Porcentaje de usuarios extranjeros
        /// </summary>
        public decimal PorcentajeExtranjeros => TotalUsuarios > 0 
            ? Math.Round((decimal)UsuariosExtranjeros / TotalUsuarios * 100, 2)
            : 0;
    }
}
