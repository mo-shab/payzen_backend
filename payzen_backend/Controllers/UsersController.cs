using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Users;
using payzen_backend.Models.Users.Dtos;
using payzen_backend.Extensions; // Pour User.GetUserId()
using BCrypt.Net;

namespace payzen_backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // ← Ajouter cette ligne pour protéger toutes les routes
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
        {
            var users = await _db.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.Id)
            .ToListAsync();

            var result = users.Select(u => new UserReadDto
            {
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt.DateTime
            });

            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetById")]
        public async Task<ActionResult<UserReadDto>> GetById(int id)
        {
            var user = await _db.Users.AsNoTracking()
                .Where(u => u.DeletedAt == null)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null) return NotFound(new { Message = "Utilisateur non trouvé" });

            var result = new UserReadDto
            {
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.DateTime
            };
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var count = await _db.Users
                .Where(u => u.DeletedAt == null)
                .CountAsync();
            return Ok(count);
        }

        [HttpPost]
        public async Task<ActionResult<UserReadDto>> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Utiliser l'utilisateur connecté
            var userId = User.GetUserId();

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return Conflict(new { Message = "Un utilisateur avec cet email existe déjà" });
            }

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return Conflict(new { Message = "Ce nom d'utilisateur est déjà pris" });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new Users
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId // ← Traçabilité
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var readDto = new UserReadDto
            {
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.DateTime
            };

            return CreatedAtRoute("GetById", new { id = user.Id }, readDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserReadDto>> Update(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId(); // ← Traçabilité

            var user = await _db.Users
                .Where(u => u.Id == id && u.DeletedAt == null)
                .FirstOrDefaultAsync();
                
            if (user == null)
            {
                return NotFound(new { Message = "Utilisateur non trouvé" });
            }

            if (dto.Username != null && dto.Username != user.Username)
            {
                if (await _db.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id))
                {
                    return Conflict(new { Message = "Ce nom d'utilisateur est déjà pris" });
                }
                user.Username = dto.Username;
            }

            if (dto.Email != null && dto.Email != user.Email)
            {
                if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                {
                    return Conflict(new { Message = "Un utilisateur avec cet email existe déjà" });
                }
                user.Email = dto.Email;
            }

            if (dto.Password != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.IsActive.HasValue)
            {
                user.IsActive = dto.IsActive.Value;
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = userId; // ← Traçabilité

            await _db.SaveChangesAsync();

            var readDto = new UserReadDto
            {
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId(); // ← Traçabilité

            var user = await _db.Users
                .Where(u => u.Id == id && u.DeletedAt == null)
                .FirstOrDefaultAsync();
                
            if (user == null)
            {
                return NotFound(new { Message = "Utilisateur non trouvé" });
            }

            // Soft delete
            user.DeletedAt = DateTimeOffset.UtcNow;
            user.IsActive = false;
            user.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
