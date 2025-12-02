namespace payzen_backend.Models.Company
{
    public class Company
    {
        public int Id { get; set; }
        public required string CompanyName { get; set; }
        public required string CompanyAddress { get; set; }
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
        public required string IceNumber { get; set; }
        public required string CnssNumber { get; set; }
        public required string IfNumber { get; set; }
        public required string RcNumber { get; set; }
        public required string RibNumber { get; set; }
        public required int PhoneNumber { get; set; }
        public required string Email { get; set; }
        public int? ManagedByCompanyId { get; set; }
        public bool IsCabinetExpert { get; set; } = false;

        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public Company? ManagedByCompany { get; set; }
        public ICollection<Company>? ManagedCompanies { get; set; }
        public ICollection<Employee.Employee>? Employees { get; set; }
        public Referentiel.City? City { get; set; }
        public Referentiel.Country? Country { get; set; }
    }
}
