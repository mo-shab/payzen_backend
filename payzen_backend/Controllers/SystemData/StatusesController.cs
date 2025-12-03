using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Referentiel;
using payzen_backend.Models.Referentiel.Dtos;

namespace payzen_backend.Controllers.SystemData
{
    [Route("api/statues")]
    [ApiController]
    [Authorize]
    public class StatusesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StatusesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère tous les statuts actifs
        /// </summary>
        /// <returns>Liste des statuts</returns>
        [HttpGet]
        //[HasPermission("READ_STATUSES")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<StatusReadDto>>> GetAll()
        {
            var statuses = await _db.Statuses
                .AsNoTracking()
                .Where(s => s.DeletedAt == null)
                .OrderBy(s => s.Name)
                .Select(s => new StatusReadDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            return Ok(statuses);
        }

        /// <summary>
        /// Récupère un statut par son ID
        /// </summary>
        /// <param name="id">ID du statut</param>
        /// <returns>Détails du statut</returns>
        [HttpGet("{id}")]
        //[HasPermission("READ_STATUSES")]
        [Produces("application/json")]
        public async Task<ActionResult<StatusReadDto>> GetById(int id)
        {
            var status = await _db.Statuses
                .AsNoTracking()
                .Where(s => s.Id == id && s.DeletedAt == null)
                .Select(s => new StatusReadDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .FirstOrDefaultAsync();

            if (status == null)
            {
                return NotFound(new { Message = "Statut non trouvé" });
            }

            return Ok(status);
        }

        /// <summary>
        /// Crée un nouveau statut
        /// </summary>
        /// <param name="dto">Données du statut à créer</param>
        /// <returns>Statut créé</returns>
        [HttpPost]
        //[HasPermission("CREATE_STATUSES")]
        [Produces("application/json")]
        public async Task<ActionResult<StatusReadDto>> Create([FromBody] StatusCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que le nom n'existe pas déjà
            var existingStatus = await _db.Statuses
                .AnyAsync(s => s.Name == dto.Name && s.DeletedAt == null);

            if (existingStatus)
            {
                return Conflict(new { Message = "Un statut avec ce nom existe déjà" });
            }

            var userId = User.GetUserId();

            var status = new Status
            {
                Name = dto.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Statuses.Add(status);
            await _db.SaveChangesAsync();

            var readDto = new StatusReadDto
            {
                Id = status.Id,
                Name = status.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = status.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un statut existant
        /// </summary>
        /// <param name="id">ID du statut à modifier</param>
        /// <param name="dto">Nouvelles données du statut</param>
        /// <returns>Statut mis à jour</returns>
        [HttpPut("{id}")]
        [HasPermission("UPDATE_STATUSES")]
        [Produces("application/json")]
        public async Task<ActionResult<StatusReadDto>> Update(int id, [FromBody] StatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var status = await _db.Statuses
                .Where(s => s.Id == id && s.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (status == null)
            {
                return NotFound(new { Message = "Statut non trouvé" });
            }

            // Si le nom est modifié, vérifier qu'il n'existe pas déjà
            if (dto.Name != null && dto.Name != status.Name)
            {
                var existingStatus = await _db.Statuses
                    .AnyAsync(s => s.Name == dto.Name && s.Id != id && s.DeletedAt == null);

                if (existingStatus)
                {
                    return Conflict(new { Message = "Un statut avec ce nom existe déjà" });
                }

                status.Name = dto.Name;
            }

            var userId = User.GetUserId();
            status.ModifiedAt = DateTimeOffset.UtcNow;
            status.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            var readDto = new StatusReadDto
            {
                Id = status.Id,
                Name = status.Name
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un statut (soft delete)
        /// </summary>
        /// <param name="id">ID du statut à supprimer</param>
        /// <returns>Confirmation de suppression</returns>
        [HttpDelete("{id}")]
        [HasPermission("DELETE_STATUSES")]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _db.Statuses
                .Where(s => s.Id == id && s.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (status == null)
            {
                return NotFound(new { Message = "Statut non trouvé" });
            }

            // Vérifier si le statut est utilisé par des employés
            var isUsedByEmployees = await _db.Employees
                .AnyAsync(e => e.StatusId == id && e.DeletedAt == null);

            if (isUsedByEmployees)
            {
                return BadRequest(new
                {
                    Message = "Impossible de supprimer ce statut car il est utilisé par des employés. Veuillez d'abord réassigner les employés à un autre statut."
                });
            }

            var userId = User.GetUserId();
            status.DeletedAt = DateTimeOffset.UtcNow;
            status.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}