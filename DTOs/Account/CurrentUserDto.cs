using sistema_vacaciones_back.DTOs.Persona;

namespace SISTEMA_VACACIONES.DTOs.Account
{
    /// <summary>
    /// DTO para la respuesta del usuario actual autenticado
    /// Incluye todos los datos necesarios para mantener la sesión
    /// </summary>
    public class CurrentUserDto
    {
        /// <summary>
        /// ID único del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Información personal del usuario
        /// </summary>
        public PersonaDto Persona { get; set; } = new();

        /// <summary>
        /// Lista de roles asignados al usuario
        /// </summary>
        public IList<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Lista de códigos de permisos del usuario
        /// </summary>
        public List<string> Permisos { get; set; } = new List<string>();

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de última conexión
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
    }
}