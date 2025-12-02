using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeDocumentUpdateDto
    {
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 500 caractères")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Le chemin ne peut pas dépasser 1000 caractères")]
        public string? FilePath { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [StringLength(100, ErrorMessage = "Le type ne peut pas dépasser 100 caractères")]
        public string? DocumentType { get; set; }
    }
}