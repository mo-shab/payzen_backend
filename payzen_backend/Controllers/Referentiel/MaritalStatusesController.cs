using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Referentiel;
using payzen_backend.Models.Referentiel.Dtos;
using static payzen_backend.Models.Permissions.PermissionsConstants;

namespace payzen_backend.Controllers.Referentiel
{
    [Route("api/marital-statuses")]
    [ApiController]
    [Authorize]
    public class MaritalStatusesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MaritalStatusesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère tous les statuts maritaux actifs
        /// </summary>
        /// <returns>Liste des statuts maritaux</returns>
        [HttpGet]
        //[HasPermission(READ_MARITAL_STATUSES)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<MaritalStatusReadDto>>> GetAll()
        {
            var maritalStatuses = await _db.MaritalStatuses
                .AsNoTracking()
                .Where(ms => ms.DeletedAt == null)
                .OrderBy(ms => ms.Name)
                .Select(ms => new MaritalStatusReadDto
                {
                    Id = ms.Id,
                    Name = ms.Name
                })
                .ToListAsync();

            return Ok(maritalStatuses);
        }

        /// <summary>
        /// Récupère un statut marital par son ID
        /// </summary>
        /// <param name="id">ID du statut marital</param>
        /// <returns>Détails du statut marital</returns>
        [HttpGet("{id}")]
        //[HasPermission(READ_MARITAL_STATUSES)]
        [Produces("application/json")]
        public async Task<ActionResult<MaritalStatusReadDto>> GetById(int id)
        {
            var maritalStatus = await _db.MaritalStatuses
                .AsNoTracking()
                .Where(ms => ms.Id == id && ms.DeletedAt == null)
                .Select(ms => new MaritalStatusReadDto
                {
                    Id = ms.Id,
                    Name = ms.Name
                })
                .FirstOrDefaultAsync();

            if (maritalStatus == null)
            {
                return NotFound(new { Message = "Statut marital non trouvé" });
            }

            return Ok(maritalStatus);
        }

        /// <summary>
        /// Crée un nouveau statut marital
        /// </summary>
        /// <param name="dto">Données du statut marital à créer</param>
        /// <returns>Statut marital créé</returns>
        [HttpPost]
        //[HasPermission(CREATE_MARITAL_STATUSES)]
        [Produces("application/json")]
        public async Task<ActionResult<MaritalStatusReadDto>> Create([FromBody] MaritalStatusCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que le nom n'existe pas déjà
            var existingMaritalStatus = await _db.MaritalStatuses
                .AnyAsync(ms => ms.Name == dto.Name && ms.DeletedAt == null);

            if (existingMaritalStatus)
            {
                return Conflict(new { Message = "Un statut marital avec ce nom existe déjà" });
            }

            var userId = User.GetUserId();

            var maritalStatus = new MaritalStatus
            {
                Name = dto.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.MaritalStatuses.Add(maritalStatus);
            await _db.SaveChangesAsync();

            var readDto = new MaritalStatusReadDto
            {
                Id = maritalStatus.Id,
                Name = maritalStatus.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = maritalStatus.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un statut marital existant
        /// </summary>
        /// <param name="id">ID du statut marital à modifier</param>
        /// <param name="dto">Nouvelles données du statut marital</param>
        /// <returns>Statut marital mis à jour</returns>
        [HttpPut("{id}")]
        //[HasPermission(UPDATE_MARITAL_STATUSES)]
        [Produces("application/json")]
        public async Task<ActionResult<MaritalStatusReadDto>> Update(int id, [FromBody] MaritalStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var maritalStatus = await _db.MaritalStatuses
                .Where(ms => ms.Id == id && ms.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (maritalStatus == null)
            {
                return NotFound(new { Message = "Statut marital non trouvé" });
            }

            // Si le nom est modifié, vérifier qu'il n'existe pas déjà
            if (dto.Name != null && dto.Name != maritalStatus.Name)
            {
                var existingMaritalStatus = await _db.MaritalStatuses
                    .AnyAsync(ms => ms.Name == dto.Name && ms.Id != id && ms.DeletedAt == null);

                if (existingMaritalStatus)
                {
                    return Conflict(new { Message = "Un statut marital avec ce nom existe déjà" });
                }

                maritalStatus.Name = dto.Name;
            }

            var userId = User.GetUserId();
            maritalStatus.ModifiedAt = DateTimeOffset.UtcNow;
            maritalStatus.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            var readDto = new MaritalStatusReadDto
            {
                Id = maritalStatus.Id,
                Name = maritalStatus.Name
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un statut marital (soft delete)
        /// </summary>
        /// <param name="id">ID du statut marital à supprimer</param>
        /// <returns>No content si succès</returns>
        [HttpDelete("{id}")]
        //[HasPermission(DELETE_MARITAL_STATUSES)]
        public async Task<IActionResult> Delete(int id)
        {
            var maritalStatus = await _db.MaritalStatuses
                .Where(ms => ms.Id == id && ms.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (maritalStatus == null)
            {
                return NotFound(new { Message = "Statut marital non trouvé" });
            }

            // Vérifier si des employés utilisent ce statut marital
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.MaritalStatusId == id && e.DeletedAt == null);

            if (hasEmployees)
            {
                return BadRequest(new { Message = "Impossible de supprimer ce statut marital car il est utilisé par des employés" });
            }

            var userId = User.GetUserId();
            maritalStatus.DeletedAt = DateTimeOffset.UtcNow;
            maritalStatus.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}