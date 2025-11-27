namespace payzen_backend.Models.Employee
{
    public class Employee
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string CinNumber { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required int Phone { get; set; }
        public required string Email { get; set; }
        public int CompanyId { get; set; }
        public int? ManagerId { get; set; }
        public int? StatusId { get; set; }
        public int? GenderId { get; set; }
        public int? NationalityId { get; set; }
        public int? EducationLevelId { get; set; }
        public int? MaritalStatusId { get; set; }
        
        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public Company.Company? Company { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee>? Subordinates { get; set; }
    }
}
