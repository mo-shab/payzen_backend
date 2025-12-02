using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeAddressUpdateDto
    {
        [StringLength(500, MinimumLength = 5, ErrorMessage = "L'adresse doit contenir entre 5 et 500 caractères")]
        public string? AddressLine1 { get; set; }

        [StringLength(500, ErrorMessage = "L'adresse ligne 2 ne peut pas dépasser 500 caractères")]
        public string? AddressLine2 { get; set; }

        [StringLength(20, MinimumLength = 4, ErrorMessage = "Le code postal doit contenir entre 4 et 20 caractères")]
        public string? ZipCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la ville doit être valide")]
        public int? CityId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'ID du pays doit être valide")]
        public int? CountryId { get; set; }
    }
}