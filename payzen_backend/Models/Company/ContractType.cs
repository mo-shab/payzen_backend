namespace payzen_backend.Models.Company
{
    public class ContractType
    {
        public int Id { get; set; }
        public required string ContractTypeName { get; set; }
        public int CompanyId { get; set; }

        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public Company? Company { get; set; } = null!;
        public ICollection<Employee.EmployeeContract>? Employees { get; set; }
    }
}
