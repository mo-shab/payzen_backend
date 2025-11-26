using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Permissions.Dtos;
using payzen_backend.Extensions;
using payzen_backend.Models.Users.Dtos;

namespace payzen_backend.Controllers
{
    [Route("api/users-roles")]
    [ApiController]
    [Authorize]
    public class UsersRolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        
        public UsersRolesController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les rôles assignés à un utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <returns>Liste des rôles de l'utilisateur</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetUserRoles(int userId)
        {
            // Vérifier que l'utilisateur existe et est actif
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
            
            if (user == null)
                return NotFound(new { Message = "Utilisateur non trouvé" });

            // Récupérer les rôles de l'utilisateur
            var roles = await _db.UsersRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && ur.DeletedAt == null)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.DeletedAt == null)
                .Select(ur => new RoleReadDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    CreatedAt = ur.Role.CreatedAt.DateTime
                })
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Récupère tous les utilisateurs ayant un rôle spécifique
        /// </summary>
        /// <param name="roleId">ID du rôle</param>
        /// <returns>Liste des utilisateurs ayant ce rôle</returns>
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetRoleUsers(int roleId)
        {
            // Vérifier que le rôle existe
            var roleExists = await _db.Roles
                .AnyAsync(r => r.Id == roleId && r.DeletedAt == null);
            
            if (!roleExists)
                return NotFound(new { Message = "Rôle non trouvé" });

            // Récupérer les utilisateurs ayant ce rôle
            var users = await _db.UsersRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == roleId && ur.DeletedAt == null)
                .Include(ur => ur.User)
                .Where(ur => ur.User.DeletedAt == null)
                .Select(ur => new UserReadDto
                {
                    Id = ur.User.Id,
                    Username = ur.User.Username,
                    Email = ur.User.Email,
                    IsActive = ur.User.IsActive,
                    CreatedAt = ur.User.CreatedAt.DateTime
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Assigne un rôle à un utilisateur
        /// </summary>
        /// <param name="dto">UserId et RoleId</param>
        /// <returns>Message de confirmation</returns>
        [HttpPost]
        public async Task<ActionResult> AssignRoleToUser([FromBody] UserRoleAssignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.GetUserId();

            // Vérifier que l'utilisateur existe et est actif
            var user = await _db.Users
                .Where(u => u.Id == dto.UserId && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { Message = "Utilisateur non trouvé" });

            if (!user.IsActive)
                return BadRequest(new { Message = "L'utilisateur est désactivé" });

            // Vérifier que le rôle existe
            var roleExists = await _db.Roles
                .AnyAsync(r => r.Id == dto.RoleId && r.DeletedAt == null);
            
            if (!roleExists)
                return NotFound(new { Message = "Rôle non trouvé" });

            // Vérifier si l'association existe déjà (même soft-deleted)
            var existingAssignment = await _db.UsersRoles
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

            if (existingAssignment != null)
            {
                if (existingAssignment.DeletedAt == null)
                {
                    return Conflict(new { Message = "L'utilisateur possède déjà ce rôle" });
                }

                // Réactiver l'association soft-deleted
                existingAssignment.DeletedAt = null;
                existingAssignment.DeletedBy = null;
                existingAssignment.UpdatedAt = DateTimeOffset.UtcNow;
                existingAssignment.UpdatedBy = currentUserId;
            }
            else
            {
                // Créer une nouvelle association
                var userRole = new UsersRoles
                {
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = currentUserId
                };

                _db.UsersRoles.Add(userRole);
            }

            await _db.SaveChangesAsync();

            return Ok(new { Message = "Rôle assigné avec succès" });
        }

        /// <summary>
        /// Assigne plusieurs rôles à un utilisateur en une seule opération
        /// </summary>
        /// <param name="dto">UserId et liste de RoleIds</param>
        /// <returns>Résumé de l'opération</returns>
        [HttpPost("bulk-assign")]
        public async Task<ActionResult> BulkAssignRoles([FromBody] UserRolesBulkAssignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.GetUserId();

            // Vérifier que l'utilisateur existe et est actif
            var user = await _db.Users
                .Where(u => u.Id == dto.UserId && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { Message = "Utilisateur non trouvé" });

            if (!user.IsActive)
                return BadRequest(new { Message = "L'utilisateur est désactivé" });

            // Vérifier que tous les rôles existent
            var validRoles = await _db.Roles
                .Where(r => dto.RoleIds.Contains(r.Id) && r.DeletedAt == null)
                .Select(r => r.Id)
                .ToListAsync();

            if (validRoles.Count != dto.RoleIds.Count)
            {
                return BadRequest(new { Message = "Un ou plusieurs rôles n'existent pas" });
            }

            var assignedCount = 0;
            var reactivatedCount = 0;
            var skippedCount = 0;

            foreach (var roleId in dto.RoleIds)
            {
                var existingAssignment = await _db.UsersRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == roleId);

                if (existingAssignment != null)
                {
                    if (existingAssignment.DeletedAt != null)
                    {
                        // Réactiver
                        existingAssignment.DeletedAt = null;
                        existingAssignment.DeletedBy = null;
                        existingAssignment.UpdatedAt = DateTimeOffset.UtcNow;
                        existingAssignment.UpdatedBy = currentUserId;
                        reactivatedCount++;
                    }
                    else
                    {
                        // Déjà assigné
                        skippedCount++;
                    }
                }
                else
                {
                    // Créer nouvelle association
                    var userRole = new UsersRoles
                    {
                        UserId = dto.UserId,
                        RoleId = roleId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = currentUserId
                    };

                    _db.UsersRoles.Add(userRole);
                    assignedCount++;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new 
            { 
                Message = "Rôles assignés avec succès",
                Assigned = assignedCount,
                Reactivated = reactivatedCount,
                Skipped = skippedCount
            });
        }

        /// <summary>
        /// Remplace tous les rôles d'un utilisateur
        /// </summary>
        /// <param name="dto">UserId et nouvelle liste de RoleIds</param>
        /// <returns>Résumé de l'opération</returns>
        [HttpPut("replace")]
        public async Task<ActionResult> ReplaceUserRoles([FromBody] UserRolesBulkAssignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.GetUserId();

            // Vérifier que l'utilisateur existe et est actif
            var user = await _db.Users
                .Where(u => u.Id == dto.UserId && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { Message = "Utilisateur non trouvé" });

            if (!user.IsActive)
                return BadRequest(new { Message = "L'utilisateur est désactivé" });

            // Vérifier que tous les rôles existent
            var validRoles = await _db.Roles
                .Where(r => dto.RoleIds.Contains(r.Id) && r.DeletedAt == null)
                .Select(r => r.Id)
                .ToListAsync();

            if (validRoles.Count != dto.RoleIds.Count)
            {
                return BadRequest(new { Message = "Un ou plusieurs rôles n'existent pas" });
            }

            // Supprimer tous les rôles actuels
            var currentRoles = await _db.UsersRoles
                .Where(ur => ur.UserId == dto.UserId && ur.DeletedAt == null)
                .ToListAsync();

            foreach (var ur in currentRoles)
            {
                ur.DeletedAt = DateTimeOffset.UtcNow;
                ur.DeletedBy = currentUserId;
            }

            // Assigner les nouveaux rôles
            var assignedCount = 0;
            var reactivatedCount = 0;

            foreach (var roleId in dto.RoleIds)
            {
                var existingAssignment = await _db.UsersRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == roleId);

                if (existingAssignment != null)
                {
                    // Réactiver
                    existingAssignment.DeletedAt = null;
                    existingAssignment.DeletedBy = null;
                    existingAssignment.UpdatedAt = DateTimeOffset.UtcNow;
                    existingAssignment.UpdatedBy = currentUserId;
                    reactivatedCount++;
                }
                else
                {
                    // Créer nouvelle association
                    var userRole = new UsersRoles
                    {
                        UserId = dto.UserId,
                        RoleId = roleId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = currentUserId
                    };

                    _db.UsersRoles.Add(userRole);
                    assignedCount++;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new 
            { 
                Message = "Rôles remplacés avec succès",
                Removed = currentRoles.Count,
                Assigned = assignedCount,
                Reactivated = reactivatedCount
            });
        }

        /// <summary>
        /// Retire un rôle d'un utilisateur (soft delete)
        /// </summary>
        /// <param name="dto">UserId et RoleId</param>
        /// <returns>204 No Content</returns>
        [HttpDelete]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] UserRoleAssignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.GetUserId();

            var userRole = await _db.UsersRoles
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId 
                                        && ur.RoleId == dto.RoleId 
                                        && ur.DeletedAt == null);

            if (userRole == null)
                return NotFound(new { Message = "Cette association utilisateur-rôle n'existe pas" });

            // Soft delete
            userRole.DeletedAt = DateTimeOffset.UtcNow;
            userRole.DeletedBy = currentUserId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
