using System.ComponentModel.DataAnnotations;

namespace payzen_backend.Models.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public required string Password { get; set; }
    }
}