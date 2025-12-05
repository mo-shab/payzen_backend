namespace payzen_backend.Models.Referentiel.Dtos
{
    /// <summary>
    /// DTO pour la lecture d'un statut marital
    /// </summary>
    public class MaritalStatusReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}