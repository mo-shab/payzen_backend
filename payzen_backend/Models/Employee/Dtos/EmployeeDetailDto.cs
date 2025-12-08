namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CinNumber { get; set; } = string.Empty;
        public string? MaritalStatusName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? StatusName { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Phone { get; set; }
        public string? CountryPhoneCode { get; set; }
        
        // Adresse
        public EmployeeAddressDto? Address { get; set; }
        
        // Informations de contrat
        public string? JobPositionName { get; set; }
        public string? ManagerName { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public string? ContractTypeName { get; set; }
        // Informations salariales
        public decimal? BaseSalary { get; set; }
        public List<SalaryComponentDto> SalaryComponents { get; set; } = new();
        public decimal TotalSalary { get; set; }
        
        // Cotisations
        public int? CNSS { get; set; }
        public int? AMO { get; set; }
        public int? CIMR { get; set; }
        // Evenements
        public List<dynamic> Events { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeAddressDto
    {
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
    }

    public class SalaryComponentDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}