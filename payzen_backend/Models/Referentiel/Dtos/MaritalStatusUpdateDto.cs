using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Referentiel.Dtos
{
    /// <summary>
    /// DTO pour la mise à jour d'un statut marital
    /// </summary>
    public class MaritalStatusUpdateDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères")]
        public string? Name { get; set; }
    }
}