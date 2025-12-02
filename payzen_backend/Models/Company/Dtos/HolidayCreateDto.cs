using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class HolidayCreateDto
    {
        [Required(ErrorMessage = "L'ID de la société est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la société doit être valide")]
        public required int CompanyId { get; set; }

        [Required(ErrorMessage = "L'ID du pays est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du pays doit être valide")]
        public required int CountryId { get; set; }

        [Required(ErrorMessage = "La date du jour férié est requise")]
        public required DateTime HolidayDate { get; set; }

        [Required(ErrorMessage = "Le nom du jour férié est requis")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 500 caractères")]
        public required string Name { get; set; }

        public bool IsFixedAnnually { get; set; } = false;
    }
}