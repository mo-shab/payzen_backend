using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    /// <summary>
    /// DTO pour créer un employé
    /// </summary>
    public class EmployeeCreateDto
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le prénom doit contenir entre 2 et 500 caractères")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom de famille est requis")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom de famille doit contenir entre 2 et 500 caractères")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Le numéro CIN est requis")]
        [StringLength(500, ErrorMessage = "Le numéro CIN ne peut pas dépasser 500 caractères")]
        public required string CinNumber { get; set; }

        [Required(ErrorMessage = "La date de naissance est requise")]
        public required DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Le numéro de téléphone est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de téléphone doit être valide")]
        public required int Phone { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(500, ErrorMessage = "L'email ne peut pas dépasser 500 caractères")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "L'ID de la société est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la société doit être valide")]
        public required int CompanyId { get; set; }
        [Required(ErrorMessage = "L'ID du département est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du département doit être valide")]
        public required int DepartementId { get; set; }

        public int? ManagerId { get; set; } = null;
        public int? StatusId { get; set; }
        public int? GenderId { get; set; }
        public int? NationalityId { get; set; }
        public int? EducationLevelId { get; set; }
        public int? MaritalStatusId { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        public string? Password { get; set; } // Optionnel : si null, génère un mot de passe temporaire
        
        public bool CreateUserAccount { get; set; } = true; // Par défaut, créer le compte
    }
}