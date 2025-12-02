using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Referentiel.Dtos
{
    public class CountryUpdateDto
    {
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 500 caractères")]
        public string? CountryName { get; set; }

        [StringLength(500, ErrorMessage = "Le nom arabe ne peut pas dépasser 500 caractères")]
        public string? CountryNameAr { get; set; }

        [StringLength(3, MinimumLength = 2, ErrorMessage = "Le code doit contenir 2 ou 3 caractères")]
        public string? CountryCode { get; set; }

        [StringLength(10, ErrorMessage = "Le code téléphonique ne peut pas dépasser 10 caractères")]
        public string? CountryPhoneCode { get; set; }

        [StringLength(500, ErrorMessage = "La nationalité ne peut pas dépasser 500 caractères")]
        public string? Nationality { get; set; }
    }
}