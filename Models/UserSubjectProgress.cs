using System.ComponentModel.DataAnnotations;

namespace AdamsScienceHub.Models
{
    public class UserSubjectProgress
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }          // FK to Users table (assumes integer id)
        public int SubjectId { get; set; }       // FK to Subjects

        // Total seconds spent
        public long TotalSeconds { get; set; } = 0;

        // last updated time
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
