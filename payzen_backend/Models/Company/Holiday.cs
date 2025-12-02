namespace payzen_backend.Models.Company
{
    /// <summary>
    /// Gestion des jours fériés par entreprise et pays
    /// </summary>
    public class Holiday
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CountryId { get; set; }
        public required DateTime HolidayDate { get; set; }
        public required string Name { get; set; }
        public bool IsFixedAnnually { get; set; } = false; // Ex: 1er Mai est fixe chaque année

        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public Company? Company { get; set; } = null!;
        public Referentiel.Country? Country { get; set; } = null!;
    }
}
