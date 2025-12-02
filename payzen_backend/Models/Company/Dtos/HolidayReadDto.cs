namespace payzen_backend.Models.Company.Dtos
{
    public class HolidayReadDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public DateTime HolidayDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsFixedAnnually { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}