namespace payzen_backend.Models.Company.Dtos
{
    public class ContractTypeReadDto
    {
        public int Id { get; set; }
        public required string ContractTypeName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
