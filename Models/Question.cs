using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace AdamsScienceHub.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public string QuestionText { get; set; } = "";

        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";

        public string CorrectAnswer { get; set; } = "";  // "A", "B", "C", or "D"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
