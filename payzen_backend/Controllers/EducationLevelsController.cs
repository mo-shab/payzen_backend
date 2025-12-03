using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Referentiel;
using payzen_backend.Models.Referentiel.Dtos;

namespace payzen_backend.Controllers
{
    [Route("api/education-levels")]
    [ApiController]
    [Authorize]
    public class EducationLevelsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EducationLevelsController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les niveaux d'éducation actifs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EducationLevelReadDto>>> GetAll()
        {
            var educationLevels = await _db.EducationLevels
                .AsNoTracking()
                .Where(el => el.DeletedAt == null)
                .OrderBy(el => el.Name)
                .ToListAsync();

            var result = educationLevels.Select(el => new EducationLevelReadDto
            {
                Id = el.Id,
                Name = el.Name,
                CreatedAt = el.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un niveau d'éducation par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EducationLevelReadDto>> GetById(int id)
        {
            var educationLevel = await _db.EducationLevels
                .AsNoTracking()
                .Where(el => el.DeletedAt == null)
                .FirstOrDefaultAsync(el => el.Id == id);

            if (educationLevel == null)
                return NotFound(new { Message = "Niveau d'éducation non trouvé" });

            var result = new EducationLevelReadDto
            {
                Id = educationLevel.Id,
                Name = educationLevel.Name,
                CreatedAt = educationLevel.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouveau niveau d'éducation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EducationLevelReadDto>> Create([FromBody] EducationLevelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Vérifier que le nom n'existe pas déjà
            var nameExists = await _db.EducationLevels
                .AnyAsync(el => el.Name == dto.Name && el.DeletedAt == null);

            if (nameExists)
                return Conflict(new { Message = "Un niveau d'éducation avec ce nom existe déjà" });

            var educationLevel = new EducationLevel
            {
                Name = dto.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.EducationLevels.Add(educationLevel);
            await _db.SaveChangesAsync();

            var readDto = new EducationLevelReadDto
            {
                Id = educationLevel.Id,
                Name = educationLevel.Name,
                CreatedAt = educationLevel.CreatedAt.DateTime
            };

            return CreatedAtAction(nameof(GetById), new { id = educationLevel.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un niveau d'éducation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<EducationLevelReadDto>> Update(int id, [FromBody] EducationLevelUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            var educationLevel = await _db.EducationLevels
                .Where(el => el.Id == id && el.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (educationLevel == null)
                return NotFound(new { Message = "Niveau d'éducation non trouvé" });

            // Mettre à jour le nom si fourni
            if (dto.Name != null && dto.Name != educationLevel.Name)
            {
                // Vérifier que le nouveau nom n'existe pas déjà
                var nameExists = await _db.EducationLevels
                    .AnyAsync(el => el.Name == dto.Name && el.Id != id && el.DeletedAt == null);

                if (nameExists)
                    return Conflict(new { Message = "Un niveau d'éducation avec ce nom existe déjà" });

                educationLevel.Name = dto.Name;
            }

            educationLevel.ModifiedAt = DateTimeOffset.UtcNow;
            educationLevel.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            var readDto = new EducationLevelReadDto
            {
                Id = educationLevel.Id,
                Name = educationLevel.Name,
                CreatedAt = educationLevel.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un niveau d'éducation (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var educationLevel = await _db.EducationLevels
                .Where(el => el.Id == id && el.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (educationLevel == null)
                return NotFound(new { Message = "Niveau d'éducation non trouvé" });

            // Vérifier si le niveau d'éducation est utilisé par des employés
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.EducationLevelId == id && e.DeletedAt == null);

            if (hasEmployees)
                return BadRequest(new { Message = "Impossible de supprimer ce niveau d'éducation car il est utilisé par des employés" });

            // Soft delete
            educationLevel.DeletedAt = DateTimeOffset.UtcNow;
            educationLevel.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}