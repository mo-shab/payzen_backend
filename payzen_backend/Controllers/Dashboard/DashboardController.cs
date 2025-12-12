using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Models.Dashboard.Dtos;

namespace payzen_backend.Controllers.Dashboard
{
    [ApiController]
    [Route("api/dashboard/employees")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db) { _db = db; }
        

        /// <summary>
        /// Récupère les statistiques du dashboard avec la liste des employés
        /// </summary>
        /// <returns>Statistiques globales et liste détaillée des employés</returns>
        [HttpGet]
        //[HasPermission("VIEW_DASHBOARD")]
        [Produces("application/json")]
        public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
        {
            // Récupérer le userId depuis les claims du token JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "uid");
            
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié" });
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest(new { Message = "ID utilisateur invalide" });
            }

            // Récupérer l'utilisateur avec son employé associé pour obtenir la CompanyId
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && u.DeletedAt == null);

            if (user == null)
            {
                return NotFound(new { Message = "Utilisateur non trouvé" });
            }

            if (user.Employee == null)
            {
                return BadRequest(new { Message = "L'utilisateur n'est pas associé à un employé" });
            }

            var companyId = user.Employee.CompanyId;

            // Comptage total des employés non supprimés de la même entreprise
            var totalEmployees = await _db.Employees
                .Where(e => e.DeletedAt == null && e.CompanyId == companyId)
                .CountAsync();

            // Comptage des employés actifs de la même entreprise
            var activeEmployees = await _db.Employees
                .Where(e => e.DeletedAt == null && e.CompanyId == companyId && e.Status != null && e.Status.Name == "Active")
                .CountAsync();
            
            // Récupération des départements de la même entreprise
            var departements = await _db.Departement
                .Where(d => d.DeletedAt == null && d.CompanyId == companyId)
                .Select(d => d.DepartementName)
                .ToListAsync();

            // Récupération des Status
            var statuses = await _db.Statuses
                .Where(s => s.DeletedAt == null)
                .Select(s => s.Name)
                .ToListAsync();

            // Récupération des employés avec toutes les relations nécessaires (filtrés par CompanyId)
            var employees = await _db.Employees
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.DeletedAt == null && e.CompanyId == companyId)
                .Include(e => e.Company)
                .Include(e => e.Departement)
                .Include(e => e.Status)
                .Include(e => e.Manager)
                .Include(e => e.Documents)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.JobPosition)
                .Include(e => e.Contracts!.Where(c => c.DeletedAt == null && c.EndDate == null))
                    .ThenInclude(c => c.ContractType)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new EmployeeDashboardItemDto
                {
                    Id = e.Id.ToString(),
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Contracts != null
                        ? e.Contracts
                            .Where(c => c.DeletedAt == null && c.EndDate == null)
                            .OrderByDescending(c => c.StartDate)
                            .Select(c => c.JobPosition!.Name)
                            .FirstOrDefault() ?? "Non assigné"
                        : "Non assigné",
                    Department = e.Departement != null ? e.Departement.DepartementName : "Non assigné",
                    Status = MapStatusToFrontend(e.Status != null ? e.Status.Name : ""),
                    StartDate = e.Contracts != null
                        ? e.Contracts
                            .Where(c => c.DeletedAt == null && c.EndDate == null)
                            .OrderByDescending(c => c.StartDate)
                            .Select(c => c.StartDate.ToString("yyyy-MM-dd"))
                            .FirstOrDefault() ?? ""
                        : "",
                    MissingDocuments = e.Documents != null
                        ? e.Documents.Count(d => d.DeletedAt == null && string.IsNullOrEmpty(d.FilePath))
                        : 0,
                    ContractType = e.Contracts != null
                        ? e.Contracts
                            .Where(c => c.DeletedAt == null && c.EndDate == null)
                            .OrderByDescending(c => c.StartDate)
                            .Select(c => c.ContractType!.ContractTypeName)
                            .FirstOrDefault() ?? ""
                        : "",
                    Manager = e.Manager != null
                        ? $"{e.Manager.FirstName} {e.Manager.LastName}"
                        : null
                })
                .ToListAsync();




            var response = new DashboardResponseDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                Employees = employees,
                Departements = departements,
                Statuses = statuses
            };

            return Ok(response);
        }

        /// <summary>
        /// Mappe le statut de la base de données vers les valeurs attendues par le frontend
        /// </summary>
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