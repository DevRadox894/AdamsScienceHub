using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace AdamsScienceHub.Controllers
{
    public class PracticeQuestionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PracticeQuestionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // SHOW SUBJECTS THAT HAVE QUESTIONS
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _db.Subjects
                .Where(s => _db.Questions.Any(q => q.SubjectId == s.SubjectId))
                .ToListAsync();

            return View(subjects);
        }

        // QUIZ SETTINGS PAGE
        public IActionResult Configure(int subjectId)
        {
            var subject = _db.Subjects.Find(subjectId);
            if (subject == null) return NotFound();

            ViewBag.Subject = subject;
            return View();
        }

        // START QUIZ
        [HttpPost]
        public IActionResult StartQuiz(int subjectId, int limit = 15, int count = 15)
        {
            var random = new Random();
            var questions = _db.Questions
                .Where(q => q.SubjectId == subjectId)
                .AsEnumerable()
                .OrderBy(q => random.Next())
                .Take(count)
                .ToList();

            if (!questions.Any())
            {
                TempData["Error"] = "No questions available for this subject.";
                return RedirectToAction("Subjects");
            }

            var subject = _db.Subjects.Find(subjectId);
            if (subject == null)
            {
                return NotFound(); // or redirect to subjects page
            }

            ViewBag.Subject = subject;
            ViewBag.TimeLimit = limit;
           


            return View(questions);
        }


        //SUBMIT

        [HttpPost]
        public IActionResult SubmitQuiz(List<int> QuestionIds, List<string> UserAnswers, int timeUsedSeconds)
        {
            if (QuestionIds == null || UserAnswers == null)
            {
                return RedirectToAction("Subjects");
            }

            int score = 0;
            int total = QuestionIds.Count;

            for (int i = 0; i < total; i++)
            {
                string? answer = (i < UserAnswers.Count) ? UserAnswers[i] : null;
                var question = _db.Questions.Find(QuestionIds[i]);

                if (question != null && answer == question.CorrectAnswer)
                {
                    score++;
                }
            }

            double percentage = (total == 0) ? 0 : (score * 100.0 / total);

            ViewBag.Score = score;
            ViewBag.Total = total;
            ViewBag.CorrectCount = score;
            ViewBag.WrongCount = total - score;

            if (percentage == 100)
                ViewBag.PerformanceMessage = "Perfect Score! Absolute mastery!";
            else if (percentage >= 75)
                ViewBag.PerformanceMessage = "Great job! Strong performance!";
            else if (percentage >= 50)
                ViewBag.PerformanceMessage = "Good effort! Keep improving!";
            else
                ViewBag.PerformanceMessage = "Keep practicing! You’ll improve!";

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user != null && QuestionIds.Any())
            {
                var firstQuestion = _db.Questions
                    .Include(q => q.Subject)
                    .FirstOrDefault(q => q.QuestionId == QuestionIds.First());

                string subjectName = firstQuestion?.Subject?.SubjectName ?? "Unknown";

                var result = new QuizResult
                {
                    UserId = user.Id,
                    SubjectName = subjectName,
                    Score = percentage,
                    TotalQuestions = total,
                    CorrectAnswers = score,
                    WrongAnswers = total - score,
                    TimeSpent = TimeSpan.FromSeconds(timeUsedSeconds).ToString(@"hh\:mm\:ss"),
                    DateTaken = DateTime.Now
                };

                _db.QuizResults.Add(result);
                _db.SaveChanges();
            }

            HttpContext.Session.SetString("QuestionIds", JsonSerializer.Serialize(QuestionIds));
            HttpContext.Session.SetString("UserAnswers", JsonSerializer.Serialize(UserAnswers));

            return View("Result");
        }

        // REVIEW QUIZ
        public IActionResult Review()
        {
            var questionIdsJson = HttpContext.Session.GetString("QuestionIds");
            var userAnswersJson = HttpContext.Session.GetString("UserAnswers");

            if (string.IsNullOrEmpty(questionIdsJson) || string.IsNullOrEmpty(userAnswersJson))
                return RedirectToAction("Subjects");

            var questionIds = JsonSerializer.Deserialize<List<int>>(questionIdsJson) ?? new List<int>();
            var userAnswers = JsonSerializer.Deserialize<List<string>>(userAnswersJson) ?? new List<string>();

            // Fetch questions in the original order
            var questions = _db.Questions
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToList()
                .OrderBy(q => questionIds.IndexOf(q.QuestionId))
                .ToList();

            ViewBag.UserAnswers = userAnswers; // pass answers separately

            return View(questions); // still List<Question>
        }



    }
}
