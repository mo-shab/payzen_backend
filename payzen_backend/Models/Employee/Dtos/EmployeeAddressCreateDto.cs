using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeAddressCreateDto
    {
        [Required(ErrorMessage = "L'ID de l'employé est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'employé doit être valide")]
        public required int EmployeeId { get; set; }

        [Required(ErrorMessage = "L'adresse ligne 1 est requise")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "L'adresse doit contenir entre 5 et 500 caractères")]
        public required string AddressLine1 { get; set; }

        [StringLength(500, ErrorMessage = "L'adresse ligne 2 ne peut pas dépasser 500 caractères")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Le code postal est requis")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Le code postal doit contenir entre 4 et 20 caractères")]
        public required string ZipCode { get; set; }

        [Required(ErrorMessage = "L'ID de la ville est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la ville doit être valide")]
        public required int CityId { get; set; }

        [Required(ErrorMessage = "L'ID du pays est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du pays doit être valide")]
        public required int CountryId { get; set; }
    }
}