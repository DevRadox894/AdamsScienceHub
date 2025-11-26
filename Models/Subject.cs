using System.ComponentModel.DataAnnotations;

namespace AdamsScienceHub.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }

        [Required]
        public string? SubjectName { get; set; }

        public string? ImagePath { get; set; }

        public bool CalculatorEnabled { get; set; } = false;

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
