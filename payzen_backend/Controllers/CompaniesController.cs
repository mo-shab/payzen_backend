using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Company;
using payzen_backend.Models.Company.Dtos;
using static payzen_backend.Models.Permissions.PermissionsConstants;

namespace payzen_backend.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CompaniesController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère toutes les sociétés actives
        /// </summary>
        [HttpGet]
        [HasPermission(READ_COMPANIES)]
        public async Task<ActionResult<IEnumerable<CompanyReadDto>>> GetAll()
        {
            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .Include(c => c.ManagedByCompany)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var result = companies.Select(c => new CompanyReadDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                CompanyAddress = c.CompanyAddress,
                CityId = c.CityId,
                CountryId = c.CountryId,
                IceNumber = c.IceNumber,
                CnssNumber = c.CnssNumber,
                IfNumber = c.IfNumber,
                RcNumber = c.RcNumber,
                RibNumber = c.RibNumber,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                ManagedByCompanyId = c.ManagedByCompanyId,
                ManagedByCompanyName = c.ManagedByCompany?.CompanyName,
                IsCabinetExpert = c.IsCabinetExpert,
                CreatedAt = c.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère une société par ID
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(VIEW_COMPANY)]
        public async Task<ActionResult<CompanyReadDto>> GetById(int id)
        {
            var company = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .Include(c => c.ManagedByCompany)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return NotFound(new { Message = "Société non trouvée" });

            var result = new CompanyReadDto
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                CompanyAddress = company.CompanyAddress,
                CityId = company.CityId,
                CountryId = company.CountryId,
                IceNumber = company.IceNumber,
                CnssNumber = company.CnssNumber,
                IfNumber = company.IfNumber,
                RcNumber = company.RcNumber,
                RibNumber = company.RibNumber,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                ManagedByCompanyId = company.ManagedByCompanyId,
                ManagedByCompanyName = company.ManagedByCompany?.CompanyName,
                IsCabinetExpert = company.IsCabinetExpert,
                CreatedAt = company.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère toutes les sociétés gérées par une société (expert-comptable)
        /// </summary>
        [HttpGet("managed-by/{managedByCompanyId}")]
        [HasPermission(VIEW_MANAGED_COMPANIES)]
        public async Task<ActionResult<IEnumerable<CompanyReadDto>>> GetManagedCompanies(int managedByCompanyId)
        {
            var managerCompanyExists = await _db.Companies.AnyAsync(c => c.Id == managedByCompanyId && c.DeletedAt == null);
            if (!managerCompanyExists)
                return NotFound(new { Message = "Société gérante non trouvée" });

            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.ManagedByCompanyId == managedByCompanyId && c.DeletedAt == null)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var result = companies.Select(c => new CompanyReadDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                CompanyAddress = c.CompanyAddress,
                CityId = c.CityId,
                CountryId = c.CountryId,
                IceNumber = c.IceNumber,
                CnssNumber = c.CnssNumber,
                IfNumber = c.IfNumber,
                RcNumber = c.RcNumber,
                RibNumber = c.RibNumber,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                ManagedByCompanyId = c.ManagedByCompanyId,
                IsCabinetExpert = c.IsCabinetExpert,
                CreatedAt = c.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère toutes les sociétés qui sont des cabinets d'experts
        /// </summary>
        [HttpGet("cabinets-experts")]
        [HasPermission(VIEW_CABINET_EXPERTS)]
        public async Task<ActionResult<IEnumerable<CompanyReadDto>>> GetCabinetsExperts()
        {
            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.IsCabinetExpert == true && c.DeletedAt == null)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var result = companies.Select(c => new CompanyReadDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                CompanyAddress = c.CompanyAddress,
                CityId = c.CityId,
                CountryId = c.CountryId,
                IceNumber = c.IceNumber,
                CnssNumber = c.CnssNumber,
                IfNumber = c.IfNumber,
                RcNumber = c.RcNumber,
                RibNumber = c.RibNumber,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                ManagedByCompanyId = c.ManagedByCompanyId,
                IsCabinetExpert = c.IsCabinetExpert,
                CreatedAt = c.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Crée une nouvelle société
        /// </summary>
        [HttpPost]
        [HasPermission(CREATE_COMPANY)]
        public async Task<ActionResult<CompanyReadDto>> Create([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Vérifier que le numéro ICE n'existe pas déjà
            if (await _db.Companies.AnyAsync(c => c.IceNumber == dto.IceNumber && c.DeletedAt == null))
                return Conflict(new { Message = "Une société avec ce numéro ICE existe déjà" });

            // Vérifier que l'email n'existe pas déjà
            if (await _db.Companies.AnyAsync(c => c.Email == dto.Email && c.DeletedAt == null))
                return Conflict(new { Message = "Une société avec cet email existe déjà" });

            // Vérifier que la société gérante existe (si fournie)
            if (dto.ManagedByCompanyId.HasValue)
            {
                var managerCompanyExists = await _db.Companies
                    .AnyAsync(c => c.Id == dto.ManagedByCompanyId && c.DeletedAt == null);
                
                if (!managerCompanyExists)
                    return NotFound(new { Message = "Société gérante non trouvée" });
            }

            var company = new Company
            {
                CompanyName = dto.CompanyName,
                CompanyAddress = dto.CompanyAddress,
                CityId = dto.CityId,
                CountryId = dto.CountryId,
                IceNumber = dto.IceNumber,
                CnssNumber = dto.CnssNumber,
                IfNumber = dto.IfNumber,
                RcNumber = dto.RcNumber,
                RibNumber = dto.RibNumber,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                ManagedByCompanyId = dto.ManagedByCompanyId,
                IsCabinetExpert = dto.IsCabinetExpert,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            // Récupérer la société créée avec ses relations
            var createdCompany = await _db.Companies
                .AsNoTracking()
                .Include(c => c.ManagedByCompany)
                .FirstAsync(c => c.Id == company.Id);

            var readDto = new CompanyReadDto
            {
                Id = createdCompany.Id,
                CompanyName = createdCompany.CompanyName,
                CompanyAddress = createdCompany.CompanyAddress,
                CityId = createdCompany.CityId,
                CountryId = createdCompany.CountryId,
                IceNumber = createdCompany.IceNumber,
                CnssNumber = createdCompany.CnssNumber,
                IfNumber = createdCompany.IfNumber,
                RcNumber = createdCompany.RcNumber,
                RibNumber = createdCompany.RibNumber,
                PhoneNumber = createdCompany.PhoneNumber,
                Email = createdCompany.Email,
                ManagedByCompanyId = createdCompany.ManagedByCompanyId,
                ManagedByCompanyName = createdCompany.ManagedByCompany?.CompanyName,
                IsCabinetExpert = createdCompany.IsCabinetExpert,
                CreatedAt = createdCompany.CreatedAt.DateTime
            };

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, readDto);
        }

        /// <summary>
        /// Met à jour une société
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(EDIT_COMPANY)]
        public async Task<ActionResult<CompanyReadDto>> Update(int id, [FromBody] CompanyUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            var company = await _db.Companies
                .Where(c => c.Id == id && c.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (company == null)
                return NotFound(new { Message = "Société non trouvée" });

            // Mettre à jour les champs si fournis
            if (dto.CompanyName != null)
                company.CompanyName = dto.CompanyName;

            if (dto.CompanyAddress != null)
                company.CompanyAddress = dto.CompanyAddress;

            if (dto.CityId.HasValue)
                company.CityId = dto.CityId;

            if (dto.CountryId.HasValue)
                company.CountryId = dto.CountryId;

            if (dto.IceNumber != null && dto.IceNumber != company.IceNumber)
            {
                if (await _db.Companies.AnyAsync(c => c.IceNumber == dto.IceNumber && c.Id != id && c.DeletedAt == null))
                    return Conflict(new { Message = "Une société avec ce numéro ICE existe déjà" });
                
                company.IceNumber = dto.IceNumber;
            }

            if (dto.CnssNumber != null)
                company.CnssNumber = dto.CnssNumber;

            if (dto.IfNumber != null)
                company.IfNumber = dto.IfNumber;

            if (dto.RcNumber != null)
                company.RcNumber = dto.RcNumber;

            if (dto.RibNumber != null)
                company.RibNumber = dto.RibNumber;

            if (dto.PhoneNumber.HasValue)
                company.PhoneNumber = dto.PhoneNumber.Value;

            if (dto.Email != null && dto.Email != company.Email)
            {
                if (await _db.Companies.AnyAsync(c => c.Email == dto.Email && c.Id != id && c.DeletedAt == null))
                    return Conflict(new { Message = "Une société avec cet email existe déjà" });
                
                company.Email = dto.Email;
            }

            if (dto.ManagedByCompanyId.HasValue)
            {
                if (dto.ManagedByCompanyId.Value == id)
                    return BadRequest(new { Message = "Une société ne peut pas se gérer elle-même" });

                var managerCompanyExists = await _db.Companies
                    .AnyAsync(c => c.Id == dto.ManagedByCompanyId && c.DeletedAt == null);
                
                if (!managerCompanyExists)
                    return NotFound(new { Message = "Société gérante non trouvée" });
                
                company.ManagedByCompanyId = dto.ManagedByCompanyId;
            }

            if (dto.IsCabinetExpert.HasValue)
                company.IsCabinetExpert = dto.IsCabinetExpert.Value;

            company.ModifiedAt = DateTimeOffset.UtcNow;
            company.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            // Récupérer la société mise à jour avec ses relations
            var updatedCompany = await _db.Companies
                .AsNoTracking()
                .Include(c => c.ManagedByCompany)
                .FirstAsync(c => c.Id == id);

            var readDto = new CompanyReadDto
            {
                Id = updatedCompany.Id,
                CompanyName = updatedCompany.CompanyName,
                CompanyAddress = updatedCompany.CompanyAddress,
                CityId = updatedCompany.CityId,
                CountryId = updatedCompany.CountryId,
                IceNumber = updatedCompany.IceNumber,
                CnssNumber = updatedCompany.CnssNumber,
                IfNumber = updatedCompany.IfNumber,
                RcNumber = updatedCompany.RcNumber,
                RibNumber = updatedCompany.RibNumber,
                PhoneNumber = updatedCompany.PhoneNumber,
                Email = updatedCompany.Email,
                ManagedByCompanyId = updatedCompany.ManagedByCompanyId,
                ManagedByCompanyName = updatedCompany.ManagedByCompany?.CompanyName,
                IsCabinetExpert = updatedCompany.IsCabinetExpert,
                CreatedAt = updatedCompany.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime une société (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [HasPermission(DELETE_COMPANY)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var company = await _db.Companies
                .Where(c => c.Id == id && c.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (company == null)
                return NotFound(new { Message = "Société non trouvée" });

            // Vérifier si la société a des employés
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.CompanyId == id && e.DeletedAt == null);

            if (hasEmployees)
                return BadRequest(new { Message = "Impossible de supprimer cette société car elle a des employés" });

            // Vérifier si la société gère d'autres sociétés
            var managesCompanies = await _db.Companies
                .AnyAsync(c => c.ManagedByCompanyId == id && c.DeletedAt == null);

            if (managesCompanies)
                return BadRequest(new { Message = "Impossible de supprimer cette société car elle gère d'autres sociétés" });

            // Soft delete
            company.DeletedAt = DateTimeOffset.UtcNow;
            company.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}