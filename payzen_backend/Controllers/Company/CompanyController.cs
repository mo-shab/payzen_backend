using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Company.Dtos;
using payzen_backend.Models.Employee;
using payzen_backend.Models.Referentiel;
using payzen_backend.Models.Users;
using payzen_backend.Services;
using payzen_backend.Extensions;
using payzen_backend.Authorization;

namespace payzen_backend.Controllers.Company
{
    [Route("api/companies")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordGeneratorService _passwordGenerator;

        public CompanyController(AppDbContext db, PasswordGeneratorService passwordGenerator)
        {
            _db = db;
            _passwordGenerator = passwordGenerator;
        }

        /// <summary>
        /// Récupère la liste de toutes les entreprises
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyListDto>>> GetAllCompanies()
        {
            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .Include(c => c.City)
                .Include(c => c.Country)
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    IsCabinetExpert = c.IsCabinetExpert,
                    Email = c.Email,
                    CountryPhoneCode = c.Country != null ? c.Country.CountryPhoneCode : null,
                    PhoneNumber = c.PhoneNumber,
                    CityName = c.City != null ? c.City.CityName : null,
                    CountryName = c.Country != null ? c.Country.CountryName : null,
                    CnssNumber = c.CnssNumber,
                    CreatedAt = c.CreatedAt.DateTime
                })
                .ToListAsync();

            return Ok(companies);
        }

        /// <summary>
        /// Récupère une entreprise par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyListDto>> GetCompanyById(int id)
        {
            var company = await _db.Companies
                .AsNoTracking()
                .Where(c => c.Id == id && c.DeletedAt == null)
                .Include(c => c.City)
                .Include(c => c.Country)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    IsCabinetExpert = c.IsCabinetExpert,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    CountryPhoneCode = c.Country != null ? c.Country.CountryPhoneCode : null,
                    CityName = c.City != null ? c.City.CityName : null,
                    CountryName = c.Country != null ? c.Country.CountryName : null,
                    CompanyAddress = c.CompanyAddress != null ? c.CompanyAddress : null,
                    IceNumber = c.IceNumber,
                    IfNumber = c.IfNumber,
                    RcNumber = c.RcNumber,
                    LegalForm = c.LegalForm,
                    FoundingDate = c.FoundingDate,
                    CnssNumber = c.CnssNumber,
                    
                    CreatedAt = c.CreatedAt.DateTime
                })
                .FirstOrDefaultAsync();

            if (company == null)
                return NotFound(new { Message = "Entreprise non trouvée" });

            return Ok(company);
        }

        /// <summary>
        /// Récupère les entreprises par ville
        /// </summary>
        [HttpGet("by-city/{cityId}")]
        public async Task<ActionResult<IEnumerable<CompanyListDto>>> GetCompaniesByCity(int cityId)
        {
            var cityExists = await _db.Cities.AnyAsync(c => c.Id == cityId && c.DeletedAt == null);
            if (!cityExists)
                return NotFound(new { Message = "Ville non trouvée" });

            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && c.CityId == cityId)
                .Include(c => c.City)
                .Include(c => c.Country)
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    IsCabinetExpert = c.IsCabinetExpert,
                    Email = c.Email,
                    CountryPhoneCode = c.Country != null ? c.Country.CountryPhoneCode : null,
                    CityName = c.City != null ? c.City.CityName : null,
                    CountryName = c.Country != null ? c.Country.CountryName : null,
                    CnssNumber = c.CnssNumber,
                    CreatedAt = c.CreatedAt.DateTime
                })
                .ToListAsync();

            return Ok(companies);
        }

        /// <summary>
        /// Récupère les entreprises par pays
        /// </summary>
        [HttpGet("by-country/{countryId}")]
        public async Task<ActionResult<IEnumerable<CompanyListDto>>> GetCompaniesByCountry(int countryId)
        {
            var countryExists = await _db.Countries.AnyAsync(c => c.Id == countryId && c.DeletedAt == null);
            if (!countryExists)
                return NotFound(new { Message = "Pays non trouvé" });

            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && c.CountryId == countryId)
                .Include(c => c.City)
                .Include(c => c.Country)
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    IsCabinetExpert = c.IsCabinetExpert,
                    Email = c.Email,
                    CountryPhoneCode = c.Country != null ? c.Country.CountryPhoneCode : null,
                    CityName = c.City != null ? c.City.CityName : null,
                    CountryName = c.Country != null ? c.Country.CountryName : null,
                    CnssNumber = c.CnssNumber,
                    CreatedAt = c.CreatedAt.DateTime
                })
                .ToListAsync();

            return Ok(companies);
        }

        /// <summary>
        /// Recherche d'entreprises par nom
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CompanyListDto>>> SearchCompanies([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { Message = "Le terme de recherche est requis" });

            var companies = await _db.Companies
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && 
                            (c.CompanyName.Contains(searchTerm) || 
                             c.Email.Contains(searchTerm) ||
                             c.CnssNumber.Contains(searchTerm)))
                .Include(c => c.City)
                .Include(c => c.Country)
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyListDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    IsCabinetExpert = c.IsCabinetExpert,
                    Email = c.Email,
                    CountryPhoneCode = c.Country != null ? c.Country.CountryPhoneCode : null,
                    CityName = c.City != null ? c.City.CityName : null,
                    CountryName = c.Country != null ? c.Country.CountryName : null,
                    CnssNumber = c.CnssNumber,
                    CreatedAt = c.CreatedAt.DateTime
                })
                .ToListAsync();

            return Ok(companies);
        }

        /// <summary>
        /// Récupère toutes les données nécessaires pour le formulaire de création d'entreprise
        /// </summary>
        [HttpGet("form-data")]
        public async Task<ActionResult<CompanyFormDataDto>> GetFormData()
        {
            var formData = new CompanyFormDataDto();

            // 1. Récupérer tous les pays
            formData.Countries = await _db.Countries
                .Where(c => c.DeletedAt == null)
                .OrderBy(c => c.CountryName)
                .Select(c => new CountryFormDto
                {
                    Id = c.Id,
                    CountryName = c.CountryName,
                    CountryNameAr = c.CountryNameAr,
                    CountryCode = c.CountryCode,
                    CountryPhoneCode = c.CountryPhoneCode
                })
                .ToListAsync();

            // 2. Récupérer toutes les villes avec leur pays
            formData.Cities = await _db.Cities
                .Where(c => c.DeletedAt == null)
                .Include(c => c.Country)
                .OrderBy(c => c.Country!.CountryName)
                .ThenBy(c => c.CityName)
                .Select(c => new CityFormDto
                {
                    Id = c.Id,
                    CityName = c.CityName,
                    CountryId = c.CountryId,
                    CountryName = c.Country != null ? c.Country.CountryName : null
                })
                .ToListAsync();

            return Ok(formData);
        }

        /// <summary>
        /// Crée une nouvelle entreprise avec son administrateur
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CompanyCreateResponseDto>> CreateCompany([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ===== VALIDATIONS PERSONNALISÉES =====

            // Validation ville
            if (!dto.CityId.HasValue && string.IsNullOrWhiteSpace(dto.CityName))
            {
                return BadRequest(new { Message = "Veuillez sélectionner une ville existante ou fournir le nom d'une nouvelle ville" });
            }

            if (dto.CityId.HasValue && !string.IsNullOrWhiteSpace(dto.CityName))
            {
                return BadRequest(new { Message = "Veuillez choisir entre une ville existante (CityId) ou une nouvelle ville (CityName), pas les deux" });
            }

            // Validation mot de passe admin
            if (!dto.GeneratePassword && string.IsNullOrWhiteSpace(dto.AdminPassword))
            {
                return BadRequest(new { Message = "Le mot de passe est requis si GeneratePassword est false" });
            }

            if (dto.GeneratePassword && !string.IsNullOrWhiteSpace(dto.AdminPassword))
            {
                return BadRequest(new { Message = "Ne fournissez pas de mot de passe si GeneratePassword est true" });
            }

            // ===== Récuprérer le Id du User courant =====
            var currentUserId = User.GetUserId();

            // ===== VÉRIFIER QUE LE PAYS EXISTE =====

            var country = await _db.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.CountryId && c.DeletedAt == null);

            if (country == null)
                return NotFound(new { Message = "Pays non trouvé" });

            // ===== GÉRER LA VILLE (Existante ou Nouvelle) =====

            int finalCityId;

            if (dto.CityId.HasValue)
            {
                var existingCity = await _db.Cities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.CityId.Value && c.DeletedAt == null);

                if (existingCity == null)
                    return NotFound(new { Message = "Ville non trouvée" });

                if (existingCity.CountryId != dto.CountryId)
                    return BadRequest(new { Message = "La ville sélectionnée n'appartient pas au pays choisi" });

                finalCityId = dto.CityId.Value;
            }
            else
            {
                var duplicateCity = await _db.Cities
                    .FirstOrDefaultAsync(c =>
                        c.CountryId == dto.CountryId &&
                        c.CityName.ToLower() == dto.CityName!.ToLower() &&
                        c.DeletedAt == null);

                if (duplicateCity != null)
                {
                    finalCityId = duplicateCity.Id;
                }
                else
                {
                    var newCity = new City
                    {
                        CityName = dto.CityName!.Trim(),
                        CountryId = dto.CountryId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = currentUserId,
                    };

                    _db.Cities.Add(newCity);
                    await _db.SaveChangesAsync();
                    finalCityId = newCity.Id;
                }
            }

            // ===== VÉRIFICATIONS D'UNICITÉ ENTREPRISE =====

            if (await _db.Companies.AnyAsync(c => c.Email == dto.CompanyEmail && c.DeletedAt == null))
                return Conflict(new { Message = "Une entreprise avec cet email existe déjà" });

            if (await _db.Companies.AnyAsync(c => c.CnssNumber == dto.CnssNumber && c.DeletedAt == null))
                return Conflict(new { Message = "Une entreprise avec ce numéro CNSS existe déjà" });

            // ===== VÉRIFICATIONS D'UNICITÉ ADMIN =====

            if (await _db.Employees.AnyAsync(e => e.Email == dto.AdminEmail && e.DeletedAt == null))
                return Conflict(new { Message = "Un employé avec cet email existe déjà" });

            if (await _db.Users.AnyAsync(u => u.Email == dto.AdminEmail && u.DeletedAt == null))
                return Conflict(new { Message = "Un utilisateur avec cet email existe déjà" });

            // ===== CRÉER L'ENTREPRISE =====

            var company = new Models.Company.Company
            {
                CompanyName = dto.CompanyName.Trim(),
                Email = dto.CompanyEmail.Trim(),
                PhoneNumber = dto.CompanyPhoneNumber.Trim(),
                CountryPhoneCode = dto.CountryPhoneCode ?? country.CountryPhoneCode,
                CompanyAddress = dto.CompanyAddress.Trim(),
                CityId = finalCityId,
                CountryId = dto.CountryId,
                CnssNumber = dto.CnssNumber.Trim(),
                IsCabinetExpert = dto.IsCabinetExpert,
                IceNumber = dto.IceNumber?.Trim(),
                IfNumber = dto.IfNumber?.Trim(),
                RcNumber = dto.RcNumber?.Trim(),
                RibNumber = dto.RibNumber?.Trim(),
                LegalForm = dto.LegalForm?.Trim(),
                FoundingDate = dto.FoundingDate,
                Currency = "MAD",
                PayrollPeriodicity = "Mensuelle",
                FiscalYearStartMonth = 1,
                BusinessSector = dto.BusinessSector?.Trim(),
                PaymentMethod = dto.PaymentMethod?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUserId
            };

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            // ===== RÉCUPÉRER LE STATUT "ACTIVE" =====

            var activeStatus = await _db.Statuses
                .FirstOrDefaultAsync(s => s.Name.ToLower() == "active" && s.DeletedAt == null);

            if (activeStatus == null)
            {
                // Créer le statut Active s'il n'existe pas
                activeStatus = new Models.Referentiel.Status
                {
                    Name = "Active",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = currentUserId
                };
                _db.Statuses.Add(activeStatus);
                await _db.SaveChangesAsync();
            }

            // ===== CRÉER L'EMPLOYÉ ADMINISTRATEUR =====

            var adminEmployee = new Employee
            {
                FirstName = dto.AdminFirstName.Trim(),
                LastName = dto.AdminLastName.Trim(),
                Email = dto.AdminEmail.Trim(),
                Phone = dto.AdminPhone.Trim(),
                CompanyId = company.Id,
                StatusId = activeStatus.Id,
                CinNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // Temporaire
                DateOfBirth = DateTime.UtcNow.AddYears(-30), // Temporaire
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUserId // Système
            };

            _db.Employees.Add(adminEmployee);
            await _db.SaveChangesAsync();

            // ===== GÉNÉRER OU UTILISER LE MOT DE PASSE =====

            string password;

            if (dto.GeneratePassword)
            {
                password = _passwordGenerator.GenerateTemporaryPassword();
            }
            else
            {
                password = dto.AdminPassword!;
            }

            // ===== CRÉER LE COMPTE UTILISATEUR =====

            var username = _passwordGenerator.GenerateUsername(dto.AdminFirstName, dto.AdminLastName);
            var suffix = 1;

            while (await _db.Users.AnyAsync(u => u.Username == username && u.DeletedAt == null))
            {
                username = _passwordGenerator.GenerateUsername(dto.AdminFirstName, dto.AdminLastName, suffix);
                suffix++;
            }

            var adminUser = new Users
            {
                EmployeeId = adminEmployee.Id,
                Username = username,
                Email = dto.AdminEmail.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUserId // Système
            };

            _db.Users.Add(adminUser);
            await _db.SaveChangesAsync();

            // ===== ASSIGNER LE RÔLE ADMIN =====

            var adminRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "admin" && r.DeletedAt == null);

            if (adminRole == null)
            {
                // Créer le rôle Admin s'il n'existe pas
                adminRole = new Models.Permissions.Roles
                {
                    Name = "Admin",
                    Description = "Administrateur de l'entreprise",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = currentUserId
                };
                _db.Roles.Add(adminRole);
                await _db.SaveChangesAsync();
            }

            var userRole = new Models.Permissions.UsersRoles
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUserId
            };

            _db.UsersRoles.Add(userRole);
            await _db.SaveChangesAsync();

            // ===== PRÉPARER LA RÉPONSE =====

            var createdCompany = await _db.Companies
                .AsNoTracking()
                .Include(c => c.City)
                .Include(c => c.Country)
                .FirstAsync(c => c.Id == company.Id);

            var response = new CompanyCreateResponseDto
            {
                Company = new CompanyReadDto
                {
                    Id = createdCompany.Id,
                    CompanyName = createdCompany.CompanyName,
                    Email = createdCompany.Email,
                    PhoneNumber = createdCompany.PhoneNumber,
                    CountryPhoneCode = createdCompany.CountryPhoneCode,
                    CompanyAddress = createdCompany.CompanyAddress,
                    CityId = createdCompany.CityId,
                    CityName = createdCompany.City?.CityName,
                    CountryId = createdCompany.CountryId,
                    CountryName = createdCompany.Country?.CountryName,
                    CnssNumber = createdCompany.CnssNumber,
                    IsCabinetExpert = createdCompany.IsCabinetExpert,
                    IceNumber = createdCompany.IceNumber,
                    IfNumber = createdCompany.IfNumber,
                    RcNumber = createdCompany.RcNumber,
                    LegalForm = createdCompany.LegalForm,
                    FoundingDate = createdCompany.FoundingDate,
                    BusinessSector = createdCompany.BusinessSector,
                    CreatedAt = createdCompany.CreatedAt.DateTime
                },
                Admin = new AdminAccountDto
                {
                    EmployeeId = adminEmployee.Id,
                    UserId = adminUser.Id,
                    Username = adminUser.Username,
                    Email = adminUser.Email,
                    FirstName = adminEmployee.FirstName,
                    LastName = adminEmployee.LastName,
                    Phone = adminEmployee.Phone,
                    Password = dto.GeneratePassword ? password : null, // Ne retourner le mot de passe que s'il a été généré
                    Message = dto.GeneratePassword
                        ? "Un mot de passe temporaire a été généré. Veuillez le changer lors de la première connexion."
                        : "Compte administrateur créé avec succès."
                }
            };

            return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, response);
        }

        /// <summary>
        /// Mise à jour partielle d'une entreprise (PATCH /api/companies/{id})
        /// Supporte : modification des champs simples, changement/création de ville, vérifications d'unicité.
        /// </summary>
        [HttpPatch("{id}")]
        //[HasPermission("EDIT_COMPANY")]
        public async Task<ActionResult<CompanyReadDto>> PatchCompany(int id, [FromBody] CompanyUpdateDto dto)
        {
            Console.WriteLine("=============Call Patch From Front End============");

            if (dto == null)
                return BadRequest(new { Message = "Données de mise à jour requises" });

            Console.WriteLine("====================");
            Console.WriteLine($"is Cabinet expert  : {dto.IsCabinetExpert}");

            var currentUserId = User.GetUserId();

            var company = await _db.Companies
                .Include(c => c.City)
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);

            if (company == null)
                return NotFound(new { Message = "Entreprise non trouvée" });

            if (!string.IsNullOrWhiteSpace(dto.CompanyEmail) && dto.CompanyEmail.Trim() != company.Email)
            {
                var exists = await _db.Companies.AnyAsync(c => c.Email == dto.CompanyEmail.Trim() && c.DeletedAt == null && c.Id != id);
                if (exists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà cet email" });

                company.Email = dto.CompanyEmail.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.CompanyName) && dto.CompanyName.Trim() != company.CompanyName)
            {
                var nameExists = await _db.Companies
                    .AnyAsync(c => c.CompanyName.ToLower() == dto.CompanyName.Trim().ToLower() && c.DeletedAt == null && c.Id != id);
                if (nameExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce nom" });
                company.CompanyName = dto.CompanyName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.CompanyPhoneNumber) && dto.CompanyPhoneNumber.Trim() != company.PhoneNumber)
            {
                // Vérification de l'unicité du numéro de téléphone
                var phoneExists = await _db.Companies
                    .AnyAsync(c => 
                    c.PhoneNumber == dto.CompanyPhoneNumber.Trim() && 
                    c.DeletedAt == null && 
                    c.Id != id
                    );

                if (phoneExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce numéro de téléphone" });

                company.PhoneNumber = dto.CompanyPhoneNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.CompanyAddress) && dto.CompanyAddress.Trim() != company.CompanyAddress)
            {
                company.CompanyAddress = dto.CompanyAddress.Trim();
            }

            if (dto.IsCabinetExpert.HasValue && dto.IsCabinetExpert.Value != company.IsCabinetExpert)
            {
                company.IsCabinetExpert = dto.IsCabinetExpert.Value;
            }

            // --- Gérer le pays si envoyé par nom ou id ---
            if (!string.IsNullOrWhiteSpace(dto.CountryName) || dto.CountryId.HasValue)
            {
                if (dto.CountryId.HasValue)
                {
                    if (company.CountryId != dto.CountryId.Value)
                    {
                        var existingCountry = await _db.Countries
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == dto.CountryId.Value && c.DeletedAt == null);

                        if (existingCountry == null)
                            return NotFound(new { Message = "Pays non trouvé" });

                        company.CountryId = existingCountry.Id;
                        company.CountryPhoneCode = existingCountry.CountryPhoneCode;
                        Console.WriteLine($"Country updated by id -> {existingCountry.CountryName}");
                    }
                }
                else
                {
                    var countryNameTrim = dto.CountryName!.Trim();
                    var existingCountry = await _db.Countries
                        .FirstOrDefaultAsync(c => c.CountryName.ToLower() == countryNameTrim.ToLower() && c.DeletedAt == null);

                    if (existingCountry != null)
                    {
                        company.CountryId = existingCountry.Id;
                        company.CountryPhoneCode = existingCountry.CountryPhoneCode;
                        Console.WriteLine($"Country set to existing -> {existingCountry.CountryName}");
                    }
                    else
                    {
                        // Création d'un pays minimal si introuvable (utiliser indicatif fourni ou fallback)
                        var phoneCode = !string.IsNullOrWhiteSpace(dto.CountryPhoneCode)
                            ? dto.CountryPhoneCode!.Trim()
                            : company.CountryPhoneCode ?? "+000";

                        var code = countryNameTrim.Length >= 3
                            ? new string(countryNameTrim.Where(char.IsLetter).Take(3).ToArray()).ToUpper()
                            : countryNameTrim.ToUpper();

                        if (string.IsNullOrWhiteSpace(code))
                            code = "UNK";

                        var newCountry = new Models.Referentiel.Country
                        {
                            CountryName = countryNameTrim,
                            CountryNameAr = null,
                            CountryCode = code,
                            CountryPhoneCode = phoneCode,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = currentUserId
                        };

                        _db.Countries.Add(newCountry);
                        await _db.SaveChangesAsync();

                        company.CountryId = newCountry.Id;
                        company.CountryPhoneCode = newCountry.CountryPhoneCode;
                        Console.WriteLine($"Country created -> {newCountry.CountryName}");
                    }
                }
            }

            // --- Gérer la ville si envoyé par nom ou id ---
            if (!string.IsNullOrWhiteSpace(dto.CityName) || dto.CityId.HasValue)
            {
                // Nécessite un CountryId pour associer la ville ; si non défini, utiliser company.CountryId
                if (company.CountryId <= 0)
                    return BadRequest(new { Message = "CountryId requis pour créer/rechercher une ville. Envoyez d'abord CountryName ou CountryId." });

                if (dto.CityId.HasValue)
                {
                    if (company.CityId != dto.CityId.Value)
                    {
                        var existingCity = await _db.Cities
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == dto.CityId.Value && c.DeletedAt == null);

                        if (existingCity == null)
                            return NotFound(new { Message = "Ville non trouvée" });

                        if (existingCity.CountryId != company.CountryId)
                            return BadRequest(new { Message = "La ville choisie n'appartient pas au pays courant de l'entreprise" });

                        company.CityId = existingCity.Id;
                        Console.WriteLine($"City updated by id -> {existingCity.CityName}");
                    }
                }
                else
                {
                    var cityNameTrim = dto.CityName!.Trim();
                    var existingCity = await _db.Cities
                        .FirstOrDefaultAsync(c =>
                            c.CityName.ToLower() == cityNameTrim.ToLower() &&
                            c.CountryId == company.CountryId &&
                            c.DeletedAt == null);

                    if (existingCity != null)
                    {
                        company.CityId = existingCity.Id;
                        Console.WriteLine($"City set to existing -> {existingCity.CityName}");
                    }
                    else
                    {
                        var newCity = new Models.Referentiel.City
                        {
                            CityName = cityNameTrim,
                            CountryId = company.CountryId,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = currentUserId
                        };

                        _db.Cities.Add(newCity);
                        await _db.SaveChangesAsync();

                        company.CityId = newCity.Id;
                        Console.WriteLine($"City created -> {newCity.CityName}");
                    }
                }
            }

            // --- mise à jour CNSS ----
            if (!string.IsNullOrWhiteSpace(dto.CnssNumber))
            {
                var cnssExists = await _db.Companies
                    .AnyAsync(c => 
                    c.CnssNumber == dto.CnssNumber.Trim() && 
                    c.DeletedAt == null && 
                    c.Id != id
                    );

                if (cnssExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce numéro CNSS" });

                var cnssTrim = dto.CnssNumber!.Trim();

                if (cnssTrim != company.CnssNumber)
                    company.CnssNumber = cnssTrim;
            }

            // ---- mise à jour ice ----
            if (!string.IsNullOrWhiteSpace(dto.IceNumber))
            {
                var iceExists = await _db.Companies
                    .AnyAsync(c => 
                    c.IceNumber == dto.IceNumber.Trim() && 
                    c.DeletedAt == null && 
                    c.Id != id
                    );

                if (iceExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce numéro ICE" });

                var iceTrim = dto.IceNumber!.Trim();

                if (iceTrim != company.IceNumber)
                    company.IceNumber = iceTrim;
            }

            // ----- Mise à Jour IfNumber
            if (!string.IsNullOrWhiteSpace(dto.IfNumber))
            {
                var ifExists = await _db.Companies
                    .AnyAsync(c =>
                    c.IfNumber == dto.IfNumber.Trim() &&
                    c.DeletedAt == null &&
                    c.Id != id
                    );
                if (ifExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce numéro IF" });
                var ifTrim = dto.IfNumber!.Trim();
                if (ifTrim != company.IfNumber)
                    company.IfNumber = ifTrim;
            }

            // mise à jour RC
            if (!string.IsNullOrWhiteSpace(dto.RcNumber))
            {
                var rcExists = await _db.Companies
                    .AnyAsync(c =>
                    c.RcNumber == dto.RcNumber.Trim() &&
                    c.DeletedAt == null &&
                    c.Id != id
                    );

                if (rcExists)
                    return Conflict(new { Message = "Une autre entreprise utilise déjà ce numéro RC" });

                var rcTrim = dto.RcNumber!.Trim();

                if (rcTrim != company.RcNumber)
                    company.RcNumber = rcTrim;
            }

            // Mise à jour form juridique
            if (!string.IsNullOrEmpty(dto.LegalForm))
            {
                var legalFormTrim = dto.LegalForm!.Trim();
                if (legalFormTrim != company.LegalForm)
                    company.LegalForm = legalFormTrim;
            }

            // Mise à jour date de fondation
            if (dto.FoundingDate.HasValue && dto.FoundingDate.Value != company.FoundingDate)
            {
                company.FoundingDate = dto.FoundingDate.Value;
            }
            // ===== Audit =====
            company.ModifiedAt = DateTimeOffset.UtcNow;
            company.ModifiedBy = currentUserId;

            var changes = _db.Entry(company).Properties
                .Where(p => p.IsModified)
                .Select(p => $"{p.Metadata.Name} => {p.CurrentValue}")
                .ToList();

            await _db.SaveChangesAsync();

            Console.WriteLine("SaveChanges completed");

            var updated = await _db.Companies
                .AsNoTracking()
                .Include(c => c.City)
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == company.Id);

            var result = new CompanyReadDto
            {
                Id = updated.Id,
                CompanyName = updated.CompanyName,
                Email = updated.Email,
                PhoneNumber = updated.PhoneNumber,
                CountryPhoneCode = updated.CountryPhoneCode,
                CompanyAddress = updated.CompanyAddress,
                CityId = updated.CityId,
                CityName = updated.City?.CityName,
                CountryId = updated.CountryId,
                CountryName = updated.Country?.CountryName,
                CnssNumber = updated.CnssNumber,
                IsCabinetExpert = updated.IsCabinetExpert,
                IceNumber = updated.IceNumber,
                IfNumber = updated.IfNumber,
                RcNumber = updated.RcNumber,
                LegalForm = updated.LegalForm,
                FoundingDate = updated.FoundingDate,
                BusinessSector = updated.BusinessSector,
                CreatedAt = updated.CreatedAt.DateTime
            };

            return Ok(result);
        }

    }
}