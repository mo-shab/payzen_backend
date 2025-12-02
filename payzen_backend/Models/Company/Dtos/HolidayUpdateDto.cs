using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class HolidayUpdateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la société doit être valide")]
        public int? CompanyId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "L'ID du pays doit être valide")]
        public int? CountryId { get; set; }

        public DateTime? HolidayDate { get; set; }

        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 500 caractères")]
        public string? Name { get; set; }

        public bool? IsFixedAnnually { get; set; }
    }
}