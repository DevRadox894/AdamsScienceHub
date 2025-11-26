using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AdamsScienceHub.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters and spaces.")]
        [MaxLength(100)]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[\w-\.]+@(gmail\.com|yahoo\.com|hotmail\.com)$", ErrorMessage = "Email must be Gmail, Yahoo, or Hotmail.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string PasswordHash { get; set; }

        public string Role { get; set; } = "User";
        public List<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    }
}
