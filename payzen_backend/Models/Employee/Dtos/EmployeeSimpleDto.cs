namespace payzen_backend.Models.Employee.Dtos
{
    /// <summary>
    /// DTO simplifié pour la liste des employés actifs d'autres entreprises
    /// </summary>
    public class EmployeeSimpleDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public string? StatusName { get; set; }
    }
}