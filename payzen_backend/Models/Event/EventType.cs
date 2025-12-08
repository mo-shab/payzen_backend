namespace payzen_backend.Models.Event
{
    public class EventType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<EventsEmployee> EventsEmployees { get; set; } = new List<EventsEmployee>();
    }
}
