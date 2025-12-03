namespace payzen_backend.Models.Dashboard.Dtos
{
    /// <summary>
    /// Représentation d'un employé dans le dashboard
    /// </summary>
    public class EmployeeDashboardItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // 'active' | 'on_leave' | 'inactive'
        public string StartDate { get; set; } = string.Empty;
        public int MissingDocuments { get; set; }
        public string ContractType { get; set; } = string.Empty; // 'CDI' | 'CDD' | 'Stage'
        public string? Manager { get; set; }
    }
}