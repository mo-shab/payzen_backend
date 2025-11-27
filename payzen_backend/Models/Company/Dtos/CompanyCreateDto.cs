using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class CompanyCreateDto
    {
        [Required(ErrorMessage = "Le nom de la société est requis")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom de la société doit contenir entre 2 et 500 caractères")]
        public required string CompanyName { get; set; }

        [Required(ErrorMessage = "L'adresse de la société est requise")]
        [StringLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères")]
        public required string CompanyAddress { get; set; }

        public int? CityId { get; set; }
        public int? CountryId { get; set; }

        [Required(ErrorMessage = "Le numéro ICE est requis")]
        [StringLength(500, ErrorMessage = "Le numéro ICE ne peut pas dépasser 500 caractères")]
        public required string IceNumber { get; set; }

        [Required(ErrorMessage = "Le numéro CNSS est requis")]
        [StringLength(500, ErrorMessage = "Le numéro CNSS ne peut pas dépasser 500 caractères")]
        public required string CnssNumber { get; set; }

        [Required(ErrorMessage = "Le numéro IF est requis")]
        [StringLength(500, ErrorMessage = "Le numéro IF ne peut pas dépasser 500 caractères")]
        public required string IfNumber { get; set; }

        [Required(ErrorMessage = "Le numéro RC est requis")]
        [StringLength(500, ErrorMessage = "Le numéro RC ne peut pas dépasser 500 caractères")]
        public required string RcNumber { get; set; }

        [Required(ErrorMessage = "Le numéro RIB est requis")]
        [StringLength(500, ErrorMessage = "Le numéro RIB ne peut pas dépasser 500 caractères")]
        public required string RibNumber { get; set; }

        [Required(ErrorMessage = "Le numéro de téléphone est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de téléphone doit être valide")]
        public required int PhoneNumber { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(500, ErrorMessage = "L'email ne peut pas dépasser 500 caractères")]
        public required string Email { get; set; }

        public int? ManagedByCompanyId { get; set; }
        public bool IsCabinetExpert { get; set; } = false;
    }
}