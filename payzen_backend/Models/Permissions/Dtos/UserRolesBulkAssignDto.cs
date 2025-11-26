using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Permissions.Dtos
{
    /// <summary>
    /// DTO pour assigner plusieurs rôles à un utilisateur en masse
    /// </summary>
    public class UserRolesBulkAssignDto
    {
        [Required(ErrorMessage = "L'ID de l'utilisateur est requis")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'utilisateur doit être valide")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Au moins un rôle doit être spécifié")]
        [MinLength(1, ErrorMessage = "Au moins un rôle doit être spécifié")]
        public List<int> RoleIds { get; set; } = new();
    }
}