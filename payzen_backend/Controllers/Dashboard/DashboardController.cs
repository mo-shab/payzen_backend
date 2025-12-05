using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Authorization;
using payzen_backend.Data;
using payzen_backend.Models.Dashboard.Dtos;

namespace payzen_backend.Controllers.Dashboard
{
    [ApiController]
    [Route("api/dashboard")]
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