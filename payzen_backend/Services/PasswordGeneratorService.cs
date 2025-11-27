using System.Security.Cryptography;
using System.Text;

namespace payzen_backend.Services
{
    /// <summary>
    /// Service pour générer des mots de passe sécurisés
    /// </summary>
    public class PasswordGeneratorService
    {
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!@#$%^&*";

        /// <summary>
        /// Génère un mot de passe temporaire sécurisé
        /// Format : 2 majuscules + 4 minuscules + 2 chiffres + 1 caractère spécial
        /// Exemple : AB12cd34!
        /// </summary>
        public string GenerateTemporaryPassword(int length = 12)
        {
            var password = new StringBuilder();
            var allChars = UpperCase + LowerCase + Digits + SpecialChars;

            // Garantir au moins 1 de chaque type
            password.Append(UpperCase[RandomNumberGenerator.GetInt32(UpperCase.Length)]);
            password.Append(UpperCase[RandomNumberGenerator.GetInt32(UpperCase.Length)]);
            password.Append(LowerCase[RandomNumberGenerator.GetInt32(LowerCase.Length)]);
            password.Append(LowerCase[RandomNumberGenerator.GetInt32(LowerCase.Length)]);
            password.Append(Digits[RandomNumberGenerator.GetInt32(Digits.Length)]);
            password.Append(Digits[RandomNumberGenerator.GetInt32(Digits.Length)]);
            password.Append(SpecialChars[RandomNumberGenerator.GetInt32(SpecialChars.Length)]);

            // Compléter avec des caractères aléatoires
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
            }

            // Mélanger les caractères
            return new string(password.ToString().OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray());
        }

        /// <summary>
        /// Génère un nom d'utilisateur basé sur le prénom et nom
        /// Format : prenom.nom ou prenom.nom2 si existe déjà
        /// </summary>
        public string GenerateUsername(string firstName, string lastName, int? suffix = null)
        {
            var username = $"{firstName.ToLower().Trim()}.{lastName.ToLower().Trim()}";
            
            // Remplacer les espaces et caractères spéciaux
            username = new string(username.Where(c => char.IsLetterOrDigit(c) || c == '.').ToArray());
            
            if (suffix.HasValue)
            {
                username += suffix.Value;
            }
            
            return username;
        }
    }
}