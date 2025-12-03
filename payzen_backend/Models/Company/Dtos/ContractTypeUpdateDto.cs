using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class ContractTypeUpdateDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom du type de contrat doit contenir entre 2 et 100 caractères")]
        public string? ContractTypeName { get; set; }
    }
}
