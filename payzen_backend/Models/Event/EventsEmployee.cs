namespace payzen_backend.Models.Event
{
    public class EventsEmployee
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int EventTypeId { get; set; }
        public DateTime EventTime { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public Employee.Employee? Employee { get; set; }
        public EventType? EventType { get; set; }
    }
}