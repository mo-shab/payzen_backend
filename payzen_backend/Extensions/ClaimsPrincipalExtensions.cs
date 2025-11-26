using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace payzen_backend.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté depuis les claims JWT
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst("uid") 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Utilisateur non authentifié");
        }

        /// <summary>
        /// Récupère l'email de l'utilisateur depuis les claims JWT
        /// </summary>
        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value 
                ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? principal.FindFirst("email")?.Value
                ?? throw new UnauthorizedAccessException("Email non trouvé dans le token");
        }

        /// <summary>
        /// Récupère le nom d'utilisateur depuis les claims JWT
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value 
                ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value 
                ?? principal.FindFirst("unique_name")?.Value
                ?? throw new UnauthorizedAccessException("Nom d'utilisateur non trouvé");
        }

        /// <summary>
        /// Vérifie si l'utilisateur est authentifié
        /// </summary>
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal.Identity?.IsAuthenticated ?? false;
        }
    }
}