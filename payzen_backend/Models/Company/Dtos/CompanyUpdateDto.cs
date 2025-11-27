using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class CompanyUpdateDto
    {
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom de la société doit contenir entre 2 et 500 caractères")]
        public string? CompanyName { get; set; }

        [StringLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères")]
        public string? CompanyAddress { get; set; }

        public int? CityId { get; set; }
        public int? CountryId { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro ICE ne peut pas dépasser 500 caractères")]
        public string? IceNumber { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro CNSS ne peut pas dépasser 500 caractères")]
        public string? CnssNumber { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro IF ne peut pas dépasser 500 caractères")]
        public string? IfNumber { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro RC ne peut pas dépasser 500 caractères")]
        public string? RcNumber { get; set; }

        [StringLength(500, ErrorMessage = "Le numéro RIB ne peut pas dépasser 500 caractères")]
        public string? RibNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de téléphone doit être valide")]
        public int? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(500, ErrorMessage = "L'email ne peut pas dépasser 500 caractères")]
        public string? Email { get; set; }

        public int? ManagedByCompanyId { get; set; }
        public bool? IsCabinetExpert { get; set; }
    }
}