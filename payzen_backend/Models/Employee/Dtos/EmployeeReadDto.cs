using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Employee.Dtos
{
    public class EmployeeReadDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CinNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int? StatusId { get; set; }
        public int? GenderId { get; set; }
        public int? NationalityId { get; set; }
        public int? EducationLevelId { get; set; }
        public int? MaritalStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}