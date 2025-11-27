using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Extensions;
using payzen_backend.Models.Employee;
using payzen_backend.Models.Employee.Dtos;
using payzen_backend.Authorization;
using payzen_backend.Services;
using payzen_backend.Models.Users;
using static payzen_backend.Models.Permissions.PermissionsConstants;

namespace payzen_backend.Controllers
{
    [Route("api/employees")]
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
        [HttpGet]
        [HasPermission(READ_EMPLOYEES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetAll()
        {
            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
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
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                NationalityId = e.NationalityId,
                EducationLevelId = e.EducationLevelId,
                MaritalStatusId = e.MaritalStatusId,
                CreatedAt = e.CreatedAt.DateTime
            });

            return Ok(result);
        }

        /// <summary>
        /// Récupère un employé par ID
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(VIEW_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> GetById(int id)
        {
            var employee = await _db.Employees
                .AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Include(e => e.Company)
                .Include(e => e.Manager)
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
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager != null ? $"{employee.Manager.FirstName} {employee.Manager.LastName}" : null,
                StatusId = employee.StatusId,
                GenderId = employee.GenderId,
                NationalityId = employee.NationalityId,
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
        [HasPermission(VIEW_COMPANY_EMPLOYEES)]
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
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                NationalityId = e.NationalityId,
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
        [HasPermission(VIEW_SUBORDINATES)]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetSubordinates(int managerId)
        {
            var managerExists = await _db.Employees.AnyAsync(e => e.Id == managerId && e.DeletedAt == null);
            if (!managerExists)
                return NotFound(new { Message = "Manager non trouvé" });

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.ManagerId == managerId && e.DeletedAt == null)
                .Include(e => e.Company)
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
                ManagerId = e.ManagerId,
                StatusId = e.StatusId,
                GenderId = e.GenderId,
                NationalityId = e.NationalityId,
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
        [HasPermission(CREATE_EMPLOYEE)]
        public async Task<ActionResult<EmployeeReadDto>> Create([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();

            // Vérifier que la société existe
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.DeletedAt == null);
            if (!companyExists)
                return NotFound(new { Message = "Société non trouvée" });

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
                ManagerId = dto.ManagerId,
                StatusId = dto.StatusId,
                GenderId = dto.GenderId,
                NationalityId = dto.NationalityId,
                EducationLevelId = dto.EducationLevelId,
                MaritalStatusId = dto.MaritalStatusId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            // 👇 NOUVEAU : Créer automatiquement un compte utilisateur si demandé
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
                    EmployeeId = employee.Id, // 👈 Lier à l'employé
                    Username = username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = userId
                };

                _db.Users.Add(createdUser);
                await _db.SaveChangesAsync();

                Console.WriteLine($"✅ User créé automatiquement - Username: {username}, Email: {dto.Email}");
            }

            // Récupérer l'employé créé avec ses relations
            var createdEmployee = await _db.Employees
                .AsNoTracking()
                .Include(e => e.Company)
                .Include(e => e.Manager)
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
                ManagerId = createdEmployee.ManagerId,
                ManagerName = createdEmployee.Manager != null ? $"{createdEmployee.Manager.FirstName} {createdEmployee.Manager.LastName}" : null,
                StatusId = createdEmployee.StatusId,
                GenderId = createdEmployee.GenderId,
                NationalityId = createdEmployee.NationalityId,
                EducationLevelId = createdEmployee.EducationLevelId,
                MaritalStatusId = createdEmployee.MaritalStatusId,
                CreatedAt = createdEmployee.CreatedAt.DateTime
            };

            // Retourner les infos du compte créé dans la réponse
            if (dto.CreateUserAccount && createdUser != null)
            {
                return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new
                {
                    Employee = readDto,
                    UserAccount = new
                    {
                        Username = createdUser.Username,
                        Email = createdUser.Email,
                        TemporaryPassword = temporaryPassword, // À envoyer par email en production
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
        [HasPermission(EDIT_EMPLOYEE)]
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

            if (dto.Phone.HasValue)
                employee.Phone = dto.Phone.Value;

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

            if (dto.NationalityId.HasValue)
                employee.NationalityId = dto.NationalityId.Value;

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
                ManagerId = updatedEmployee.ManagerId,
                ManagerName = updatedEmployee.Manager != null ? $"{updatedEmployee.Manager.FirstName} {updatedEmployee.Manager.LastName}" : null,
                StatusId = updatedEmployee.StatusId,
                GenderId = updatedEmployee.GenderId,
                NationalityId = updatedEmployee.NationalityId,
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
        [HasPermission(DELETE_EMPLOYEE)]
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
    }
}