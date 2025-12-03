namespace payzen_backend.Models.Dashboard.Dtos
{
    /// <summary>
    /// Réponse complète du dashboard avec statistiques et liste des employés
    /// </summary>
    public class DashboardResponseDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public List<EmployeeDashboardItemDto> Employees { get; set; } = new();
    }
}