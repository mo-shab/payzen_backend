using payzen_backend.Data;
using payzen_backend.Models.Event;

namespace payzen_backend.Services
{
    public class EventService
    {
        private readonly AppDbContext _db;

        public EventService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Crée un événement automatiquement
        /// </summary>
        public async Task CreateEventAsync(int employeeId, int eventTypeId, int createdBy, DateTime? eventTime = null)
        {
            var newEvent = new EventsEmployee
            {
                EmployeeId = employeeId,
                EventTypeId = eventTypeId,
                EventTime = eventTime ?? DateTime.Now,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            };

            _db.EventsEmployee.Add(newEvent);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Types d'événements prédéfinis
        /// </summary>
        public static class EventTypes
        {
            public const int SalaryComponentAdded = 1;      // Ajout d'un composant de salaire
            public const int SalaryComponentModified = 2;   // Modification d'un composant de salaire
            public const int SalaryComponentDeleted = 3;    // Suppression d'un composant de salaire
            public const int ContractCreated = 4;           // Création d'un contrat
            public const int ContractModified = 5;          // Modification d'un contrat
            public const int ContractTerminated = 6;        // Fin de contrat
            public const int EmployeeCreated = 7;           // Création d'employé
            public const int EmployeeModified = 8;          // Modification d'employé
            public const int EmployeeDeleted = 9;           // Suppression d'employé
            public const int StatusChanged = 10;            // Changement de statut
            public const int DepartmentChanged = 11;        // Changement de département
        }
    }
}