namespace payzen_backend.Models.Company.Dtos
{
    public class CompanyListDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsCabinetExpert { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CountryPhoneCode { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CompanyAddress { get; set; } = string.Empty;
        public string CnssNumber { get; set; } = string.Empty;
        public string? IceNumber { get; set; }
        public string? IfNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}