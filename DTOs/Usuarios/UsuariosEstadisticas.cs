using System;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para estadísticas generales de usuarios del sistema
    /// </summary>
    public class UsuariosEstadisticas
    {
        /// <summary>
        /// Total de usuarios registrados (excluyendo eliminados)
        /// </summary>
        public int TotalUsuarios { get; set; }

        /// <summary>
        /// Usuarios activos en el sistema
        /// </summary>
        public int UsuariosActivos { get; set; }

        /// <summary>
        /// Usuarios inactivos en el sistema
        /// </summary>
        public int UsuariosInactivos { get; set; }

        /// <summary>
        /// Usuarios que deben cambiar su contraseña en el próximo login
        /// </summary>
        public int UsuariosPendientesCambioContrasena { get; set; }

        /// <summary>
        /// Usuarios marcados como extranjeros
        /// </summary>
        public int UsuariosExtranjeros { get; set; }

        /// <summary>
        /// Fecha y hora de generación de las estadísticas
        /// </summary>
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    }
}
