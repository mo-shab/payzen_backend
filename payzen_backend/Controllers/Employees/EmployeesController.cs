using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Dashboard.Dtos;
using payzen_backend.Models.Employee;
using payzen_backend.Models.Employee.Dtos;
using payzen_backend.Models.Users;
using payzen_backend.Services;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Permissions.Dtos;

using static payzen_backend.Models.Permissions.PermissionsConstants;

namespace payzen_backend.Controllers.Employees
{
    [Route("api/employee")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordGeneratorService _passwordGenerator;

        public EmployeesController(AppDbContext db, PasswordGeneratorService passwordGenerator)
        {
            _db = db;
            _passwordGenerator = passwordGenerator;
        }

        /// <summary>
        /// Récupère tous les employés actifs
        /// </summary>
        [HttpGet("summary")]
        //[HasPermission(READ_EMPLOYEES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetAll()
        {
            // Comptage total des employés non supprimés
            var totalEmployees = await _db.Employees
                .Where(e => e.DeletedAt == null)
                .CountAsync();

            // Comptage des employés actifs
            var activeEmployees = await _db.Employees
                .Where(e => e.DeletedAt == null && e.Status != null && e.Status.Name == "Active")
                .CountAsync();

            // Récupération des employés avec toutes les relations nécessaires
            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Departement)
                .Include(e => e.Status)
                .Include(e => e.Manager)
                .Include(e => e.Documents)
                .Include(e => e.Contracts.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.JobPosition)
                .Include(e => e.Contracts.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.ContractType)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new EmployeeDashboardItemDto
                {
                    Id = e.Id.ToString(),
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Contracts
                        .Where(c => c.DeletedAt == null && c.EndDate == null)
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => c.JobPosition.Name)
                        .FirstOrDefault() ?? "Non assigné",
                    Department = e.Departement != null ? e.Departement.DepartementName : "Non assigné",
                    Status = MapStatusToFrontend(e.Status != null ? e.Status.Name : ""),
                    StartDate = e.Contracts
                        .Where(c => c.DeletedAt == null && c.EndDate == null)
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => c.StartDate.ToString("yyyy-MM-dd"))
                        .FirstOrDefault() ?? "",
                    MissingDocuments = e.Documents != null
                        ? e.Documents.Count(d => d.DeletedAt == null && string.IsNullOrEmpty(d.FilePath))
                        : 0,
                    ContractType = e.Contracts
                        .Where(c => c.DeletedAt == null && c.EndDate == null)
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => c.ContractType.ContractTypeName)
                        .FirstOrDefault() ?? "",
                    Manager = e.Manager != null
                        ? $"{e.Manager.FirstName} {e.Manager.LastName}"
                        : null
                })
                .ToListAsync();

            var response = new DashboardResponseDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                Employees = employees
            };

            return Ok(response);
        }

        /// <summary>
        /// Récupère les détails complets d'un employé par ID
        /// </summary>
        [HttpGet("{id}/details")]
        //[HasPermission(VIEW_EMPLOYEE)]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployeeDetails(int id)
        {
            var employee = await _db.Employees
                .AsNoTracking()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .Include(e => e.Status)
                .Include(e => e.MaritalStatus)
                //.Include(e => e.Nationality)
                .Include(e => e.Addresses!.Where(a => a.DeletedAt == null))
                    .ThenInclude(e => e.City)
                    .ThenInclude(e => e.Country)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.JobPosition)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.ContractType)
                .Include(e => e.Salaries!.Where(s => s.DeletedAt == null && s.EndDate == null))
                    .ThenInclude(s => s.Components!.Where(c => c.DeletedAt == null && c.EndDate == null))
                .FirstOrDefaultAsync();


            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            // Récupérer le contrat actif
            var activeContract = employee.Contracts?
                .Where(c => c.DeletedAt == null && c.EndDate == null)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            // Récupérer le salaire actif
            var activeSalary = employee.Salaries?
                .Where(s => s.DeletedAt == null && s.EndDate == null)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefault();

            // Récupérer l'adresse active (la plus récente)
            var activeAddress = employee.Addresses?
                .Where(a => a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();         

            // Calculer les composants salariaux
            var salaryComponents = activeSalary?.Components?
                .Where(c => c.DeletedAt == null && c.EndDate == null)
                .Select(c => new SalaryComponentDto
                {
                    ComponentName = c.ComponentType,
                    Amount = c.Amount
                })
                .ToList() ?? new List<SalaryComponentDto>();

            // Calculer le salaire total
            decimal baseSalary = activeSalary?.BaseSalary ?? 0;
            decimal totalComponents = salaryComponents.Sum(c => c.Amount);
            decimal totalSalary = baseSalary + totalComponents;

            // Récupérer les événements
            var events = await _db.EventsEmployee
                .Where(ev => ev.EmployeeId == employee.Id && ev.DeletedAt == null)
                .OrderByDescending(ev => ev.EventTime)
                .Select(ev => new
                {
                    Id = ev.Id,
                    EventTypeName = ev.EventType != null ? ev.EventType.Name : "",
                    EventTypeDescription = ev.EventType != null ? ev.EventType.Description : null,
                    EventTime = ev.EventTime,
                    CreatedAt = ev.CreatedAt.DateTime
                })
                .ToListAsync();

            var result = new EmployeeDetailDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                CinNumber = employee.CinNumber,
                MaritalStatusName = employee.MaritalStatus?.Name,
                DateOfBirth = employee.DateOfBirth,
                StatusName = employee.Status?.Name,
                Email = employee.Email,
                Phone = employee.Phone,
                CountryPhoneCode = activeAddress?.City?.Country?.CountryPhoneCode,

                // Adresse
                Address = activeAddress != null ? new EmployeeAddressDto
                {
                    AddressLine1 = activeAddress.AddressLine1,
                    AddressLine2 = activeAddress.AddressLine2,
                    ZipCode = activeAddress.ZipCode,
                    CityName = activeAddress.City?.CityName ?? "",
                    CountryName = activeAddress.City?.Country?.CountryName,
                } : null,

                // Informations de contrat
                JobPositionName = activeContract?.JobPosition?.Name,
                ManagerName = employee.Manager != null
                    ? $"{employee.Manager.FirstName} {employee.Manager.LastName}"
                    : null,
                ContractStartDate = activeContract?.StartDate,
                ContractTypeName = activeContract?.ContractType?.ContractTypeName,
                departments = employee.Departement != null ? employee.Departement.DepartementName : null,

                // Informations salariales
                BaseSalary = baseSalary,
                SalaryComponents = salaryComponents,
                TotalSalary = totalSalary,

                // Numéro CNSS, AMO, CIMR A coder plus tartd dans le model employee
                CNSS = employee.CnssNumber,
                AmoNumber = "74647",
                CIMR = employee.CimrNumber,

                // Événements
                Events = events.Select(ev => (dynamic)ev).ToList(),

                CreatedAt = employee.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère un employé par ID
        /// </summary>
        [HttpGet("{id}")]
        //[HasPermission(VIEW_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> GetById(int id)
        {
            var employee = await _db.Employees
                .AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            var result = new EmployeeReadDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                CinNumber = employee.CinNumber,
                DateOfBirth = employee.DateOfBirth,
                Phone = employee.Phone,
                Email = employee.Email,
                CompanyId = employee.CompanyId,
                CompanyName = employee.Company?.CompanyName ?? "",
                DepartementId = employee.DepartementId,
                DepartementName = employee.Departement?.DepartementName,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager != null ? $"{employee.Manager.FirstName} {employee.Manager.LastName}" : null,
                StatusId = employee.StatusId,
                GenderId = employee.GenderId,
                //NationalityId = employee.NationalityId,
                EducationLevelId = employee.EducationLevelId,
                MaritalStatusId = employee.MaritalStatusId,
                CreatedAt = employee.CreatedAt.DateTime
            };

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les employés d'une société
        /// </summary>
        [HttpGet("company/{companyId}")]
        //[HasPermission(VIEW_COMPANY_EMPLOYEES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetByCompanyId(int companyId)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            var result = employees.Select(e => new EmployeeReadDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CinNumber = e.CinNumber,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone,
                Email = e.Email,
                CompanyId = e.CompanyId,
                CompanyName = e.Company?.CompanyName ?? "",
                DepartementId = e.DepartementId,
                DepartementName = e.Departement?.DepartementName,
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                //NationalityId = e.NationalityId,
                EducationLevelId = e.EducationLevelId,
                MaritalStatusId = e.MaritalStatusId,
                CreatedAt = e.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les employés d'un département
        /// </summary>
        [HttpGet("departement/{departementId}")]
        //[HasPermission(VIEW_COMPANY_EMPLOYEES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetByDepartementId(int departementId)
        {
            var departementExists = await _db.Departement.AnyAsync(d => d.Id == departementId && d.DeletedAt == null);
            if (!departementExists)
                return NotFound(new { Message = "Département non trouvé" });

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.DepartementId == departementId && e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            var result = employees.Select(e => new EmployeeReadDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CinNumber = e.CinNumber,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone,
                Email = e.Email,
                CompanyId = e.CompanyId,
                CompanyName = e.Company?.CompanyName ?? "",
                DepartementId = e.DepartementId,
                DepartementName = e.Departement?.DepartementName,
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                //NationalityId = e.NationalityId,
                EducationLevelId = e.EducationLevelId,
                MaritalStatusId = e.MaritalStatusId,
                CreatedAt = e.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère tous les subordonnés d'un manager
        /// </summary>
        [HttpGet("manager/{managerId}/subordinates")]
        //[HasPermission(VIEW_SUBORDINATES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetSubordinates(int managerId)
        {
            var managerExists = await _db.Employees.AnyAsync(e => e.Id == managerId && e.DeletedAt == null);
            if (!managerExists)
                return NotFound(new { Message = "Manager non trouvé" });

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.ManagerId == managerId && e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Departement)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            var result = employees.Select(e => new EmployeeReadDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CinNumber = e.CinNumber,
                DateOfBirth = e.DateOfBirth,
                Phone = e.Phone,
                Email = e.Email,
                CompanyId = e.CompanyId,
                CompanyName = e.Company?.CompanyName ?? "",
                DepartementId = e.DepartementId,
                DepartementName = e.Departement?.DepartementName,
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                //NationalityId = e.NationalityId,
                EducationLevelId = e.EducationLevelId,
                MaritalStatusId = e.MaritalStatusId,
                CreatedAt = e.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Crée un nouvel employé et un compte utilisateur associé
        /// </summary>
        [HttpPost]
        //[HasPermission(CREATE_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> Create([FromBody] EmployeeCreateDto dto)
        {
            Console.WriteLine("=======================");
            Console.WriteLine(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Vérifier que la société existe
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            // Vérifier que le département existe et appartient à la bonne société
            var departement = await _db.Departement
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dto.DepartementId && d.DeletedAt == null);

            if (departement == null)
                return NotFound(new { Message = "Département non trouvé" });

            if (departement.CompanyId != dto.CompanyId)
                return BadRequest(new { Message = "Le département ne correspond pas à la société spécifiée" });

            // Vérifier que le CIN n'existe pas déjà
            if (await _db.Employees.AnyAsync(e => e.CinNumber == dto.CinNumber && e.DeletedAt == null))
                return Conflict(new { Message = "Un employé avec ce numéro CIN existe déjà" });

            // Vérifier que l'email n'existe pas déjà (dans Employees)
            if (await _db.Employees.AnyAsync(e => e.Email == dto.Email && e.DeletedAt == null))
                return Conflict(new { Message = "Un employé avec cet email existe déjà" });

            // Vérifier que l'email n'existe pas déjà (dans Users)
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.DeletedAt == null))
                return Conflict(new { Message = "Un utilisateur avec cet email existe déjà" });

            // Vérifier que le manager existe (si fourni)
            if (dto.ManagerId.HasValue)
            {
                var managerExists = await _db.Employees.AnyAsync(e => e.Id == dto.ManagerId && e.DeletedAt == null);
                if (!managerExists)
                    return NotFound(new { Message = "Manager non trouvé" });
            }

            // Créer l'employé
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CinNumber = dto.CinNumber,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Email = dto.Email,
                CompanyId = dto.CompanyId,
                DepartementId = dto.DepartementId,
                ManagerId = dto.ManagerId,
                StatusId = dto.StatusId,
                GenderId = dto.GenderId,
                //NationalityId = dto.NationalityId,
                EducationLevelId = dto.EducationLevelId,
                MaritalStatusId = dto.MaritalStatusId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            // Créer automatiquement un compte utilisateur si demandé
            string? temporaryPassword = null;
            Users? createdUser = null;

            if (dto.CreateUserAccount)
            {
                // Générer un nom d'utilisateur unique
                var baseUsername = _passwordGenerator.GenerateUsername(dto.FirstName, dto.LastName);
                var username = baseUsername;
                var suffix = 1;

                while (await _db.Users.AnyAsync(u => u.Username == username && u.DeletedAt == null))
                {
                    username = _passwordGenerator.GenerateUsername(dto.FirstName, dto.LastName, suffix);
                    suffix++;
                }

                // Utiliser le mot de passe fourni ou générer un temporaire
                temporaryPassword = dto.Password ?? _passwordGenerator.GenerateTemporaryPassword();

                // Créer le compte utilisateur
                createdUser = new Users
                {
                    EmployeeId = employee.Id,
                    Username = username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId
                };

                _db.Users.Add(createdUser);
                await _db.SaveChangesAsync();

                Console.WriteLine($"User créé automatiquement - Username: {username}, Email: {dto.Email}");
            }

            // Récupérer l'employé créé avec ses relations
            var createdEmployee = await _db.Employees
                .AsNoTracking()
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .FirstAsync(e => e.Id == employee.Id);

            var readDto = new EmployeeReadDto
            {
                Id = createdEmployee.Id,
                FirstName = createdEmployee.FirstName,
                LastName = createdEmployee.LastName,
                CinNumber = createdEmployee.CinNumber,
                DateOfBirth = createdEmployee.DateOfBirth,
                Phone = createdEmployee.Phone,
                Email = createdEmployee.Email,
                CompanyId = createdEmployee.CompanyId,
                CompanyName = createdEmployee.Company?.CompanyName ?? "",
                DepartementId = createdEmployee.DepartementId,
                DepartementName = createdEmployee.Departement?.DepartementName,
                ManagerId = createdEmployee.ManagerId,
                ManagerName = createdEmployee.Manager != null ? $"{createdEmployee.Manager.FirstName} {createdEmployee.Manager.LastName}" : null,
                StatusId = createdEmployee.StatusId,
                GenderId = createdEmployee.GenderId,
                //NationalityId = createdEmployee.NationalityId,
                EducationLevelId = createdEmployee.EducationLevelId,
                MaritalStatusId = createdEmployee.MaritalStatusId,
                CreatedAt = createdEmployee.CreatedAt.DateTime
            };

            // Assigné un role par défaut au nouvel utilisateur
            if (dto.CreateUserAccount && createdUser != null)
            {
                var defaultRole = await _db.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Name == "Employee" && r.DeletedAt == null);
                if (defaultRole != null)
                {
                    var userRole = new UsersRoles
                    {
                        UserId = createdUser.Id,
                        RoleId = defaultRole.Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = userId
                    };
                    _db.UsersRoles.Add(userRole);
                    await _db.SaveChangesAsync();
                }
            }
            // Retourner les infos du compte créé dans la réponse
            if (dto.CreateUserAccount && createdUser != null)
            {
                return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new
                {
                    Employee = readDto,
                    UserAccount = new
                    {
                        createdUser.Username,
                        createdUser.Email,
                        TemporaryPassword = temporaryPassword,
                        Message = "Un compte utilisateur a été créé. Le mot de passe temporaire doit être changé lors de la première connexion."
                    }
                });
            }

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, readDto);
        }

        /// <summary>
        /// Met à jour un employé
        /// </summary>
        [HttpPut("{id}")]
        //[HasPermission(EDIT_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> Update(int id, [FromBody] EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            var employee = await _db.Employees
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            // Mettre à jour les champs si fournis
            if (dto.FirstName != null)
                employee.FirstName = dto.FirstName;

            if (dto.LastName != null)
                employee.LastName = dto.LastName;

            if (dto.CinNumber != null && dto.CinNumber != employee.CinNumber)
            {
                if (await _db.Employees.AnyAsync(e => e.CinNumber == dto.CinNumber && e.Id != id && e.DeletedAt == null))
                    return Conflict(new { Message = "Un employé avec ce numéro CIN existe déjà" });
                
                employee.CinNumber = dto.CinNumber;
            }

            if (dto.DateOfBirth.HasValue)
                employee.DateOfBirth = dto.DateOfBirth.Value;

            if (string.IsNullOrEmpty(dto.Phone))
                employee.Phone = dto.Phone;

            if (dto.Email != null && dto.Email != employee.Email)
            {
                if (await _db.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id && e.DeletedAt == null))
                    return Conflict(new { Message = "Un employé avec cet email existe déjà" });
                
                employee.Email = dto.Email;
            }

            if (dto.CompanyId.HasValue)
            {
                var companyExists = await _db.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.DeletedAt == null);
                if (!companyExists)
                    return NotFound(new { Message = "Société non trouvée" });
                
                employee.CompanyId = dto.CompanyId.Value;
            }

            // Gestion du DepartementId
            if (dto.DepartementId.HasValue)
            {
                var departement = await _db.Departement
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DepartementId && d.DeletedAt == null);

                if (departement == null)
                    return NotFound(new { Message = "Département non trouvé" });

                // Vérifier que le département appartient à la société de l'employé
                var currentCompanyId = dto.CompanyId ?? employee.CompanyId;
                if (departement.CompanyId != currentCompanyId)
                    return BadRequest(new { Message = "Le département ne correspond pas à la société de l'employé" });

                employee.DepartementId = dto.DepartementId.Value;
            }

            if (dto.ManagerId.HasValue)
            {
                if (dto.ManagerId.Value == id)
                    return BadRequest(new { Message = "Un employé ne peut pas être son propre manager" });

                var managerExists = await _db.Employees.AnyAsync(e => e.Id == dto.ManagerId && e.DeletedAt == null);
                if (!managerExists)
                    return NotFound(new { Message = "Manager non trouvé" });
                
                employee.ManagerId = dto.ManagerId.Value;
            }

            if (dto.StatusId.HasValue)
                employee.StatusId = dto.StatusId.Value;

            if (dto.GenderId.HasValue)
                employee.GenderId = dto.GenderId.Value;

            //if (dto.NationalityId.HasValue)
            //    employee.NationalityId = dto.NationalityId.Value;

            if (dto.EducationLevelId.HasValue)
                employee.EducationLevelId = dto.EducationLevelId.Value;

            if (dto.MaritalStatusId.HasValue)
                employee.MaritalStatusId = dto.MaritalStatusId.Value;

            employee.ModifiedAt = DateTimeOffset.UtcNow;
            employee.ModifiedBy = userId;

            await _db.SaveChangesAsync();

            // Récupérer l'employé mis à jour avec ses relations
            var updatedEmployee = await _db.Employees
                .AsNoTracking()
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .FirstAsync(e => e.Id == id);

            var readDto = new EmployeeReadDto
            {
                Id = updatedEmployee.Id,
                FirstName = updatedEmployee.FirstName,
                LastName = updatedEmployee.LastName,
                CinNumber = updatedEmployee.CinNumber,
                DateOfBirth = updatedEmployee.DateOfBirth,
                Phone = updatedEmployee.Phone,
                Email = updatedEmployee.Email,
                CompanyId = updatedEmployee.CompanyId,
                CompanyName = updatedEmployee.Company?.CompanyName ?? "",
                DepartementId = updatedEmployee.DepartementId,
                DepartementName = updatedEmployee.Departement?.DepartementName,
                ManagerId = updatedEmployee.ManagerId,
                ManagerName = updatedEmployee.Manager != null ? $"{updatedEmployee.Manager.FirstName} {updatedEmployee.Manager.LastName}" : null,
                StatusId = updatedEmployee.StatusId,
                GenderId = updatedEmployee.GenderId,
                //NationalityId = updatedEmployee.NationalityId,
                EducationLevelId = updatedEmployee.EducationLevelId,
                MaritalStatusId = updatedEmployee.MaritalStatusId,
                CreatedAt = updatedEmployee.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Supprime un employé (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        //[HasPermission(DELETE_EMPLOYEE)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();

            var employee = await _db.Employees
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            // Vérifier si l'employé est manager d'autres employés
            var hasSubordinates = await _db.Employees
                .AnyAsync(e => e.ManagerId == id && e.DeletedAt == null);

            if (hasSubordinates)
                return BadRequest(new { Message = "Impossible de supprimer cet employé car il est manager d'autres employés" });

            // Vérifier si l'employé est lié à un utilisateur
            var hasUser = await _db.Users
                .AnyAsync(u => u.EmployeeId == id && u.DeletedAt == null);

            if (hasUser)
                return BadRequest(new { Message = "Impossible de supprimer cet employé car il est lié à un compte utilisateur" });

            // Soft delete
            employee.DeletedAt = DateTimeOffset.UtcNow;
            employee.DeletedBy = userId;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Récupère toutes les données nécessaires pour le formulaire de création/modification d'employé
        /// </summary>
        /// <param name="companyId">ID de l'entreprise (optionnel, si non fourni utilise l'entreprise de l'utilisateur connecté)</param>
        [HttpGet("form-data")]
        public async Task<ActionResult<EmployeeFormDataDto>> GetFormData([FromQuery] int? companyId = null)
        {
            // Récupérer l'utilisateur authentifié
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "uid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié" });
            }

            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);

            if (user?.Employee == null)
            {
                return BadRequest(new { Message = "L'utilisateur n'est pas associé à un employé" });
            }

            // Utiliser le companyId fourni ou celui de l'utilisateur
            var targetCompanyId = companyId ?? user.Employee.CompanyId;

            // Vérifier que l'utilisateur a accès à cette entreprise
            if (companyId.HasValue && companyId.Value != user.Employee.CompanyId)
            {
                // Vérifier si l'utilisateur a les permissions pour accéder à d'autres entreprises
                // Pour l'instant, on restreint à sa propre entreprise
                return Forbid();
            }

            var formData = new EmployeeFormDataDto();

            // 1. Récupérer les statuts
            formData.Statuses = await _db.Statuses
                .Where(s => s.DeletedAt == null)
                .Select(s => new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            // 2. Récupérer les genres
            formData.Genders = await _db.Genders
                .Where(g => g.DeletedAt == null)
                .Select(g => new GenderDto
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .OrderBy(g => g.Name)
                .ToListAsync();

            // 3. Récupérer les niveaux d'éducation
            formData.EducationLevels = await _db.EducationLevels
                .Where(e => e.DeletedAt == null)
                .Select(e => new EducationLevelDto
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .OrderBy(e => e.Name)
                .ToListAsync();

            // 4. Récupérer les statuts matrimoniaux
            formData.MaritalStatuses = await _db.MaritalStatuses
                .Where(m => m.DeletedAt == null)
                .Select(m => new MaritalStatusDto
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .OrderBy(m => m.Name)
                .ToListAsync();

            // 5. Récupérer les pays
            formData.Countries = await _db.Countries
                .Where(c => c.DeletedAt == null)
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    CountryName = c.CountryName,
                    CountryPhoneCode = c.CountryPhoneCode
                })
                .OrderBy(c => c.CountryName)
                .ToListAsync();

            // 6. Récupérer les villes
            formData.Cities = await _db.Cities
                .Where(c => c.DeletedAt == null)
                .Include(c => c.Country)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    CityName = c.CityName,
                    CountryId = c.CountryId,
                    CountryName = c.Country != null ? c.Country.CountryName : null
                })
                .OrderBy(c => c.CountryName)
                .ThenBy(c => c.CityName)
                .ToListAsync();

            // 7. Récupérer les départements de l'entreprise
            formData.Departements = await _db.Departement
                .Where(d => d.DeletedAt == null && d.CompanyId == targetCompanyId)
                .Select(d => new DepartementDto
                {
                    Id = d.Id,
                    DepartementName = d.DepartementName,
                    CompanyId = d.CompanyId
                })
                .OrderBy(d => d.DepartementName)
                .ToListAsync();

            // 8. Récupérer les postes de l'entreprise
            formData.JobPositions = await _db.JobPositions
                .Where(j => j.DeletedAt == null && j.CompanyId == targetCompanyId)
                .Select(j => new JobPositionDto
                {
                    Id = j.Id,
                    Name = j.Name,
                    CompanyId = j.CompanyId
                })
                .OrderBy(j => j.Name)
                .ToListAsync();

            // 9. Récupérer les types de contrat de l'entreprise
            formData.ContractTypes = await _db.ContractTypes
                .Where(c => c.DeletedAt == null && c.CompanyId == targetCompanyId)
                .Select(c => new ContractTypeDto
                {
                    Id = c.Id,
                    ContractTypeName = c.ContractTypeName,
                    CompanyId = c.CompanyId
                })
                .OrderBy(c => c.ContractTypeName)
                .ToListAsync();

            // 10. Récupérer les managers potentiels (employés actifs de l'entreprise)
            formData.PotentialManagers = await _db.Employees
                .Where(e => e.DeletedAt == null && e.CompanyId == targetCompanyId)
                .Include(e => e.Departement)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    FullName = e.FirstName + " " + e.LastName,
                    DepartementName = e.Departement != null ? e.Departement.DepartementName : null
                })
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
            // 11. Récupérer les nationalités
            formData.Nationalities = await _db.Nationalities
                .Where(n => n.DeletedAt == null)
                .Select(n => new NationalityDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .OrderBy(n => n.Name)
                .ToListAsync();

            return Ok(formData);
        }

        private static string MapStatusToFrontend(string statusName)
        {
            return statusName switch
            {
                "Active" => "Active",
                "En congé" => "on_leave",
                "Suspendu" => "inactive",
                "Licencié" => "licencié",
                _ => "inactive"
            };
        }
    }
}