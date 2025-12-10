namespace payzen_backend.Models.Employee.Dtos
{
    /// <summary>
    /// DTO contenant toutes les données nécessaires pour le formulaire de création/modification d'employé
    /// </summary>
    public class EmployeeFormDataDto
    {
        // Données de référence générales
        public List<StatusDto> Statuses { get; set; } = new();
        public List<GenderDto> Genders { get; set; } = new();
        public List<EducationLevelDto> EducationLevels { get; set; } = new();
        public List<MaritalStatusDto> MaritalStatuses { get; set; } = new();
        public List<NationalityDto> Nationalities { get; set; } = new();

        // Données géographiques
        public List<CountryDto> Countries { get; set; } = new();
        public List<CityDto> Cities { get; set; } = new();
        
        // Données de l'entreprise (filtrées par companyId)
        public List<DepartementDto> Departements { get; set; } = new();
        public List<JobPositionDto> JobPositions { get; set; } = new();
        public List<ContractTypeDto> ContractTypes { get; set; } = new();
        public List<EmployeeDto> PotentialManagers { get; set; } = new();
    }

    // DTOs simplifiés pour les listes
    public class StatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class GenderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class EducationLevelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class MaritalStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CountryDto
    {
        public int Id { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string? CountryPhoneCode { get; set; }
    }

    public class CityDto
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
    }

    public class DepartementDto
    {
        public int Id { get; set; }
        public string DepartementName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }

    public class JobPositionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }

    public class ContractTypeDto
    {
        public int Id { get; set; }
        public string ContractTypeName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DepartementName { get; set; }
    }

    public class NationalityDto
    {   public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}