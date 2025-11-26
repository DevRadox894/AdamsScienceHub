using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdamsScienceHub.Models
{
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required]
        public string TopicTitle { get; set; } = null!;

        // FK to Subject
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        // Path to uploaded PDF (e.g. /materials/abc.pdf)
        public string FilePath { get; set; } = string.Empty;

        // optional YouTube url
        public string? VideoUrl { get; set; }

        // Optional: number of pages (can be set manually or kept 0)
        public int PageCount { get; set; } = 0;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
