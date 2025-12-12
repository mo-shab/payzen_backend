using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeUpdateDto
    {
        // ===== DONNÉES PRINCIPALES (Employee) =====
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le prénom doit contenir entre 2 et 500 caractères")]
        public string? FirstName { get; set; }

        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom de famille doit contenir entre 2 et 500 caractères")]
        public string? LastName { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro CIN ne peut pas dépasser 500 caractères")]
        public string? CinNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(500, ErrorMessage = "L'email ne peut pas dépasser 500 caractères")]
        public string? Email { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'ID du departement doit être valide")]
        public int? DepartementId { get; set; }

        public int? ManagerId { get; set; }
        public int? StatusId { get; set; }
        public int? GenderId { get; set; }
        public int? NationalityId { get; set; }
        public int? EducationLevelId { get; set; }
        public int? MaritalStatusId { get; set; }
        public int? CnssNumber { get; set; }
        public int? CimrNumber { get; set; }

        // ===== DONNÉES AVEC HISTORIQUE =====
        // Contrat - Si fourni, créera un nouveau contrat et fermera l'ancien
        public int? JobPositionId { get; set; }
        public int? ContractTypeId { get; set; }
        public DateTime? ContractStartDate { get; set; }

        // Salaire - Si fourni, créera un nouveau salaire et fermera l'ancien
        [Range(0.01, double.MaxValue, ErrorMessage = "Le salaire doit être supérieur à 0")]
        public decimal? Salary { get; set; }
        public DateTime? SalaryEffectiveDate { get; set; }

        // Adresse - Si fourni, créera une nouvelle adresse
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? ZipCode { get; set; }
        public int? CityId { get; set; }
    }
}