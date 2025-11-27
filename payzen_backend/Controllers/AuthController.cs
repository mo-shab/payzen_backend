using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Auth;
using payzen_backend.Services;

namespace payzen_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, JwtService jwt, IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
        }

        /// <summary>
        /// Authentifie un utilisateur avec son email et mot de passe
        /// </summary>
        /// <param name="loginRequest">Données de connexion (email et mot de passe)</param>
        /// <returns>Token JWT et informations utilisateur</returns>
        /// <response code="200">Authentification réussie, retourne le token JWT</response>
        /// <response code="401">Email ou mot de passe incorrect</response>
        /// <response code="400">Données de connexion invalides</response>
        [HttpPost("login")]
        [Produces("application/json")] // Retourne du JSON
        // Retiré [Consumes] pour être plus tolérant
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {            
            // Validation du modèle
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
                return BadRequest(new 
                { 
                    Message = "Données invalides",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            if (loginRequest == null)
            {
                Console.WriteLine("❌ loginRequest is null");
                return BadRequest(new { Message = "Les données de connexion sont requises" });
            }

            // Recherche de l'utilisateur avec ses relations Employee
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee) // 👈 Inclure l'employé si lié
                .Where(u => u.Email == loginRequest.Email
                        && u.IsActive == true
                        && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            Console.WriteLine($"👤 User found in database: {user != null}");

            if (user == null)
            {
                Console.WriteLine("❌ User not found or inactive");
                return Unauthorized(new { Message = "Email ou mot de passe incorrect" });
            }

            var passwordValid = user.VerifyPassword(loginRequest.Password);
            Console.WriteLine($"🔑 Password valid: {passwordValid}");

            if (!passwordValid)
            {
                Console.WriteLine("❌ Invalid password");
                return Unauthorized(new { Message = "Email ou mot de passe incorrect" });
            }

            // 👇 Récupérer les rôles de l'utilisateur
            var userRoles = await _db.UsersRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == user.Id && ur.DeletedAt == null)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            // 👇 Récupérer toutes les permissions de l'utilisateur via ses rôles
            var userPermissions = await _db.RolesPermissions
                .AsNoTracking()
                .Where(rp => _db.UsersRoles
                    .Where(ur => ur.UserId == user.Id && ur.DeletedAt == null)
                    .Select(ur => ur.RoleId)
                    .Contains(rp.RoleId) && rp.DeletedAt == null)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            // Génération du token JWT
            var token = await _jwt.GenerateTokenAsync(user.Id, user.Email);

            var expiresInMinutes = int.Parse(_config["JwtSettings:ExpiresInMinutes"] ?? "120");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            Console.WriteLine("✅ Login successful - Token generated");
            Console.WriteLine($"✅ ========== END LOGIN ==========\n");

            // 👇 Créer la réponse avec toutes les informations
            var response = new LoginResponse
            {
                Message = "Authentification réussie",
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.Employee?.FirstName ?? "",
                    LastName = user.Employee?.LastName ?? "",
                    Roles = userRoles,
                    Permissions = userPermissions
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Déconnexion (côté client)
        /// </summary>
        [HttpPost("logout")]
        [Produces("application/json")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "Déconnexion réussie. Veuillez supprimer le token côté client." });
        }
    }
}
