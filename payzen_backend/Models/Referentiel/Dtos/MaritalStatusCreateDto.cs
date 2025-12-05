using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Referentiel.Dtos
{
    /// <summary>
    /// DTO pour la création d'un statut marital
    /// </summary>
    public class MaritalStatusCreateDto
    {
        [Required(ErrorMessage = "Le nom du statut marital est requis")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères")]
        public required string Name { get; set; }
    }
}