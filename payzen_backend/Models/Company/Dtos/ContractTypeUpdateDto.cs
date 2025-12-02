using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class ContractTypeUpdateDto
    {
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom du type de contrat doit contenir entre 2 et 500 caractères")]
        public string? ContractTypeName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la société doit être valide")]
        public int? CompanyId { get; set; }
    }
}
