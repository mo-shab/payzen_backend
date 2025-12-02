using payzen_backend.Models.Company;

namespace payzen_backend.Models.Referentiel
{
    /// <summary>
    /// Référentiel des pays pour adresses et nationalités
    /// </summary>
    public class Country
    {
        public int Id { get; set; }
        public required string CountryName { get; set; }
        public string? CountryNameAr { get; set; }
        public required string CountryCode { get; set; }
        public required string CountryPhoneCode { get; set; }
        public required string Nationality { get; set; }

        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public ICollection<City>? Cities { get; set; }
        public ICollection<Company.Company>? Companies { get; set; }
        public ICollection<Holiday>? Holidays { get; set; }
    }
}