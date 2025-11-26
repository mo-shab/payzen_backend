using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace payzen_backend.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Récupère l'ID de l'utilisateur depuis le claim "uid"
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("uid")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("User ID claim not found in token");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("Invalid User ID format in token");
            }

            return userId;
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

        /// <summary>
        /// Récupère toutes les permissions de l'utilisateur depuis les claims
        /// </summary>
        public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Vérifie si l'utilisateur possède une permission spécifique
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.Claims
                .Any(c => c.Type == "permission" && c.Value == permission);
        }

        /// <summary>
        /// Vérifie si l'utilisateur possède au moins une des permissions spécifiées
        /// </summary>
        public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
        {
            var userPermissions = user.GetPermissions();
            return permissions.Any(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Vérifie si l'utilisateur possède toutes les permissions spécifiées
        /// </summary>
        public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
        {
            var userPermissions = user.GetPermissions();
            return permissions.All(p => userPermissions.Contains(p));
        }
    }
}