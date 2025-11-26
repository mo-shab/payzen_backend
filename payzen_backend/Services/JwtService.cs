using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace payzen_backend.Services
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expires;

        public JwtService(IConfiguration config)
        {
            _key = config["JwtSettings:Key"] 
                ?? throw new InvalidOperationException("JWT Key not configured");
            _issuer = config["JwtSettings:Issuer"] 
                ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = config["JwtSettings:Audience"] 
                ?? throw new InvalidOperationException("JWT Audience not configured");
            _expires = int.Parse(config["JwtSettings:ExpiresInMinutes"] ?? "120");
        }

        /// <summary>
        /// Génère un token JWT pour un utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="email">Email de l'utilisateur (utilisé comme identifiant unique)</param>
        /// <returns>Token JWT signé</returns>
        public string GenerateToken(int userId, string email)
        {
            // CHANGEMENT: Utilisation de l'email au lieu du username
            // Pourquoi: L'email est l'identifiant unique pour la connexion
            var claims = new[]
            {
                // Claim standard "sub" (subject) - contient l'ID utilisateur
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                
                // Claim "unique_name" - contient l'email
                new Claim(JwtRegisteredClaimNames.UniqueName, email),
                
                // Claim personnalisé "uid" - facilite la récupération de l'ID
                new Claim("uid", userId.ToString()),
                
                // Claim personnalisé "email" - facilite la récupération de l'email
                new Claim(JwtRegisteredClaimNames.Email, email),
                
                // Claim "jti" - ID unique du token (utile pour la révocation)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expires),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
