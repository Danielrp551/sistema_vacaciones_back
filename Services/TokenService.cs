using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using sistema_vacaciones_back.Interfaces;

namespace sistema_vacaciones_back.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
        }
        public string CreateToken(IdentityUser user)
        {
            // Validaciones de entrada
            if (user == null || string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("Usuario inválido para generar token");

            var jti = Guid.NewGuid().ToString(); // JWT ID único para cada token
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expires = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(); // 24 horas de expiración

            var claims = new List<Claim>
            {
                // Claims estándar JWT
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (Usuario ID)
                new Claim(JwtRegisteredClaimNames.Jti, jti), // JWT ID único
                new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToString(), ClaimValueTypes.Integer64), // Issued At
                new Claim(JwtRegisteredClaimNames.Exp, expires.ToString(), ClaimValueTypes.Integer64), // Expiration
                
                // Claims de usuario
                new Claim(ClaimTypes.NameIdentifier, user.Id), // Para compatibilidad con ASP.NET Core
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName ?? string.Empty),
                
                // Claims adicionales de seguridad
                new Claim("user_id", user.Id), // Claim explícito del usuario
                new Claim("token_type", "access_token"), // Tipo de token
                new Claim("scope", "api_access") // Scope del token
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24), // Expiración explícita
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
                NotBefore = DateTime.UtcNow, // Token válido desde ahora
                IssuedAt = DateTime.UtcNow // Momento de emisión
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}