public class UserProgressViewModel
{
    // Summary
    public int TotalQuizzesTaken { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public double WorstScore { get; set; }

    // Subject Performance
    public List<SubjectPerformance> SubjectPerformances { get; set; } = new List<SubjectPerformance>();

    // Quiz History
    public List<QuizHistoryItem> QuizHistory { get; set; } = new List<QuizHistoryItem>();
}

public class SubjectPerformance
{
    public string SubjectName { get; set; } = string.Empty;
    public int QuizzesTaken { get; set; }
    public double AverageScore { get; set; }
}

public class QuizHistoryItem
{
    public DateTime Date { get; set; }
    public string Subject { get; set; } = string.Empty;
    public double Score { get; set; }
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public string TimeSpent { get; set; } = string.Empty;
}
