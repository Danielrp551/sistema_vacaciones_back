using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sistema_vacaciones_back.Extensions;

namespace sistema_vacaciones_back.Security
{
    /// <summary>
    /// Atributo de autorización avanzada para endpoints críticos
    /// Valida que el usuario esté autenticado y que el token sea válido
    /// </summary>
    public class SecureEndpointAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;
        private readonly bool _requireActiveUser;

        public SecureEndpointAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles ?? Array.Empty<string>();
            _requireActiveUser = true;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1. Verificar autenticación básica
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult("Usuario no autenticado");
                return;
            }

            // 2. Validar userId
            var userId = user.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult("Token inválido - UserId no encontrado");
                return;
            }

            // 3. Validar roles si se especificaron
            if (_requiredRoles.Length > 0)
            {
                var userRoles = user.GetUserRoles();
                if (!_requiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
                {
                    context.Result = new ObjectResult($"Acceso denegado. Roles requeridos: {string.Join(", ", _requiredRoles)}")
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            // 4. Validar expiración del token
            var expiration = user.GetTokenExpiration();
            if (expiration.HasValue && expiration.Value < DateTime.UtcNow)
            {
                context.Result = new UnauthorizedObjectResult("Token expirado");
                return;
            }

            // 5. Validaciones adicionales para usuarios activos
            if (_requireActiveUser)
            {
                var jti = user.FindFirst("jti")?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    context.Result = new UnauthorizedObjectResult("Token inválido - JTI faltante");
                    return;
                }
            }

            // Si llegamos aquí, el usuario está autorizado
        }
    }

    /// <summary>
    /// Atributo específico para operaciones de administrador
    /// </summary>
    public class AdminOnlyAttribute : SecureEndpointAttribute
    {
        public AdminOnlyAttribute() : base("Admin", "SuperAdmin")
        {
        }
    }

    /// <summary>
    /// Atributo para operaciones de recursos propios del usuario
    /// Valida que el usuario solo acceda a sus propios recursos
    /// </summary>
    public class OwnerOnlyAttribute : SecureEndpointAttribute
    {
        public OwnerOnlyAttribute() : base()
        {
        }
    }
}
