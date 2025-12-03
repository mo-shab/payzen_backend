using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Permissions.Dtos;
using payzen_backend.Extensions;

namespace payzen_backend.Controllers.Auth
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RolesController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les rôles actifs (non supprimés)
        /// </summary>
        /// <returns>Liste de tous les rôles sous forme de RoleReadDto</returns>
        /// <response code="200">Retourne la liste des rôles</response>
        /// <response code="401">Si l'utilisateur n'est pas authentifié</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetAll()
        {
            var roles = await _db.Roles
                .AsNoTracking()
                .Where(r => r.DeletedAt == null)
                .OrderBy(r => r.Name)
                .ToListAsync();

            var result = roles.Select(r => new RoleReadDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un rôle par ID
        /// </summary>
        [HttpGet("{id}", Name = "GetRoleById")]
        public async Task<ActionResult<RoleReadDto>> GetById(int id)
        {
            var role = await _db.Roles
                .AsNoTracking()
                .Where(r => r.DeletedAt == null)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound(new { Message = "Rôle non trouvé" });

            var result = new RoleReadDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouveau rôle
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RoleReadDto>> Create([FromBody] RoleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            if (await _db.Roles.AnyAsync(r => r.Name == dto.Name && r.DeletedAt == null))
            {
                return Conflict(new { Message = "Un rôle avec ce nom existe déjà" });
            }

            var role = new Roles
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            var readDto = new RoleReadDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt.DateTime
            };

            return CreatedAtRoute("GetRoleById", new { id = role.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un rôle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleReadDto>> Update(int id, [FromBody] RoleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            var role = await _db.Roles
                .Where(r => r.Id == id && r.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (role == null)
                return NotFound(new { Message = "Rôle non trouvé" });

            if (dto.Name != null && dto.Name != role.Name)
            {
                if (await _db.Roles.AnyAsync(r => r.Name == dto.Name && r.Id != id && r.DeletedAt == null))
                {
                    return Conflict(new { Message = "Un rôle avec ce nom existe déjà" });
                }
                role.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                role.Description = dto.Description;
            }

            role.UpdatedAt = DateTimeOffset.UtcNow;
            role.UpdatedBy = userId;

            await _db.SaveChangesAsync();

            var readDto = new RoleReadDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un rôle (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var role = await _db.Roles
                .Where(r => r.Id == id && r.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (role == null)
                return NotFound(new { Message = "Rôle non trouvé" });

            // Vérifier si le rôle est assigné à des utilisateurs
            var isAssignedToUsers = await _db.UsersRoles
                .AnyAsync(ur => ur.RoleId == id && ur.DeletedAt == null);

            if (isAssignedToUsers)
            {
                return BadRequest(new { Message = "Impossible de supprimer ce rôle car il est assigné à des utilisateurs" });
            }

            // Soft delete des permissions associées au rôle
            var rolePermissions = await _db.RolesPermissions
                .Where(rp => rp.RoleId == id && rp.DeletedAt == null)
                .ToListAsync();

            foreach (var rp in rolePermissions)
            {
                rp.DeletedAt = DateTimeOffset.UtcNow;
                rp.DeletedBy = userId;
            }

            // Soft delete du rôle
            role.DeletedAt = DateTimeOffset.UtcNow;
            role.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}