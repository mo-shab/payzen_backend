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
    [Route("api/holidays")]
    [ApiController]
    [Authorize]
    public class HolidaysController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HolidaysController(AppDbContext db) => _db = db;

        /// <summary>
        /// Récupère tous les jours fériés actifs
        /// </summary>
        [HttpGet]
        [HasPermission("READ_HOLIDAYS")]
        public async Task<ActionResult<IEnumerable<HolidayReadDto>>> GetAll()
        {
            var holidays = await _db.Holidays
                .AsNoTracking()
                .Where(h => h.DeletedAt == null)
                .Include(h => h.Company)
                .Include(h => h.Country)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            var result = holidays.Select(h => new HolidayReadDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company?.CompanyName ?? "",
                CountryId = h.CountryId,
                CountryName = h.Country?.CountryName ?? "",
                HolidayDate = h.HolidayDate,
                Name = h.Name,
                IsFixedAnnually = h.IsFixedAnnually,
                CreatedAt = h.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un jour férié par ID
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission("VIEW_HOLIDAY")]
        public async Task<ActionResult<HolidayReadDto>> GetById(int id)
        {
            var holiday = await _db.Holidays
                .AsNoTracking()
                .Where(h => h.DeletedAt == null)
                .Include(h => h.Company)
                .Include(h => h.Country)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (holiday == null)
                return NotFound(new { Message = "Jour férié non trouvé" });

            var result = new HolidayReadDto
            {
                Id = holiday.Id,
                CompanyId = holiday.CompanyId,
                CompanyName = holiday.Company?.CompanyName ?? "",
                CountryId = holiday.CountryId,
                CountryName = holiday.Country?.CountryName ?? "",
                HolidayDate = holiday.HolidayDate,
                Name = holiday.Name,
                IsFixedAnnually = holiday.IsFixedAnnually,
                CreatedAt = holiday.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les jours fériés d'une société
        /// </summary>
        [HttpGet("company/{companyId}")]
        [HasPermission("READ_HOLIDAYS")]
        public async Task<ActionResult<IEnumerable<HolidayReadDto>>> GetByCompanyId(int companyId)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            var holidays = await _db.Holidays
                .AsNoTracking()
                .Where(h => h.CompanyId == companyId && h.DeletedAt == null)
                .Include(h => h.Company)
                .Include(h => h.Country)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            var result = holidays.Select(h => new HolidayReadDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company?.CompanyName ?? "",
                CountryId = h.CountryId,
                CountryName = h.Country?.CountryName ?? "",
                HolidayDate = h.HolidayDate,
                Name = h.Name,
                IsFixedAnnually = h.IsFixedAnnually,
                CreatedAt = h.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les jours fériés d'un pays
        /// </summary>
        [HttpGet("country/{countryId}")]
        [HasPermission("READ_HOLIDAYS")]
        public async Task<ActionResult<IEnumerable<HolidayReadDto>>> GetByCountryId(int countryId)
        {
            var countryExists = await _db.Countries.AnyAsync(c => c.Id == countryId && c.DeletedAt == null);
            if (!countryExists)
                return NotFound(new { Message = "Pays non trouvé" });

            var holidays = await _db.Holidays
                .AsNoTracking()
                .Where(h => h.CountryId == countryId && h.DeletedAt == null)
                .Include(h => h.Company)
                .Include(h => h.Country)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            var result = holidays.Select(h => new HolidayReadDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company?.CompanyName ?? "",
                CountryId = h.CountryId,
                CountryName = h.Country?.CountryName ?? "",
                HolidayDate = h.HolidayDate,
                Name = h.Name,
                IsFixedAnnually = h.IsFixedAnnually,
                CreatedAt = h.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère les jours fériés d'une société pour une année donnée
        /// </summary>
        [HttpGet("company/{companyId}/year/{year}")]
        [HasPermission("READ_HOLIDAYS")]
        public async Task<ActionResult<IEnumerable<HolidayReadDto>>> GetByCompanyAndYear(int companyId, int year)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var holidays = await _db.Holidays
                .AsNoTracking()
                .Where(h => h.CompanyId == companyId 
                         && h.HolidayDate >= startDate 
                         && h.HolidayDate <= endDate 
                         && h.DeletedAt == null)
                .Include(h => h.Company)
                .Include(h => h.Country)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            var result = holidays.Select(h => new HolidayReadDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company?.CompanyName ?? "",
                CountryId = h.CountryId,
                CountryName = h.Country?.CountryName ?? "",
                HolidayDate = h.HolidayDate,
                Name = h.Name,
                IsFixedAnnually = h.IsFixedAnnually,
                CreatedAt = h.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouveau jour férié
        /// </summary>
        [HttpPost]
        [HasPermission("CREATE_HOLIDAY")]
        public async Task<ActionResult<HolidayReadDto>> Create([FromBody] HolidayCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new 
                { 
                    Message = "Données invalides",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            // Vérifier que la société existe
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            // Vérifier que le pays existe
            var countryExists = await _db.Countries.AnyAsync(c => c.Id == dto.CountryId && c.DeletedAt == null);
            if (!countryExists)
                return NotFound(new { Message = "Pays non trouvé" });

            // Vérifier qu'un jour férié avec la même date n'existe pas déjà pour cette société
            var holidayExists = await _db.Holidays
                .AnyAsync(h => h.CompanyId == dto.CompanyId 
                            && h.HolidayDate.Date == dto.HolidayDate.Date 
                            && h.DeletedAt == null);

            if (holidayExists)
                return Conflict(new { Message = "Un jour férié existe déjà à cette date pour cette société" });

            var holiday = new Holiday
            {
                CompanyId = dto.CompanyId,
                CountryId = dto.CountryId,
                HolidayDate = dto.HolidayDate.Date,
                Name = dto.Name,
                IsFixedAnnually = dto.IsFixedAnnually,
                CreatedBy = User.GetUserId(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Holidays.Add(holiday);
            await _db.SaveChangesAsync();

            var createdHoliday = await _db.Holidays
                .AsNoTracking()
                .Include(h => h.Company)
                .Include(h => h.Country)
                .FirstAsync(h => h.Id == holiday.Id);

            var result = new HolidayReadDto
            {
                Id = createdHoliday.Id,
                CompanyId = createdHoliday.CompanyId,
                CompanyName = createdHoliday.Company?.CompanyName ?? "",
                CountryId = createdHoliday.CountryId,
                CountryName = createdHoliday.Country?.CountryName ?? "",
                HolidayDate = createdHoliday.HolidayDate,
                Name = createdHoliday.Name,
                IsFixedAnnually = createdHoliday.IsFixedAnnually,
                CreatedAt = createdHoliday.CreatedAt.DateTime
            };

            return CreatedAtAction(nameof(GetById), new { id = holiday.Id }, result);
        }

        /// <summary>
        /// Met à jour un jour férié
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission("UPDATE_HOLIDAY")]
        public async Task<ActionResult<HolidayReadDto>> Update(int id, [FromBody] HolidayUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new 
                { 
                    Message = "Données invalides",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var holiday = await _db.Holidays.FirstOrDefaultAsync(h => h.Id == id && h.DeletedAt == null);
            if (holiday == null)
                return NotFound(new { Message = "Jour férié non trouvé" });

            if (dto.CompanyId.HasValue && dto.CompanyId.Value != holiday.CompanyId)
            {
                var companyExists = await _db.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value && c.DeletedAt == null);
                if (!companyExists)
                    return NotFound(new { Message = "Société non trouvée" });
                
                holiday.CompanyId = dto.CompanyId.Value;
            }

            if (dto.CountryId.HasValue && dto.CountryId.Value != holiday.CountryId)
            {
                var countryExists = await _db.Countries.AnyAsync(c => c.Id == dto.CountryId.Value && c.DeletedAt == null);
                if (!countryExists)
                    return NotFound(new { Message = "Pays non trouvé" });
                
                holiday.CountryId = dto.CountryId.Value;
            }

            if (dto.HolidayDate.HasValue && dto.HolidayDate.Value.Date != holiday.HolidayDate.Date)
            {
                var currentCompanyId = dto.CompanyId ?? holiday.CompanyId;
                var dateExists = await _db.Holidays
                    .AnyAsync(h => h.CompanyId == currentCompanyId 
                                && h.HolidayDate.Date == dto.HolidayDate.Value.Date 
                                && h.Id != id 
                                && h.DeletedAt == null);

                if (dateExists)
                    return Conflict(new { Message = "Un jour férié existe déjà à cette date pour cette société" });
                
                holiday.HolidayDate = dto.HolidayDate.Value.Date;
            }

            if (dto.Name != null)
                holiday.Name = dto.Name;

            if (dto.IsFixedAnnually.HasValue)
                holiday.IsFixedAnnually = dto.IsFixedAnnually.Value;

            holiday.ModifiedAt = DateTimeOffset.UtcNow;
            holiday.ModifiedBy = User.GetUserId();

            await _db.SaveChangesAsync();

            var updatedHoliday = await _db.Holidays
                .AsNoTracking()
                .Include(h => h.Company)
                .Include(h => h.Country)
                .FirstAsync(h => h.Id == id);

            var result = new HolidayReadDto
            {
                Id = updatedHoliday.Id,
                CompanyId = updatedHoliday.CompanyId,
                CompanyName = updatedHoliday.Company?.CompanyName ?? "",
                CountryId = updatedHoliday.CountryId,
                CountryName = updatedHoliday.Country?.CountryName ?? "",
                HolidayDate = updatedHoliday.HolidayDate,
                Name = updatedHoliday.Name,
                IsFixedAnnually = updatedHoliday.IsFixedAnnually,
                CreatedAt = updatedHoliday.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Supprime un jour férié (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [HasPermission("DELETE_HOLIDAY")]
        public async Task<IActionResult> Delete(int id)
        {
            var holiday = await _db.Holidays.FirstOrDefaultAsync(h => h.Id == id && h.DeletedAt == null);
            if (holiday == null)
                return NotFound(new { Message = "Jour férié non trouvé" });

            holiday.DeletedAt = DateTimeOffset.UtcNow;
            holiday.DeletedBy = User.GetUserId();

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}