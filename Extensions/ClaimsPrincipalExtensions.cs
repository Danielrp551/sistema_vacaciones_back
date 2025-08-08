using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace sistema_vacaciones_back.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtiene el ID del usuario actual del token JWT con validaciones de seguridad
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>El ID del usuario validado o null si no es válido</returns>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            // Verificar que el usuario esté autenticado
            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            // Buscar el claim del usuario con validación
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        user.FindFirst("sub")?.Value ??
                        user.FindFirst("user_id")?.Value;

            // Validar que el userId no esté vacío y tenga formato válido (GUID)
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            // Validación adicional: verificar que es un GUID válido para mayor seguridad
            if (!Guid.TryParse(userId, out _))
                return null;

            return userId;
        }

        /// <summary>
        /// Obtiene el email del usuario actual del token JWT con validaciones
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>El email del usuario validado o null si no es válido</returns>
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            var email = user.FindFirst(ClaimTypes.Email)?.Value ??
                       user.FindFirst("email")?.Value;

            // Validación básica de formato de email
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return null;

            return email;
        }

        /// <summary>
        /// Obtiene el nombre de usuario del token JWT con validaciones
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>El nombre de usuario validado o null si no es válido</returns>
        public static string? GetUserName(this ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            var userName = user.FindFirst(ClaimTypes.GivenName)?.Value ??
                          user.FindFirst("given_name")?.Value ??
                          user.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(userName) ? null : userName;
        }

        /// <summary>
        /// Obtiene los roles del usuario del token JWT
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>Lista de roles del usuario</returns>
        public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return Enumerable.Empty<string>();

            return user.FindAll(ClaimTypes.Role)
                      .Select(c => c.Value)
                      .Where(role => !string.IsNullOrWhiteSpace(role));
        }

        /// <summary>
        /// Verifica si el usuario tiene un rol específico
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <param name="role">Rol a verificar</param>
        /// <returns>True si el usuario tiene el rol</returns>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            if (!user.Identity?.IsAuthenticated ?? true || string.IsNullOrWhiteSpace(role))
                return false;

            return user.GetUserRoles().Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene la fecha de expiración del token
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>Fecha de expiración del token o null</returns>
        public static DateTime? GetTokenExpiration(this ClaimsPrincipal user)
        {
            var exp = user.FindFirst("exp")?.Value;
            if (string.IsNullOrEmpty(exp) || !long.TryParse(exp, out var expUnix))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(expUnix).DateTime;
        }
    }
}
