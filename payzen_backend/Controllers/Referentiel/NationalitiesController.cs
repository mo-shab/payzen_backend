using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Referentiel;
using payzen_backend.Models.Referentiel.Dtos;
using static payzen_backend.Models.Permissions.PermissionsConstants;

namespace payzen_backend.Controllers.Referentiel
{
    [Route("api/nationalities")]
    [ApiController]
    [Authorize]
    public class NationalitiesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NationalitiesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère toutes les nationalités actives
        /// </summary>
        /// <returns>Liste des nationalités</returns>
        [HttpGet]
        //[HasPermission(READ_NATIONALITIES)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<NationalityReadDto>>> GetAll()
        {
            var nationalities = await _db.Nationalities
                .AsNoTracking()
                .Where(n => n.DeletedAt == null)
                .OrderBy(n => n.Name)
                .Select(n => new NationalityReadDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Ok(nationalities);
        }

        /// <summary>
        /// Récupère une nationalité par son ID
        /// </summary>
        /// <param name="id">ID de la nationalité</param>
        /// <returns>Détails de la nationalité</returns>
        [HttpGet("{id}")]
        //[HasPermission(READ_NATIONALITIES)]
        [Produces("application/json")]
        public async Task<ActionResult<NationalityReadDto>> GetById(int id)
        {
            var nationality = await _db.Nationalities
                .AsNoTracking()
                .Where(n => n.Id == id && n.DeletedAt == null)
                .Select(n => new NationalityReadDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .FirstOrDefaultAsync();

            if (nationality == null)
            {
                return NotFound(new { Message = "Nationalité non trouvée" });
            }

            return Ok(nationality);
        }

        /// <summary>
        /// Crée une nouvelle nationalité
        /// </summary>
        /// <param name="dto">Données de la nationalité à créer</param>
        /// <returns>Nationalité créée</returns>
        [HttpPost]
        //[HasPermission(CREATE_NATIONALITIES)]
        [Produces("application/json")]
        public async Task<ActionResult<NationalityReadDto>> Create([FromBody] NationalityCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que le nom n'existe pas déjà
            var existingNationality = await _db.Nationalities
                .AnyAsync(n => n.Name == dto.Name && n.DeletedAt == null);

            if (existingNationality)
            {
                return Conflict(new { Message = "Une nationalité avec ce nom existe déjà" });
            }

            var userId = User.GetUserId();

            var nationality = new Nationality
            {
                Name = dto.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Nationalities.Add(nationality);
            await _db.SaveChangesAsync();

            var readDto = new NationalityReadDto
            {
                Id = nationality.Id,
                Name = nationality.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = nationality.Id }, readDto);
        }

        /// <summary>
        /// Met à jour une nationalité existante
        /// </summary>
        /// <param name="id">ID de la nationalité à modifier</param>
        /// <param name="dto">Nouvelles données de la nationalité</param>
        /// <returns>Nationalité mise à jour</returns>
        [HttpPut("{id}")]
        //[HasPermission(UPDATE_NATIONALITIES)]
        [Produces("application/json")]
        public async Task<ActionResult<NationalityReadDto>> Update(int id, [FromBody] NationalityUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nationality = await _db.Nationalities
                .Where(n => n.Id == id && n.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (nationality == null)
            {
                return NotFound(new { Message = "Nationalité non trouvée" });
            }

            // Si le nom est modifié, vérifier qu'il n'existe pas déjà
            if (dto.Name != null && dto.Name != nationality.Name)
            {
                var existingNationality = await _db.Nationalities
                    .AnyAsync(n => n.Name == dto.Name && n.Id != id && n.DeletedAt == null);

                if (existingNationality)
                {
                    return Conflict(new { Message = "Une nationalité avec ce nom existe déjà" });
                }

                nationality.Name = dto.Name;
            }

            var userId = User.GetUserId();
            nationality.ModifiedAt = DateTimeOffset.UtcNow;
            nationality.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            var readDto = new NationalityReadDto
            {
                Id = nationality.Id,
                Name = nationality.Name
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime une nationalité (soft delete)
        /// </summary>
        /// <param name="id">ID de la nationalité à supprimer</param>
        /// <returns>No content si succès</returns>
        [HttpDelete("{id}")]
        //[HasPermission(DELETE_NATIONALITIES)]
        public async Task<IActionResult> Delete(int id)
        {
            var nationality = await _db.Nationalities
                .Where(n => n.Id == id && n.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (nationality == null)
            {
                return NotFound(new { Message = "Nationalité non trouvée" });
            }

            // Vérifier si des employés utilisent cette nationalité
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.NationalityId == id && e.DeletedAt == null);

            if (hasEmployees)
            {
                return BadRequest(new { Message = "Impossible de supprimer cette nationalité car elle est utilisée par des employés" });
            }

            var userId = User.GetUserId();
            nationality.DeletedAt = DateTimeOffset.UtcNow;
            nationality.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}