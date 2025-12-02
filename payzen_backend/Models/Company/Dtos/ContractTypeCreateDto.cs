using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Company.Dtos
{
    public class ContractTypeCreateDto
    {
        [Required(ErrorMessage = "Le nom du type du contrat est requis")]
        [StringLength(50)]
        public required string ContractTypeName { get; set; }
        [Required(ErrorMessage = "L'ID de la société est requis")]
        public int CompanyId { get; set; }
    }
}
