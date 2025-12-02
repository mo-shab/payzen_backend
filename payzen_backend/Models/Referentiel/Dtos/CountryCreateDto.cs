using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Referentiel.Dtos
{
    public class CountryCreateDto
    {
        [Required(ErrorMessage = "Le nom du pays est requis")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 500 caractères")]
        public required string CountryName { get; set; }

        [StringLength(500, ErrorMessage = "Le nom arabe ne peut pas dépasser 500 caractères")]
        public string? CountryNameAr { get; set; }

        [Required(ErrorMessage = "Le code pays est requis")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Le code doit contenir 2 ou 3 caractères")]
        public required string CountryCode { get; set; }

        [Required(ErrorMessage = "Le code téléphonique est requis")]
        [StringLength(10, ErrorMessage = "Le code téléphonique ne peut pas dépasser 10 caractères")]
        public required string CountryPhoneCode { get; set; }

        [Required(ErrorMessage = "La nationalité est requise")]
        [StringLength(500, ErrorMessage = "La nationalité ne peut pas dépasser 500 caractères")]
        public required string Nationality { get; set; }
    }
}