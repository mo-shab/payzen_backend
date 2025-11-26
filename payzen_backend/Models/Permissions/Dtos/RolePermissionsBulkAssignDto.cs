using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Permissions.Dtos
{
    public class RolePermissionsBulkAssignDto
    {
        [Required(ErrorMessage = "L'ID du rôle est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du rôle doit être valide")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Au moins une permission doit être spécifiée")]
        [MinLength(1, ErrorMessage = "Au moins une permission doit être spécifiée")]
        public List<int> PermissionIds { get; set; } = new();
    }
}