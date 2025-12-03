using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Referentiel.Dtos
{
    public class EducationLevelCreateDto
    {
        [Required(ErrorMessage = "Le nom du niveau d'éducation est requis")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom du niveau d'éducation doit contenir entre 2 et 100 caractères")]
        public required string Name { get; set; }
    }
}