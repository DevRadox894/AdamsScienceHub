using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdamsScienceHub.Models
{
    public class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectId { get; set; }

        [Required]
        public string SubjectName { get; set; } = null!;

        public string? ImagePath { get; set; }

        public bool CalculatorEnabled { get; set; } = false;

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
