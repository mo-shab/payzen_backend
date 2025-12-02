using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Company;
using payzen_backend.Models.Company.Dtos;
using payzen_backend.Authorization;
using payzen_backend.Extensions;

namespace payzen_backend.Controllers
{
    [Route("api/departements")]
    [ApiController]
    [Authorize]
    public class DepartementController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DepartementController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les départements actifs
        /// </summary>
        [HttpGet]
        //[HasPermission("READ_DEPARTEMENTS")]
        public async Task<ActionResult<IEnumerable<DepartementReadDto>>> ReadAll()
        {
            var departements = await _db.Departement
                .AsNoTracking()
                .Where(d => d.DeletedAt == null)
                .Include(d => d.Company)
                .OrderBy(d => d.DepartementName)
                .ToListAsync();

            var result = departements.Select(d => new DepartementReadDto
            {
                Id = d.Id,
                DepartementName = d.DepartementName,
                CompanyId = d.CompanyId,
                CompanyName = d.Company?.CompanyName ?? "",
                CreatedAt = d.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un département par ID
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission("VIEW_DEPARTEMENTS")]
        public async Task<ActionResult<DepartementReadDto>> ReadById(int id)
        {
            var departement = await _db.Departement
                .AsNoTracking()
                .Where(d => d.DeletedAt == null)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (departement == null)
                return NotFound(new { Message = "Département non trouvé" });

            var result = new DepartementReadDto
            {
                Id = departement.Id,
                DepartementName = departement.DepartementName,
                CompanyId = departement.CompanyId,
                CompanyName = departement.Company?.CompanyName ?? "",
                CreatedAt = departement.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les départements d'une société
        /// </summary>
        [HttpGet("company/{companyId}")]
        [HasPermission("READ_DEPARTEMENTS")]
        public async Task<ActionResult<IEnumerable<DepartementReadDto>>> GetByCompanyId(int companyId)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            var departements = await _db.Departement
                .AsNoTracking()
                .Where(d => d.CompanyId == companyId && d.DeletedAt == null)
                .Include(d => d.Company)
                .OrderBy(d => d.DepartementName)
                .ToListAsync();

            var result = departements.Select(d => new DepartementReadDto
            {
                Id = d.Id,
                DepartementName = d.DepartementName,
                CompanyId = d.CompanyId,
                CompanyName = d.Company?.CompanyName ?? "",
                CreatedAt = d.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouveau département
        /// </summary>
        [HttpPost]
        [HasPermission("CREATE_DEPARTEMENTS")]
        public async Task<ActionResult<DepartementReadDto>> Create([FromBody] DepartementCreateDto departementDto)
        {
            // Validation du modèle
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    Message = "Données invalides",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Vérifier que la company existe
            var companyExists = await _db.Companies
                .AsNoTracking()
                .AnyAsync(c => c.Id == departementDto.CompanyId && c.DeletedAt == null);

            if (!companyExists)
            {
                return BadRequest(new { Message = "La société spécifiée n'existe pas" });
            }

            // Vérifier qu'un département avec le même nom n'existe pas déjà pour cette société
            var departementExists = await _db.Departement
                .AsNoTracking()
                .AnyAsync(d => d.CompanyId == departementDto.CompanyId 
                            && d.DepartementName == departementDto.DepartementName 
                            && d.DeletedAt == null);

            if (departementExists)
            {
                return Conflict(new { Message = "Un département avec ce nom existe déjà dans cette société" });
            }

            try
            {
                var departement = new Departement
                {
                    DepartementName = departementDto.DepartementName,
                    CompanyId = departementDto.CompanyId,
                    CreatedBy = User.GetUserId(),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _db.Departement.Add(departement);
                await _db.SaveChangesAsync();

                var result = new DepartementReadDto
                {
                    Id = departement.Id,
                    DepartementName = departement.DepartementName,
                    CompanyId = departement.CompanyId,
                    CreatedAt = departement.CreatedAt.DateTime
                };

                return CreatedAtAction(nameof(ReadById), new { id = departement.Id }, result);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la création du département", Details = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour un département
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission("UPDATE_DEPARTEMENTS")]
        public async Task<ActionResult<DepartementReadDto>> Update(int id, [FromBody] DepartementUpdateDto departementDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    Message = "Données invalides",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userId = User.GetUserId();

            // Récupérer le département à modifier
            var departement = await _db.Departement
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);

            if (departement == null)
                return NotFound(new { Message = "Département non trouvé" });

            // Mettre à jour le nom si fourni
            if (departementDto.DepartementName != null && departementDto.DepartementName != departement.DepartementName)
            {
                // Vérifier qu'un département avec ce nom n'existe pas déjà dans la même société
                var nameExists = await _db.Departement
                    .AsNoTracking()
                    .AnyAsync(d => d.CompanyId == departement.CompanyId 
                                && d.DepartementName == departementDto.DepartementName 
                                && d.Id != id 
                                && d.DeletedAt == null);

                if (nameExists)
                {
                    return Conflict(new { Message = "Un département avec ce nom existe déjà dans cette société" });
                }

                departement.DepartementName = departementDto.DepartementName;
            }

            // Mettre à jour la société si fournie
            if (departementDto.CompanyId.HasValue && departementDto.CompanyId.Value != departement.CompanyId)
            {
                // Vérifier que la nouvelle société existe
                var companyExists = await _db.Companies
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == departementDto.CompanyId.Value && c.DeletedAt == null);

                if (!companyExists)
                {
                    return BadRequest(new { Message = "La société spécifiée n'existe pas" });
                }

                // Vérifier si le département a des employés
                var hasEmployees = await _db.Employees
                    .AnyAsync(e => e.DepartementId == id && e.DeletedAt == null);

                if (hasEmployees)
                {
                    return BadRequest(new { Message = "Impossible de changer la société car le département contient des employés" });
                }

                // Vérifier qu'un département avec le même nom n'existe pas dans la nouvelle société
                var nameExistsInNewCompany = await _db.Departement
                    .AsNoTracking()
                    .AnyAsync(d => d.CompanyId == departementDto.CompanyId.Value 
                                && d.DepartementName == departement.DepartementName 
                                && d.Id != id 
                                && d.DeletedAt == null);

                if (nameExistsInNewCompany)
                {
                    return Conflict(new { Message = "Un département avec ce nom existe déjà dans la société cible" });
                }

                departement.CompanyId = departementDto.CompanyId.Value;
            }

            departement.UpdatedAt = DateTimeOffset.UtcNow;
            departement.UpdatedBy = userId;

            try
            {
                await _db.SaveChangesAsync();

                // Récupérer le département mis à jour avec ses relations
                var updatedDepartement = await _db.Departement
                    .AsNoTracking()
                    .Include(d => d.Company)
                    .FirstAsync(d => d.Id == id);

                var result = new DepartementReadDto
                {
                    Id = updatedDepartement.Id,
                    DepartementName = updatedDepartement.DepartementName,
                    CompanyId = updatedDepartement.CompanyId,
                    CompanyName = updatedDepartement.Company?.CompanyName ?? "",
                    CreatedAt = updatedDepartement.CreatedAt.DateTime,
                    UpdatedAt = updatedDepartement.UpdatedAt?.DateTime
                };

                return Ok(result);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la mise à jour du département", Details = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un département (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [HasPermission("DELETE_DEPARTEMENTS")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var departement = await _db.Departement
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);

            if (departement == null)
                return NotFound(new { Message = "Département non trouvé" });

            // Vérifier si le département contient des employés actifs
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.DepartementId == id && e.DeletedAt == null);

            if (hasEmployees)
            {
                return BadRequest(new { Message = "Impossible de supprimer ce département car il contient des employés actifs" });
            }

            try
            {
                // Soft delete
                departement.DeletedAt = DateTimeOffset.UtcNow;
                departement.DeletedBy = userId;

                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { Message = "Erreur lors de la suppression du département", Details = ex.Message });
            }
        }
    }
}