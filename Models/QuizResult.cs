using AdamsScienceHub.Models;

public class QuizResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTime DateTaken { get; set; }
    public int TotalQuestions { get; set; }
    public double Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public string TimeSpent { get; set; } = string.Empty;

    // Navigation property
    public User? User { get; set; }
}
