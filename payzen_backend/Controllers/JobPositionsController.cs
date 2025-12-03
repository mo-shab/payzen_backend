using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Company;
using payzen_backend.Models.Company.Dtos;

namespace payzen_backend.Controllers
{
    [Route("api/job-positions")]
    [ApiController]
    [Authorize]
    public class JobPositionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public JobPositionsController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les postes actifs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobPositionReadDto>>> GetAll()
        {
            var jobPositions = await _db.JobPositions
                .AsNoTracking()
                .Where(jp => jp.DeletedAt == null)
                .Include(jp => jp.Company)
                .OrderBy(jp => jp.Name)
                .ToListAsync();

            var result = jobPositions.Select(jp => new JobPositionReadDto
            {
                Id = jp.Id,
                Name = jp.Name,
                CompanyId = jp.CompanyId,
                CompanyName = jp.Company?.CompanyName,
                CreatedAt = jp.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un poste par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobPositionReadDto>> GetById(int id)
        {
            var jobPosition = await _db.JobPositions
                .AsNoTracking()
                .Where(jp => jp.DeletedAt == null)
                .Include(jp => jp.Company)
                .FirstOrDefaultAsync(jp => jp.Id == id);

            if (jobPosition == null)
                return NotFound(new { Message = "Poste non trouvé" });

            var result = new JobPositionReadDto
            {
                Id = jobPosition.Id,
                Name = jobPosition.Name,
                CompanyId = jobPosition.CompanyId,
                CompanyName = jobPosition.Company?.CompanyName,
                CreatedAt = jobPosition.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les postes d'une société
        /// </summary>
        [HttpGet("by-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<JobPositionReadDto>>> GetByCompany(int companyId)
        {
            var companyExists = await _db.Companies
                .AnyAsync(c => c.Id == companyId && c.DeletedAt == null);

            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            var jobPositions = await _db.JobPositions
                .AsNoTracking()
                .Where(jp => jp.CompanyId == companyId && jp.DeletedAt == null)
                .OrderBy(jp => jp.Name)
                .ToListAsync();

            var result = jobPositions.Select(jp => new JobPositionReadDto
            {
                Id = jp.Id,
                Name = jp.Name,
                CompanyId = jp.CompanyId,
                CreatedAt = jp.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouveau poste
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<JobPositionReadDto>> Create([FromBody] JobPositionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Vérifier que la société existe
            var companyExists = await _db.Companies
                .AnyAsync(c => c.Id == dto.CompanyId && c.DeletedAt == null);

            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            // Vérifier que le nom du poste n'existe pas déjà pour cette société
            var nameExists = await _db.JobPositions
                .AnyAsync(jp => jp.Name == dto.Name && 
                               jp.CompanyId == dto.CompanyId && 
                               jp.DeletedAt == null);

            if (nameExists)
                return Conflict(new { Message = "Un poste avec ce nom existe déjà pour cette société" });

            var jobPosition = new JobPosition
            {
                Name = dto.Name,
                CompanyId = dto.CompanyId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.JobPositions.Add(jobPosition);
            await _db.SaveChangesAsync();

            // Récupérer le poste créé avec ses relations
            var createdJobPosition = await _db.JobPositions
                .AsNoTracking()
                .Include(jp => jp.Company)
                .FirstAsync(jp => jp.Id == jobPosition.Id);

            var readDto = new JobPositionReadDto
            {
                Id = createdJobPosition.Id,
                Name = createdJobPosition.Name,
                CompanyId = createdJobPosition.CompanyId,
                CompanyName = createdJobPosition.Company?.CompanyName,
                CreatedAt = createdJobPosition.CreatedAt.DateTime
            };

            return CreatedAtAction(nameof(GetById), new { id = jobPosition.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un poste
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<JobPositionReadDto>> Update(int id, [FromBody] JobPositionUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            var jobPosition = await _db.JobPositions
                .Where(jp => jp.Id == id && jp.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (jobPosition == null)
                return NotFound(new { Message = "Poste non trouvé" });

            // Mettre à jour le nom si fourni
            if (dto.Name != null && dto.Name != jobPosition.Name)
            {
                // Vérifier que le nouveau nom n'existe pas déjà pour cette société
                var nameExists = await _db.JobPositions
                    .AnyAsync(jp => jp.Name == dto.Name && 
                                   jp.CompanyId == jobPosition.CompanyId && 
                                   jp.Id != id && 
                                   jp.DeletedAt == null);

                if (nameExists)
                    return Conflict(new { Message = "Un poste avec ce nom existe déjà pour cette société" });

                jobPosition.Name = dto.Name;
            }

            jobPosition.ModifiedAt = DateTimeOffset.UtcNow;
            jobPosition.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            // Récupérer le poste mis à jour avec ses relations
            var updatedJobPosition = await _db.JobPositions
                .AsNoTracking()
                .Include(jp => jp.Company)
                .FirstAsync(jp => jp.Id == id);

            var readDto = new JobPositionReadDto
            {
                Id = updatedJobPosition.Id,
                Name = updatedJobPosition.Name,
                CompanyId = updatedJobPosition.CompanyId,
                CompanyName = updatedJobPosition.Company?.CompanyName,
                CreatedAt = updatedJobPosition.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un poste (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var jobPosition = await _db.JobPositions
                .Where(jp => jp.Id == id && jp.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (jobPosition == null)
                return NotFound(new { Message = "Poste non trouvé" });

            // Vérifier si le poste est utilisé dans des contrats d'employés
            var hasEmployeeContracts = await _db.EmployeeContracts
                .AnyAsync(ec => ec.JobPositionId == id && ec.DeletedAt == null);

            if (hasEmployeeContracts)
                return BadRequest(new { Message = "Impossible de supprimer ce poste car il est utilisé dans des contrats d'employés" });

            // Soft delete
            jobPosition.DeletedAt = DateTimeOffset.UtcNow;
            jobPosition.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}