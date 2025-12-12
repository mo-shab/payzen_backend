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
using payzen_backend.Models.Event;
using payzen_backend.Services;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Permissions.Dtos;
using Microsoft.AspNetCore.JsonPatch;


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
        private readonly EmployeeEventLogService _eventLogService;

        public EmployeesController(
            AppDbContext db,
            PasswordGeneratorService passwordGenerator,
            EmployeeEventLogService eventLogServicee)
        {
            _db = db;
            _passwordGenerator = passwordGenerator;
            _eventLogService = eventLogServicee;
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
                //.Include(e => e.NationalityId)
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Récupérer l'utilisateur authentifié pour obtenir son CompanyId
            var currentUser = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);

            if (currentUser?.Employee == null)
            {
                return BadRequest(new { Message = "L'utilisateur n'est pas associé à un employé" });
            }

            // Utiliser le CompanyId fourni ou celui de l'utilisateur connecté
            var companyId = dto.CompanyId ?? currentUser.Employee.CompanyId;

            // Vérifier que la société existe
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

            // Vérifier les permissions : l'utilisateur peut-il créer un employé pour une autre société ?
            if (dto.CompanyId.HasValue && dto.CompanyId.Value != currentUser.Employee.CompanyId)
            {
                // TODO: Vérifier si l'utilisateur a les permissions pour créer des employés dans d'autres sociétés
                // Pour l'instant, on interdit
                return Forbid();
            }

            // Vérifier que le département existe et appartient à la bonne société
            if (dto.DepartementId.HasValue)
            {
                var departement = await _db.Departement
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DepartementId && d.DeletedAt == null);

                if (departement == null)
                    return NotFound(new { Message = "Département non trouvé" });

                if (departement.CompanyId != companyId)
                    return BadRequest(new { Message = "Le département ne correspond pas à la société spécifiée" });
            }

            // Vérifier que le JobPosition existe et appartient à la bonne société (si fourni)
            if (dto.JobPositionId.HasValue)
            {
                var jobPosition = await _db.JobPositions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(jp => jp.Id == dto.JobPositionId && jp.DeletedAt == null);
        
                if (jobPosition == null)
                    return NotFound(new { Message = "Poste de travail non trouvé" });

                if (jobPosition.CompanyId != companyId)
                    return BadRequest(new { Message = "Le poste de travail ne correspond pas à la société spécifiée" });
            }

            // Vérifier que le ContractType existe et appartient à la bonne société (si fourni)
            if (dto.ContractTypeId.HasValue)
            {
                var contractType = await _db.ContractTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.Id == dto.ContractTypeId && ct.DeletedAt == null);
        
                if (contractType == null)
                    return NotFound(new { Message = "Type de contrat non trouvé" });

                if (contractType.CompanyId != companyId)
                    return BadRequest(new { Message = "Le type de contrat ne correspond pas à la société spécifiée" });
            }

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

            // Créer l'employé avec le CompanyId déterminé
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CinNumber = dto.CinNumber,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Email = dto.Email,
                CompanyId = companyId,
                DepartementId = dto.DepartementId,
                ManagerId = dto.ManagerId,
                StatusId = dto.StatusId,
                GenderId = dto.GenderId,
                NationalityId = dto.NationalityId,
                EducationLevelId = dto.EducationLevelId,
                MaritalStatusId = dto.MaritalStatusId,
                CnssNumber = dto.CnssNumber,
                CimrNumber = dto.CimrNumber,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            // ===== CRÉER LE CONTRAT DE L'EMPLOYÉ (si JobPosition et ContractType sont fournis) =====
            if (dto.JobPositionId.HasValue && dto.ContractTypeId.HasValue)
            {
                var employeeContract = new EmployeeContract
                {
                    EmployeeId = employee.Id,
                    CompanyId = companyId,
                    JobPositionId = dto.JobPositionId.Value,
                    ContractTypeId = dto.ContractTypeId.Value,
                    StartDate = dto.StartDate ?? DateTime.UtcNow,
                    EndDate = null, // Contrat actif
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId
                };

                _db.EmployeeContracts.Add(employeeContract);
                await _db.SaveChangesAsync();
            }

            // ===== CRÉER LE SALAIRE DE L'EMPLOYÉ (si fourni) =====
            if (dto.Salary.HasValue && dto.Salary.Value > 0)
            {
                var employeeSalary = new EmployeeSalary
                {
                    EmployeeId = employee.Id,
                    ContractId = companyId,
                    BaseSalary = dto.Salary.Value,
                    EffectiveDate = dto.StartDate ?? DateTime.UtcNow,
                    EndDate = null, // Salaire actif
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId
                };

                _db.EmployeeSalaries.Add(employeeSalary);
                await _db.SaveChangesAsync();
            }

            // ===== CRÉER L'ADRESSE DE L'EMPLOYÉ (si fournie) =====
            if (dto.CityId.HasValue && !string.IsNullOrEmpty(dto.AddressLine1))
            {
                var employeeAddress = new EmployeeAddress
                {
                    EmployeeId = employee.Id,
                    CityId = dto.CityId.Value,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    ZipCode = dto.ZipCode,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId
                };

                _db.EmployeeAddresses.Add(employeeAddress);
                await _db.SaveChangesAsync();
            }

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
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.JobPosition)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.ContractType)
                .FirstAsync(e => e.Id == employee.Id);

            // Récupérer le contrat actif pour le DTO
            var activeContract = createdEmployee.Contracts?
                .Where(c => c.DeletedAt == null && c.EndDate == null)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

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
                NationalityId = createdEmployee.NationalityId,
                EducationLevelId = createdEmployee.EducationLevelId,
                MaritalStatusId = createdEmployee.MaritalStatusId,
                JobPostionName = activeContract?.JobPosition?.Name,
                CreatedAt = createdEmployee.CreatedAt.DateTime
            };

            // Assigné un role par défaut au nouvel utilisateur
            if (dto.CreateUserAccount && createdUser != null)
            {
                var defaultRole = await _db.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Name == "employee" && r.DeletedAt == null);
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

            // Récupérer l'utilisateur authentifié
            var currentUser = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);

            if (currentUser?.Employee == null)
                return BadRequest(new { Message = "L'utilisateur n'est pas associé à un employé" });

            // Récupérer l'employé avec toutes les relations nécessaires pour le logging
            var employee = await _db.Employees
                .Include(e => e.Status)
                .Include(e => e.Gender)
                .Include(e => e.Nationality)
                .Include(e => e.EducationLevel)
                .Include(e => e.MaritalStatus)
                .Include(e => e.Departement)
                .Include(e => e.Manager)
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            // Vérifier les permissions
            if (employee.CompanyId != currentUser.Employee.CompanyId)
            {
                // TODO: Vérifier si l'utilisateur a les permissions pour modifier des employés d'autres sociétés
                return Forbid();
            }

            var updateTime = DateTimeOffset.UtcNow;
            bool hasChanges = false;

            // ===== MISE À JOUR AVEC LOGGING =====

            // FirstName
            if (dto.FirstName != null && dto.FirstName != employee.FirstName)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.FirstNameChanged,
                    oldValue: employee.FirstName,
                    newValue: dto.FirstName,
                    createdBy: userId
                );
                employee.FirstName = dto.FirstName;
                hasChanges = true;
            }

            // LastName
            if (dto.LastName != null && dto.LastName != employee.LastName)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.LastNameChanged,
                    oldValue: employee.LastName,
                    newValue: dto.LastName,
                    createdBy: userId
                );
                employee.LastName = dto.LastName;
                hasChanges = true;
            }

            // CIN Number
            if (dto.CinNumber != null && dto.CinNumber != employee.CinNumber)
            {
                if (await _db.Employees.AnyAsync(e => e.CinNumber == dto.CinNumber && e.Id != id && e.DeletedAt == null))
                    return Conflict(new { Message = "Un employé avec ce numéro CIN existe déjà" });

                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.CinNumberChanged,
                    oldValue: employee.CinNumber,
                    newValue: dto.CinNumber,
                    createdBy: userId
                );
                employee.CinNumber = dto.CinNumber;
                hasChanges = true;
            }

            // Date of Birth
            if (dto.DateOfBirth.HasValue && dto.DateOfBirth != employee.DateOfBirth)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.DateOfBirthChanged,
                    oldValue: employee.DateOfBirth.ToString("yyyy-MM-dd"),
                    newValue: dto.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                    createdBy: userId
                );
                employee.DateOfBirth = dto.DateOfBirth.Value;
                hasChanges = true;
            }

            // Phone
            if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != employee.Phone)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.PhoneChanged,
                    oldValue: employee.Phone,
                    newValue: dto.Phone,
                    createdBy: userId
                );
                employee.Phone = dto.Phone;
                hasChanges = true;
            }

            // Email
            if (dto.Email != null && dto.Email != employee.Email)
            {
                if (await _db.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id && e.DeletedAt == null))
                    return Conflict(new { Message = "Un employé avec cet email existe déjà" });

                if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.EmployeeId != id && u.DeletedAt == null))
                    return Conflict(new { Message = "Un utilisateur avec cet email existe déjà" });

                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.EmailChanged,
                    oldValue: employee.Email,
                    newValue: dto.Email,
                    createdBy: userId
                );
                employee.Email = dto.Email;
                hasChanges = true;
            }

            // Department
            if (dto.DepartementId.HasValue && dto.DepartementId != employee.DepartementId)
            {
                var newDepartement = await _db.Departement
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DepartementId && d.DeletedAt == null);

                if (newDepartement == null)
                    return NotFound(new { Message = "Département non trouvé" });

                if (newDepartement.CompanyId != employee.CompanyId)
                    return BadRequest(new { Message = "Le département ne correspond pas à la société de l'employé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.DepartmentChanged,
                    oldValueId: employee.DepartementId,
                    oldValueName: employee.Departement?.DepartementName,
                    newValueId: dto.DepartementId,
                    newValueName: newDepartement.DepartementName,
                    createdBy: userId
                );

                employee.DepartementId = dto.DepartementId;
                hasChanges = true;
            }

            // Manager
            if (dto.ManagerId.HasValue && dto.ManagerId != employee.ManagerId)
            {
                if (dto.ManagerId.Value == id)
                    return BadRequest(new { Message = "Un employé ne peut pas être son propre manager" });

                var newManager = await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == dto.ManagerId && e.DeletedAt == null);

                if (newManager == null)
                    return NotFound(new { Message = "Manager non trouvé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.ManagerChanged,
                    oldValueId: employee.ManagerId,
                    oldValueName: employee.Manager != null ? $"{employee.Manager.FirstName} {employee.Manager.LastName}" : null,
                    newValueId: dto.ManagerId,
                    newValueName: $"{newManager.FirstName} {newManager.LastName}",
                    createdBy: userId
                );

                employee.ManagerId = dto.ManagerId;
                hasChanges = true;
            }

            // Status
            if (dto.StatusId.HasValue && dto.StatusId != employee.StatusId)
            {
                var newStatus = await _db.Statuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.StatusId && s.DeletedAt == null);

                if (newStatus == null)
                    return NotFound(new { Message = "Statut non trouvé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.StatusChanged,
                    oldValueId: employee.StatusId,
                    oldValueName: employee.Status?.Name,
                    newValueId: dto.StatusId,
                    newValueName: newStatus.Name,
                    createdBy: userId
                );

                employee.StatusId = dto.StatusId;
                hasChanges = true;
            }

            // Gender
            if (dto.GenderId.HasValue && dto.GenderId != employee.GenderId)
            {
                var newGender = await _db.Genders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == dto.GenderId && g.DeletedAt == null);

                if (newGender == null)
                    return NotFound(new { Message = "Genre non trouvé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.GenderChanged,
                    oldValueId: employee.GenderId,
                    oldValueName: employee.Gender?.Name,
                    newValueId: dto.GenderId,
                    newValueName: newGender.Name,
                    createdBy: userId
                );

                employee.GenderId = dto.GenderId;
                hasChanges = true;
            }

            // Nationality
            if (dto.NationalityId.HasValue && dto.NationalityId != employee.NationalityId)
            {
                var newNationality = await _db.Nationalities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Id == dto.NationalityId && n.DeletedAt == null);

                if (newNationality == null)
                    return NotFound(new { Message = "Nationalité non trouvée" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.NationalityChanged,
                    oldValueId: employee.NationalityId,
                    oldValueName: employee.Nationality?.Name,
                    newValueId: dto.NationalityId,
                    newValueName: newNationality.Name,
                    createdBy: userId
                );

                employee.NationalityId = dto.NationalityId;
                hasChanges = true;
            }

            // Education Level
            if (dto.EducationLevelId.HasValue && dto.EducationLevelId != employee.EducationLevelId)
            {
                var newEducationLevel = await _db.EducationLevels
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == dto.EducationLevelId && e.DeletedAt == null);

                if (newEducationLevel == null)
                    return NotFound(new { Message = "Niveau d'éducation non trouvé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.EducationLevelChanged,
                    oldValueId: employee.EducationLevelId,
                    oldValueName: employee.EducationLevel?.Name,
                    newValueId: dto.EducationLevelId,
                    newValueName: newEducationLevel.Name,
                    createdBy: userId
                );

                employee.EducationLevelId = dto.EducationLevelId;
                hasChanges = true;
            }

            // Marital Status
            if (dto.MaritalStatusId.HasValue && dto.MaritalStatusId != employee.MaritalStatusId)
            {
                var newMaritalStatus = await _db.MaritalStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.MaritalStatusId && m.DeletedAt == null);

                if (newMaritalStatus == null)
                    return NotFound(new { Message = "Statut matrimonial non trouvé" });

                await _eventLogService.LogRelationEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.MaritalStatusChanged,
                    oldValueId: employee.MaritalStatusId,
                    oldValueName: employee.MaritalStatus?.Name,
                    newValueId: dto.MaritalStatusId,
                    newValueName: newMaritalStatus.Name,
                    createdBy: userId
                );

                employee.MaritalStatusId = dto.MaritalStatusId;
                hasChanges = true;
            }

            // CNSS Number
            if (dto.CnssNumber.HasValue && dto.CnssNumber.ToString() != employee.CnssNumber)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.CnssNumberChanged,
                    oldValue: employee.CnssNumber,
                    newValue: dto.CnssNumber.ToString(),
                    createdBy: userId
                );
                employee.CnssNumber = dto.CnssNumber.ToString();
                hasChanges = true;
            }

            // CIMR Number
            if (dto.CimrNumber.HasValue && dto.CimrNumber.ToString() != employee.CimrNumber)
            {
                await _eventLogService.LogSimpleEventAsync(
                    employeeId: id,
                    eventName: EmployeeEventLogService.EventNames.CimrNumberChanged,
                    oldValue: employee.CimrNumber,
                    newValue: dto.CimrNumber.ToString(),
                    createdBy: userId
                );
                employee.CimrNumber = dto.CimrNumber.ToString();
                hasChanges = true;
            }

            // ===== MISE À JOUR DU CONTRAT (avec historique) =====
            if ((dto.JobPositionId.HasValue || dto.ContractTypeId.HasValue) && dto.ContractStartDate.HasValue)
            {
                // Récupérer le contrat actif
                var activeContractForUpdate = await _db.EmployeeContracts
                    .Include(c => c.JobPosition)
                    .Include(c => c.ContractType)
                    .FirstOrDefaultAsync(c => 
                        c.EmployeeId == id && 
                        c.DeletedAt == null && 
                        c.EndDate == null);

                bool contractChanged = false;
                string? oldJobPositionName = null;
                string? newJobPositionName = null;
                string? oldContractTypeName = null;
                string? newContractTypeName = null;

                // Vérifier si le JobPosition a changé
                if (dto.JobPositionId.HasValue && activeContractForUpdate?.JobPositionId != dto.JobPositionId)
                {
                    var newJobPosition = await _db.JobPositions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(jp => jp.Id == dto.JobPositionId && jp.DeletedAt == null);

                    if (newJobPosition == null)
                        return NotFound(new { Message = "Poste de travail non trouvé" });

                    if (newJobPosition.CompanyId != employee.CompanyId)
                        return BadRequest(new { Message = "Le poste de travail ne correspond pas à la société de l'employé" });

                    oldJobPositionName = activeContractForUpdate?.JobPosition?.Name;
                    newJobPositionName = newJobPosition.Name;
                    contractChanged = true;
                }

                // Vérifier si le ContractType a changé
                if (dto.ContractTypeId.HasValue && activeContractForUpdate?.ContractTypeId != dto.ContractTypeId)
                {
                    var newContractType = await _db.ContractTypes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct => ct.Id == dto.ContractTypeId && ct.DeletedAt == null);

                    if (newContractType == null)
                        return NotFound(new { Message = "Type de contrat non trouvé" });

                    if (newContractType.CompanyId != employee.CompanyId)
                        return BadRequest(new { Message = "Le type de contrat ne correspond pas à la société de l'employé" });

                    oldContractTypeName = activeContractForUpdate?.ContractType?.ContractTypeName;
                    newContractTypeName = newContractType.ContractTypeName;
                    contractChanged = true;
                }

                // Si des changements sont détectés, créer un nouveau contrat et fermer l'ancien
                if (contractChanged)
                {
                    if (activeContractForUpdate != null)
                    {
                        // Fermer l'ancien contrat
                        activeContractForUpdate.EndDate = dto.ContractStartDate.Value;
                        activeContractForUpdate.ModifiedAt = updateTime;
                        activeContractForUpdate.ModifiedBy = userId;

                        // Logger la fin de contrat
                        await _eventLogService.LogSimpleEventAsync(
                            employeeId: id,
                            eventName: EmployeeEventLogService.EventNames.ContractTerminated,
                            oldValue: $"{oldJobPositionName} - {oldContractTypeName}",
                            newValue: dto.ContractStartDate.Value.ToString("yyyy-MM-dd"),
                            createdBy: userId
                        );
                    }

                    // Créer le nouveau contrat
                    var newContract = new EmployeeContract
                    {
                        EmployeeId = id,
                        CompanyId = employee.CompanyId,
                        JobPositionId = dto.JobPositionId ?? activeContractForUpdate!.JobPositionId,
                        ContractTypeId = dto.ContractTypeId ?? activeContractForUpdate!.ContractTypeId,
                        StartDate = dto.ContractStartDate.Value,
                        EndDate = null,
                        CreatedAt = updateTime,
                        CreatedBy = userId
                    };

                    _db.EmployeeContracts.Add(newContract);
                    await _db.SaveChangesAsync();

                    // Logger les changements
                    if (dto.JobPositionId.HasValue && oldJobPositionName != newJobPositionName)
                    {
                        await _eventLogService.LogRelationEventAsync(
                            employeeId: id,
                            eventName: EmployeeEventLogService.EventNames.JobPositionChanged,
                            oldValueId: activeContractForUpdate?.JobPositionId,
                            oldValueName: oldJobPositionName,
                            newValueId: dto.JobPositionId,
                            newValueName: newJobPositionName,
                            createdBy: userId
                        );
                    }

                    if (dto.ContractTypeId.HasValue && oldContractTypeName != newContractTypeName)
                    {
                        await _eventLogService.LogRelationEventAsync(
                            employeeId: id,
                            eventName: EmployeeEventLogService.EventNames.ContractTypeChanged,
                            oldValueId: activeContractForUpdate?.ContractTypeId,
                            oldValueName: oldContractTypeName,
                            newValueId: dto.ContractTypeId,
                            newValueName: newContractTypeName,
                            createdBy: userId
                        );
                    }

                    // Logger la création du nouveau contrat
                    await _eventLogService.LogSimpleEventAsync(
                        employeeId: id,
                        eventName: EmployeeEventLogService.EventNames.ContractCreated,
                        oldValue: null,
                        newValue: $"{newJobPositionName ?? oldJobPositionName} - {newContractTypeName ?? oldContractTypeName}",
                        createdBy: userId
                    );

                    hasChanges = true;
                }
            }

            // ===== MISE À JOUR DU SALAIRE (avec historique) =====
            if (dto.Salary.HasValue && dto.SalaryEffectiveDate.HasValue)
            {
                // Récupérer le contrat actif (nécessaire pour le salaire)
                var activeContractForSalary = await _db.EmployeeContracts  // ← RENOMMÉ
                    .FirstOrDefaultAsync(c => 
                        c.EmployeeId == id && 
                        c.DeletedAt == null && 
                        c.EndDate == null);

                if (activeContractForSalary == null)  // ← RENOMMÉ
                    return BadRequest(new { Message = "Aucun contrat actif trouvé pour cet employé. Veuillez d'abord créer un contrat." });

                // Récupérer le salaire actif
                var activeSalary = await _db.EmployeeSalaries
                    .FirstOrDefaultAsync(s => 
                        s.EmployeeId == id && 
                        s.DeletedAt == null && 
                        s.EndDate == null);

                // Vérifier si le salaire a changé
                if (activeSalary == null || activeSalary.BaseSalary != dto.Salary.Value)
                {
                    decimal oldSalary = activeSalary?.BaseSalary ?? 0;

                    if (activeSalary != null)
                    {
                        // Fermer l'ancien salaire
                        activeSalary.EndDate = dto.SalaryEffectiveDate.Value;
                        activeSalary.ModifiedAt = updateTime;
                        activeSalary.ModifiedBy = userId;
                    }

                    // Créer le nouveau salaire
                    var newSalary = new EmployeeSalary
                    {
                        EmployeeId = id,
                        ContractId = activeContractForSalary.Id,
                        BaseSalary = dto.Salary.Value,
                        EffectiveDate = dto.SalaryEffectiveDate.Value,
                        EndDate = null,
                        CreatedAt = updateTime,
                        CreatedBy = userId
                    };

                    _db.EmployeeSalaries.Add(newSalary);
                    await _db.SaveChangesAsync();

                    // Logger le changement de salaire
                    await _eventLogService.LogSimpleEventAsync(
                        employeeId: id,
                        eventName: EmployeeEventLogService.EventNames.SalaryUpdated,
                        oldValue: oldSalary.ToString("N2"),
                        newValue: dto.Salary.Value.ToString("N2"),
                        createdBy: userId
                    );

                    hasChanges = true;
                }
            }

            // ===== MISE À JOUR DE L'ADRESSE =====
            if (dto.CityId.HasValue && !string.IsNullOrEmpty(dto.AddressLine1) && !string.IsNullOrEmpty(dto.ZipCode))
            {
                // Récupérer l'adresse active
                var activeAddress = await _db.EmployeeAddresses
                    .Include(a => a.City)
                    .FirstOrDefaultAsync(a => 
                        a.EmployeeId == id && 
                        a.DeletedAt == null);

                bool addressChanged = false;

                // Vérifier si l'adresse a changé
                if (activeAddress != null)
                {
                    if (activeAddress.CityId != dto.CityId ||
                        activeAddress.AddressLine1 != dto.AddressLine1 ||
                        activeAddress.AddressLine2 != dto.AddressLine2 ||
                        activeAddress.ZipCode != dto.ZipCode)
                    {
                        addressChanged = true;
                    }
                }
                else
                {
                    addressChanged = true; // Nouvelle adresse
                }

                if (addressChanged)
                {
                    var newCity = await _db.Cities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == dto.CityId && c.DeletedAt == null);

                    if (newCity == null)
                        return NotFound(new { Message = "Ville non trouvée" });

                    string? oldAddressValue = null;
                    if (activeAddress != null)
                    {
                        // Marquer l'ancienne adresse comme supprimée (soft delete)
                        activeAddress.DeletedAt = updateTime;
                        activeAddress.DeletedBy = userId;

                        oldAddressValue = $"{activeAddress.AddressLine1}, {activeAddress.City?.CityName}";
                    }

                    // Créer la nouvelle adresse
                    var newAddress = new EmployeeAddress
                    {
                        EmployeeId = id,
                        CityId = dto.CityId.Value,
                        AddressLine1 = dto.AddressLine1,
                        AddressLine2 = dto.AddressLine2,
                        ZipCode = dto.ZipCode,
                        CreatedAt = updateTime,
                        CreatedBy = userId
                    };

                    _db.EmployeeAddresses.Add(newAddress);
                    await _db.SaveChangesAsync();

                    // Logger le changement d'adresse
                    await _eventLogService.LogSimpleEventAsync(
                        employeeId: id,
                        eventName: activeAddress == null 
                            ? EmployeeEventLogService.EventNames.AddressCreated 
                            : EmployeeEventLogService.EventNames.AddressUpdated,
                        oldValue: oldAddressValue,
                        newValue: $"{dto.AddressLine1}, {newCity.CityName}",
                        createdBy: userId
                    );

                    hasChanges = true;
                }
            }

            // Mettre à jour ModifiedAt et ModifiedBy si des changements ont été effectués
            if (hasChanges)
            {
                employee.ModifiedAt = updateTime;
                employee.ModifiedBy = userId;
                await _db.SaveChangesAsync();
            }

            // Récupérer l'employé mis à jour avec ses relations
            var updatedEmployee = await _db.Employees
                .AsNoTracking()
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Include(e => e.Departement)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.JobPosition)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.ContractType)
                .FirstAsync(e => e.Id == id);

            var activeContract = updatedEmployee.Contracts?
                .Where(c => c.DeletedAt == null && c.EndDate == null)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

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
                EducationLevelId = updatedEmployee.EducationLevelId,
                MaritalStatusId = updatedEmployee.MaritalStatusId,
                JobPostionName = activeContract?.JobPosition?.Name,
                CreatedAt = updatedEmployee.CreatedAt.DateTime
            };

            return Ok(readDto);
        }

        /// <summary>
        /// Mise à jour partielle d'un employé
        /// </summary>
        [HttpPatch("{id}")]
        // [HasPermission(EDIT_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> PartialUpdate(int id, [FromBody] EmployeeUpdateDto dto)
        {
            Console.WriteLine("=== PATCH appelé depuis le frontend ===");
            
            if (dto == null)
                return BadRequest(new { Message = "Données de mise à jour invalides" });

            var userId = User.GetUserId();
            Console.WriteLine($"User ID: {userId}");

            // Récupérer l'utilisateur authentifié
            var currentUser = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);

            if (currentUser?.Employee == null)
                return BadRequest(new { Message = "L'utilisateur n'est pas associé à un employé" });

            // Récupérer l'employé existant avec toutes ses relations
            var employee = await _db.Employees
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                .Include(e => e.Salaries!.Where(s => s.DeletedAt == null && s.EndDate == null))
                .Include(e => e.Addresses!.Where(a => a.DeletedAt == null))
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (employee == null)
                return NotFound(new { Message = "Employé non trouvé" });

            Console.WriteLine($"Employé trouvé: {employee.FirstName} {employee.LastName}");

            // Vérifier les permissions
            if (employee.CompanyId != currentUser.Employee.CompanyId)
            {
                return Forbid();
            }

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

            // Récupérer l'adresse active
            var activeAddress = employee.Addresses?
                .Where(a => a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            // Créer un DTO complet avec les valeurs actuelles
            var completeDto = new EmployeeUpdateDto
            {
                // Informations principales
                FirstName = dto.FirstName ?? employee.FirstName,
                LastName = dto.LastName ?? employee.LastName,
                CinNumber = dto.CinNumber ?? employee.CinNumber,
                DateOfBirth = dto.DateOfBirth ?? employee.DateOfBirth,
                Phone = dto.Phone ?? employee.Phone,
                Email = dto.Email ?? employee.Email,
                DepartementId = dto.DepartementId ?? employee.DepartementId,
                ManagerId = dto.ManagerId ?? employee.ManagerId,
                StatusId = dto.StatusId ?? employee.StatusId,
                GenderId = dto.GenderId ?? employee.GenderId,
                NationalityId = dto.NationalityId ?? employee.NationalityId,
                EducationLevelId = dto.EducationLevelId ?? employee.EducationLevelId,
                MaritalStatusId = dto.MaritalStatusId ?? employee.MaritalStatusId,
                CnssNumber = dto.CnssNumber ?? (string.IsNullOrEmpty(employee.CnssNumber) ? null : int.TryParse(employee.CnssNumber, out var cnss) ? cnss : null),
                CimrNumber = dto.CimrNumber ?? (string.IsNullOrEmpty(employee.CimrNumber) ? null : int.TryParse(employee.CimrNumber, out var cimr) ? cimr : null),

                // Informations de contrat
                JobPositionId = dto.JobPositionId ?? activeContract?.JobPositionId,
                ContractTypeId = dto.ContractTypeId ?? activeContract?.ContractTypeId,
                ContractStartDate = dto.ContractStartDate ?? activeContract?.StartDate,

                // Informations de salaire
                Salary = dto.Salary ?? activeSalary?.BaseSalary,
                SalaryEffectiveDate = dto.SalaryEffectiveDate ?? activeSalary?.EffectiveDate,

                // Informations d'adresse
                AddressLine1 = dto.AddressLine1 ?? activeAddress?.AddressLine1,
                AddressLine2 = dto.AddressLine2 ?? activeAddress?.AddressLine2,
                ZipCode = dto.ZipCode ?? activeAddress?.ZipCode,
                CityId = dto.CityId ?? activeAddress?.CityId
            };

    Console.WriteLine("=== Données reçues ===");
    Console.WriteLine($"FirstName: {dto.FirstName}");
    Console.WriteLine($"LastName: {dto.LastName}");
    Console.WriteLine($"Email: {dto.Email}");
    Console.WriteLine($"Phone: {dto.Phone}");
    Console.WriteLine("====================");

    // Valider le modèle
    if (!TryValidateModel(completeDto))
    {
        Console.WriteLine("Validation échouée:");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"- {error.ErrorMessage}");
        }
        return BadRequest(ModelState);
    }

    // Appeler la méthode de mise à jour complète avec le DTO fusionné
    var result = await Update(id, completeDto);
    
    Console.WriteLine("=== Mise à jour terminée ===");
    
    return result;
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
            employee.DeletedAt = DateTime.UtcNow;
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

        /// <summary>
        /// Récupère l'historique des modifications d'un employé
        /// </summary>
        [HttpGet("{id}/history")]
        //[HasPermission(VIEW_EMPLOYEE)]
        public async Task<ActionResult<IEnumerable<EmployeeEventLog>>> GetEmployeeHistory(int id)
        {
            var employeeExists = await _db.Employees.AnyAsync(e => e.Id == id && e.DeletedAt == null);
            if (!employeeExists)
                return NotFound(new { Message = "Employé non trouvé" });

            var history = await _db.EmployeeEventLogs
                .Where(e => e.employeeId == id)
                .OrderByDescending(e => e.createdAt)
                .ToListAsync();

            return Ok(history);
        }
    }
}