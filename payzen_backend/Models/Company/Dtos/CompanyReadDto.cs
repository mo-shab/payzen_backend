namespace payzen_backend.Models.Company.Dtos
{
    public class CompanyReadDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
        public string IceNumber { get; set; } = string.Empty;
        public string CnssNumber { get; set; } = string.Empty;
        public string IfNumber { get; set; } = string.Empty;
        public string RcNumber { get; set; } = string.Empty;
        public string RibNumber { get; set; } = string.Empty;
        public int PhoneNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public int? ManagedByCompanyId { get; set; }
        public string? ManagedByCompanyName { get; set; }
        public bool IsCabinetExpert { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}