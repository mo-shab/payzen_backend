namespace payzen_backend.Models.Referentiel.Dtos
{
    public class EducationLevelReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}