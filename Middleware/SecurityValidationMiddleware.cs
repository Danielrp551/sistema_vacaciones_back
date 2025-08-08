using System.Security.Claims;
using sistema_vacaciones_back.Extensions;

namespace sistema_vacaciones_back.Middleware
{
    /// <summary>
    /// Middleware para validaciones adicionales de seguridad en cada request
    /// </summary>
    public class SecurityValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityValidationMiddleware> _logger;

        public SecurityValidationMiddleware(RequestDelegate next, ILogger<SecurityValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo validar rutas que requieren autenticación
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var validationResult = await ValidateUserSecurity(context);
                
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validación de seguridad falló: {Reason} para usuario {UserId}", 
                        validationResult.Reason, context.User.GetUserId());
                    
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token inválido o expirado");
                    return;
                }
            }

            await _next(context);
        }

        private Task<ValidationResult> ValidateUserSecurity(HttpContext context)
        {
            try
            {
                var user = context.User;

                // 1. Validar que el userId sea un GUID válido
                var userId = user.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult(ValidationResult.Invalid("UserId no encontrado en el token"));
                }

                // 2. Validar expiration del token
                var expiration = user.GetTokenExpiration();
                if (expiration.HasValue && expiration.Value < DateTime.UtcNow)
                {
                    return Task.FromResult(ValidationResult.Invalid("Token expirado"));
                }

                // 3. Validar que el token tenga los claims mínimos requeridos
                if (string.IsNullOrEmpty(user.FindFirst("jti")?.Value))
                {
                    return Task.FromResult(ValidationResult.Invalid("Token sin JTI (JWT ID)"));
                }

                // 4. Validar scope del token
                var scope = user.FindFirst("scope")?.Value;
                if (scope != "api_access")
                {
                    return Task.FromResult(ValidationResult.Invalid("Scope inválido en el token"));
                }

                // 5. Validar que el token sea de tipo access_token
                var tokenType = user.FindFirst("token_type")?.Value;
                if (tokenType != "access_token")
                {
                    return Task.FromResult(ValidationResult.Invalid("Tipo de token inválido"));
                }

                return Task.FromResult(ValidationResult.Valid());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante validación de seguridad");
                return Task.FromResult(ValidationResult.Invalid("Error interno de validación"));
            }
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Reason { get; set; } = string.Empty;

            public static ValidationResult Valid() => new() { IsValid = true };
            public static ValidationResult Invalid(string reason) => new() { IsValid = false, Reason = reason };
        }
    }
}
