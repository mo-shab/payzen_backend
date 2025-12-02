namespace payzen_backend.Models.Employee
{
    /// <summary>
    /// Composants du salaire (Primes, Indemnités, Déductions, etc.)
    /// </summary>
    public class EmployeeSalaryComponent
    {
        public int Id { get; set; }
        public int EmployeeSalaryId { get; set; }
        public required string ComponentType { get; set; } // Prime, Indemnité, Déduction, Bonus, etc.
        public decimal Amount { get; set; }
        public required DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Champs d'audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation properties
        public EmployeeSalary? EmployeeSalary { get; set; } = null!;
    }
}